using Maliev.MessagingContracts;
using Maliev.MessagingContracts.Contracts.Invoices;
using Maliev.PdfService.Api.Consumers;
using Maliev.PdfService.Api.Services;
using Maliev.PdfService.Infrastructure.Data;
using Maliev.PdfService.Domain.Entities;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Testcontainers.PostgreSql;
using Xunit;

namespace Maliev.PdfService.Tests.Unit;

/// <summary>
/// Unit tests for invoice finalized consumer PDF generation behavior.
/// </summary>
public class InvoiceFinalizedConsumerTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _dbContainer =
#pragma warning disable CS0618
        new PostgreSqlBuilder().WithImage("postgres:18-alpine")
        .Build();
#pragma warning restore CS0618

    private readonly Mock<IPdfGenerator> _pdfGeneratorMock = new();
    private readonly Mock<IUploadServiceClient> _uploadServiceMock = new();
    private readonly Mock<IInvoiceServiceClient> _invoiceServiceClientMock = new();
    private readonly Mock<ILogger<InvoiceFinalizedConsumer>> _loggerMock = new();
    private InvoiceFinalizedConsumer _consumer = null!;
    private PdfDbContext _dbContext = null!;

    /// <summary>
    /// Starts the PostgreSQL test container and creates a consumer under test.
    /// </summary>
    /// <returns>A task that represents the asynchronous initialization operation.</returns>
    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();

        var options = new DbContextOptionsBuilder<PdfDbContext>()
            .UseNpgsql(_dbContainer.GetConnectionString())
            .Options;
        _dbContext = new PdfDbContext(options);
        await _dbContext.Database.EnsureCreatedAsync();
        var dbContext = _dbContext;

        _invoiceServiceClientMock
            .Setup(x => x.GetInvoiceByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid invoiceId, CancellationToken _) => new InvoiceDto
            {
                InvoiceId = invoiceId,
                InvoiceNumber = "INV-001",
                CustomerName = "Test Customer",
                IssueDate = DateTime.UtcNow,
                DueDate = DateTime.UtcNow.AddDays(30),
                SubTotalAmount = 1000m,
                TaxAmount = 70m,
                TotalAmount = 1070m,
                Items =
                [
                    new InvoiceItemDto
                    {
                        Description = "Test Item",
                        Quantity = 1m,
                        UnitPrice = 1000m,
                        TotalAmount = 1070m
                    }
                ]
            });

        _consumer = new InvoiceFinalizedConsumer(
            _pdfGeneratorMock.Object,
            _uploadServiceMock.Object,
            dbContext,
            _loggerMock.Object,
            _invoiceServiceClientMock.Object);
    }

    /// <summary>
    /// Disposes the PostgreSQL test container after the test run.
    /// </summary>
    /// <returns>A task that represents the asynchronous disposal operation.</returns>
    public async Task DisposeAsync()
    {
        await _dbContext.DisposeAsync();
        await _dbContainer.DisposeAsync();
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

}
