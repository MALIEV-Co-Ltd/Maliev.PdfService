using Maliev.PdfService.Api.Services;
using Maliev.PdfService.Api.Services.Layouts;
using Maliev.PdfService.Data.Entities;
using Microsoft.AspNetCore.Hosting;
using Moq;
using QuestPDF.Infrastructure;
using Xunit;

namespace Maliev.PdfService.Tests.Unit;

public class PdfGeneratorTests
{
    static PdfGeneratorTests()
    {
        QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;
    }

    private readonly Mock<IDocumentFactory> _documentFactoryMock = new();
    private readonly Mock<IWebHostEnvironment> _envMock = new();
    private readonly PdfGenerator _generator;

    public PdfGeneratorTests()
    {
        _generator = new PdfGenerator(_documentFactoryMock.Object, _envMock.Object);
    }

    [Fact]
    public async Task GeneratePdfAsync_ReturnsPdfBytes()
    {
        // Arrange
        var data = new { };
        _documentFactoryMock.Setup(x => x.CreateDocument(It.IsAny<DocumentType>(), It.IsAny<object>()))
            .Returns(new QuotationDocument(data));

        _envMock.Setup(x => x.EnvironmentName).Returns("Production");

        // Act
        var result = await _generator.GeneratePdfAsync(DocumentType.Quotation, data);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }

    [Fact]
    public async Task GeneratePdfAsync_HandlesDevelopmentEnvironment()
    {
        // Arrange
        var data = new { };
        _documentFactoryMock.Setup(x => x.CreateDocument(It.IsAny<DocumentType>(), It.IsAny<object>()))
            .Returns(new QuotationDocument(data));

        _envMock.Setup(x => x.EnvironmentName).Returns("Development");

        // Act
        var result = await _generator.GeneratePdfAsync(DocumentType.Quotation, data);

        // Assert
        Assert.NotNull(result);
        // Note: ShowInCompanion is commented out or might throw if companion not running,
        // but our current mock setup should allow the code to pass.
    }
}
