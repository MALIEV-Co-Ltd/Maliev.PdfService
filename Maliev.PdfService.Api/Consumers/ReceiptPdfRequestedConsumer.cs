using Maliev.MessagingContracts;
using Maliev.MessagingContracts.Contracts.Pdf;
using Maliev.MessagingContracts.Contracts.Receipts;
using Maliev.PdfService.Api.Services;
using Maliev.PdfService.Infrastructure.Data;
using Maliev.PdfService.Domain.Entities;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace Maliev.PdfService.Api.Consumers;

/// <summary>
/// Consumes ReceiptPdfRequestedEvent to process rich PDF generation for receipts.
/// </summary>
public class ReceiptPdfRequestedConsumer : IConsumer<ReceiptPdfRequestedEvent>
{
    private readonly IPdfGenerator _pdfGenerator;
    private readonly IUploadServiceClient _uploadService;
    private readonly PdfDbContext _dbContext;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<ReceiptPdfRequestedConsumer> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ReceiptPdfRequestedConsumer"/> class.
    /// </summary>
    public ReceiptPdfRequestedConsumer(
        IPdfGenerator pdfGenerator,
        IUploadServiceClient uploadService,
        PdfDbContext dbContext,
        IPublishEndpoint publishEndpoint,
        ILogger<ReceiptPdfRequestedConsumer> logger)
    {
        _pdfGenerator = pdfGenerator;
        _uploadService = uploadService;
        _dbContext = dbContext;
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task Consume(ConsumeContext<ReceiptPdfRequestedEvent> context)
    {
        var payload = context.Message.Payload;
        _logger.LogInformation("Processing rich PDF generation for receipt: {ReceiptNumber}", payload.ReceiptNumber);

        var log = new GenerationRequest
        {
            ReferenceId = payload.ReceiptId.ToString(),
            TemplateCode = payload.TemplateId,
            DocumentType = DocumentType.Receipt,
            Status = GenerationStatus.Processing,
            DataJson = System.Text.Json.JsonSerializer.Serialize(payload),
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.GenerationRequests.Add(log);
        await _dbContext.SaveChangesAsync();

        try
        {
            var pdfBytes = await _pdfGenerator.GeneratePdfAsync(DocumentType.Receipt, payload, log.TemplateCode);

            var fileName = $"Receipt_{payload.ReceiptNumber}_{Guid.NewGuid()}.pdf";
            var storagePath = _pdfGenerator.GetStoragePath(DocumentType.Receipt, payload.ReceiptId.ToString(), fileName);

            var url = await _uploadService.UploadFileAsync(fileName, pdfBytes, "application/pdf", storagePath);

            log.Status = GenerationStatus.Completed;
            log.StorageUrl = url;
            log.CompletedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();

            // Publish PdfGenerationCompletedEvent
            await _publishEndpoint.Publish(new PdfGenerationCompletedEvent(
                MessageId: Guid.NewGuid(),
                MessageName: nameof(PdfGenerationCompletedEvent),
                MessageType: MessageType.Event,
                MessageVersion: "1.0.0",
                PublishedBy: "PdfService",
                ConsumedBy: ["ReceiptService"],
                CorrelationId: context.CorrelationId ?? Guid.NewGuid(),
                CausationId: context.MessageId,
                OccurredAtUtc: DateTimeOffset.UtcNow,
                IsPublic: false,
                Payload: new PdfGenerationCompletedEventPayload(
                    RequestId: log.Id.ToString(),
                    ReferenceId: payload.ReceiptId.ToString(),
                    DocumentType: DocumentType.Receipt.ToString(),
                    StorageUrl: url,
                    CompletedAt: DateTimeOffset.UtcNow
                )
            ));

            _logger.LogInformation("Successfully generated and uploaded PDF for receipt: {ReceiptNumber}", payload.ReceiptNumber);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate PDF for receipt: {ReceiptNumber}", payload.ReceiptNumber);

            log.Status = GenerationStatus.Failed;
            log.ErrorMessage = ex.Message;
            log.CompletedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();

            await _publishEndpoint.Publish(new PdfGenerationFailedEvent(
                MessageId: Guid.NewGuid(),
                MessageName: nameof(PdfGenerationFailedEvent),
                MessageType: MessageType.Event,
                MessageVersion: "1.0.0",
                PublishedBy: "PdfService",
                ConsumedBy: ["ReceiptService"],
                CorrelationId: context.CorrelationId ?? Guid.NewGuid(),
                CausationId: context.MessageId,
                OccurredAtUtc: DateTimeOffset.UtcNow,
                IsPublic: false,
                Payload: new PdfGenerationFailedEventPayload(
                    RequestId: log.Id.ToString(),
                    ReferenceId: payload.ReceiptId.ToString(),
                    DocumentType: DocumentType.Receipt.ToString(),
                    ErrorMessage: ex.Message,
                    FailedAt: DateTimeOffset.UtcNow
                )
            ));
        }
    }
}
