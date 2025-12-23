using System.ComponentModel.DataAnnotations;

namespace Maliev.PdfService.Data.Entities;

public class GenerationRequest
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string ReferenceId { get; set; } = string.Empty;

    [Required]
    public DocumentType DocumentType { get; set; }

    [Required]
    public GenerationStatus Status { get; set; } = GenerationStatus.Pending;

    public string? StorageUrl { get; set; }

    public string? ErrorMessage { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? CompletedAt { get; set; }
}

public enum DocumentType
{
    Quotation,
    Invoice,
    Receipt,
    Report
}

public enum GenerationStatus
{
    Pending,
    Processing,
    Completed,
    Failed
}
