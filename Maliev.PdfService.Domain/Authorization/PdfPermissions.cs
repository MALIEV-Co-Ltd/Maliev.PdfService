namespace Maliev.PdfService.Api.Authorization;

/// <summary>
/// Constants for PDF Service permissions.
/// Follows GCP-style naming: {service}.{resource}.{action}
/// </summary>
public static class PdfPermissions
{
    /// <summary>Permission to generate new PDF documents.</summary>
    public const string GenerationCreate = "pdf.generations.create";
    /// <summary>Permission to read generated PDF document records.</summary>
    public const string GenerationRead = "pdf.generations.read";
    /// <summary>Permission to list available PDF templates.</summary>
    public const string TemplatesList = "pdf.templates.list";

    /// <summary>
    /// Collection of all defined PDF permissions with descriptions.
    /// </summary>
    public static readonly IReadOnlyDictionary<string, string> AllWithDescriptions = new Dictionary<string, string>
    {
        { GenerationCreate, "Ability to generate new PDF documents" },
        { GenerationRead, "Ability to read generated PDF document records" },
        { TemplatesList, "Ability to list available PDF templates" }
    };

    /// <summary>All available permission codes</summary>
    public static IEnumerable<string> All => AllWithDescriptions.Keys;
}
