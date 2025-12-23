using Maliev.PdfService.Api.Consumers;
using Maliev.PdfService.Api.Services;
using Maliev.PdfService.Data.Entities;
using MassTransit;
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
        _consumer = new InvoiceFinalizedConsumer(_pdfGeneratorMock.Object, _uploadServiceMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task Consume_CallsGeneratorAndUpload()
    {
        // Arrange
        var contextMock = new Mock<ConsumeContext<InvoiceFinalizedEvent>>();
        var message = new InvoiceFinalizedEvent(Guid.NewGuid(), "INV-001", new { });
        contextMock.Setup(x => x.Message).Returns(message);

        _pdfGeneratorMock.Setup(x => x.GeneratePdfAsync(DocumentType.Invoice, It.IsAny<object>(), null))
            .ReturnsAsync(new byte[] { 1, 2, 3 });

        _uploadServiceMock.Setup(x => x.UploadFileAsync(It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("https://storage.com/inv.pdf");

        // Act
        await _consumer.Consume(contextMock.Object);

        // Assert
        _pdfGeneratorMock.Verify(x => x.GeneratePdfAsync(DocumentType.Invoice, message.InvoiceData, null), Times.Once);
        _uploadServiceMock.Verify(x => x.UploadFileAsync(It.IsAny<string>(), It.IsAny<byte[]>(), "application/pdf", It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Consume_Throws_WhenGeneratorFails()
    {
        // Arrange
        var contextMock = new Mock<ConsumeContext<InvoiceFinalizedEvent>>();
        var message = new InvoiceFinalizedEvent(Guid.NewGuid(), "INV-FAIL", new { });
        contextMock.Setup(x => x.Message).Returns(message);

        _pdfGeneratorMock.Setup(x => x.GeneratePdfAsync(It.IsAny<DocumentType>(), It.IsAny<object>(), null))
            .ThrowsAsync(new Exception("Fail"));

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => _consumer.Consume(contextMock.Object));
    }
}
