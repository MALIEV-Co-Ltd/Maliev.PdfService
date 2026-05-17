namespace Maliev.PdfService.Api.Models.Data;

/// <summary>
/// Data contract for a Commerce product bill of materials PDF.
/// </summary>
public class CommerceBomData
{
    /// <summary>Gets or sets the product title.</summary>
    public string ProductTitle { get; set; } = string.Empty;

    /// <summary>Gets or sets the product handle.</summary>
    public string ProductHandle { get; set; } = string.Empty;

    /// <summary>Gets or sets the product brand.</summary>
    public string? Brand { get; set; }

    /// <summary>Gets or sets the product type.</summary>
    public string ProductType { get; set; } = string.Empty;

    /// <summary>Gets or sets the product publication status.</summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>Gets or sets the date the BOM was generated.</summary>
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Gets or sets the default ISO currency code.</summary>
    public string Currency { get; set; } = "THB";

    /// <summary>Gets or sets the BOM line items.</summary>
    public List<CommerceBomItemData> Items { get; set; } = [];

    /// <summary>Gets or sets the total expected cost.</summary>
    public decimal TotalCost { get; set; }
}

/// <summary>
/// Data contract for one Commerce product bill of materials line.
/// </summary>
public class CommerceBomItemData
{
    /// <summary>Gets or sets the one-based line index.</summary>
    public int Index { get; set; }

    /// <summary>Gets or sets the material, component, or consumable name.</summary>
    public string ItemName { get; set; } = string.Empty;

    /// <summary>Gets or sets the item specification, grade, color, size, or supplier reference.</summary>
    public string? Specification { get; set; }

    /// <summary>Gets or sets the quantity used by one sellable product unit.</summary>
    public decimal Quantity { get; set; }

    /// <summary>Gets or sets the unit of measure.</summary>
    public string Unit { get; set; } = "pcs";

    /// <summary>Gets or sets the expected unit cost.</summary>
    public decimal UnitCost { get; set; }

    /// <summary>Gets or sets the ISO currency code.</summary>
    public string Currency { get; set; } = "THB";

    /// <summary>Gets or sets the expected line total.</summary>
    public decimal LineTotal { get; set; }

    /// <summary>Gets or sets internal notes about the item.</summary>
    public string? Notes { get; set; }
}
