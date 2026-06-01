using Maliev.MessagingContracts.Contracts.Invoices;
using Maliev.PdfService.Api.Models.Data;
using Maliev.PdfService.Api.Services;
using Maliev.PdfService.Infrastructure.Data;
using Maliev.PdfService.Domain.Entities;
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
    private readonly IInvoiceServiceClient _invoiceServiceClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="InvoiceFinalizedConsumer"/> class.
    /// </summary>
    public InvoiceFinalizedConsumer(
        IPdfGenerator pdfGenerator,
        IUploadServiceClient uploadService,
        PdfDbContext dbContext,
        ILogger<InvoiceFinalizedConsumer> logger,
        IInvoiceServiceClient invoiceServiceClient)
    {
        _pdfGenerator = pdfGenerator;
        _uploadService = uploadService;
        _dbContext = dbContext;
        _logger = logger;
        _invoiceServiceClient = invoiceServiceClient;
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
            var invoiceDto = await _invoiceServiceClient.GetInvoiceByIdAsync(payload.InvoiceId, context.CancellationToken)
                ?? throw new InvalidOperationException($"Invoice {payload.InvoiceId} could not be loaded from InvoiceService.");
            var invoiceNumber = string.IsNullOrWhiteSpace(invoiceDto.InvoiceNumber)
                ? payload.InvoiceNumber
                : invoiceDto.InvoiceNumber;

            var invoiceData = new InvoiceData
            {
                InvoiceNumber = invoiceNumber,
                DocumentType = "TaxInvoice",
                IssueDate = invoiceDto.IssueDate,
                DueDate = invoiceDto.DueDate,
                CustomerName = invoiceDto.CustomerName,
                Currency = payload.Currency ?? "THB",
                Subtotal = invoiceDto.SubTotalAmount,
                TaxAmount = invoiceDto.TaxAmount,
                GrandTotal = invoiceDto.TotalAmount,
                Items = invoiceDto.Items.Select((item, idx) => new InvoiceItemData
                {
                    Index = idx + 1,
                    Description = item.Description,
                    Quantity = item.Quantity,
                    Unit = "pcs",
                    UnitPrice = item.UnitPrice,
                    LineSubtotal = item.TotalAmount,
                    LineTaxAmount = 0,
                    LineTotal = item.TotalAmount
                }).ToList()
            };

            var pdfBytes = await _pdfGenerator.GeneratePdfAsync(DocumentType.Invoice, invoiceData, log.TemplateCode);

            var fileName = $"Invoice_{payload.InvoiceNumber}_{Guid.NewGuid()}.pdf";
            var storagePath = _pdfGenerator.GetStoragePath(DocumentType.Invoice, payload.InvoiceId.ToString(), fileName);

            var url = await _uploadService.UploadFileAsync(fileName, pdfBytes, "application/pdf", storagePath);

            log.Status = GenerationStatus.Completed;
            log.StorageUrl = url;
            log.StoragePath = storagePath;
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
