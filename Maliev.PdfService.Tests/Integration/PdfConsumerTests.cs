using Moq;
using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Maliev.MessagingContracts.Generated;
using Maliev.PdfService.Api.Consumers;
using Maliev.PdfService.Api.Services;
using Maliev.PdfService.Data.Data;
using Maliev.PdfService.Data.Entities;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Maliev.PdfService.Tests.Fixtures;
using Microsoft.Extensions.Logging;

namespace Maliev.PdfService.Api.Tests.Integration;

public class PdfConsumerTests : IClassFixture<PdfServiceTestFactory>
{
    private readonly PdfServiceTestFactory _factory;

    public PdfConsumerTests(PdfServiceTestFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Consume_PdfGenerationRequestedEvent_ShouldGenerateAndUpload()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<PdfDbContext>();
        var pdfGenerator = scope.ServiceProvider.GetRequiredService<IPdfGenerator>();
        var uploadService = scope.ServiceProvider.GetRequiredService<IUploadServiceClient>();
        var publishEndpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<PdfGenerationRequestedConsumer>>();

        var requestId = Guid.NewGuid();
        var log = new GenerationRequest
        {
            Id = requestId,
            ReferenceId = "REF-001",
            TemplateCode = "INV-STD-01",
            DocumentType = DocumentType.Invoice,
            Status = GenerationStatus.Pending,
            DataJson = "{\"InvoiceNumber\":\"INV-001\"}",
            CreatedAt = DateTime.UtcNow
        };
        context.GenerationRequests.Add(log);
        await context.SaveChangesAsync();

        _factory.UploadServiceMock.Setup(s => s.UploadFileAsync(It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("http://storage.com/test.pdf");

        var consumer = new PdfGenerationRequestedConsumer(pdfGenerator, uploadService, context, publishEndpoint, logger);

        var evt = new PdfGenerationRequestedEvent(
            MessageId: Guid.NewGuid(),
            MessageName: "PdfGenerationRequestedEvent",
            MessageType: MessageType.Event,
            MessageVersion: "1.0",
            PublishedBy: "Test",
            ConsumedBy: new[] { "Pdf" },
            CorrelationId: Guid.NewGuid(),
            CausationId: null,
            OccurredAtUtc: DateTimeOffset.UtcNow,
            IsPublic: true,
            Payload: new PdfGenerationRequestedEventPayload(
                RequestId: requestId.ToString(),
                ReferenceId: "REF-001",
                DocumentType: "Invoice",
                RequestedAt: DateTimeOffset.UtcNow
            )
        );

        var mockContext = new Mock<ConsumeContext<PdfGenerationRequestedEvent>>();
        mockContext.Setup(m => m.Message).Returns(evt);

        // Act
        await consumer.Consume(mockContext.Object);

        // Assert
        var updatedLog = await context.GenerationRequests.FindAsync(requestId);
        Assert.Equal(GenerationStatus.Completed, updatedLog!.Status);
        Assert.NotNull(updatedLog.StorageUrl);
    }

    [Fact]
    public async Task Consume_FileDeletedEvent_ShouldClearStorageUrl()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<PdfDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<FileDeletedEventConsumer>>();

        var fileId = "file-123";
        var log = new GenerationRequest
        {
            Id = Guid.NewGuid(),
            ReferenceId = "REF-DEL",
            TemplateCode = "T",
            DocumentType = DocumentType.Invoice,
            Status = GenerationStatus.Completed,
            StorageUrl = $"http://storage.com/{fileId}.pdf",
            CreatedAt = DateTime.UtcNow
        };
        context.GenerationRequests.Add(log);
        await context.SaveChangesAsync();

        var consumer = new FileDeletedEventConsumer(context, logger);
        var evt = new FileDeletedEvent(
            MessageId: Guid.NewGuid(),
            MessageName: "FileDeletedEvent",
            MessageType: MessageType.Event,
            MessageVersion: "1.0",
            PublishedBy: "Upload",
            ConsumedBy: new[] { "Pdf" },
            CorrelationId: Guid.NewGuid(),
            CausationId: null,
            OccurredAtUtc: DateTimeOffset.UtcNow,
            IsPublic: true,
            Payload: new FileDeletedEventPayload(
                FileId: fileId,
                UploadId: "UP-123",
                ServiceId: "PDF",
                StoragePath: "path/to/file",
                DeletedAt: DateTimeOffset.UtcNow,
                DeletedBy: Guid.NewGuid().ToString(),
                Reason: "Test"
            )
        );

        var mockContext = new Mock<ConsumeContext<FileDeletedEvent>>();
        mockContext.Setup(m => m.Message).Returns(evt);

        // Act
        await consumer.Consume(mockContext.Object);

        // Assert
        var updatedLog = await context.GenerationRequests.FindAsync(log.Id);
        Assert.Null(updatedLog!.StorageUrl);
    }

    [Fact]
    public async Task Consume_ReceiptPdfRequestedEvent_ShouldGenerateAndUpload()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<PdfDbContext>();
        var pdfGenerator = scope.ServiceProvider.GetRequiredService<IPdfGenerator>();
        var uploadService = scope.ServiceProvider.GetRequiredService<IUploadServiceClient>();
        var publishEndpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<ReceiptPdfRequestedConsumer>>();

        var receiptId = Guid.NewGuid();
        _factory.UploadServiceMock.Setup(s => s.UploadFileAsync(It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("http://storage.com/receipt.pdf");

        var consumer = new ReceiptPdfRequestedConsumer(pdfGenerator, uploadService, context, publishEndpoint, logger);

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
                ReceiptNumber: "RCP-123",
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

        // Act
        await consumer.Consume(mockContext.Object);

        // Assert
        var log = await context.GenerationRequests.FirstOrDefaultAsync(r => r.ReferenceId == receiptId.ToString());
        Assert.NotNull(log);
        Assert.Equal(GenerationStatus.Completed, log.Status);
    }
}
