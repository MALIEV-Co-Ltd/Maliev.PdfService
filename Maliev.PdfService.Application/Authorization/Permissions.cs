namespace Maliev.PdfService.Application.Authorization;

/// <summary>
/// Defines the permissions for the PDF Service.
/// </summary>
public static class PdfPermissions
{
    public const string TemplateCreate = "pdf.templates.create";
    public const string TemplateRead = "pdf.templates.read";
    public const string TemplateUpdate = "pdf.templates.update";
    public const string TemplateDelete = "pdf.templates.delete";

    public const string DocumentGenerate = "pdf.documents.generate";
    public const string DocumentRead = "pdf.documents.read";
    public const string DocumentDelete = "pdf.documents.delete";

    public const string WebhookCreate = "pdf.webhooks.create";
    public const string WebhookRead = "pdf.webhooks.read";
    public const string WebhookDelete = "pdf.webhooks.delete";

    public const string AdminAll = "pdf.admin.all";

    public static readonly IReadOnlyDictionary<string, string> AllWithDescriptions = new Dictionary<string, string>
    {
        { TemplateCreate, "Create PDF templates" },
        { TemplateRead, "Read PDF templates" },
        { TemplateUpdate, "Update PDF templates" },
        { TemplateDelete, "Delete PDF templates" },
        { DocumentGenerate, "Generate PDF documents" },
        { DocumentRead, "Read PDF documents" },
        { DocumentDelete, "Delete PDF documents" },
        { WebhookCreate, "Create PDF webhooks" },
        { WebhookRead, "Read PDF webhooks" },
        { WebhookDelete, "Delete PDF webhooks" },
        { AdminAll, "Full PDF admin access" },
    };

    public static string[] All => AllWithDescriptions.Keys.ToArray();
}
