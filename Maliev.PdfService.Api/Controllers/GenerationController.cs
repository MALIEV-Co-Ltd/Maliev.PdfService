using Maliev.PdfService.Api.Models.Requests;
using Maliev.PdfService.Api.Services;
using Maliev.PdfService.Data.Data;
using Maliev.PdfService.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;
using Maliev.MessagingContracts.Generated;
using MassTransit;

namespace Maliev.PdfService.Api.Controllers;

[ApiVersion("1.0")]
[Route("pdf/v{version:apiVersion}/generations")]
[ApiController]
public class GenerationController : ControllerBase
{
    private readonly IPdfGenerator _pdfGenerator;
    private readonly IUploadServiceClient _uploadService;
    private readonly PdfDbContext _dbContext;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<GenerationController> _logger;

    public GenerationController(
        IPdfGenerator pdfGenerator,
        IUploadServiceClient uploadService,
        PdfDbContext dbContext,
        IPublishEndpoint publishEndpoint,
        ILogger<GenerationController> logger)
    {
        _pdfGenerator = pdfGenerator;
        _uploadService = uploadService;
        _dbContext = dbContext;
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    [HttpPost("generate")]
    public async Task<IActionResult> Generate([FromBody] GeneratePdfRequest request)
    {
        GenerationRequest log = new()
        {
            ReferenceId = request.ReferenceId,
            DocumentType = request.DocumentType,
            Status = GenerationStatus.Processing,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.GenerationRequests.Add(log);
        await _dbContext.SaveChangesAsync();

        try
        {
            var pdfBytes = await _pdfGenerator.GeneratePdfAsync(request.DocumentType, request.Data);

            var fileName = $"{request.DocumentType}_{request.ReferenceId}_{Guid.NewGuid()}.pdf";
            var storagePath = $"pdfs/{request.DocumentType.ToString().ToLower()}/{request.ReferenceId}/{fileName}";

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
                CorrelationId: Guid.NewGuid(),
                CausationId: null,
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

            return Ok(new { requestId = log.Id, storageUrl = url });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate PDF for request {RequestId}", log.Id);

            log.Status = GenerationStatus.Failed;
            log.ErrorMessage = ex.Message;
            log.CompletedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();

            // Publish PdfGenerationFailedEvent
            await _publishEndpoint.Publish(new PdfGenerationFailedEvent(
                MessageId: Guid.NewGuid(),
                MessageName: "PdfGenerationFailedEvent",
                MessageType: MessageType.Event,
                MessageVersion: "1.0.0",
                PublishedBy: "PdfService",
                ConsumedBy: ["InvoiceService", "QuotationService", "ReceiptService"],
                CorrelationId: Guid.NewGuid(),
                CausationId: null,
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

            return StatusCode(500, new { error = "Failed to generate PDF", requestId = log.Id });
        }
    }

    [HttpPost("generate/async")]
    public async Task<IActionResult> GenerateAsync([FromBody] GeneratePdfRequest request)
    {
        var log = new GenerationRequest
        {
            ReferenceId = request.ReferenceId,
            DocumentType = request.DocumentType,
            Status = GenerationStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.GenerationRequests.Add(log);
        await _dbContext.SaveChangesAsync();

        // Publish PdfGenerationRequestedEvent
        await _publishEndpoint.Publish(new PdfGenerationRequestedEvent(
            MessageId: Guid.NewGuid(),
            MessageName: "PdfGenerationRequestedEvent",
            MessageType: MessageType.Event,
            MessageVersion: "1.0.0",
            PublishedBy: "PdfService",
            ConsumedBy: ["PdfService"],
            CorrelationId: Guid.NewGuid(),
            CausationId: null,
            OccurredAtUtc: DateTimeOffset.UtcNow,
            IsPublic: false,
            Payload: new PdfGenerationRequestedEventPayload(
                RequestId: log.Id.ToString(),
                ReferenceId: log.ReferenceId,
                DocumentType: log.DocumentType.ToString(),
                RequestedAt: DateTimeOffset.UtcNow
            )
        ));

        return Accepted(new { requestId = log.Id });
    }
}
