using Maliev.MessagingContracts.Generated;
using Maliev.PdfService.Api.Models.Data;
using Maliev.PdfService.Api.Services;
using Maliev.PdfService.Data.Data;
using Maliev.PdfService.Data.Entities;
using MassTransit;

namespace Maliev.PdfService.Api.Consumers;

/// <summary>
/// Consumes InvoiceCreatedEvent to automatically generate an invoice PDF.
/// </summary>
public class InvoiceFinalizedConsumer : IConsumer<InvoiceCreatedEvent>
{
    private readonly IPdfGenerator _pdfGenerator;
    private readonly IUploadServiceClient _uploadService;
    private readonly PdfDbContext _dbContext;
    private readonly ILogger<InvoiceFinalizedConsumer> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="InvoiceFinalizedConsumer"/> class.
    /// </summary>
    public InvoiceFinalizedConsumer(
        IPdfGenerator pdfGenerator,
        IUploadServiceClient uploadService,
        PdfDbContext dbContext,
        ILogger<InvoiceFinalizedConsumer> logger)
    {
        _pdfGenerator = pdfGenerator;
        _uploadService = uploadService;
        _dbContext = dbContext;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task Consume(ConsumeContext<InvoiceCreatedEvent> context)
    {
        var message = context.Message;
        var payload = message.Payload;
        _logger.LogInformation("Processing PDF for created invoice: {InvoiceNumber}", payload.InvoiceNumber);

        var log = new GenerationRequest
        {
            ReferenceId = payload.InvoiceId.ToString(),
            TemplateCode = "INV-AUTO-01",
            DocumentType = DocumentType.Invoice,
            Status = GenerationStatus.Processing,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.GenerationRequests.Add(log);
        await _dbContext.SaveChangesAsync();

        try
        {
            // TODO: Fetch full invoice details from InvoiceService if needed
            var invoiceData = new InvoiceData
            {
                InvoiceNumber = payload.InvoiceNumber,
                // ... map other fields from payload
            };

            var pdfBytes = await _pdfGenerator.GeneratePdfAsync(DocumentType.Invoice, invoiceData, log.TemplateCode);

            var fileName = $"Invoice_{payload.InvoiceNumber}_{Guid.NewGuid()}.pdf";
            var storagePath = _pdfGenerator.GetStoragePath(DocumentType.Invoice, payload.InvoiceId.ToString(), fileName);

            var url = await _uploadService.UploadFileAsync(fileName, pdfBytes, "application/pdf", storagePath);

            log.Status = GenerationStatus.Completed;
            log.StorageUrl = url;
            log.CompletedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Successfully generated and uploaded PDF for invoice: {InvoiceNumber}", payload.InvoiceNumber);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate or upload PDF for invoice: {InvoiceNumber}", payload.InvoiceNumber);

            log.Status = GenerationStatus.Failed;
            log.ErrorMessage = ex.Message;
            log.CompletedAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();

            throw;
        }
    }
}
