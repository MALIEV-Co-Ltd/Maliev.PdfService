using Maliev.MessagingContracts.Contracts.Invoices;
using Maliev.MessagingContracts;
using Maliev.PdfService.Api.Consumers;
using Maliev.PdfService.Api.Services;
using Maliev.PdfService.Data.Data;
using Maliev.PdfService.Data.Entities;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Maliev.PdfService.Tests.Unit;

public class InvoiceFinalizedConsumerTests
{
    private readonly Mock<IPdfGenerator> _pdfGeneratorMock = new();
    private readonly Mock<IUploadServiceClient> _uploadServiceMock = new();
    private readonly Mock<ILogger<InvoiceFinalizedConsumer>> _loggerMock = new();
    private readonly InvoiceFinalizedConsumer _consumer;

    public InvoiceFinalizedConsumerTests()
    {
        var options = new DbContextOptionsBuilder<PdfDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        var dbContext = new PdfDbContext(options);

        var invoiceClientMock = new Moq.Mock<Maliev.PdfService.Api.Services.IInvoiceServiceClient>();
        _consumer = new InvoiceFinalizedConsumer(_pdfGeneratorMock.Object, _uploadServiceMock.Object, dbContext, _loggerMock.Object, invoiceClientMock.Object);
    }

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
