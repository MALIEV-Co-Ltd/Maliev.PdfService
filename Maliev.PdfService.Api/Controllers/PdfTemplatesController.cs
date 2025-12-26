using Maliev.PdfService.Data.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Asp.Versioning;

namespace Maliev.PdfService.Api.Controllers;

[ApiVersion("1.0")]
[Route("pdf/v{version:apiVersion}/templates")]
[ApiController]
public class PdfTemplatesController : ControllerBase
{
    private readonly PdfDbContext _dbContext;

    public PdfTemplatesController(PdfDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<IActionResult> GetTemplates()
    {
        var templates = await _dbContext.PdfTemplates
            .Select(t => new { t.Code, t.Name, t.CreatedAt })
            .ToListAsync();

        return Ok(templates);
    }
}
