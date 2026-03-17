namespace Maliev.PdfService.Api.Models.Data;

/// <summary>
/// Data contract for Invoice documents.
/// </summary>
public class InvoiceData
{
    /// <summary>The human-readable invoice number.</summary>
    public string InvoiceNumber { get; set; } = string.Empty;
    /// <summary>The list of line items in the invoice.</summary>
    public List<InvoiceItemData> Items { get; set; } = new();
    /// <summary>The currency code (e.g., THB, USD).</summary>
    public string Currency { get; set; } = string.Empty;
}

/// <summary>
/// Data contract for individual items in an Invoice.
/// </summary>
public class InvoiceItemData
{
    /// <summary>The item index.</summary>
    public int Index { get; set; }
    /// <summary>The item description.</summary>
    public string Description { get; set; } = string.Empty;
    /// <summary>The quantity.</summary>
    public double Quantity { get; set; }
    /// <summary>The total price for this item.</summary>
    public double TotalPrice { get; set; }
}
