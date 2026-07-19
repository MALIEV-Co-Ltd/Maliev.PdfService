using Maliev.PdfService.Api.Controllers;
using Maliev.PdfService.Api.Models.Data;
using Maliev.PdfService.Api.Services;
using Maliev.PdfService.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace Maliev.PdfService.Tests.Unit;

/// <summary>
/// Unit tests for blog practical note PDF rendering endpoints.
/// </summary>
public sealed class BlogPracticalNotesControllerTests
{
    /// <summary>
    /// Verifies the endpoint renders a blog practical note through the PDF generator and returns PDF bytes.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Fact]
    public async Task Render_ReturnsPdfFile_FromBlogPracticalNoteDocument()
    {
        // Arrange
        byte[] expectedBytes = [(byte)'%', (byte)'P', (byte)'D', (byte)'F'];
        var generator = new Mock<IPdfGenerator>();
        generator
            .Setup(service => service.GeneratePdfAsync(DocumentType.BlogPracticalNote, It.IsAny<BlogPracticalNoteData>(), null))
            .ReturnsAsync(expectedBytes);
        var controller = new BlogPracticalNotesController(generator.Object);
        var request = new BlogPracticalNoteData
        {
            Slug = "silicone-master-preparation",
            Title = "Silicone casting starts with the master",
            Summary = "The mold repeats the quality and defects of the master pattern.",
            Category = "Casting",
            PublicUrl = "https://www.maliev.com/blog/silicone-master-preparation"
        };

        // Act
        var result = await controller.Render(request);

        // Assert
        var file = Assert.IsType<FileContentResult>(result);
        Assert.Equal("application/pdf", file.ContentType);
        Assert.Equal("silicone-master-preparation-maliev-practical-note.pdf", file.FileDownloadName);
        Assert.Equal(expectedBytes, file.FileContents);
        generator.Verify(
            service => service.GeneratePdfAsync(DocumentType.BlogPracticalNote, It.Is<BlogPracticalNoteData>(data => data.Slug == request.Slug), null),
            Times.Once);
    }
}
