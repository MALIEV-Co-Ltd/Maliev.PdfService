using System.ComponentModel.DataAnnotations;

namespace Maliev.PdfService.Data.Entities;

/// <summary>
/// Represents a reusable document layout strategy.
/// </summary>
public class PdfTemplate
{
    /// <summary>The unique identifier.</summary>
    [Key]
    public Guid Id { get; set; }

    /// <summary>Unique template identifier (e.g., "INV-STD-01").</summary>
    [Required]
    [MaxLength(50)]
    public string Code { get; set; } = string.Empty;

    /// <summary>Human-readable name.</summary>
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    /// <summary>Full name of the QuestPDF IDocument implementation.</summary>
    [Required]
    public string LayoutClass { get; set; } = string.Empty;

    /// <summary>JSON configuration (colors, logo URL, etc.).</summary>
    [Required]
    public string ConfigJson { get; set; } = "{}";

    /// <summary>Creation timestamp.</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
