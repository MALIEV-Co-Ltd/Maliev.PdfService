using System.ComponentModel.DataAnnotations;

namespace Maliev.PdfService.Data.Entities;

public class PdfTemplate
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string Code { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string LayoutClass { get; set; } = string.Empty;

    [Required]
    public string ConfigJson { get; set; } = "{}";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
