using Maliev.MessagingContracts.Generated;
using Maliev.PdfService.Api.Services;
using Maliev.PdfService.Data.Data;
using Maliev.PdfService.Data.Entities;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace Maliev.PdfService.Api.Consumers;

/// <summary>
/// Consumes PdfGenerationRequestedEvent to process PDF generation asynchronously.
/// </summary>
public class PdfGenerationRequestedConsumer : IConsumer<PdfGenerationRequestedEvent>
{
    private readonly IPdfGenerator _pdfGenerator;
    private readonly IUploadServiceClient _uploadService;
    private readonly PdfDbContext _dbContext;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<PdfGenerationRequestedConsumer> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="PdfGenerationRequestedConsumer"/> class.
    /// </summary>
    public PdfGenerationRequestedConsumer(
        IPdfGenerator pdfGenerator,
        IUploadServiceClient uploadService,
        PdfDbContext dbContext,
        IPublishEndpoint publishEndpoint,
        ILogger<PdfGenerationRequestedConsumer> logger)
    {
        _pdfGenerator = pdfGenerator;
        _uploadService = uploadService;
        _dbContext = dbContext;
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task Consume(ConsumeContext<PdfGenerationRequestedEvent> context)
    {
        var payload = context.Message.Payload;
        _logger.LogInformation("Asynchronously processing PDF generation for request: {RequestId}", payload.RequestId);

        var requestId = Guid.Parse(payload.RequestId);
        var log = await _dbContext.GenerationRequests.FindAsync(requestId);

        if (log == null)
        {
            _logger.LogWarning("Generation request {RequestId} not found in database.", requestId);
            return;
        }

        if (log.Status != GenerationStatus.Pending)
        {
            _logger.LogInformation("Generation request {RequestId} is already in {Status} status. Skipping.", requestId, log.Status);
            return;
        }

        log.Status = GenerationStatus.Processing;
        await _dbContext.SaveChangesAsync();

        try
        {
            if (string.IsNullOrEmpty(log.DataJson))
            {
                throw new InvalidOperationException("Generation data is missing for asynchronous request.");
            }

            var data = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>(log.DataJson);
            var pdfBytes = await _pdfGenerator.GeneratePdfAsync(log.DocumentType, data, log.TemplateCode);

            var fileName = $"{log.DocumentType}_{log.ReferenceId}_{Guid.NewGuid()}.pdf";
            var storagePath = _pdfGenerator.GetStoragePath(log.DocumentType, log.ReferenceId, fileName);

            var url = await _uploadService.UploadFileAsync(fileName, pdfBytes, "application/pdf", storagePath);

            log.Status = GenerationStatus.Completed;
            log.StorageUrl = url;
            log.CompletedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();

            // Publish PdfGenerationCompletedEvent
            await _publishEndpoint.Publish(new PdfGenerationCompletedEvent(
                MessageId: Guid.NewGuid(),
                MessageName: "PdfGenerationCompletedEvent",
                MessageType: MessageType.Event,
                MessageVersion: "1.0.0",
                PublishedBy: "PdfService",
                ConsumedBy: ["InvoiceService", "QuotationService", "ReceiptService"],
                CorrelationId: context.CorrelationId ?? Guid.NewGuid(),
                CausationId: context.MessageId,
                OccurredAtUtc: DateTimeOffset.UtcNow,
                IsPublic: false,
                Payload: new PdfGenerationCompletedEventPayload(
                    RequestId: log.Id.ToString(),
                    ReferenceId: log.ReferenceId,
                    DocumentType: log.DocumentType.ToString(),
                    StorageUrl: url,
                    CompletedAt: DateTimeOffset.UtcNow
                )
            ));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate PDF asynchronously for request {RequestId}", requestId);

            log.Status = GenerationStatus.Failed;
            log.ErrorMessage = ex.Message;
            log.CompletedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();

            await _publishEndpoint.Publish(new PdfGenerationFailedEvent(
                MessageId: Guid.NewGuid(),
                MessageName: "PdfGenerationFailedEvent",
                MessageType: MessageType.Event,
                MessageVersion: "1.0.0",
                PublishedBy: "PdfService",
                ConsumedBy: ["InvoiceService", "QuotationService", "ReceiptService"],
                CorrelationId: context.CorrelationId ?? Guid.NewGuid(),
                CausationId: context.MessageId,
                OccurredAtUtc: DateTimeOffset.UtcNow,
                IsPublic: false,
                Payload: new PdfGenerationFailedEventPayload(
                    RequestId: log.Id.ToString(),
                    ReferenceId: log.ReferenceId,
                    DocumentType: log.DocumentType.ToString(),
                    ErrorMessage: ex.Message,
                    FailedAt: DateTimeOffset.UtcNow
                )
            ));
        }
    }
}
