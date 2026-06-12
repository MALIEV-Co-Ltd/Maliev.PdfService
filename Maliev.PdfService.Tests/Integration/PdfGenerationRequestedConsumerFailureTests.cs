using Maliev.MessagingContracts;
using Maliev.MessagingContracts.Contracts.Pdf;
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

/// <summary>
/// Integration tests for failure handling in the PDF generation request consumer.
/// </summary>
public class PdfGenerationRequestedConsumerFailureTests : IClassFixture<PdfServiceTestFactory>
{
    private readonly PdfServiceTestFactory _factory;

    /// <summary>
    /// Initializes a new instance of the <see cref="PdfGenerationRequestedConsumerFailureTests"/> class.
    /// </summary>
    /// <param name="factory">The PDF service test factory.</param>
    public PdfGenerationRequestedConsumerFailureTests(PdfServiceTestFactory factory)
    {
        _factory = factory;
    }

    /// <summary>
    /// Verifies that a missing generation request is ignored without throwing.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Fact]
    public async Task Consume_PdfGenerationRequestedEvent_RequestNotFound_DoesNotThrow()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<PdfDbContext>();
        var pdfGenerator = scope.ServiceProvider.GetRequiredService<IPdfGenerator>();
        var uploadService = scope.ServiceProvider.GetRequiredService<IUploadServiceClient>();
        var publishEndpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<PdfGenerationRequestedConsumer>>();

        var consumer = new PdfGenerationRequestedConsumer(pdfGenerator, uploadService, context, publishEndpoint, logger);

