using Maliev.PdfService.Api.Services;
using Maliev.PdfService.Data.Entities;
using MassTransit;

namespace Maliev.PdfService.Api.Consumers;

// Placeholder for the actual contract from Maliev.MessagingContracts
public record InvoiceFinalizedEvent(Guid InvoiceId, string InvoiceNumber, object InvoiceData);

public class InvoiceFinalizedConsumer : IConsumer<InvoiceFinalizedEvent>
{
    private readonly IPdfGenerator _pdfGenerator;
    private readonly IUploadServiceClient _uploadService;
    private readonly ILogger<InvoiceFinalizedConsumer> _logger;

    public InvoiceFinalizedConsumer(IPdfGenerator pdfGenerator, IUploadServiceClient uploadService, ILogger<InvoiceFinalizedConsumer> logger)
    {
        _pdfGenerator = pdfGenerator;
        _uploadService = uploadService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<InvoiceFinalizedEvent> context)
    {
        var message = context.Message;
        _logger.LogInformation("Processing PDF for finalized invoice: {InvoiceNumber}", message.InvoiceNumber);

        try
        {
            var pdfBytes = await _pdfGenerator.GeneratePdfAsync(DocumentType.Invoice, message.InvoiceData);
            
            var fileName = $"Invoice_{message.InvoiceNumber}_{Guid.NewGuid()}.pdf";
            var storagePath = $"pdfs/invoice/{message.InvoiceId}/{fileName}";
            
            await _uploadService.UploadFileAsync(fileName, pdfBytes, "application/pdf", storagePath);

            _logger.LogInformation("Successfully generated and uploaded PDF for invoice: {InvoiceNumber}", message.InvoiceNumber);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate or upload PDF for invoice: {InvoiceNumber}", message.InvoiceNumber);
            throw;
        }
    }
}
