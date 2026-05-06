using Maliev.PdfService.Api.Models.Data;
using Maliev.PdfService.Api.Services.Layouts;
using QuestPDF.Fluent;
using UglyToad.PdfPig;
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
    /// Tests that the quotation terms heading is kept on the same page as the terms content.
    /// </summary>
    [Fact]
    public void QuotationDocument_KeepsTermsHeadingWithTermsContent()
    {
        // Arrange
        const string terms = "test term";
        var document = new QuotationDocument(new QuotationData
        {
            QuotationNumber = "Q-KEEP-TOGETHER",
            CustomerName = "Acme Thailand",
            Items = Enumerable.Range(1, 7).Select(i => new QuotationItemData
            {
                Index = i,
                PartName = $"d18-19-{i}.stp",
                MaterialName = "Aluminum 6061-T6",
                ManufacturingProcess = "CNC Milling",
                Quantity = 4,
                QuantityUnit = "pcs",
                UnitPrice = 2500,
                LineTotal = 10000,
                Notes = "Bounding box: 43 x 22 x 43 mm | Surface finish: As-machined | Tolerance: Medium (ISO 2768-m) | Surface roughness: Ra 3.2 um | Inspection: Standard"
            }).ToList(),
            Subtotal = 50000,
            ManualDiscountAmount = 300,
            ShippingCost = 350,
            TaxAmount = 3503.50m,
            TotalAmount = 53553.50m,
            DeliveryExpectations = "Standard: 6 - 9 business days after order confirmation",
            SpecialTerms = terms,
        });

        // Act
        var pages = ExtractPageText(document.GeneratePdf());

        // Assert
        Assert.Contains(pages, page => page.Contains("/ TERMS", StringComparison.Ordinal) && page.Contains(terms, StringComparison.Ordinal));
        Assert.DoesNotContain(pages, page => page.Contains("/ TERMS", StringComparison.Ordinal) && !page.Contains(terms, StringComparison.Ordinal));
    }

    /// <summary>
    /// Tests that quotation item manufacturing notes are labeled separately from configuration details.
    /// </summary>
    [Fact]
    public void QuotationDocument_GeneratesPdf_WithLabeledManufacturingNote()
    {
        // Arrange
        var document = new QuotationDocument(new QuotationData
        {
            QuotationNumber = "Q-NOTE",
            CustomerName = "Acme Thailand",
            Items =
            [
                new QuotationItemData
                {
                    Index = 1,
                    PartName = "bracket.step",
                    MaterialName = "Aluminum 6061-T6",
                    ManufacturingProcess = "CNC Milling",
                    DetailLines =
                    [
                        "Bounding box: 43 x 22 x 43 mm",
                        "Tolerance: Medium (ISO 2768-m)",
                    ],
                    Notes = "test 13",
                    Quantity = 1,
                    QuantityUnit = "pcs",
                    UnitPrice = 100,
                    LineTotal = 100
                }
            ],
            Subtotal = 100,
            TotalAmount = 100
        });

        // Act
        var pages = ExtractPageText(document.GeneratePdf());
        var text = string.Join(Environment.NewLine, pages);

        // Assert
        Assert.Contains("Note: test 13", text, StringComparison.Ordinal);
    }

    /// <summary>
    /// Tests that QuotationDocument generates the compact summary when shipping and discount are zero and shipping address is omitted.
    /// </summary>
    [Fact]
    public void QuotationDocument_GeneratesPdf_WithBillingOnlyAddressAndZeroAdjustments()
    {
        // Arrange
        var document = new QuotationDocument(new QuotationData
        {
            QuotationNumber = "Q-BILLING-ONLY",
            CustomerName = "Acme Thailand",
            CustomerType = "Corporate",
            BillingAddressLines =
            [
                "88 Billing Road",
                "Bangkok 10110",
            ],
            Items =
            [
                new QuotationItemData
                {
                    Index = 1,
                    PartName = "bracket.step",
                    MaterialName = "Delrin / POM",
                    ManufacturingProcess = "CNC Milling",
                    Quantity = 1,
                    QuantityUnit = "pcs",
                    UnitPrice = 100,
                    LineTotal = 100
                }
            ],
            SubtotalBeforeDiscount = 100,
            ManualDiscountAmount = 0,
            ShippingCost = 0,
            Subtotal = 100,
            TaxAmount = 7,
            TotalAmount = 107,
            DeliveryExpectations = "Standard: 6 - 9 business days after order confirmation",
        });

        // Act
        var pdf = document.GeneratePdf();

        // Assert
        Assert.NotNull(pdf);
        Assert.NotEmpty(pdf);
    }

    /// <summary>
    /// Tests that QuotationDocument generates a PDF with automatic quoted-by metadata.
    /// </summary>
    [Fact]
    public void QuotationDocument_GeneratesPdf_WithQuotedByMetadata()
    {
        // Arrange
        var document = new QuotationDocument(new QuotationData
        {
            QuotationNumber = "Q-QUOTED-BY",
            CustomerName = "Acme Thailand",
            QuotedByName = "Alex Kim",
            QuotedByEmail = "alex.kim@maliev.com",
            QuotedAt = new DateTime(2026, 5, 3, 8, 30, 0, DateTimeKind.Utc),
            Items =
            [
                new QuotationItemData
                {
                    Index = 1,
                    PartName = "bracket.step",
                    MaterialName = "PLA",
                    ManufacturingProcess = "3D Printing (FDM)",
                    Quantity = 1,
                    UnitPrice = 100,
                    LineTotal = 100
                }
            ],
            Subtotal = 100,
            TaxAmount = 7,
            TotalAmount = 107
        });

        // Act
        var pdf = document.GeneratePdf();

        // Assert
        Assert.NotNull(pdf);
        Assert.NotEmpty(pdf);
    }

    /// <summary>
    /// Tests that unavailable remote quotation thumbnails are skipped instead of failing PDF generation.
    /// </summary>
    [Fact]
    public void QuotationDocument_GeneratesPdf_WhenRemoteThumbnailCannotBeLoaded()
    {
        // Arrange
        var document = new QuotationDocument(new QuotationData
        {
            QuotationNumber = "Q-REMOTE-THUMBNAIL",
            CustomerName = "Acme Thailand",
            Items =
            [
                new QuotationItemData
                {
                    Index = 1,
                    MaterialName = "PLA",
                    PartName = "bracket.step",
                    ManufacturingProcess = "3D Printing (FDM)",
                    ThumbnailUrl = "https://127.0.0.1:9/missing-thumbnail.webp",
                    Quantity = 1,
                    UnitPrice = 100,
                    LineTotal = 100
                }
            ],
            Subtotal = 100,
            TotalAmount = 100
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

    private static IReadOnlyList<string> ExtractPageText(byte[] pdf)
    {
        using var stream = new MemoryStream(pdf);
        using var document = PdfDocument.Open(stream);

        return document.GetPages().Select(page => page.Text).ToList();
    }
}
