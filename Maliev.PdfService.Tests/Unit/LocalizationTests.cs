using Maliev.PdfService.Api.Models.Data;
using Maliev.PdfService.Api.Services.Layouts;
using QuestPDF.Fluent;
using Xunit;

namespace Maliev.PdfService.Tests.Unit;

/// <summary>
/// Unit tests for localization in PDF documents.
/// </summary>
public class LocalizationTests
{
    static LocalizationTests()
    {
        QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;
    }

    /// <summary>
    /// Tests that InvoiceDocument can be created with localized content.
    /// </summary>
    [Fact]
    public void InvoiceDocument_CanBeCreated()
    {
        // Arrange
        var data = new InvoiceData
        {
            InvoiceNumber = "INV-001",
            Items = new List<InvoiceItemData>
            {
                new InvoiceItemData { Index = 1, Description = "Test Item (ภาษาไทย)", Quantity = 1m, UnitPrice = 100m, LineSubtotal = 100m, LineTotal = 100m }
            }
        };

        // Act
        var document = new InvoiceDocument(data);

        // Assert
        Assert.NotNull(document);
    }
}
