using Maliev.PdfService.Api.Authorization;
using Maliev.Aspire.ServiceDefaults.IAM;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Maliev.PdfService.Api.Services;

/// <summary>
/// Registers PDF Service permissions and roles with the centralized IAM service on startup.
/// </summary>
public class PdfIAMRegistrationService : IAMRegistrationService
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PdfIAMRegistrationService"/> class.
    /// </summary>
    public PdfIAMRegistrationService(
        IConfiguration configuration,
        ILogger<PdfIAMRegistrationService> logger)
        : base(configuration, logger, "pdf")
    {
    }

    /// <inheritdoc/>
    protected override IEnumerable<PermissionRegistration> GetPermissions()
    {
        return PdfPermissions.AllWithDescriptions.Select(p => new PermissionRegistration
        {
            PermissionId = p.Key,
            Description = p.Value
        });
    }

    /// <inheritdoc/>
    protected override IEnumerable<RoleRegistration> GetPredefinedRoles()
    {
        return PdfPredefinedRoles.All.Select(r => new RoleRegistration
        {
            RoleId = r.RoleId,
            Description = r.Description,
            PermissionIds = r.Permissions.ToList(),
            IsCustom = false
        });
    }
}
