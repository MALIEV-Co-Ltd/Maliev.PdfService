using Maliev.PdfService.Api.Models.Data;
using Maliev.PdfService.Api.Services.Layouts;
using Maliev.PdfService.Domain.Entities;
using QuestPDF.Infrastructure;

namespace Maliev.PdfService.Api.Services;

/// <summary>
/// Default implementation of the document factory.
/// </summary>
public class DocumentFactory : IDocumentFactory
{
    /// <inheritdoc/>
    public IDocument CreateDocument(DocumentType type, object data)
    {
        return type switch
        {
            DocumentType.Invoice => new InvoiceDocument(MapToInvoiceData(data)),
            DocumentType.Quotation => new QuotationDocument(MapToQuotationData(data)),
            DocumentType.Receipt => new ReceiptDocument(data),
            DocumentType.Report => new FinancialReportDocument(data),
            DocumentType.DeliveryNote => new DeliveryNoteDocument(MapToDeliveryNoteData(data)),
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }

    private static InvoiceData MapToInvoiceData(object data)
    {
        if (data is InvoiceData invoiceData) return invoiceData;

        if (data is System.Text.Json.JsonElement jsonElement)
        {
            return System.Text.Json.JsonSerializer.Deserialize<InvoiceData>(jsonElement.GetRawText(), new System.Text.Json.JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? throw new InvalidOperationException($"Failed to deserialize {nameof(InvoiceData)} from JsonElement");
        }

        throw new InvalidOperationException($"Data must be of type {nameof(InvoiceData)} or JsonElement representing it. Actual type: {data?.GetType().Name ?? "null"}");
    }

    private static QuotationData MapToQuotationData(object data)
    {
        if (data is QuotationData quotationData) return quotationData;

        if (data is System.Text.Json.JsonElement jsonElement)
        {
            return System.Text.Json.JsonSerializer.Deserialize<QuotationData>(jsonElement.GetRawText(), new System.Text.Json.JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? throw new InvalidOperationException($"Failed to deserialize {nameof(QuotationData)} from JsonElement");
        }

        throw new InvalidOperationException($"Data must be of type {nameof(QuotationData)} or JsonElement representing it. Actual type: {data?.GetType().Name ?? "null"}");
    }

    private static DeliveryNoteData MapToDeliveryNoteData(object data)
    {
        if (data is DeliveryNoteData deliveryNoteData) return deliveryNoteData;

        if (data is System.Text.Json.JsonElement jsonElement)
        {
            return System.Text.Json.JsonSerializer.Deserialize<DeliveryNoteData>(jsonElement.GetRawText(), new System.Text.Json.JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? throw new InvalidOperationException($"Failed to deserialize {nameof(DeliveryNoteData)} from JsonElement");
        }

        throw new InvalidOperationException($"Data must be of type {nameof(DeliveryNoteData)} or JsonElement representing it. Actual type: {data?.GetType().Name ?? "null"}");
    }
}
