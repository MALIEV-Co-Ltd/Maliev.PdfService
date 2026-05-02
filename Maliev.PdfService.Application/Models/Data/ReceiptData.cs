namespace Maliev.PdfService.Api.Models.Data;

/// <summary>
/// Data contract for Receipt documents.
/// </summary>
public class ReceiptData
{
    /// <summary>The receipt number.</summary>
    public string ReceiptNumber { get; set; } = string.Empty;
    /// <summary>The date of the receipt.</summary>
    public DateTime ReceiptDate { get; set; }
    /// <summary>The customer name.</summary>
    public string CustomerName { get; set; } = string.Empty;
    /// <summary>Customer type: Corporate or Individual.</summary>
    public string CustomerType { get; set; } = "Corporate";
    /// <summary>Branch name or "สำนักงานใหญ่" for head office (corporate only).</summary>
    public string? CustomerBranch { get; set; }
    /// <summary>Customer tax identification number (or national ID for individuals).</summary>
    public string? CustomerTaxId { get; set; }
    /// <summary>The customer address.</summary>
    public string? CustomerAddress { get; set; }
    /// <summary>The payment method (Cash, Credit Card, Transfer).</summary>
    public string PaymentMethod { get; set; } = string.Empty;
    /// <summary>Reference number from payment.</summary>
    public string? ReferenceNumber { get; set; }
    /// <summary>The list of items in the receipt.</summary>
    public List<ReceiptItemData> Items { get; set; } = new();
    /// <summary>The subtotal amount.</summary>
    public double Subtotal { get; set; }
    /// <summary>The tax amount.</summary>
    public double TaxAmount { get; set; }
    /// <summary>The discount amount.</summary>
    public double DiscountAmount { get; set; }
    /// <summary>The total amount.</summary>
    public double TotalAmount { get; set; }
    /// <summary>The currency code (e.g., THB).</summary>
    public string Currency { get; set; } = "THB";
    /// <summary>Additional notes.</summary>
    public string? Notes { get; set; }
    /// <summary>The company name.</summary>
    public string CompanyName { get; set; } = string.Empty;
    /// <summary>The company address.</summary>
    public string? CompanyAddress { get; set; }
    /// <summary>The company tax ID.</summary>
    public string? CompanyTaxId { get; set; }
    /// <summary>The company phone number.</summary>
    public string? CompanyPhone { get; set; }
}

/// <summary>
/// Data contract for individual items in a Receipt.
/// </summary>
public class ReceiptItemData
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
