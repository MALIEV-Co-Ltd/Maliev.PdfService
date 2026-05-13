using System.Text.Json.Serialization;

namespace Maliev.PdfService.Api.Services;

/// <summary>
/// Client for fetching invoice details.
/// </summary>
public interface IInvoiceServiceClient
{
    /// <summary>
    /// Gets invoice by id.
    /// </summary>
    Task<InvoiceDto?> GetInvoiceByIdAsync(Guid invoiceId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Dto for invoice details.
/// </summary>
public class InvoiceDto
{
    /// <summary>Invoice ID.</summary>
    [JsonPropertyName("id")]
    public Guid InvoiceId { get; set; }

    /// <summary>Invoice Number.</summary>
    [JsonPropertyName("invoiceNumber")]
    public string InvoiceNumber { get; set; } = string.Empty;

    /// <summary>Customer Name.</summary>
    [JsonPropertyName("customerName")]
    public string CustomerName { get; set; } = string.Empty;

    /// <summary>Issue Date.</summary>
    [JsonPropertyName("issueDate")]
    public DateTime IssueDate { get; set; }

    /// <summary>Due Date.</summary>
    [JsonPropertyName("dueDate")]
    public DateTime DueDate { get; set; }

    /// <summary>Subtotal amount.</summary>
    [JsonPropertyName("subtotal")]
    public decimal SubTotalAmount { get; set; }

    /// <summary>Tax amount.</summary>
    [JsonPropertyName("taxAmount")]
    public decimal TaxAmount { get; set; }

    /// <summary>Total amount.</summary>
    [JsonPropertyName("grandTotal")]
    public decimal TotalAmount { get; set; }

    /// <summary>Items.</summary>
    [JsonPropertyName("lines")]
    public List<InvoiceItemDto> Items { get; set; } = new();
}

/// <summary>
/// Dto for invoice items.
/// </summary>
public class InvoiceItemDto
{
    /// <summary>Description.</summary>
    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    /// <summary>Quantity.</summary>
    [JsonPropertyName("quantity")]
    public decimal Quantity { get; set; }

    /// <summary>Unit Price.</summary>
    [JsonPropertyName("unitPrice")]
    public decimal UnitPrice { get; set; }

    /// <summary>Total Amount.</summary>
    [JsonPropertyName("lineTotal")]
    public decimal TotalAmount { get; set; }
}
