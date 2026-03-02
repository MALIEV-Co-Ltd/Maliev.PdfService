using Maliev.PdfService.Api.Controllers;
using Maliev.PdfService.Api.Models.Requests;
using Maliev.PdfService.Api.Services;
using Maliev.PdfService.Infrastructure.Data;
using Maliev.PdfService.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using MassTransit;
using System.Text.Json;
using Xunit;

namespace Maliev.PdfService.Tests.Unit;

public class GenerationControllerTests
{
    private readonly Mock<IPdfGenerator> _pdfGeneratorMock = new();
    private readonly Mock<IUploadServiceClient> _uploadServiceMock = new();
    private readonly Mock<IPublishEndpoint> _publishEndpointMock = new();
    private readonly Mock<ILogger<GenerationController>> _loggerMock = new();
    private readonly PdfDbContext _dbContext;
    private readonly GenerationController _controller;

    public GenerationControllerTests()
    {
        var options = new DbContextOptionsBuilder<PdfDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _dbContext = new PdfDbContext(options);
        _controller = new GenerationController(
            _pdfGeneratorMock.Object,
            _uploadServiceMock.Object,
            _dbContext,
            _publishEndpointMock.Object,
            _loggerMock.Object);
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

        _pdfGeneratorMock.Setup(x => x.GeneratePdfAsync(It.IsAny<DocumentType>(), It.IsAny<object>(), "T1"))
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

        _pdfGeneratorMock.Setup(x => x.GeneratePdfAsync(It.IsAny<DocumentType>(), It.IsAny<object>(), "T1"))
            .ThrowsAsync(new Exception("Generation failed"));

        // Act
        var result = await _controller.Generate(request);

        // Assert
        var statusCodeResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, statusCodeResult.StatusCode);
        var count = await _dbContext.GenerationRequests.CountAsync();
        Assert.Equal(1, count);
        var saved = await _dbContext.GenerationRequests.FirstAsync();
        Assert.Equal(GenerationStatus.Failed, saved.Status);
        Assert.Equal("Generation failed", saved.ErrorMessage);
    }
}
