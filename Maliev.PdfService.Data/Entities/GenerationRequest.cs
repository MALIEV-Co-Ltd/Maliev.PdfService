using System.ComponentModel.DataAnnotations;

namespace Maliev.PdfService.Data.Entities;

/// <summary>
/// Tracks the lifecycle of a specific PDF generation task.
/// </summary>
public class GenerationRequest
{
    /// <summary>The unique identifier.</summary>
    [Key]
    public Guid Id { get; set; }

    /// <summary>The ID of the source document (e.g., InvoiceId).</summary>
    [Required]
    [MaxLength(100)]
    public string ReferenceId { get; set; } = string.Empty;

    /// <summary>Unique template identifier (e.g., "INV-STD-01").</summary>
    [Required]
    [MaxLength(50)]
    public string TemplateCode { get; set; } = string.Empty;

    /// <summary>The type of document generated.</summary>
    [Required]
    public DocumentType DocumentType { get; set; }

    /// <summary>The current status of the request.</summary>
    [Required]
    public GenerationStatus Status { get; set; } = GenerationStatus.Pending;

    /// <summary>The raw data payload used for generation.</summary>
    public string? DataJson { get; set; }

    /// <summary>Public URL to the generated file in storage.</summary>
    public string? StorageUrl { get; set; }

    /// <summary>Failure details if status is Failed.</summary>
    public string? ErrorMessage { get; set; }

    /// <summary>Request timestamp.</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Completion timestamp.</summary>
    public DateTime? CompletedAt { get; set; }
}

/// <summary>
/// Defines the supported document types.
/// </summary>
public enum DocumentType
{
    /// <summary>Quotation document.</summary>
    Quotation,
    /// <summary>Invoice document.</summary>
    Invoice,
    /// <summary>Receipt document.</summary>
    Receipt,
    /// <summary>Financial or operational report.</summary>
    Report,
    /// <summary>Delivery note document (ใบส่งของ).</summary>
    DeliveryNote
}

/// <summary>
/// Defines the possible states of a generation request.
/// </summary>
public enum GenerationStatus
{
    /// <summary>Request received and waiting for processing.</summary>
    Pending,
    /// <summary>Rendering or uploading in progress.</summary>
    Processing,
    /// <summary>File stored and URL available.</summary>
    Completed,
    /// <summary>An error occurred during generation or storage.</summary>
    Failed
}
