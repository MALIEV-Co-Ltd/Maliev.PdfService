using Maliev.Aspire.ServiceDefaults.IAM;

namespace Maliev.PdfService.Api.Services;

public class PdfIAMRegistrationService : IAMRegistrationService
{
    public PdfIAMRegistrationService(
        IHttpClientFactory httpClientFactory,
        ILogger<PdfIAMRegistrationService> logger)
        : base(httpClientFactory, logger, "PdfService")
    {
    }

    protected override IEnumerable<PermissionRegistration> GetPermissions()
    {
        return new[]
        {
            new PermissionRegistration
            {
                PermissionId = "pdf.generation.create",
                Description = "Ability to generate new PDF documents"
            },
            new PermissionRegistration
            {
                PermissionId = "pdf.templates.list",
                Description = "Ability to list available PDF templates"
            }
        };
    }

    protected override IEnumerable<RoleRegistration> GetPredefinedRoles()
    {
        return new[]
        {
            new RoleRegistration
            {
                RoleId = "roles.pdf.generator",
                Description = "Standard role for services that need to generate PDFs",
                PermissionIds = new List<string> { "pdf.generation.create", "pdf.templates.list" }
            },
            new RoleRegistration
            {
                RoleId = "roles.pdf.admin",
                Description = "Full administrative access to PDF service",
                PermissionIds = new List<string> { "pdf.generation.create", "pdf.templates.list" } // Listing all for now
            }
        };
    }
}
