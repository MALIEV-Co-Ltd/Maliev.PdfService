namespace Maliev.PdfService.Api.Authorization;

/// <summary>
/// Predefined roles for the PDF Service.
/// </summary>
public static class PdfPredefinedRoles
{
    /// <summary>Standard role for services that need to generate PDFs.</summary>
    public const string Generator = "roles.pdf.generator";
    /// <summary>Full administrative access to PDF service.</summary>
    public const string Admin = "roles.pdf.admin";

    /// <summary>
    /// Collection of all predefined roles.
    /// </summary>
    public static readonly IReadOnlyList<(string RoleId, string Description, string[] Permissions)> All = new List<(string, string, string[])>
    {
        (Generator, "Standard role for services that need to generate PDFs", new[]
        {
            PdfPermissions.GenerationCreate,
            PdfPermissions.TemplatesList
        }),
        (Admin, "Full administrative access to PDF service", PdfPermissions.All.ToArray())
    };
}
