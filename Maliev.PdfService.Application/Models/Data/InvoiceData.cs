namespace Maliev.PdfService.Api.Models.Data;

/// <summary>
/// Data contract for Invoice documents.
/// Covers all invoice types: Tax Invoice, Invoice, Credit Note, Debit Note.
/// </summary>
public class InvoiceData
{
    // ─── Document Metadata ────────────────────────────────────────────────
    /// <summary>The human-readable invoice number (e.g., INV-2026-00123).</summary>
    public string InvoiceNumber { get; set; } = string.Empty;

    /// <summary>The document type: TaxInvoice, Invoice, CreditNote, DebitNote.</summary>
    public string DocumentType { get; set; } = "Invoice";

    /// <summary>The date the invoice was issued.</summary>
    public DateTime IssueDate { get; set; } = DateTime.UtcNow;

    /// <summary>The payment due date.</summary>
    public DateTime? DueDate { get; set; }

    /// <summary>Payment terms in days (e.g., 30).</summary>
    public int? PaymentTermsDays { get; set; }

    /// <summary>Credit term code (e.g., NET30).</summary>
    public string? CreditTermCode { get; set; }

    /// <summary>Customer purchase order reference number.</summary>
    public string? PoNumber { get; set; }

    /// <summary>Quotation reference number this invoice is based on.</summary>
    public string? QuotationReference { get; set; }

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
    /// <summary>Customer / buyer name.</summary>
    public string CustomerName { get; set; } = string.Empty;

    /// <summary>Customer type: Corporate or Individual.</summary>
    public string CustomerType { get; set; } = "Corporate";

    /// <summary>Branch name or "สำนักงานใหญ่" for head office (corporate only).</summary>
    public string? CustomerBranch { get; set; }

    /// <summary>Customer tax identification number (or national ID for individuals).</summary>
    public string? CustomerTaxId { get; set; }

    /// <summary>Customer billing address.</summary>
    public string? BillingAddress { get; set; }

    /// <summary>Customer shipping address (if different from billing).</summary>
    public string? ShippingAddress { get; set; }

    // ─── Financial Details ────────────────────────────────────────────────
    /// <summary>Currency code (e.g., THB, USD).</summary>
    public string Currency { get; set; } = "THB";

    /// <summary>Exchange rate to base currency (1 if same currency).</summary>
    public decimal ExchangeRate { get; set; } = 1m;

    /// <summary>Sum of all line totals before tax and discounts.</summary>
    public decimal Subtotal { get; set; }

    /// <summary>Total discount amount from all line items.</summary>
    public decimal? TotalDiscountAmount { get; set; }

    /// <summary>Total VAT / tax amount.</summary>
    public decimal TaxAmount { get; set; }

    /// <summary>Withholding tax amount (ภาษีหัก ณ ที่จ่าย).</summary>
    public decimal WithholdingTaxAmount { get; set; }

    /// <summary>Withholding tax percentage (e.g., 1, 1.5, 3).</summary>
    public decimal? WithholdingTaxPercentage { get; set; }

    /// <summary>Grand total after tax and deductions.</summary>
    public decimal GrandTotal { get; set; }

    /// <summary>Late payment fee percentage.</summary>
    public decimal? LateFeePercentage { get; set; }

    // ─── Bank / Payment Details ───────────────────────────────────────────
    /// <summary>Bank name for payment transfer.</summary>
    public string? BankName { get; set; }

    /// <summary>Bank account number.</summary>
    public string? BankAccountNumber { get; set; }

    /// <summary>Bank account name.</summary>
    public string? BankAccountName { get; set; }

    /// <summary>Bank branch name or code.</summary>
    public string? BankBranch { get; set; }

    // ─── Line Items ───────────────────────────────────────────────────────
    /// <summary>The list of line items in the invoice.</summary>
    public List<InvoiceItemData> Items { get; set; } = new();

    /// <summary>Optional notes or payment instructions.</summary>
    public string? Notes { get; set; }
}

/// <summary>
/// Data contract for individual line items in an Invoice.
/// </summary>
public class InvoiceItemData
{
    /// <summary>The sequential line number.</summary>
    public int Index { get; set; }

    /// <summary>Product or service code.</summary>
    public string? ItemCode { get; set; }

    /// <summary>Description of the product or service.</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>Quantity ordered.</summary>
    public decimal Quantity { get; set; }

    /// <summary>Unit of measure (e.g., pcs, hrs, kg).</summary>
    public string Unit { get; set; } = "pcs";

    /// <summary>Unit price before discount.</summary>
    public decimal UnitPrice { get; set; }

    /// <summary>Discount percentage for this line (0–100).</summary>
    public decimal DiscountPercentage { get; set; }

    /// <summary>VAT category (e.g., VAT7, ExemptVAT, ZeroVAT).</summary>
    public string? TaxCategory { get; set; }

    /// <summary>VAT rate applied (e.g., 7).</summary>
    public decimal TaxRate { get; set; } = 7m;

    /// <summary>Line subtotal before tax (Qty × UnitPrice × (1 − Discount%)).</summary>
    public decimal LineSubtotal { get; set; }

    /// <summary>VAT amount for this line.</summary>
    public decimal LineTaxAmount { get; set; }

    /// <summary>Total for this line including VAT.</summary>
    public decimal LineTotal { get; set; }
}
