using Maliev.PdfService.Api.Controllers;
using Maliev.PdfService.Api.Models.Data;
using Maliev.PdfService.Api.Services;
using Maliev.PdfService.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace Maliev.PdfService.Tests.Unit;

/// <summary>
/// Unit tests for material datasheet PDF rendering endpoints.
/// </summary>
public sealed class MaterialDatasheetsControllerTests
{
    /// <summary>
    /// Verifies the endpoint renders a material datasheet through the PDF generator and returns PDF bytes.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Fact]
    public async Task Render_ReturnsPdfFile_FromMaterialDatasheetDocument()
    {
        // Arrange
        byte[] expectedBytes = [(byte)'%', (byte)'P', (byte)'D', (byte)'F'];
        var generator = new Mock<IPdfGenerator>();
        generator
            .Setup(service => service.GeneratePdfAsync(DocumentType.MaterialDatasheet, It.IsAny<MaterialDatasheetData>(), null))
            .ReturnsAsync(expectedBytes);
        var controller = new MaterialDatasheetsController(generator.Object);
        var request = new MaterialDatasheetData
        {
            Slug = "pa12-nylon",
            Name = "PA12 nylon",
            CategoryLabel = "Powder-bed nylon",
            ProcessLabel = "MJF / SLS",
            Family = "Powder-bed engineering nylon",
            PublicUrl = "https://www.maliev.com/materials/pa12-nylon",
            Disclaimer = "Typical values - final grade confirmed at quotation."
        };

        // Act
        var result = await controller.Render(request);

        // Assert
        var file = Assert.IsType<FileContentResult>(result);
        Assert.Equal("application/pdf", file.ContentType);
        Assert.Equal("pa12-nylon-maliev-material-datasheet.pdf", file.FileDownloadName);
        Assert.Equal(expectedBytes, file.FileContents);
        generator.Verify(
            service => service.GeneratePdfAsync(DocumentType.MaterialDatasheet, It.Is<MaterialDatasheetData>(data => data.Slug == request.Slug), null),
            Times.Once);
    }
}
