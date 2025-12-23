using Maliev.PdfService.Api.Models.Requests;
using Maliev.PdfService.Api.Services;
using Maliev.PdfService.Data.Data;
using Maliev.PdfService.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;

namespace Maliev.PdfService.Api.Controllers;

[ApiVersion("1.0")]
[Route("pdf/v{version:apiVersion}")]
[ApiController]
public class GenerationController : ControllerBase
{
    private readonly IPdfGenerator _pdfGenerator;
    private readonly IUploadServiceClient _uploadService;
    private readonly PdfDbContext _dbContext;

    public GenerationController(IPdfGenerator pdfGenerator, IUploadServiceClient uploadService, PdfDbContext dbContext)
    {
        _pdfGenerator = pdfGenerator;
        _uploadService = uploadService;
        _dbContext = dbContext;
    }

    [HttpPost("generate")]
    public async Task<IActionResult> Generate([FromBody] GeneratePdfRequest request)
    {
        var pdfBytes = await _pdfGenerator.GeneratePdfAsync(request.DocumentType, request.Data);
        
        var fileName = $"{request.DocumentType}_{request.ReferenceId}_{Guid.NewGuid()}.pdf";
        var storagePath = $"pdfs/{request.DocumentType.ToString().ToLower()}/{request.ReferenceId}/{fileName}";
        
        var url = await _uploadService.UploadFileAsync(fileName, pdfBytes, "application/pdf", storagePath);

        var log = new GenerationRequest
        {
            ReferenceId = request.ReferenceId,
            DocumentType = request.DocumentType,
            Status = GenerationStatus.Completed,
            StorageUrl = url,
            CreatedAt = DateTime.UtcNow,
            CompletedAt = DateTime.UtcNow
        };

        _dbContext.GenerationRequests.Add(log);
        await _dbContext.SaveChangesAsync();

        return Ok(new { requestId = log.Id, storageUrl = url });
    }

    [HttpPost("generate/async")]
    public async Task<IActionResult> GenerateAsync([FromBody] GeneratePdfRequest request)
    {
        var log = new GenerationRequest
        {
            ReferenceId = request.ReferenceId,
            DocumentType = request.DocumentType,
            Status = GenerationStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.GenerationRequests.Add(log);
        await _dbContext.SaveChangesAsync();

        // In a real implementation, this would enqueue a background job or publish a message
        // For MVP, we'll just return the accepted status
        
        return Accepted(new { requestId = log.Id });
    }
}
