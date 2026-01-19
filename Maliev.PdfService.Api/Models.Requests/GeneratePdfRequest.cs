using Maliev.PdfService.Data.Entities;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace Maliev.PdfService.Api.Models.Requests;

/// <summary>
/// Request model for generating a PDF document.
/// </summary>
public class GeneratePdfRequest
{
    /// <summary>The unique code of the template to use.</summary>
    [Required]
    public string TemplateCode { get; set; } = string.Empty;

    /// <summary>The business reference ID (e.g., InvoiceId).</summary>
    [Required]
    [MaxLength(100)]
    public string ReferenceId { get; set; } = string.Empty;

    /// <summary>The type of document to generate.</summary>
    [Required]
    public DocumentType DocumentType { get; set; }

    /// <summary>The data payload to bind to the template.</summary>
    [Required]
    public JsonElement Data { get; set; }
}
