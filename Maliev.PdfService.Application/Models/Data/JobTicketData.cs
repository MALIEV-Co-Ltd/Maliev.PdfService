namespace Maliev.PdfService.Api.Models.Data;

/// <summary>
/// Data contract for Job Ticket documents — shop floor manufacturing work orders.
/// Combines Job entity data with enrichment from OrderService.
/// </summary>
public class JobTicketData
{
    // ─── Ticket Metadata ──────────────────────────────────────────────────
    /// <summary>Unique job ticket number (e.g., JT-2026-00042).</summary>
    public string JobTicketNumber { get; set; } = string.Empty;

    /// <summary>Date the job ticket was issued.</summary>
    public DateTime IssuedDate { get; set; } = DateTime.UtcNow;

    /// <summary>Production priority level (1 = highest).</summary>
    public int Priority { get; set; } = 5;

    // ─── Job Reference ────────────────────────────────────────────────────
    /// <summary>Job ID from JobService.</summary>
    public string JobId { get; set; } = string.Empty;

    /// <summary>Order ID from OrderService.</summary>
    public string OrderId { get; set; } = string.Empty;

    /// <summary>Customer order reference number.</summary>
    public string? OrderReference { get; set; }

    /// <summary>Delivery deadline for this job.</summary>
    public DateTime? DeliveryDeadline { get; set; }

    // ─── Customer Information ─────────────────────────────────────────────
    /// <summary>Customer name for this order.</summary>
    public string CustomerName { get; set; } = string.Empty;

    /// <summary>Customer ID for identification.</summary>
    public string? CustomerId { get; set; }

    // ─── Part Information ─────────────────────────────────────────────────
    /// <summary>Part name / description.</summary>
    public string PartName { get; set; } = string.Empty;

    /// <summary>Material name and specification.</summary>
    public string MaterialName { get; set; } = string.Empty;

    /// <summary>Color or finish of the material.</summary>
    public string? ColorName { get; set; }

    /// <summary>Quantity to produce.</summary>
    public int Quantity { get; set; } = 1;

    /// <summary>Surface finishing specification (e.g., As-printed, Sanded, Painted).</summary>
    public string? SurfaceFinishing { get; set; }

    // ─── Manufacturing Technology ─────────────────────────────────────────
    /// <summary>Manufacturing technology: FDM, SLA, CNC, Scanning, Design.</summary>
    public string Technology { get; set; } = string.Empty;

    /// <summary>Assigned machine ID or name.</summary>
    public string? AssignedMachine { get; set; }

    /// <summary>Estimated volume in cm³ (for 3D printing).</summary>
    public decimal? VolumeCm3 { get; set; }

    /// <summary>Estimated production time in minutes.</summary>
    public int? EstimatedMinutes { get; set; }

    // ─── 3D Printing Specific ─────────────────────────────────────────────
    /// <summary>Whether thread tapping is required post-print.</summary>
    public bool ThreadTapRequired { get; set; }

    /// <summary>Whether heat-set inserts are required.</summary>
    public bool InsertRequired { get; set; }

    /// <summary>Whether part marking / engraving is required.</summary>
    public bool PartMarking { get; set; }

    /// <summary>Customer requirements or special instructions.</summary>
    public string? Requirements { get; set; }

    // ─── CNC Specific ─────────────────────────────────────────────────────
    /// <summary>Dimensional tolerance specification (CNC).</summary>
    public string? Tolerance { get; set; }

    /// <summary>Surface roughness specification, e.g., Ra 1.6 (CNC).</summary>
    public string? SurfaceRoughness { get; set; }

    /// <summary>Inspection type required (CNC).</summary>
    public string? InspectionType { get; set; }

    // ─── Sign-offs ────────────────────────────────────────────────────────
    /// <summary>Notes from the production planner.</summary>
    public string? Notes { get; set; }

    // ─── Reference Images ──────────────────────────────────────────────────
    /// <summary>Job reference images for all 6 part sides: Front, Left, Right, Back, Top, Bottom.</summary>
    public Dictionary<string, string>? PreviewImages { get; set; }
}
