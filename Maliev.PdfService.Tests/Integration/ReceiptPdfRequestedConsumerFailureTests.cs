using Maliev.MessagingContracts;
using Maliev.MessagingContracts.Contracts.Receipts;
using Maliev.PdfService.Api.Consumers;
using Maliev.PdfService.Api.Services;
using Maliev.PdfService.Domain.Entities;
using Maliev.PdfService.Infrastructure.Data;
using Maliev.PdfService.Tests.Fixtures;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Maliev.PdfService.Tests.Integration;

public class ReceiptPdfRequestedConsumerFailureTests : IClassFixture<PdfServiceTestFactory>
{
    private readonly PdfServiceTestFactory _factory;

    public ReceiptPdfRequestedConsumerFailureTests(PdfServiceTestFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Consume_ReceiptPdfRequestedEvent_GeneratorFails_PublishesFailedEvent()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<PdfDbContext>();
        var pdfGeneratorMock = new Mock<IPdfGenerator>();
        var uploadService = scope.ServiceProvider.GetRequiredService<IUploadServiceClient>();
        var publishEndpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<ReceiptPdfRequestedConsumer>>();

        pdfGeneratorMock.Setup(g => g.GeneratePdfAsync(It.IsAny<DocumentType>(), It.IsAny<object>(), It.IsAny<string?>()))
            .ThrowsAsync(new InvalidOperationException("Receipt PDF generation failed"));

        var consumer = new ReceiptPdfRequestedConsumer(pdfGeneratorMock.Object, uploadService, context, publishEndpoint, logger);

        var receiptId = Guid.NewGuid();
        var evt = new ReceiptPdfRequestedEvent(
            MessageId: Guid.NewGuid(),
            MessageName: "ReceiptPdfRequestedEvent",
            MessageType: MessageType.Event,
            MessageVersion: "1.0",
            PublishedBy: "Receipt",
            ConsumedBy: new[] { "Pdf" },
            CorrelationId: Guid.NewGuid(),
            CausationId: null,
            OccurredAtUtc: DateTimeOffset.UtcNow,
            IsPublic: true,
            Payload: new ReceiptPdfRequestedEventPayload(
                ReceiptId: receiptId,
                ReceiptNumber: "RCP-FAIL-123",
                CustomerDetails: new ReceiptPdfRequestedEventPayloadCustomerDetails("Cust", "T-123", "Addr"),
                FinancialDetails: new ReceiptPdfRequestedEventPayloadFinancialDetails(DateTimeOffset.UtcNow, 100, 0, 0, 100, "USD", "Cash"),
                LineItems: new List<ReceiptPdfRequestedEventPayloadLineItemsItem> { new ReceiptPdfRequestedEventPayloadLineItemsItem(1, "Item", 1, 100, 0, 100) },
                TaxFields: new ReceiptPdfRequestedEventPayloadTaxFields("T-123", 7, "None"),
                TemplateId: "RCP-STD-01",
                RequestedAt: DateTimeOffset.UtcNow
            )
        );

        var mockContext = new Mock<ConsumeContext<ReceiptPdfRequestedEvent>>();
        mockContext.Setup(m => m.Message).Returns(evt);

        await consumer.Consume(mockContext.Object);

        var log = await context.GenerationRequests.FirstOrDefaultAsync(r => r.ReferenceId == receiptId.ToString());
        Assert.NotNull(log);
        Assert.Equal(GenerationStatus.Failed, log.Status);
        Assert.Equal("Receipt PDF generation failed", log.ErrorMessage);
    }

    [Fact]
    public async Task Consume_ReceiptPdfRequestedEvent_UploadFails_PublishesFailedEvent()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<PdfDbContext>();
        var pdfGenerator = scope.ServiceProvider.GetRequiredService<IPdfGenerator>();
        var uploadServiceMock = new Mock<IUploadServiceClient>();
        var publishEndpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<ReceiptPdfRequestedConsumer>>();

        uploadServiceMock.Setup(s => s.UploadFileAsync(It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Upload failed"));

        var consumer = new ReceiptPdfRequestedConsumer(pdfGenerator, uploadServiceMock.Object, context, publishEndpoint, logger);

        var receiptId = Guid.NewGuid();
        var evt = new ReceiptPdfRequestedEvent(
            MessageId: Guid.NewGuid(),
            MessageName: "ReceiptPdfRequestedEvent",
            MessageType: MessageType.Event,
            MessageVersion: "1.0",
            PublishedBy: "Receipt",
            ConsumedBy: new[] { "Pdf" },
            CorrelationId: Guid.NewGuid(),
            CausationId: null,
            OccurredAtUtc: DateTimeOffset.UtcNow,
            IsPublic: true,
            Payload: new ReceiptPdfRequestedEventPayload(
                ReceiptId: receiptId,
                ReceiptNumber: "RCP-UPLOAD-FAIL",
                CustomerDetails: new ReceiptPdfRequestedEventPayloadCustomerDetails("Cust", "T-123", "Addr"),
                FinancialDetails: new ReceiptPdfRequestedEventPayloadFinancialDetails(DateTimeOffset.UtcNow, 100, 0, 0, 100, "USD", "Cash"),
                LineItems: new List<ReceiptPdfRequestedEventPayloadLineItemsItem> { new ReceiptPdfRequestedEventPayloadLineItemsItem(1, "Item", 1, 100, 0, 100) },
                TaxFields: new ReceiptPdfRequestedEventPayloadTaxFields("T-123", 7, "None"),
                TemplateId: "RCP-STD-01",
                RequestedAt: DateTimeOffset.UtcNow
            )
        );

        var mockContext = new Mock<ConsumeContext<ReceiptPdfRequestedEvent>>();
        mockContext.Setup(m => m.Message).Returns(evt);

        await consumer.Consume(mockContext.Object);

        var log = await context.GenerationRequests.FirstOrDefaultAsync(r => r.ReferenceId == receiptId.ToString());
        Assert.NotNull(log);
        Assert.Equal(GenerationStatus.Failed, log.Status);
        Assert.Equal("Upload failed", log.ErrorMessage);
    }
}
