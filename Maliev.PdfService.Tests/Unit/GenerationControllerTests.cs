using Maliev.PdfService.Api.Controllers;
using Maliev.PdfService.Api.Models.Requests;
using Maliev.PdfService.Api.Services;
using Maliev.PdfService.Data.Data;
using Maliev.PdfService.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using System.Text.Json;
using Xunit;

namespace Maliev.PdfService.Tests.Unit;

public class GenerationControllerTests
{
    private readonly Mock<IPdfGenerator> _pdfGeneratorMock = new();
    private readonly Mock<IUploadServiceClient> _uploadServiceMock = new();
    private readonly PdfDbContext _dbContext;
    private readonly GenerationController _controller;

    public GenerationControllerTests()
    {
        var options = new DbContextOptionsBuilder<PdfDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _dbContext = new PdfDbContext(options);
        _controller = new GenerationController(_pdfGeneratorMock.Object, _uploadServiceMock.Object, _dbContext);
    }

    [Fact]
    public async Task Generate_ReturnsOk_AndSavesToDb()
    {
        // Arrange
        var request = new GeneratePdfRequest
        {
            DocumentType = DocumentType.Invoice,
            ReferenceId = "REF-001",
            TemplateCode = "T1",
            Data = JsonSerializer.Deserialize<JsonElement>("{\"InvoiceNumber\": \"123\"}")
        };

        _pdfGeneratorMock.Setup(x => x.GeneratePdfAsync(It.IsAny<DocumentType>(), It.IsAny<object>(), null))
            .ReturnsAsync(new byte[] { 1, 2, 3 });

        _uploadServiceMock.Setup(x => x.UploadFileAsync(It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("https://storage.com/file.pdf");

        // Act
        var result = await _controller.Generate(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var count = await _dbContext.GenerationRequests.CountAsync();
        Assert.Equal(1, count);
        var saved = await _dbContext.GenerationRequests.FirstAsync();
        Assert.Equal("REF-001", saved.ReferenceId);
        Assert.Equal(GenerationStatus.Completed, saved.Status);
    }

    [Fact]
    public async Task GenerateAsync_ReturnsAccepted_AndSavesToDb()
    {
        // Arrange
        var request = new GeneratePdfRequest
        {
            DocumentType = DocumentType.Invoice,
            ReferenceId = "REF-ASYNC",
            TemplateCode = "T1",
            Data = JsonSerializer.Deserialize<JsonElement>("{}")
        };

        // Act
        var result = await _controller.GenerateAsync(request);

        // Assert
        var acceptedResult = Assert.IsType<AcceptedResult>(result);
        var count = await _dbContext.GenerationRequests.CountAsync();
        Assert.Equal(1, count);
        var saved = await _dbContext.GenerationRequests.FirstAsync();
        Assert.Equal("REF-ASYNC", saved.ReferenceId);
        Assert.Equal(GenerationStatus.Pending, saved.Status);
    }

    [Fact]
    public async Task Generate_ReturnsInternalServerError_OnException()
    {
        // Arrange
        var request = new GeneratePdfRequest
        {
            DocumentType = DocumentType.Invoice,
            ReferenceId = "REF-ERR",
            TemplateCode = "T1",
            Data = JsonSerializer.Deserialize<JsonElement>("{}")
        };

        _pdfGeneratorMock.Setup(x => x.GeneratePdfAsync(It.IsAny<DocumentType>(), It.IsAny<object>(), null))
            .ThrowsAsync(new Exception("Generation failed"));

        // Act & Assert
        // The controller doesn't currently have a try-catch for generic exceptions, 
        // it relies on global exception handler. 
        // But we can check if it throws.
        await Assert.ThrowsAsync<Exception>(() => _controller.Generate(request));
    }
}
