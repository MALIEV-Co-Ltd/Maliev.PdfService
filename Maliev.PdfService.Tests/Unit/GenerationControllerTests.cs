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
using Testcontainers.PostgreSql;

namespace Maliev.PdfService.Tests.Unit;

/// <summary>
/// Unit tests for PDF generation controller behavior and persistence.
/// </summary>
public class GenerationControllerTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _dbContainer = 
                #pragma warning disable CS0618
        new PostgreSqlBuilder().WithImage("postgres:18-alpine")
        .Build();
#pragma warning restore CS0618

    private readonly Mock<IPdfGenerator> _pdfGeneratorMock = new();
    private readonly Mock<IUploadServiceClient> _uploadServiceMock = new();
    private readonly Mock<IPublishEndpoint> _publishEndpointMock = new();
    private readonly Mock<ILogger<GenerationController>> _loggerMock = new();
    private PdfDbContext _dbContext = null!;
    private GenerationController _controller = null!;

    /// <summary>
    /// Starts the PostgreSQL test container and creates a controller under test.
    /// </summary>
    /// <returns>A task that represents the asynchronous initialization operation.</returns>
    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();

        var options = new DbContextOptionsBuilder<PdfDbContext>()
            .UseNpgsql(_dbContainer.GetConnectionString())
            .Options;

        _dbContext = new PdfDbContext(options);
        await _dbContext.Database.EnsureCreatedAsync();

        _controller = new GenerationController(
            _pdfGeneratorMock.Object,
            _uploadServiceMock.Object,
            _dbContext,
            _publishEndpointMock.Object,
            _loggerMock.Object);
    }

    /// <summary>
    /// Disposes the PostgreSQL test container after the test run.
    /// </summary>
    /// <returns>A task that represents the asynchronous disposal operation.</returns>
    public async Task DisposeAsync()
    {
        await _dbContainer.DisposeAsync();
    }

    /// <summary>
    /// Verifies that synchronous generation returns OK and stores a completed request.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
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

    /// <summary>
    /// Verifies that asynchronous generation returns Accepted and stores a pending request.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
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

    /// <summary>
    /// Verifies that synchronous generation failures return an internal server error and store failure details.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
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




