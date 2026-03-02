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
    public Guid InvoiceId { get; set; }
    /// <summary>Invoice Number.</summary>
    public string InvoiceNumber { get; set; } = string.Empty;
    /// <summary>Customer Name.</summary>
    public string CustomerName { get; set; } = string.Empty;
    /// <summary>Issue Date.</summary>
    public DateTime IssueDate { get; set; }
    /// <summary>Due Date.</summary>
    public DateTime DueDate { get; set; }
    /// <summary>Subtotal amount.</summary>
    public decimal SubTotalAmount { get; set; }
    /// <summary>Tax amount.</summary>
    public decimal TaxAmount { get; set; }
    /// <summary>Total amount.</summary>
    public decimal TotalAmount { get; set; }
    /// <summary>Items.</summary>
    public List<InvoiceItemDto> Items { get; set; } = new();
}

/// <summary>
/// Dto for invoice items.
/// </summary>
public class InvoiceItemDto
{
    /// <summary>Description.</summary>
    public string Description { get; set; } = string.Empty;
    /// <summary>Quantity.</summary>
    public decimal Quantity { get; set; }
    /// <summary>Unit Price.</summary>
    public decimal UnitPrice { get; set; }
    /// <summary>Total Amount.</summary>
    public decimal TotalAmount { get; set; }
}
