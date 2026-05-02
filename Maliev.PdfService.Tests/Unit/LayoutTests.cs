using Maliev.PdfService.Api.Models.Data;
using Maliev.PdfService.Api.Services.Layouts;
using QuestPDF.Fluent;
using Xunit;

namespace Maliev.PdfService.Tests.Unit;

/// <summary>
/// Unit tests for PDF layout documents.
/// </summary>
public class LayoutTests
{
    static LayoutTests()
    {
        QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;
    }

    /// <summary>
    /// Tests that InvoiceDocument generates a PDF with empty items.
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
    /// Tests that InvoiceDocument generates a PDF with many items.
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
                UnitPrice = i * 10m,
                LineSubtotal = i * 10m,
                LineTotal = i * 10m
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
    /// Tests that QuotationDocument generates a PDF.
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
    /// Tests that ReceiptDocument generates a PDF.
    /// </summary>
    [Fact]
    public void ReceiptDocument_GeneratesPdf()
    {
        // Arrange
        var data = new ReceiptData
        {
            ReceiptNumber = "RCP-001",
            ReceiptDate = DateTime.UtcNow,
            CustomerName = "Test Customer",
            PaymentMethod = "Cash",
            Items = new List<ReceiptItemData>
            {
                new() { Index = 1, Description = "Product A", Quantity = 2, UnitPrice = 100, TotalPrice = 200 }
            },
            Subtotal = 200,
            TaxAmount = 14,
            TotalAmount = 214,
            CompanyName = "Test Company"
        };
        var document = new ReceiptDocument(data);

        // Act
        var pdf = document.GeneratePdf();

        // Assert
        Assert.NotNull(pdf);
        Assert.NotEmpty(pdf);
    }

    /// <summary>
    /// Tests that FinancialReportDocument generates a PDF.
    /// </summary>
    [Fact]
    public void FinancialReportDocument_GeneratesPdf()
    {
        // Arrange
        var data = new FinancialReportData
        {
            ReportTitle = "Monthly Report",
            ReportNumber = "RPT-001",
            ReportDate = DateTime.UtcNow,
            PeriodStart = DateTime.UtcNow.AddMonths(-1),
            PeriodEnd = DateTime.UtcNow,
            CompanyName = "Test Company",
            Sections = new List<ReportSection>
            {
                new()
                {
                    SectionTitle = "Revenue",
                    LineItems = new List<ReportLineItem>
                    {
                        new() { Description = "Sales", Amount = 100000 },
                        new() { Description = "Services", Amount = 50000 }
                    },
                    SectionTotal = 150000
                }
            },
            TotalRevenue = 150000,
            TotalExpenses = 100000,
            NetProfit = 50000
        };
        var document = new FinancialReportDocument(data);

        // Act
        var pdf = document.GeneratePdf();

        // Assert
        Assert.NotNull(pdf);
        Assert.NotEmpty(pdf);
    }
}
