using Maliev.PdfService.Api.Models.Data;
using Maliev.PdfService.Api.Services.Layouts;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace Maliev.PdfService.Tests.Integration;

/// <summary>
/// Integration tests that generate reviewable PDF preview files for each supported template.
/// </summary>
public class PdfPreviewTests
{
    private static readonly string OutputDirectory = Path.Combine(FindRepositoryRoot(), "artifacts", "pdf-previews");

    static PdfPreviewTests()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    /// <summary>
    /// Generates preview PDF files for all supported document templates.
    /// </summary>
    [Fact]
    public void GeneratePreviewPdfs_WritesAllTemplateFiles()
    {
        Directory.CreateDirectory(OutputDirectory);

        foreach (var stalePdf in Directory.EnumerateFiles(OutputDirectory, "*.pdf"))
        {
            File.Delete(stalePdf);
        }

        var generatedFiles = new List<string>();
        foreach (var preview in CreatePreviewDocuments())
        {
            var filePath = Path.Combine(OutputDirectory, preview.FileName);
            var bytes = preview.Document.GeneratePdf();

            File.WriteAllBytes(filePath, bytes);
            generatedFiles.Add(filePath);
            Console.WriteLine($"Generated PDF preview: {filePath}");
        }

        Assert.Equal(5, generatedFiles.Count);
        foreach (var filePath in generatedFiles)
        {
            var fileInfo = new FileInfo(filePath);
            Assert.True(fileInfo.Exists, $"Expected preview PDF to exist: {filePath}");
            Assert.True(fileInfo.Length > 0, $"Expected preview PDF to contain bytes: {filePath}");
        }
    }

    private static IEnumerable<(string FileName, IDocument Document)> CreatePreviewDocuments()
    {
        yield return ("Invoice_Standard.pdf", new InvoiceDocument(new InvoiceData
        {
            InvoiceNumber = "INV-PREVIEW-001",
            Currency = "THB",
            Items =
            [
                new InvoiceItemData { Index = 1, Description = "CNC machined aluminum bracket", Quantity = 2, TotalPrice = 2400 },
                new InvoiceItemData { Index = 2, Description = "3D printed nylon jig - ภาษาไทย", Quantity = 1, TotalPrice = 850 }
            ]
        }));

        yield return ("Quotation_Standard.pdf", new QuotationDocument(new QuotationData
        {
            QuotationNumber = "QUO-PREVIEW-001",
            CustomerName = "MALIEV Preview Customer",
            QuotationDate = new DateTime(2026, 5, 2),
            Currency = "THB",
            TotalAmount = 3250,
            Items =
            [
                new QuotationItemData { Index = 1, Description = "Prototype enclosure", Quantity = 3, UnitPrice = 750, TotalPrice = 2250 },
                new QuotationItemData { Index = 2, Description = "Finishing and inspection", Quantity = 1, UnitPrice = 1000, TotalPrice = 1000 }
            ]
        }));

        yield return ("Receipt_Standard.pdf", new ReceiptDocument(new
        {
            ReceiptNumber = "RCP-PREVIEW-001",
            CustomerName = "MALIEV Preview Customer",
            PaymentMethod = "Bank transfer",
            TotalAmount = 3250,
            Currency = "THB"
        }));

        yield return ("DeliveryNote_Standard.pdf", new DeliveryNoteDocument(new DeliveryNoteData
        {
            DeliveryNoteNumber = "DN-PREVIEW-001",
            OrderNumber = "ORD-PREVIEW-001",
            CustomerName = "MALIEV Preview Customer",
            CustomerAddress = "123 Manufacturing Road, Bangkok 10110",
            DeliveryDate = new DateTime(2026, 5, 2),
            TrackingNumber = "TRACK-PREVIEW-001",
            CarrierName = "MALIEV Logistics",
            DeliveryContact = "Somsak Preview",
            DeliveryPhone = "+66 2 000 0000",
            Notes = "Handle with care. Partial shipment accepted.",
            Items =
            [
                new DeliveryNoteItemData
                {
                    ProductCode = "CNC-BRACKET",
                    ProductName = "Aluminum bracket - ชิ้นงานตัวอย่าง",
                    QuantityOrdered = 10,
                    QuantityManufactured = 10,
                    QuantityDelivered = 8,
                    UnitOfMeasure = "pcs"
                },
                new DeliveryNoteItemData
                {
                    ProductCode = "NYLON-JIG",
                    ProductName = "Nylon inspection jig",
                    QuantityOrdered = 4,
                    QuantityManufactured = 4,
                    QuantityDelivered = 4,
                    UnitOfMeasure = "pcs"
                }
            ]
        }));

        yield return ("FinancialReport_Standard.pdf", new FinancialReportDocument(new
        {
            ReportTitle = "Monthly Manufacturing Summary",
            ReportNumber = "RPT-PREVIEW-001",
            Period = "May 2026",
            TotalRevenue = 150000,
            TotalExpenses = 92000,
            NetProfit = 58000,
            Currency = "THB"
        }));
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "Maliev.PdfService.slnx")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("Could not locate the Maliev.PdfService repository root.");
    }
}
