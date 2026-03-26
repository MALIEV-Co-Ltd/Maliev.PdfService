namespace Maliev.PdfService.Application.Authorization;

/// <summary>
/// Provides access to predefined roles for the PDF Service.
/// </summary>
public static class PdfPredefinedRoles
{
    public const string Admin = "roles.pdf.admin";
    public const string Operator = "roles.pdf.operator";
    public const string Viewer = "roles.pdf.viewer";

    public static readonly IReadOnlyList<(string RoleId, string Description, string[] Permissions)> All = new List<(string, string, string[])>
    {
        (
            Admin,
            "PDF Administrator with full access",
            new[]
            {
                PdfPermissions.TemplateCreate,
                PdfPermissions.TemplateRead,
                PdfPermissions.TemplateUpdate,
                PdfPermissions.TemplateDelete,
                PdfPermissions.DocumentGenerate,
                PdfPermissions.DocumentRead,
                PdfPermissions.DocumentDelete,
                PdfPermissions.WebhookCreate,
                PdfPermissions.WebhookRead,
                PdfPermissions.WebhookDelete,
                PdfPermissions.AdminAll,
            }
        ),
        (
            Operator,
            "PDF Operator with template and document access",
            new[]
            {
                PdfPermissions.TemplateCreate,
                PdfPermissions.TemplateRead,
                PdfPermissions.TemplateUpdate,
                PdfPermissions.DocumentGenerate,
                PdfPermissions.DocumentRead,
                PdfPermissions.WebhookCreate,
                PdfPermissions.WebhookRead,
            }
        ),
        (
            Viewer,
            "PDF Viewer with read-only access",
            new[]
            {
                PdfPermissions.TemplateRead,
                PdfPermissions.DocumentRead,
                PdfPermissions.WebhookRead,
            }
        ),
    };
}
