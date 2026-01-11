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
            // TODO: In a real scenario, we might need to fetch the data from the source service
            // if it's not included in the 'Requested' event, or ensure it's persisted elsewhere.
            // For this implementation, we assume 'data' is either in the event or retrieved.
            // Since the event payload doesn't have 'Data', this is a gap.
            // For now, we'll mark it as failed with a relevant message if data is missing.

            throw new NotSupportedException("Asynchronous generation requires data persistence or inclusion in the event.");
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
