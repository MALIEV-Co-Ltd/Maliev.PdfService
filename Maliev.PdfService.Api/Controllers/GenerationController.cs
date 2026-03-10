using Asp.Versioning;
using Maliev.Aspire.ServiceDefaults.Authorization;
using Maliev.MessagingContracts;
using Maliev.MessagingContracts.Contracts.Pdf;
using Maliev.PdfService.Api.Authorization;
using Maliev.PdfService.Api.Models.Requests;
using Maliev.PdfService.Api.Services;
using Maliev.PdfService.Infrastructure.Data;
using Maliev.PdfService.Domain.Entities;
using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Maliev.PdfService.Api.Controllers;

/// <summary>
/// Controller for handling PDF generation requests.
/// </summary>
[ApiVersion("1.0")]
[Route("pdf/v{version:apiVersion}/generations")]
[ApiController]
[Authorize]
public class GenerationController : ControllerBase
{
    private readonly IPdfGenerator _pdfGenerator;
    private readonly IUploadServiceClient _uploadService;
    private readonly PdfDbContext _dbContext;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<GenerationController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="GenerationController"/> class.
    /// </summary>
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

    /// <summary>
    /// Generates a PDF document synchronously.
    /// </summary>
    /// <param name="request">The generation request.</param>
    /// <returns>The generated document URL.</returns>
    [HttpPost("generate")]
    [RequirePermission(PdfPermissions.GenerationCreate)]
    public async Task<IActionResult> Generate([FromBody] GeneratePdfRequest request)
    {
        GenerationRequest log = new()
        {
            ReferenceId = request.ReferenceId,
            TemplateCode = request.TemplateCode,
            DocumentType = request.DocumentType,
            Status = GenerationStatus.Processing,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.GenerationRequests.Add(log);
        await _dbContext.SaveChangesAsync();

        var correlationIdStr = HttpContext?.Items["CorrelationId"] as string ?? Guid.NewGuid().ToString();
        var correlationId = Guid.Parse(correlationIdStr);

        try
        {
            var pdfBytes = await _pdfGenerator.GeneratePdfAsync(request.DocumentType, request.Data, request.TemplateCode);

            var fileName = $"{request.DocumentType}_{request.ReferenceId}_{Guid.NewGuid()}.pdf";
            var storagePath = _pdfGenerator.GetStoragePath(request.DocumentType, request.ReferenceId, fileName);

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
                CorrelationId: correlationId,
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
                CorrelationId: correlationId,
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

    /// <summary>
    /// Enqueues a PDF document for asynchronous generation.
    /// </summary>
    /// <param name="request">The generation request.</param>
    /// <returns>The request ID.</returns>
    [HttpPost("generate/async")]
    [RequirePermission(PdfPermissions.GenerationCreate)]
    public async Task<IActionResult> GenerateAsync([FromBody] GeneratePdfRequest request)
    {
        var log = new GenerationRequest
        {
            ReferenceId = request.ReferenceId,
            TemplateCode = request.TemplateCode,
            DocumentType = request.DocumentType,
            Status = GenerationStatus.Pending,
            DataJson = request.Data.GetRawText(),
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.GenerationRequests.Add(log);
        await _dbContext.SaveChangesAsync();

        var correlationIdStr = HttpContext?.Items["CorrelationId"] as string ?? Guid.NewGuid().ToString();
        var correlationId = Guid.Parse(correlationIdStr);

        // Publish PdfGenerationRequestedEvent
        await _publishEndpoint.Publish(new PdfGenerationRequestedEvent(
            MessageId: Guid.NewGuid(),
            MessageName: "PdfGenerationRequestedEvent",
            MessageType: MessageType.Event,
            MessageVersion: "1.0.0",
            PublishedBy: "PdfService",
            ConsumedBy: ["PdfService"],
            CorrelationId: correlationId,
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
