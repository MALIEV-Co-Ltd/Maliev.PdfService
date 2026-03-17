namespace Maliev.PdfService.Api.Models.Data;

/// <summary>
/// Data contract for Quotation documents.
/// </summary>
public class QuotationData
{
    /// <summary>The human-readable quotation number.</summary>
    public string QuotationNumber { get; set; } = string.Empty;
    /// <summary>The name of the customer.</summary>
    public string CustomerName { get; set; } = string.Empty;
    /// <summary>The date of the quotation.</summary>
    public DateTime QuotationDate { get; set; }
    /// <summary>The list of line items in the quotation.</summary>
    public List<QuotationItemData> Items { get; set; } = new();
    /// <summary>The total amount.</summary>
    public double TotalAmount { get; set; }
    /// <summary>The currency code (e.g., THB).</summary>
    public string Currency { get; set; } = string.Empty;
}

/// <summary>
/// Data contract for individual items in a Quotation.
/// </summary>
public class QuotationItemData
{
    /// <summary>The item index.</summary>
    public int Index { get; set; }
    /// <summary>The item description.</summary>
    public string Description { get; set; } = string.Empty;
    /// <summary>The quantity.</summary>
    public double Quantity { get; set; }
    /// <summary>The unit price.</summary>
    public double UnitPrice { get; set; }
    /// <summary>The total price for this item.</summary>
    public double TotalPrice { get; set; }
}
