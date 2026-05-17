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
            DocumentType.Receipt => new ReceiptDocument(MapToReceiptData(data)),
            DocumentType.Report => new FinancialReportDocument(MapToFinancialReportData(data)),
            DocumentType.DeliveryNote => new DeliveryNoteDocument(MapToDeliveryNoteData(data)),
            DocumentType.JobTicket => new JobTicketDocument(MapToJobTicketData(data)),
            DocumentType.CommerceBom => new CommerceBomDocument(MapToCommerceBomData(data)),
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

    private static ReceiptData MapToReceiptData(object data)
    {
        if (data is ReceiptData receiptData) return receiptData;

        if (data is System.Text.Json.JsonElement jsonElement)
        {
            return System.Text.Json.JsonSerializer.Deserialize<ReceiptData>(jsonElement.GetRawText(), new System.Text.Json.JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? throw new InvalidOperationException($"Failed to deserialize {nameof(ReceiptData)} from JsonElement");
        }

        throw new InvalidOperationException($"Data must be of type {nameof(ReceiptData)} or JsonElement representing it. Actual type: {data?.GetType().Name ?? "null"}");
    }

    private static FinancialReportData MapToFinancialReportData(object data)
    {
        if (data is FinancialReportData reportData) return reportData;

        if (data is System.Text.Json.JsonElement jsonElement)
        {
            return System.Text.Json.JsonSerializer.Deserialize<FinancialReportData>(jsonElement.GetRawText(), new System.Text.Json.JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? throw new InvalidOperationException($"Failed to deserialize {nameof(FinancialReportData)} from JsonElement");
        }

        throw new InvalidOperationException($"Data must be of type {nameof(FinancialReportData)} or JsonElement representing it. Actual type: {data?.GetType().Name ?? "null"}");
    }

    private static JobTicketData MapToJobTicketData(object data)
    {
        if (data is JobTicketData jobTicketData) return jobTicketData;

        if (data is System.Text.Json.JsonElement jsonElement)
        {
            return System.Text.Json.JsonSerializer.Deserialize<JobTicketData>(jsonElement.GetRawText(), new System.Text.Json.JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? throw new InvalidOperationException($"Failed to deserialize {nameof(JobTicketData)} from JsonElement");
        }

        throw new InvalidOperationException($"Data must be of type {nameof(JobTicketData)} or JsonElement representing it. Actual type: {data?.GetType().Name ?? "null"}");
    }

    private static CommerceBomData MapToCommerceBomData(object data)
    {
        if (data is CommerceBomData bomData) return bomData;

        if (data is System.Text.Json.JsonElement jsonElement)
        {
            return System.Text.Json.JsonSerializer.Deserialize<CommerceBomData>(jsonElement.GetRawText(), new System.Text.Json.JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? throw new InvalidOperationException($"Failed to deserialize {nameof(CommerceBomData)} from JsonElement");
        }

        throw new InvalidOperationException($"Data must be of type {nameof(CommerceBomData)} or JsonElement representing it. Actual type: {data?.GetType().Name ?? "null"}");
    }
}
