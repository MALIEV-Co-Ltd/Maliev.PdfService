namespace Maliev.PdfService.Api.Models.Data;

/// <summary>
/// Data contract for Quotation documents (ใบเสนอราคา).
/// </summary>
public class QuotationData
{
    // ─── Document Metadata ────────────────────────────────────────────────
    /// <summary>The human-readable quotation number (e.g., Q-2026-00042).</summary>
    public string QuotationNumber { get; set; } = string.Empty;

    /// <summary>Version number of this quotation revision.</summary>
    public int VersionNumber { get; set; } = 1;

    /// <summary>The date the quotation was issued.</summary>
    public DateTime QuotationDate { get; set; } = DateTime.UtcNow;

    /// <summary>The date the quotation validity starts.</summary>
    public DateTime ValidityStart { get; set; } = DateTime.UtcNow;

    /// <summary>The date the quotation expires.</summary>
    public DateTime ValidityEnd { get; set; } = DateTime.UtcNow.AddDays(30);

    // ─── Seller (Company) Information ─────────────────────────────────────
    /// <summary>Seller / issuing company name.</summary>
    public string SellerName { get; set; } = "MALIEV Co., Ltd.";

    /// <summary>Seller tax identification number.</summary>
    public string? SellerTaxId { get; set; }

    /// <summary>Seller address.</summary>
    public string? SellerAddress { get; set; }

    /// <summary>Seller phone number.</summary>
    public string? SellerPhone { get; set; }

    /// <summary>Seller email address.</summary>
    public string? SellerEmail { get; set; }

    // ─── Customer Information ─────────────────────────────────────────────
    /// <summary>The name of the customer.</summary>
    public string CustomerName { get; set; } = string.Empty;

    /// <summary>Customer type: Corporate or Individual.</summary>
    public string CustomerType { get; set; } = "Corporate";

    /// <summary>Branch name or "สำนักงานใหญ่" for head office (corporate only).</summary>
    public string? CustomerBranch { get; set; }

    /// <summary>Customer tax identification number (or national ID for individuals).</summary>
    public string? CustomerTaxId { get; set; }

    /// <summary>Customer phone number.</summary>
    public string? CustomerPhone { get; set; }

    /// <summary>Customer address.</summary>
    public string? CustomerAddress { get; set; }

    /// <summary>Customer billing address.</summary>
    public string? BillingAddress { get; set; }

    /// <summary>Customer shipping address.</summary>
    public string? ShippingAddress { get; set; }

    /// <summary>Contact person name at the customer.</summary>
    public string? ContactPerson { get; set; }

    // ─── Financial Details ────────────────────────────────────────────────
    /// <summary>The currency code (e.g., THB, USD).</summary>
    public string Currency { get; set; } = "THB";

    /// <summary>Total price before discounts.</summary>
    public decimal SubtotalBeforeDiscount { get; set; }

    /// <summary>Total discount amount.</summary>
    public decimal TotalDiscount { get; set; }

    /// <summary>Subtotal after discounts, before tax.</summary>
    public decimal Subtotal { get; set; }

    /// <summary>VAT / tax amount.</summary>
    public decimal TaxAmount { get; set; }

    /// <summary>Grand total including tax.</summary>
    public decimal TotalAmount { get; set; }

    // ─── Delivery & Terms ─────────────────────────────────────────────────
    /// <summary>Expected delivery lead time description (e.g., "10–14 business days").</summary>
    public string? DeliveryExpectations { get; set; }

    /// <summary>Special terms and conditions for this quotation.</summary>
    public string? SpecialTerms { get; set; }

    /// <summary>Summary of changes from previous version (if revision).</summary>
    public string? ChangeSummary { get; set; }

    // ─── Line Items & Discounts ───────────────────────────────────────────
    /// <summary>The list of line items in the quotation.</summary>
    public List<QuotationItemData> Items { get; set; } = new();

    /// <summary>Discount structures applied to this quotation.</summary>
    public List<QuotationDiscountData> Discounts { get; set; } = new();

    /// <summary>Optional internal notes (not shown on customer copy).</summary>
    public string? Notes { get; set; }
}

/// <summary>
/// Data contract for individual line items in a Quotation.
/// </summary>
public class QuotationItemData
{
    /// <summary>The sequential line number.</summary>
    public int Index { get; set; }

    /// <summary>Material name or product identifier.</summary>
    public string MaterialName { get; set; } = string.Empty;

    /// <summary>Manufacturing process (e.g., FDM, SLA, CNC Milling).</summary>
    public string? ManufacturingProcess { get; set; }

    /// <summary>Quantity.</summary>
    public decimal Quantity { get; set; }

    /// <summary>Unit of measure (e.g., pcs, kg, hrs).</summary>
    public string QuantityUnit { get; set; } = "pcs";

    /// <summary>Unit price.</summary>
    public decimal UnitPrice { get; set; }

    /// <summary>Total line price (Qty × UnitPrice).</summary>
    public decimal LineTotal { get; set; }

    /// <summary>Additional notes for this line (e.g., surface finish, tolerance).</summary>
    public string? Notes { get; set; }
}

/// <summary>
/// Data contract for a discount applied to a Quotation.
/// </summary>
public class QuotationDiscountData
{
    /// <summary>Type of discount: Percentage, FixedAmount, VolumeBased.</summary>
    public string DiscountType { get; set; } = "Percentage";

    /// <summary>Discount value (% or fixed amount depending on type).</summary>
    public decimal DiscountValue { get; set; }

    /// <summary>Conditions or description for this discount.</summary>
    public string? Conditions { get; set; }
}
