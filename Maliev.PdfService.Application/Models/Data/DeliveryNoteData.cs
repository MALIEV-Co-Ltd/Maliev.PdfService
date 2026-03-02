namespace Maliev.PdfService.Api.Models.Data;

/// <summary>
/// Data contract for Delivery Note documents.
/// </summary>
public class DeliveryNoteData
{
    /// <summary>The delivery note number (e.g., DN-2026-000001).</summary>
    public string DeliveryNoteNumber { get; set; } = string.Empty;

    /// <summary>The related order number.</summary>
    public string? OrderNumber { get; set; }

    /// <summary>Customer business name.</summary>
    public string CustomerName { get; set; } = string.Empty;

    /// <summary>Full shipping address (formatted).</summary>
    public string CustomerAddress { get; set; } = string.Empty;

    /// <summary>Scheduled delivery date.</summary>
    public DateTime DeliveryDate { get; set; }

    /// <summary>Logistics tracking number.</summary>
    public string? TrackingNumber { get; set; }

    /// <summary>Carrier/shipping company name.</summary>
    public string? CarrierName { get; set; }

    /// <summary>Delivery contact person name.</summary>
    public string? DeliveryContact { get; set; }

    /// <summary>Delivery contact phone number.</summary>
    public string? DeliveryPhone { get; set; }

    /// <summary>List of items being delivered.</summary>
    public List<DeliveryNoteItemData> Items { get; set; } = new();

    /// <summary>Delivery instructions or special notes.</summary>
    public string? Notes { get; set; }
}

/// <summary>
/// Data contract for individual items in a Delivery Note.
/// </summary>
public class DeliveryNoteItemData
{
    /// <summary>Product SKU or code.</summary>
    public string ProductCode { get; set; } = string.Empty;

    /// <summary>Product name.</summary>
    public string ProductName { get; set; } = string.Empty;

    /// <summary>Quantity originally ordered.</summary>
    public decimal QuantityOrdered { get; set; }

    /// <summary>Quantity manufactured/ready.</summary>
    public decimal QuantityManufactured { get; set; }

    /// <summary>Quantity being delivered in this shipment.</summary>
    public decimal QuantityDelivered { get; set; }

    /// <summary>Unit of measure (e.g., pcs, kg, m).</summary>
    public string UnitOfMeasure { get; set; } = string.Empty;
}
