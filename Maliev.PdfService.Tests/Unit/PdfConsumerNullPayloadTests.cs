using Maliev.MessagingContracts.Contracts.Pdf;
using Maliev.MessagingContracts.Contracts.Receipts;
using Maliev.MessagingContracts.Contracts.Uploads;
using Maliev.PdfService.Api.Consumers;
using Maliev.PdfService.Api.Services;
using Maliev.PdfService.Domain.Entities;
using MassTransit;
using Microsoft.Extensions.Logging;
using Moq;

namespace Maliev.PdfService.Tests.Unit;

/// <summary>
/// Verifies malformed document boundary events are ignored before side effects.
/// </summary>
public sealed class PdfConsumerNullPayloadTests
{
    /// <summary>
    /// Ensures malformed PDF generation requests are ignored before side effects.
    /// </summary>
    [Fact]
    public async Task PdfGenerationRequestedConsumer_WithoutPayload_IsIgnored()
    {
        var pdfGenerator = new Mock<IPdfGenerator>();
        var uploadService = new Mock<IUploadServiceClient>();
        var publishEndpoint = new Mock<IPublishEndpoint>();
        var consumer = new PdfGenerationRequestedConsumer(
            pdfGenerator.Object,
            uploadService.Object,
            null!,
            publishEndpoint.Object,
            Mock.Of<ILogger<PdfGenerationRequestedConsumer>>());

        await consumer.Consume(CreateContext(new PdfGenerationRequestedEvent { Payload = null! }).Object);

        pdfGenerator.Verify(
            generator => generator.GeneratePdfAsync(
                It.IsAny<DocumentType>(),
                It.IsAny<object>(),
                It.IsAny<string?>()),
            Times.Never);
        publishEndpoint.Verify(
            endpoint => endpoint.Publish(It.IsAny<object>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    /// <summary>
    /// Ensures malformed receipt PDF requests are ignored before side effects.
    /// </summary>
    [Fact]
    public async Task ReceiptPdfRequestedConsumer_WithoutPayload_IsIgnored()
    {
        var pdfGenerator = new Mock<IPdfGenerator>();
        var uploadService = new Mock<IUploadServiceClient>();
        var publishEndpoint = new Mock<IPublishEndpoint>();
        var consumer = new ReceiptPdfRequestedConsumer(
            pdfGenerator.Object,
            uploadService.Object,
            null!,
            publishEndpoint.Object,
            Mock.Of<ILogger<ReceiptPdfRequestedConsumer>>());

        await consumer.Consume(CreateContext(new ReceiptPdfRequestedEvent { Payload = null! }).Object);

        pdfGenerator.Verify(
            generator => generator.GeneratePdfAsync(
                It.IsAny<DocumentType>(),
                It.IsAny<object>(),
                It.IsAny<string?>()),
            Times.Never);
        publishEndpoint.Verify(
            endpoint => endpoint.Publish(It.IsAny<object>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    /// <summary>
    /// Ensures malformed file deletion events are ignored before database access.
    /// </summary>
    [Fact]
    public async Task FileDeletedEventConsumer_WithoutPayload_IsIgnored()
    {
        var consumer = new FileDeletedEventConsumer(
            null!,
            Mock.Of<ILogger<FileDeletedEventConsumer>>());

        await consumer.Consume(CreateContext(new FileDeletedEvent { Payload = null! }).Object);
    }

    private static Mock<ConsumeContext<T>> CreateContext<T>(T message)
        where T : class
    {
        var context = new Mock<ConsumeContext<T>>();
        context.Setup(c => c.Message).Returns(message);
        context.Setup(c => c.CancellationToken).Returns(CancellationToken.None);
        return context;
    }
}
