using Asp.Versioning;
using Maliev.Aspire.ServiceDefaults.Authorization;
using Maliev.PdfService.Api.Authorization;
using Maliev.PdfService.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Maliev.PdfService.Api.Controllers;

/// <summary>
/// Controller for managing PDF templates.
/// </summary>
[ApiVersion("1.0")]
[Route("pdf/v{version:apiVersion}/templates")]
[ApiController]
[Authorize]
public class PdfTemplatesController : ControllerBase
{
    private readonly PdfDbContext _dbContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="PdfTemplatesController"/> class.
    /// </summary>
    public PdfTemplatesController(PdfDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Retrieves a list of all available PDF templates.
    /// </summary>
    /// <returns>A list of template summaries.</returns>
    [HttpGet]
    [RequirePermission(PdfPermissions.TemplatesList)]
    public async Task<IActionResult> GetTemplates()
    {
        var templates = await _dbContext.PdfTemplates
            .Select(t => new { t.Code, t.Name, t.CreatedAt })
            .ToListAsync();

        return Ok(templates);
    }
}