        var nonExistentRequestId = Guid.NewGuid();
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
                RequestId: nonExistentRequestId.ToString(),
                ReferenceId: "REF-001",
                DocumentType: "Invoice",
                RequestedAt: DateTimeOffset.UtcNow
            )
        );

        var mockContext = new Mock<ConsumeContext<PdfGenerationRequestedEvent>>();
        mockContext.Setup(m => m.Message).Returns(evt);

        await consumer.Consume(mockContext.Object);
    }

    /// <summary>
    /// Verifies that an already completed generation request is skipped.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Fact]
    public async Task Consume_PdfGenerationRequestedEvent_AlreadyProcessed_Skips()
    {
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
            Status = GenerationStatus.Completed,
            DataJson = "{\"InvoiceNumber\":\"INV-001\"}",
            CreatedAt = DateTime.UtcNow,
            CompletedAt = DateTime.UtcNow
        };
        context.GenerationRequests.Add(log);
        await context.SaveChangesAsync();

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

        await consumer.Consume(mockContext.Object);

        var updatedLog = await context.GenerationRequests.FindAsync(requestId);
        Assert.Equal(GenerationStatus.Completed, updatedLog!.Status);
    }

    /// <summary>
    /// Verifies that generator failures mark the request as failed.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Fact]
    public async Task Consume_PdfGenerationRequestedEvent_GeneratorFails_PublishesFailedEvent()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<PdfDbContext>();
        var pdfGeneratorMock = new Mock<IPdfGenerator>();
        var uploadService = scope.ServiceProvider.GetRequiredService<IUploadServiceClient>();
        var publishEndpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<PdfGenerationRequestedConsumer>>();

        var requestId = Guid.NewGuid();
        var log = new GenerationRequest
        {
            Id = requestId,
            ReferenceId = "REF-FAIL",
            TemplateCode = "INV-STD-01",
            DocumentType = DocumentType.Invoice,
            Status = GenerationStatus.Pending,
            DataJson = "{\"InvoiceNumber\":\"INV-001\"}",
            CreatedAt = DateTime.UtcNow
        };
        context.GenerationRequests.Add(log);
        await context.SaveChangesAsync();

        pdfGeneratorMock.Setup(g => g.GeneratePdfAsync(It.IsAny<DocumentType>(), It.IsAny<object>(), It.IsAny<string?>()))
            .ThrowsAsync(new InvalidOperationException("PDF generation failed"));

        var consumer = new PdfGenerationRequestedConsumer(pdfGeneratorMock.Object, uploadService, context, publishEndpoint, logger);

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
                ReferenceId: "REF-FAIL",
                DocumentType: "Invoice",
                RequestedAt: DateTimeOffset.UtcNow
            )
        );

        var mockContext = new Mock<ConsumeContext<PdfGenerationRequestedEvent>>();
        mockContext.Setup(m => m.Message).Returns(evt);

        await consumer.Consume(mockContext.Object);

        var updatedLog = await context.GenerationRequests.FindAsync(requestId);
        Assert.Equal(GenerationStatus.Failed, updatedLog!.Status);
        Assert.Equal("PDF generation failed", updatedLog.ErrorMessage);
    }

    /// <summary>
    /// Verifies that missing generation data records a failed request.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Fact]
    public async Task Consume_PdfGenerationRequestedEvent_MissingDataJson_Throws()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<PdfDbContext>();
        var pdfGeneratorMock = new Mock<IPdfGenerator>();
        var uploadService = scope.ServiceProvider.GetRequiredService<IUploadServiceClient>();
        var publishEndpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<PdfGenerationRequestedConsumer>>();

        var requestId = Guid.NewGuid();
        var log = new GenerationRequest
        {
            Id = requestId,
            ReferenceId = "REF-NODATA",
            TemplateCode = "INV-STD-01",
            DocumentType = DocumentType.Invoice,
            Status = GenerationStatus.Pending,
            DataJson = null,
            CreatedAt = DateTime.UtcNow
        };
        context.GenerationRequests.Add(log);
        await context.SaveChangesAsync();

        var consumer = new PdfGenerationRequestedConsumer(pdfGeneratorMock.Object, uploadService, context, publishEndpoint, logger);

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
                ReferenceId: "REF-NODATA",
                DocumentType: "Invoice",
                RequestedAt: DateTimeOffset.UtcNow
            )
        );

        var mockContext = new Mock<ConsumeContext<PdfGenerationRequestedEvent>>();
        mockContext.Setup(m => m.Message).Returns(evt);

        await consumer.Consume(mockContext.Object);

        var updatedLog = await context.GenerationRequests.FindAsync(requestId);
        Assert.Equal(GenerationStatus.Failed, updatedLog!.Status);
        Assert.Contains("Generation data is missing", updatedLog.ErrorMessage);
    }

    /// <summary>
    /// Verifies completion publish failures remain retryable and do not persist a completed terminal state.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Fact]
    public async Task Consume_PdfGenerationRequestedEvent_CompletedEventPublishFails_KeepsRequestProcessing()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<PdfDbContext>();
        var pdfGeneratorMock = new Mock<IPdfGenerator>();
        var uploadServiceMock = new Mock<IUploadServiceClient>();
        var publishEndpointMock = new Mock<IPublishEndpoint>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<PdfGenerationRequestedConsumer>>();

        var requestId = Guid.NewGuid();
        var log = new GenerationRequest
        {
            Id = requestId,
            ReferenceId = "REF-PUBLISH-FAIL",
            TemplateCode = "INV-STD-01",
            DocumentType = DocumentType.Invoice,
            Status = GenerationStatus.Pending,
            DataJson = "{\"invoiceNumber\":\"INV-001\"}",
            CreatedAt = DateTime.UtcNow
        };
        context.GenerationRequests.Add(log);
        await context.SaveChangesAsync();

        pdfGeneratorMock
            .Setup(g => g.GeneratePdfAsync(It.IsAny<DocumentType>(), It.IsAny<object>(), It.IsAny<string?>()))
            .ReturnsAsync([1, 2, 3]);
        pdfGeneratorMock
            .Setup(g => g.GetStoragePath(It.IsAny<DocumentType>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns("pdfs/invoice/REF-PUBLISH-FAIL/invoice.pdf");
        uploadServiceMock
            .Setup(s => s.UploadFileAsync(It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("https://storage.example/invoice.pdf");
        publishEndpointMock
            .Setup(p => p.Publish(It.IsAny<PdfGenerationCompletedEvent>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("completion publish failed"));

        var consumer = new PdfGenerationRequestedConsumer(pdfGeneratorMock.Object, uploadServiceMock.Object, context, publishEndpointMock.Object, logger);
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
                ReferenceId: "REF-PUBLISH-FAIL",
                DocumentType: "Invoice",
                RequestedAt: DateTimeOffset.UtcNow
            )
        );

        var mockContext = new Mock<ConsumeContext<PdfGenerationRequestedEvent>>();
        mockContext.Setup(m => m.Message).Returns(evt);

        await Assert.ThrowsAsync<InvalidOperationException>(() => consumer.Consume(mockContext.Object));

        var updatedLog = await context.GenerationRequests
            .AsNoTracking()
            .SingleAsync(r => r.Id == requestId);

        Assert.Equal(GenerationStatus.Processing, updatedLog.Status);
        Assert.Null(updatedLog.ErrorMessage);
        publishEndpointMock.Verify(p => p.Publish(It.IsAny<PdfGenerationFailedEvent>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
