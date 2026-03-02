using Maliev.PdfService.Api.Models.Data;
using Maliev.PdfService.Api.Services.Layouts;
using QuestPDF.Fluent;
using Xunit;

namespace Maliev.PdfService.Tests.Unit;

public class LocalizationTests
{
    static LocalizationTests()
    {
        QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;
    }

    [Fact]
    public void InvoiceDocument_CanBeCreated()
    {
        // Arrange
        var data = new InvoiceData
        {
            InvoiceNumber = "INV-001",
            Items = new List<InvoiceItemData>
            {
                new InvoiceItemData { Index = 1, Description = "Test Item (ภาษาไทย)", Quantity = 1.0, TotalPrice = 100.0 }
            }
        };

        // Act
        var document = new InvoiceDocument(data);

        // Assert
        Assert.NotNull(document);
    }
}
