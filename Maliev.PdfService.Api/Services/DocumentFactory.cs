using Maliev.PdfService.Api.Models.Data;
using Maliev.PdfService.Api.Services.Layouts;
using Maliev.PdfService.Data.Entities;
using QuestPDF.Infrastructure;

namespace Maliev.PdfService.Api.Services;

public interface IDocumentFactory
{
    IDocument CreateDocument(DocumentType type, object data);
}

public class DocumentFactory : IDocumentFactory
{
    public IDocument CreateDocument(DocumentType type, object data)
    {
        return type switch
        {
            DocumentType.Invoice => CreateInvoiceDocument(data),
            DocumentType.Quotation => new QuotationDocument(data),
            DocumentType.Receipt => new ReceiptDocument(data),
            DocumentType.Report => new FinancialReportDocument(data),
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }

    private IDocument CreateInvoiceDocument(object data)
    {
        if (data is InvoiceData invoiceData) return new InvoiceDocument(invoiceData);
        
        // Handle JsonElement or other types by serializing/deserializing to concrete DTO
        var json = System.Text.Json.JsonSerializer.Serialize(data);
        var mapped = System.Text.Json.JsonSerializer.Deserialize<InvoiceData>(json, new System.Text.Json.JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        }) ?? throw new InvalidOperationException("Failed to map invoice data");
        
        return new InvoiceDocument(mapped);
    }
}
