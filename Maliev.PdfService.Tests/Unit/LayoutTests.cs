using Maliev.PdfService.Api.Models.Data;
using Maliev.PdfService.Api.Services.Layouts;
using QuestPDF.Fluent;
using Xunit;

namespace Maliev.PdfService.Tests.Unit;

/// <summary>
/// Unit tests that validate QuestPDF layout documents can render PDF bytes.
/// </summary>
public class LayoutTests
{
    static LayoutTests()
    {
        QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;
    }

    /// <summary>
    /// Verifies that invoice layout generation succeeds when the invoice has no line items.
    /// </summary>
    [Fact]
    public void InvoiceDocument_GeneratesPdf_WithEmptyItems()
    {
        // Arrange
        var data = new InvoiceData
        {
            InvoiceNumber = "INV-EMPTY",
            Items = new List<InvoiceItemData>()
        };
        var document = new InvoiceDocument(data);

        // Act
        var pdf = document.GeneratePdf();

        // Assert
        Assert.NotNull(pdf);
        Assert.NotEmpty(pdf);
    }

    /// <summary>
    /// Verifies that invoice layout generation succeeds when the invoice has many line items.
    /// </summary>
    [Fact]
    public void InvoiceDocument_GeneratesPdf_WithManyItems()
    {
        // Arrange
        var data = new InvoiceData
        {
            InvoiceNumber = "INV-MANY",
            Items = Enumerable.Range(1, 100).Select(i => new InvoiceItemData
            {
                Index = i,
                Description = $"Item {i}",
                Quantity = 1,
                TotalPrice = i * 10
            }).ToList()
        };
        var document = new InvoiceDocument(data);

        // Act
        var pdf = document.GeneratePdf();

        // Assert
        Assert.NotNull(pdf);
        Assert.NotEmpty(pdf);
    }

    /// <summary>
    /// Verifies that quotation layout generation produces PDF bytes.
    /// </summary>
    [Fact]
    public void QuotationDocument_GeneratesPdf()
    {
        // Arrange
        var document = new QuotationDocument(new QuotationData { QuotationNumber = "Q-001" });

        // Act
        var pdf = document.GeneratePdf();

        // Assert
        Assert.NotNull(pdf);
        Assert.NotEmpty(pdf);
    }

    /// <summary>
    /// Verifies that receipt layout generation produces PDF bytes.
    /// </summary>
    [Fact]
    public void ReceiptDocument_GeneratesPdf()
    {
        // Arrange
        var document = new ReceiptDocument(new { });

        // Act
        var pdf = document.GeneratePdf();

        // Assert
        Assert.NotNull(pdf);
        Assert.NotEmpty(pdf);
    }

    /// <summary>
    /// Verifies that financial report layout generation produces PDF bytes.
    /// </summary>
    [Fact]
    public void FinancialReportDocument_GeneratesPdf()
    {
        // Arrange
        var document = new FinancialReportDocument(new { });

        // Act
        var pdf = document.GeneratePdf();

        // Assert
        Assert.NotNull(pdf);
        Assert.NotEmpty(pdf);
    }
}
