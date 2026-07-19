using System.Buffers.Binary;
using System.IO.Compression;
using System.Reflection;
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
    /// Tests that quotation item manufacturing notes render below the note label.
    /// </summary>
    [Fact]
    public void QuotationDocument_GeneratesPdf_WithSingleManufacturingNote_UsesNoteHeading()
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
                    Notes = "hello test 123",
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
        Assert.Contains("Note:", text, StringComparison.Ordinal);
        Assert.Contains("hello test 123", text, StringComparison.Ordinal);
    }

    /// <summary>
    /// Tests that repeated configuration details are not labeled as manufacturing notes.
    /// </summary>
    [Fact]
    public void QuotationDocument_GeneratesPdf_WithRepeatedDetailsInNotes_LabelsOnlyManufacturingNote()
    {
        // Arrange
        var document = new QuotationDocument(new QuotationData
        {
            QuotationNumber = "Q-NOTE-CONFIG",
            CustomerName = "Acme Thailand",
            Items =
            [
                new QuotationItemData
                {
                    Index = 1,
                    PartName = "systemboard.stl",
                    MaterialName = "ABS",
                    ManufacturingProcess = "3D Printing (FDM)",
                    DetailLines =
                    [
                        "Bounding box: 12 x 12 x 7.16 mm",
                        "Surface finish: As-printed",
                        "Tolerance: FDM Standard +-0.3mm",
                        "Inspection: Standard",
                        "gawewev",
                    ],
                    Notes = "Bounding box: 12 x 12 x 7.16 mm | Surface finish: As-printed | Tolerance: FDM Standard +-0.3mm | Inspection: Standard | gawewev",
                    Quantity = 4,
                    QuantityUnit = "pcs",
                    UnitPrice = 313.04m,
                    LineTotal = 1252.15m
                }
            ],
            Subtotal = 1252.15m,
            TotalAmount = 1252.15m
        });

        // Act
        var pages = ExtractPageText(document.GeneratePdf());
        var text = string.Join(Environment.NewLine, pages);

        // Assert
        Assert.Contains("Bounding box: 12 x 12 x 7.16 mm", text, StringComparison.Ordinal);
        Assert.Contains("Surface finish: As-printed", text, StringComparison.Ordinal);
        Assert.Contains("Tolerance: FDM Standard +-0.3mm", text, StringComparison.Ordinal);
        Assert.Contains("Inspection: Standard", text, StringComparison.Ordinal);
        Assert.Contains("Note:", text, StringComparison.Ordinal);
        Assert.Contains("gawewev", text, StringComparison.Ordinal);
        Assert.DoesNotContain("Note: Bounding box:", text, StringComparison.Ordinal);
        Assert.DoesNotContain("Note: Surface finish:", text, StringComparison.Ordinal);
        Assert.DoesNotContain("Note: Tolerance:", text, StringComparison.Ordinal);
        Assert.DoesNotContain("Note: Inspection:", text, StringComparison.Ordinal);
        Assert.Equal(1, CountOccurrences(text, "gawewev"));
    }

    /// <summary>
    /// Tests that multiple manufacturing note lines share one note heading.
    /// </summary>
    [Fact]
    public void QuotationDocument_GeneratesPdf_WithMultipleManufacturingNotes_UsesSingleNoteHeading()
    {
        // Arrange
        var document = new QuotationDocument(new QuotationData
        {
            QuotationNumber = "Q-MULTI-NOTE",
            CustomerName = "Acme Thailand",
            Items =
            [
                new QuotationItemData
                {
                    Index = 1,
                    PartName = "d11-12.stp",
                    MaterialName = "PLA",
                    ManufacturingProcess = "3D Printing (FDM)",
                    DetailLines =
                    [
                        "Bounding box: 38 x 22 x 38 mm",
                        "Surface finish: Sanded",
                        "Tolerance: FDM Standard +-0.3mm",
                        "Inspection: Standard",
                        "gbnfgc",
                        "bsrtbsrtb",
                        "srtbsrtbst",
                    ],
                    Notes = "Note: gbnfgc\r\nNote: bsrtbsrtb\r\nNote: srtbsrtbst",
                    Quantity = 6,
                    QuantityUnit = "pcs",
                    UnitPrice = 100,
                    LineTotal = 600
                }
            ],
            Subtotal = 600,
            TotalAmount = 600
        });

        // Act
        var pages = ExtractPageText(document.GeneratePdf());
        var text = string.Join(Environment.NewLine, pages);

        // Assert
        Assert.Equal(1, CountOccurrences(text, "Note:"));
        Assert.Equal(1, CountOccurrences(text, "gbnfgc"));
        Assert.Equal(1, CountOccurrences(text, "bsrtbsrtb"));
        Assert.Equal(1, CountOccurrences(text, "srtbsrtbst"));
    }

    /// <summary>
    /// Tests that multi-line note detail entries are not duplicated outside the note block.
    /// </summary>
    [Fact]
    public void QuotationDocument_GeneratesPdf_WithMultilineNoteDetail_RemovesDuplicatedDetailBlock()
    {
        // Arrange
        var document = new QuotationDocument(new QuotationData
        {
            QuotationNumber = "Q-MULTILINE-NOTE",
            CustomerName = "Acme Thailand",
            Items =
            [
                new QuotationItemData
                {
                    Index = 1,
                    PartName = "d11-12.stp",
                    MaterialName = "PLA",
                    ManufacturingProcess = "3D Printing (FDM)",
                    DetailLines =
                    [
                        "Bounding box: 38 x 22 x 38 mm",
                        "Surface finish: Sanded",
                        "Tolerance: FDM Standard +-0.3mm",
                        "Inspection: Standard",
                        "gbnfgc\r\nbsrtbsrtb\r\n\r\nsrtbsrtbst",
                    ],
                    Notes = "Note: gbnfgc\r\nNote: bsrtbsrtb\r\nNote: srtbsrtbst",
                    Quantity = 6,
                    QuantityUnit = "pcs",
                    UnitPrice = 100,
                    LineTotal = 600
                }
            ],
            Subtotal = 600,
            TotalAmount = 600
        });

        // Act
        var pages = ExtractPageText(document.GeneratePdf());
        var text = string.Join(Environment.NewLine, pages);

        // Assert
        Assert.Equal(1, CountOccurrences(text, "Note:"));
        Assert.Equal(1, CountOccurrences(text, "gbnfgc"));
        Assert.Equal(1, CountOccurrences(text, "bsrtbsrtb"));
        Assert.Equal(1, CountOccurrences(text, "srtbsrtbst"));
    }

    /// <summary>
    /// Tests that quotation drawing detail lists are rendered as separate bullet points.
    /// </summary>
    [Fact]
    public void QuotationDocument_GeneratesPdf_WithDrawingDetailsAsBulletPoints()
    {
        // Arrange
        var document = new QuotationDocument(new QuotationData
        {
            QuotationNumber = "Q-DRAWINGS",
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
                        "Tolerance: Medium (ISO 2768-m)",
                        "Drawings: bracket-front.pdf, bracket-side.pdf",
                    ],
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
        Assert.Contains("Drawings:", text, StringComparison.Ordinal);
        Assert.Contains("- bracket-front.pdf", text, StringComparison.Ordinal);
        Assert.Contains("- bracket-side.pdf", text, StringComparison.Ordinal);
        Assert.DoesNotContain("Drawings: bracket-front.pdf, bracket-side.pdf", text, StringComparison.Ordinal);
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
            QuotedByPhone = "+66810000000",
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
        var pages = ExtractPageText(document.GeneratePdf());
        var text = string.Join(Environment.NewLine, pages);

        // Assert
        Assert.Contains("Alex Kim", text, StringComparison.Ordinal);
        Assert.Contains("alex.kim@maliev.com", text, StringComparison.Ordinal);
        Assert.Contains("+66810000000", text, StringComparison.Ordinal);
    }

    /// <summary>
    /// Tests that manual discount descriptions are not rendered as customer-facing discount notes.
    /// </summary>
    [Fact]
    public void QuotationDocument_GeneratesPdf_WithManualDiscount_DoesNotRenderManualDiscountDescription()
    {
        // Arrange
        var document = new QuotationDocument(new QuotationData
        {
            QuotationNumber = "Q-MANUAL-DISCOUNT",
            CustomerName = "Acme Thailand",
            Currency = "THB",
            SubtotalBeforeDiscount = 1000,
            TotalDiscount = 100,
            ManualDiscountAmount = 100,
            Subtotal = 900,
            TotalAmount = 900,
            Discounts =
            [
                new QuotationDiscountData
                {
                    DiscountType = "FixedAmount",
                    DiscountValue = 100,
                    Conditions = "Manual discount"
                }
            ],
            Items =
            [
                new QuotationItemData
                {
                    Index = 1,
                    PartName = "bracket.step",
                    MaterialName = "PLA",
                    ManufacturingProcess = "3D Printing (FDM)",
                    Quantity = 1,
                    UnitPrice = 1000,
                    LineTotal = 1000
                }
            ],
        });

        // Act
        var text = string.Join(Environment.NewLine, ExtractPageText(document.GeneratePdf()));

        // Assert
        Assert.Contains("Discount:", text, StringComparison.Ordinal);
        Assert.DoesNotContain("Manual discount", text, StringComparison.Ordinal);
        Assert.DoesNotContain("FixedAmount: THB 100.00", text, StringComparison.Ordinal);
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
    /// Tests that quotation thumbnail preparation removes edge-connected white PNG backgrounds.
    /// </summary>
    [Fact]
    public void QuotationDocument_PreparesThumbnailBytes_RemovesWhitePngBackground()
    {
        // Arrange
        var png = CreateRgbaPng(3, 3, (x, y) => x == 1 && y == 1
            ? ((byte)120, (byte)120, (byte)120, (byte)255)
            : ((byte)255, (byte)255, (byte)255, (byte)255));

        var prepareMethod = typeof(QuotationDocument).GetMethod(
            "PrepareThumbnailBytes",
            BindingFlags.Static | BindingFlags.NonPublic);

        // Act
        Assert.NotNull(prepareMethod);
        var prepared = Assert.IsType<byte[]>(prepareMethod.Invoke(null, [png]));
        var rgba = DecodeTestRgbaPng(prepared, out var width, out var height);

        // Assert
        Assert.Equal(3, width);
        Assert.Equal(3, height);
        Assert.Equal(0, rgba[3]);

        var centerPixel = ((1 * width) + 1) * 4;
        Assert.Equal(120, rgba[centerPixel]);
        Assert.Equal(120, rgba[centerPixel + 1]);
        Assert.Equal(120, rgba[centerPixel + 2]);
        Assert.Equal(255, rgba[centerPixel + 3]);
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

    /// <summary>
    /// Tests that CommerceBomDocument generates a PDF with BOM items.
    /// </summary>
    [Fact]
    public void CommerceBomDocument_GeneratesPdf_WithBomItems()
    {
        // Arrange
        var document = new CommerceBomDocument(new CommerceBomData
        {
            ProductTitle = "Pneumatic Injection Molding Machine 30g",
            ProductHandle = "pneumatic-injection-molding-machine-30g",
            Brand = "MALIEV",
            ProductType = "Injection molding machine",
            Status = "Draft",
            Currency = "THB",
            TotalCost = 4250m,
            SourcingTimeDays = 14,
            Items =
            [
                new CommerceBomItemData
                {
                    Index = 1,
                    ItemName = "Pneumatic cylinder",
                    PartNumber = "MLV-PIMM-30G-CYL",
                    AssemblyName = "Injection unit",
                    SubassemblyName = "Pneumatic drive",
                    ImageUrl = "https://assets.maliev.com/bom/pimm-cylinder.png",
                    DrawingUrl = "https://assets.maliev.com/drawings/pimm-cylinder.pdf",
                    SupplierName = "MALIEV approved supplier",
                    SupplierUrl = "https://supplier.example/pimm-cylinder",
                    Specification = "30g shot size",
                    Quantity = 1,
                    Unit = "pcs",
                    UnitCost = 4250m,
                    Currency = "THB",
                    LineTotal = 4250m,
                    LeadTimeDays = 10,
                    SourcingTimeDays = 4,
                    Notes = "Verify stroke length before purchase."
                }
            ]
        });

        // Act
        var pdf = document.GeneratePdf();
        var text = string.Join(Environment.NewLine, ExtractPageText(pdf));

        // Assert
        Assert.NotEmpty(pdf);
        Assert.Contains("PRODUCT BOM", text, StringComparison.Ordinal);
        Assert.Contains("Pneumatic cylinder", text, StringComparison.Ordinal);
        Assert.Contains("MLV-PIMM-30G-CYL", text, StringComparison.Ordinal);
        Assert.Contains("Injection unit", text, StringComparison.Ordinal);
        Assert.Contains("Pneumatic drive", text, StringComparison.Ordinal);
        Assert.Contains("MALIEV approved supplier", text, StringComparison.Ordinal);
        Assert.Contains("supplier.example/pimm-cylinder", text, StringComparison.Ordinal);
        Assert.Contains("drawings/pimm-cylinder.pdf", text, StringComparison.Ordinal);
        Assert.Contains("Lead time: 10 days", text, StringComparison.Ordinal);
        Assert.Contains("Sourcing time: 14 days", text, StringComparison.Ordinal);
        Assert.Contains("Verify stroke length before purchase.", text, StringComparison.Ordinal);
    }

    /// <summary>
    /// Tests that a blog practical note PDF contains printable branding, content, and CTA URLs.
    /// </summary>
    [Fact]
    public void BlogPracticalNoteDocument_GeneratesPdf_WithNavigationAndCtas()
    {
        // Arrange
        var document = new BlogPracticalNoteDocument(new BlogPracticalNoteData
        {
            Slug = "silicone-master-preparation",
            Title = "Silicone casting starts with the master",
            Summary = "The mold repeats the quality and defects of the master pattern.",
            Category = "Casting",
            PublicUrl = "https://www.maliev.com/blog/silicone-master-preparation",
            CoverImage = new BlogPracticalNoteImageData
            {
                Alt = "Silicone casting master",
                Caption = "Start with the master.",
                Bytes = CreateRgbaPng(2, 2, (_, _) => (byte.MaxValue, (byte)245, (byte)225, byte.MaxValue))
            },
            Sections =
            [
                new BlogPracticalNoteSectionData
                {
                    Title = "Review the mold behavior before the part",
                    Body = "Casting and molding decisions are affected by draft, wall thickness, gates, vents, shrinkage, bubbles, and master quality.",
                    Items =
                    [
                        "A better master makes a better batch.",
                        "Fix cosmetic defects before molding."
                    ],
                    Image = new BlogPracticalNoteImageData
                    {
                        Alt = "Master part",
                        Caption = "The master defines the batch.",
                        Bytes = CreateRgbaPng(2, 2, (_, _) => ((byte)80, (byte)120, (byte)180, byte.MaxValue))
                    }
                }
            ],
            Takeaways =
            [
                "A better master makes a better batch.",
                "Parting lines should be chosen intentionally."
            ]
        });

        // Act
        var pdf = document.GeneratePdf();
        var text = string.Join(Environment.NewLine, ExtractPageText(pdf));

        // Assert
        Assert.NotEmpty(pdf);
        Assert.Contains("MALIEV", text, StringComparison.Ordinal);
        Assert.Contains("Silicone", text, StringComparison.Ordinal);
        Assert.Contains("master", text, StringComparison.Ordinal);
        Assert.Contains("Read before quoting", text, StringComparison.Ordinal);
        Assert.Contains("Before you upload", text, StringComparison.Ordinal);
        Assert.Contains("Get part price", text, StringComparison.Ordinal);
        Assert.Contains("Compare materials", text, StringComparison.Ordinal);
        Assert.Contains("https://quote.maliev.com/projects/new", text, StringComparison.Ordinal);
        Assert.Contains("https://www.maliev.com/materials", text, StringComparison.Ordinal);
        Assert.Contains("Social media", text, StringComparison.Ordinal);
        Assert.Contains("LINE", text, StringComparison.Ordinal);
        Assert.Contains("Official Account @maliev", text, StringComparison.Ordinal);
        Assert.Contains("Facebook", text, StringComparison.Ordinal);
        Assert.Contains("YouTube", text, StringComparison.Ordinal);
        Assert.Contains("Instagram", text, StringComparison.Ordinal);
    }

    /// <summary>
    /// Tests that a material datasheet PDF contains printable branding, specifications, guidance, and CTA URLs.
    /// </summary>
    [Fact]
    public void MaterialDatasheetDocument_GeneratesPdf_WithSpecificationsAndCtas()
    {
        // Arrange
        var document = new MaterialDatasheetDocument(new MaterialDatasheetData
        {
            Slug = "pa12-nylon",
            Name = "PA12 nylon",
            CategoryLabel = "Powder-bed nylon",
            ProcessLabel = "MJF / SLS",
            Family = "Powder-bed engineering nylon",
            PublicUrl = "https://www.maliev.com/materials/pa12-nylon",
            Disclaimer = "Typical values - final grade confirmed at quotation.",
            CoverImage = new MaterialDatasheetImageData
            {
                Alt = "PA12 nylon part",
                Caption = "Powder-bed nylon housing.",
                Bytes = CreateRgbaPng(2, 2, (_, _) => (byte.MaxValue, (byte)245, (byte)225, byte.MaxValue))
            },
            Specs =
            [
                new MaterialDatasheetSpecData { Label = "Tensile strength", Value = "~48 MPa", Note = "ISO 527 (typical)" },
                new MaterialDatasheetSpecData { Label = "Elongation at break", Value = "~15 %" }
            ],
            Bands =
            [
                new MaterialDatasheetBandData { Label = "Best fit", Value = "Functional prototypes, housings, clips" },
                new MaterialDatasheetBandData { Label = "Heat", Value = "Up to ~80 C continuous" }
            ],
            Pros = "Tough, isotropic, good chemical resistance.",
            Cons = "Porous as-printed; dye for color consistency."
        });

        // Act
        var pdf = document.GeneratePdf();
        var text = string.Join(Environment.NewLine, ExtractPageText(pdf));

        // Assert
        Assert.NotEmpty(pdf);
        Assert.Contains("MALIEV", text, StringComparison.Ordinal);
        Assert.Contains("PA12 nylon", text, StringComparison.Ordinal);
        Assert.Contains("Material Datasheet", text, StringComparison.Ordinal);
        Assert.Contains("Specifications", text, StringComparison.Ordinal);
        Assert.Contains("Tensile strength", text, StringComparison.Ordinal);
        Assert.Contains("~48 MPa", text, StringComparison.Ordinal);
        Assert.Contains("ISO 527 (typical)", text, StringComparison.Ordinal);
        Assert.Contains("Selection guidance", text, StringComparison.Ordinal);
        Assert.Contains("Best fit", text, StringComparison.Ordinal);
        Assert.Contains("Pros", text, StringComparison.Ordinal);
        Assert.Contains("Watch / check before use", text, StringComparison.Ordinal);
        Assert.Contains("Typical values - final grade confirmed at quotation.", text, StringComparison.Ordinal);
        Assert.Contains("https://www.maliev.com/materials/pa12-nylon", text, StringComparison.Ordinal);
        Assert.Contains("Get part price", text, StringComparison.Ordinal);
        Assert.Contains("Compare materials", text, StringComparison.Ordinal);
        Assert.Contains("https://quote.maliev.com/projects/new", text, StringComparison.Ordinal);
        Assert.Contains("https://www.maliev.com/materials", text, StringComparison.Ordinal);
        Assert.Contains("Social media", text, StringComparison.Ordinal);
    }

    /// <summary>
    /// Tests that a material datasheet PDF renders for Thai-culture content via the bilingual font fallback.
    /// </summary>
    [Fact]
    public void MaterialDatasheetDocument_GeneratesPdf_WithThaiCulture()
    {
        // Arrange
        var document = new MaterialDatasheetDocument(new MaterialDatasheetData
        {
            Slug = "pa12-nylon",
            CultureName = "th-TH",
            Name = "ไนลอน PA12",
            CategoryLabel = "ไนลอนเพาเดอร์เบด",
            ProcessLabel = "MJF / SLS",
            Family = "ไนลอนวิศวกรรมแบบเพาเดอร์เบด",
            PublicUrl = "https://www.maliev.com/th/materials/pa12-nylon",
            Disclaimer = "ค่าทั่วไป - ยืนยันเกรดสุดท้ายเมื่อเสนอราคา",
            Specs =
            [
                new MaterialDatasheetSpecData { Label = "ความแข็งแรงดึง", Value = "~48 MPa", Note = "ISO 527" }
            ],
            Bands =
            [
                new MaterialDatasheetBandData { Label = "เหมาะสำหรับ", Value = "ต้นแบบใช้งานจริง ชิ้นส่วนเปลือก" }
            ],
            Pros = "เหนียว ทนสารเคมีได้ดี",
            Cons = "ผิวพรุนหลังพิมพ์ ควรย้อมสีเพื่อความสม่ำเสมอ"
        });

        // Act
        var pdf = document.GeneratePdf();

        // Assert
        Assert.NotNull(pdf);
        Assert.NotEmpty(pdf);
    }

    /// <summary>
    /// Tests that Make Studio chat transcripts render as a timeline with customer-safe markdown text.
    /// </summary>
    [Fact]
    public void ChatTranscriptDocument_RendersTimelineMarkdownAndOmitsToolTraces()
    {
        // Arrange
        var document = new ChatTranscriptDocument(new ChatTranscriptData
        {
            SessionId = "6759e7c8-757d-47db-81c1-9e8b85842fa6",
            CustomerName = "Nattapong",
            Language = "en",
            GeneratedAt = DateTimeOffset.Parse("2026-07-08T01:43:00+07:00"),
            Messages =
            [
                new ChatMessageData
                {
                    Role = "user",
                    Content = "Yes, please continue.",
                    Timestamp = DateTimeOffset.Parse("2026-07-08T08:43:20+07:00")
                },
                new ChatMessageData
                {
                    Role = "assistant",
                    Content = """
Tool Call: tools.quote_acknowledge_dfm_findings
{
  "findings_acknowledged": "All findings are acknowledged."
}
Tool Result: Success

**Acknowledged**
- I will continue.
- I will prepare the quotation.
""",
                    Timestamp = DateTimeOffset.Parse("2026-07-08T08:43:22+07:00")
                }
            ]
        });

        // Act
        var text = string.Join(Environment.NewLine, ExtractPageText(document.GeneratePdf()));

        // Assert
        Assert.Contains("Chat Transcript", text, StringComparison.Ordinal);
        Assert.Contains("Make Studio Customer Assistant", text, StringComparison.Ordinal);
        Assert.Contains("Nattapong", text, StringComparison.Ordinal);
        Assert.Contains("MALIEV Assistant", text, StringComparison.Ordinal);
        Assert.Contains("Acknowledged", text, StringComparison.Ordinal);
        Assert.Contains("I will continue.", text, StringComparison.Ordinal);
        Assert.DoesNotContain("Tool Call:", text, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Tool Result:", text, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("tools.quote_acknowledge_dfm_findings", text, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("**Acknowledged**", text, StringComparison.Ordinal);
    }

    private static IReadOnlyList<string> ExtractPageText(byte[] pdf)
    {
        using var stream = new MemoryStream(pdf);
        using var document = PdfDocument.Open(stream);

        return document.GetPages().Select(page => page.Text).ToList();
    }

    private static int CountOccurrences(string value, string searchText)
    {
        var count = 0;
        var startIndex = 0;
        while ((startIndex = value.IndexOf(searchText, startIndex, StringComparison.Ordinal)) >= 0)
        {
            count++;
            startIndex += searchText.Length;
        }

        return count;
    }

    private static byte[] CreateRgbaPng(int width, int height, Func<int, int, (byte Red, byte Green, byte Blue, byte Alpha)> pixelFactory)
    {
        using var imageData = new MemoryStream();
        using (var zlib = new ZLibStream(imageData, CompressionLevel.Optimal, leaveOpen: true))
        {
            for (var y = 0; y < height; y++)
            {
                zlib.WriteByte(0);
                for (var x = 0; x < width; x++)
                {
                    var pixel = pixelFactory(x, y);
                    zlib.WriteByte(pixel.Red);
                    zlib.WriteByte(pixel.Green);
                    zlib.WriteByte(pixel.Blue);
                    zlib.WriteByte(pixel.Alpha);
                }
            }
        }

        using var png = new MemoryStream();
        png.Write([137, 80, 78, 71, 13, 10, 26, 10]);

        Span<byte> header = stackalloc byte[13];
        BinaryPrimitives.WriteInt32BigEndian(header, width);
        BinaryPrimitives.WriteInt32BigEndian(header[4..], height);
        header[8] = 8;
        header[9] = 6;
        WriteTestPngChunk(png, "IHDR"u8, header);
        WriteTestPngChunk(png, "IDAT"u8, imageData.ToArray());
        WriteTestPngChunk(png, "IEND"u8, []);

        return png.ToArray();
    }

    private static byte[] DecodeTestRgbaPng(byte[] png, out int width, out int height)
    {
        width = BinaryPrimitives.ReadInt32BigEndian(png.AsSpan(16, 4));
        height = BinaryPrimitives.ReadInt32BigEndian(png.AsSpan(20, 4));

        using var idat = new MemoryStream();
        var offset = 8;
        while (offset < png.Length)
        {
            var length = BinaryPrimitives.ReadInt32BigEndian(png.AsSpan(offset, 4));
            var type = png.AsSpan(offset + 4, 4);
            if (type.SequenceEqual("IDAT"u8))
                idat.Write(png, offset + 8, length);

            if (type.SequenceEqual("IEND"u8))
                break;

            offset += length + 12;
        }

        idat.Position = 0;
        using var zlib = new ZLibStream(idat, CompressionMode.Decompress);
        using var decompressed = new MemoryStream();
        zlib.CopyTo(decompressed);

        var bytes = decompressed.ToArray();
        var rgba = new byte[width * height * 4];
        var sourceOffset = 0;
        var targetOffset = 0;
        for (var y = 0; y < height; y++)
        {
            Assert.Equal(0, bytes[sourceOffset++]);
            Array.Copy(bytes, sourceOffset, rgba, targetOffset, width * 4);
            sourceOffset += width * 4;
            targetOffset += width * 4;
        }

        return rgba;
    }

    private static void WriteTestPngChunk(Stream stream, ReadOnlySpan<byte> type, ReadOnlySpan<byte> data)
    {
        Span<byte> length = stackalloc byte[4];
        BinaryPrimitives.WriteInt32BigEndian(length, data.Length);
        stream.Write(length);
        stream.Write(type);
        stream.Write(data);

        using var crcInput = new MemoryStream();
        crcInput.Write(type);
        crcInput.Write(data);

        Span<byte> crc = stackalloc byte[4];
        BinaryPrimitives.WriteUInt32BigEndian(crc, PngCrc32(crcInput.ToArray()));
        stream.Write(crc);
    }

    private static uint PngCrc32(byte[] bytes)
    {
        var crc = 0xffffffffu;
        foreach (var value in bytes)
        {
            crc ^= value;
            for (var bit = 0; bit < 8; bit++)
                crc = (crc & 1) == 1 ? (crc >> 1) ^ 0xedb88320u : crc >> 1;
        }

        return crc ^ 0xffffffffu;
    }
}
