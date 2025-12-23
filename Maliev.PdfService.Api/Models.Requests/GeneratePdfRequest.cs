using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Maliev.PdfService.Data.Entities;

namespace Maliev.PdfService.Api.Models.Requests;

public class GeneratePdfRequest
{
    [Required]
    public string TemplateCode { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string ReferenceId { get; set; } = string.Empty;

    [Required]
    public DocumentType DocumentType { get; set; }

    [Required]
    public JsonElement Data { get; set; }
}
