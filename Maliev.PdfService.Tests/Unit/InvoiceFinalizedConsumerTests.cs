using Maliev.MessagingContracts;
using Maliev.MessagingContracts.Contracts.Invoices;
using Maliev.PdfService.Api.Consumers;
using Maliev.PdfService.Api.Services;
using Maliev.PdfService.Infrastructure.Data;
using Maliev.PdfService.Domain.Entities;
using MassTransit;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Maliev.PdfService.Tests.Unit;

/// <summary>
/// Unit tests for invoice finalized consumer PDF generation behavior.
/// </summary>
public class InvoiceFinalizedConsumerTests : IDisposable
{
    private readonly Mock<IPdfGenerator> _pdfGeneratorMock = new();
    private readonly Mock<IUploadServiceClient> _uploadServiceMock = new();
    private readonly Mock<ILogger<InvoiceFinalizedConsumer>> _loggerMock = new();
    private readonly InvoiceFinalizedConsumer _consumer;
    private readonly SqliteConnection _connection;
    private readonly PdfDbContext _dbContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="InvoiceFinalizedConsumerTests"/> class.
    /// </summary>
    public InvoiceFinalizedConsumerTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<PdfDbContext>()
            .UseSqlite(_connection)
            .Options;
        _dbContext = new PdfDbContext(options);
        _dbContext.Database.EnsureCreated();
        var dbContext = _dbContext;

        var invoiceClientMock = new Moq.Mock<Maliev.PdfService.Api.Services.IInvoiceServiceClient>();
        _consumer = new InvoiceFinalizedConsumer(_pdfGeneratorMock.Object, _uploadServiceMock.Object, dbContext, _loggerMock.Object, invoiceClientMock.Object);
    }

    /// <summary>
    /// Verifies that invoice consumption calls PDF generation and upload services.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Fact]
    public async Task Consume_CallsGeneratorAndUpload()
    {
        // Arrange
        var invoiceId = Guid.NewGuid();
        var payload = new InvoiceCreatedEventPayload(invoiceId, "INV-001", null, null, Guid.NewGuid(), 1000.0, "USD", null, DateTimeOffset.UtcNow);
        var message = new InvoiceCreatedEvent(Guid.NewGuid(), "InvoiceCreated", MessageType.Event, "1.0", "InvoiceService", new[] { "PdfService" }, Guid.NewGuid(), null, DateTimeOffset.UtcNow, false, payload);
        var contextMock = new Mock<ConsumeContext<InvoiceCreatedEvent>>();
        contextMock.Setup(x => x.Message).Returns(message);

        _pdfGeneratorMock.Setup(x => x.GeneratePdfAsync(DocumentType.Invoice, It.IsAny<object>(), It.IsAny<string>()))
            .ReturnsAsync(new byte[] { 1, 2, 3 });

        _uploadServiceMock.Setup(x => x.UploadFileAsync(It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("https://storage.com/inv.pdf");

        _pdfGeneratorMock.Setup(x => x.GetStoragePath(It.IsAny<DocumentType>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns("some/path");

        // Act
        await _consumer.Consume(contextMock.Object);

        // Assert
        _pdfGeneratorMock.Verify(x => x.GeneratePdfAsync(DocumentType.Invoice, It.IsAny<object>(), It.IsAny<string>()), Times.Once);
        _uploadServiceMock.Verify(x => x.UploadFileAsync(It.IsAny<string>(), It.IsAny<byte[]>(), "application/pdf", It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Verifies that PDF generator failures are propagated by the invoice consumer.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Fact]
    public async Task Consume_Throws_WhenGeneratorFails()
    {
        // Arrange
        var invoiceId = Guid.NewGuid();
        var payload = new InvoiceCreatedEventPayload(invoiceId, "INV-FAIL", null, null, Guid.NewGuid(), 1000.0, "USD", null, DateTimeOffset.UtcNow);
        var message = new InvoiceCreatedEvent(Guid.NewGuid(), "InvoiceCreated", MessageType.Event, "1.0", "InvoiceService", new[] { "PdfService" }, Guid.NewGuid(), null, DateTimeOffset.UtcNow, false, payload);
        var contextMock = new Mock<ConsumeContext<InvoiceCreatedEvent>>();
        contextMock.Setup(x => x.Message).Returns(message);

        _pdfGeneratorMock.Setup(x => x.GeneratePdfAsync(It.IsAny<DocumentType>(), It.IsAny<object>(), It.IsAny<string>()))
            .ThrowsAsync(new Exception("Fail"));

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => _consumer.Consume(contextMock.Object));
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        _dbContext.Dispose();
        _connection.Dispose();
        GC.SuppressFinalize(this);
    }
}
