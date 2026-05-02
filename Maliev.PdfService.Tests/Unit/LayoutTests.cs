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
    /// Tests that QuotationDocument supports complete customer and line item details.
    /// </summary>
    [Fact]
    public void QuotationDocument_GeneratesPdf_WithCompleteQuoteDetails()
    {
        // Arrange
        var document = new QuotationDocument(new QuotationData
        {
            QuotationNumber = "Q-COMPLETE",
            CustomerName = "Acme Thailand",
            CustomerType = "Corporate",
            CustomerTaxId = "0105559999999",
            ContactPerson = "Jane Buyer",
            BillingAddress = "88 Billing Road, Bangkok 10110",
            ShippingAddress = "99 Shipping Road, Nonthaburi 11120",
            Items =
            [
                new QuotationItemData
                {
                    Index = 1,
                    MaterialName = "PLA - bracket.step",
                    ManufacturingProcess = "3D Printing (FDM)",
                    Quantity = 2,
                    UnitPrice = 100,
                    LineTotal = 200,
                    Notes = "Finish: As-printed | Tolerance: FDM Standard +-0.3mm | Color: Black | Drawing: bracket-drawing.pdf"
                }
            ],
            Subtotal = 200,
            TaxAmount = 14,
            TotalAmount = 214,
            DeliveryExpectations = "7 business days after order confirmation",
            SpecialTerms = "Prices are indicative until project review is completed.",
        });

        // Act
        var pdf = document.GeneratePdf();

        // Assert
        Assert.NotNull(pdf);
        Assert.NotEmpty(pdf);
    }

    /// <summary>
    /// Tests that QuotationDocument loads embedded layout resources independent of the current working directory.
    /// </summary>
    [Fact]
    public void QuotationDocument_GeneratesPdf_WhenCurrentDirectoryDoesNotContainResources()
    {
        // Arrange
        var originalDirectory = Directory.GetCurrentDirectory();
        var temporaryDirectory = Path.Combine(Path.GetTempPath(), $"pdf-layout-{Guid.NewGuid():N}");
        Directory.CreateDirectory(temporaryDirectory);

        try
        {
            Directory.SetCurrentDirectory(temporaryDirectory);
            var document = new QuotationDocument(new QuotationData
            {
                QuotationNumber = "Q-RESOURCE",
                CustomerName = "Customer",
                Items =
                [
                    new QuotationItemData
                    {
                        Index = 1,
                        MaterialName = "PLA - bracket.step",
                        ManufacturingProcess = "FDM",
                        Quantity = 2,
                        UnitPrice = 100,
                        LineTotal = 200
                    }
                ],
                Subtotal = 200,
                TotalAmount = 200
            });

            // Act
            var pdf = document.GeneratePdf();

            // Assert
            Assert.NotNull(pdf);
            Assert.NotEmpty(pdf);
        }
        finally
        {
            Directory.SetCurrentDirectory(originalDirectory);
            Directory.Delete(temporaryDirectory, recursive: true);
        }
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
