using Maliev.PdfService.Api.Models.Data;
using Maliev.PdfService.Api.Services;
using Maliev.PdfService.Api.Services.Layouts;
using Maliev.PdfService.Domain.Entities;
using Microsoft.AspNetCore.Hosting;
using Moq;
using QuestPDF.Infrastructure;
using Xunit;

namespace Maliev.PdfService.Tests.Unit;

/// <summary>
/// Unit tests for PDF document generation behavior.
/// </summary>
public class PdfGeneratorTests
{
    static PdfGeneratorTests()
    {
        QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;
    }

    private readonly Mock<IDocumentFactory> _documentFactoryMock = new();
    private readonly Mock<IWebHostEnvironment> _envMock = new();
    private readonly PdfGenerator _generator;

    /// <summary>
    /// Initializes a new instance of the <see cref="PdfGeneratorTests"/> class.
    /// </summary>
    public PdfGeneratorTests()
    {
        _generator = new PdfGenerator(_documentFactoryMock.Object, _envMock.Object);
    }

    /// <summary>
    /// Verifies that PDF generation returns non-empty bytes for a valid production request.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Fact]
    public async Task GeneratePdfAsync_ReturnsPdfBytes()
    {
        // Arrange
        var data = new QuotationData { QuotationNumber = "Q-001" };
        _documentFactoryMock.Setup(x => x.CreateDocument(It.IsAny<DocumentType>(), It.IsAny<object>()))
            .Returns(new QuotationDocument(data));

        _envMock.Setup(x => x.EnvironmentName).Returns("Production");

        // Act
        var result = await _generator.GeneratePdfAsync(DocumentType.Quotation, data);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }

    /// <summary>
    /// Verifies that PDF generation completes successfully when the host environment is development.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Fact]
    public async Task GeneratePdfAsync_HandlesDevelopmentEnvironment()
    {
        // Arrange
        var data = new QuotationData { QuotationNumber = "Q-002" };
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
