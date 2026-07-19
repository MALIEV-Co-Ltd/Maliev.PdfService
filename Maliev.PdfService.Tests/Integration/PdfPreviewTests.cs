using Maliev.PdfService.Api.Models.Data;
using Maliev.PdfService.Api.Services.Layouts;
using Maliev.PdfService.Tests.TestData;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using Xunit;

namespace Maliev.PdfService.Tests.Integration;

/// <summary>
/// Preview tests that generate real PDFs for visual inspection.
/// </summary>
[Trait("Category", "PdfPreview")]
public class PdfPreviewTests : IDisposable
{
    private static readonly string OutputDirectory;

    static PdfPreviewTests()
    {
        QuestPDF.Settings.License = LicenseType.Community;
        OutputDirectory = Path.Combine(FindRepositoryRoot(), "artifacts", "pdf-previews");

        PrepareOutputDirectory();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PdfPreviewTests"/> class and ensures the preview output directory exists.
    /// </summary>
    public PdfPreviewTests()
    {
        Directory.CreateDirectory(OutputDirectory);
    }

    /// <inheritdoc />
    public void Dispose()
    {
    }

    // ==================== INVOICE TESTS ====================

    // Standard Invoice (ใบแจ้งหนี้)
    /// <summary>
    /// Generates the InvoiceStandard English preview PDF for manual review.
    /// </summary>
    [Fact]
    public void InvoiceStandard_English()
    {
        var document = new InvoiceDocument(DocumentTestData.InvoiceStandard_English);
        GenerateAndSave(document, "InvoiceStandard_English.pdf");
    }

    /// <summary>
    /// Generates the InvoiceStandard Thai preview PDF for manual review.
    /// </summary>
    [Fact]
    public void InvoiceStandard_Thai()
    {
        var document = new InvoiceDocument(DocumentTestData.InvoiceStandard_Thai);
        GenerateAndSave(document, "InvoiceStandard_Thai.pdf");
    }

    // Tax Invoice (ใบกำกับภาษี)
    /// <summary>
    /// Generates the Invoice English SingleItem preview PDF for manual review.
    /// </summary>
    [Fact]
    public void Invoice_English_SingleItem()
    {
        var document = new InvoiceDocument(DocumentTestData.InvoiceEnglish_SingleItem);
        GenerateAndSave(document, "Invoice_English_SingleItem.pdf");
    }

    /// <summary>
    /// Generates the Invoice English ManyItems preview PDF for manual review.
    /// </summary>
    [Fact]
    public void Invoice_English_ManyItems()
    {
        var document = new InvoiceDocument(DocumentTestData.InvoiceEnglish_ManyItems);
        GenerateAndSave(document, "Invoice_English_ManyItems.pdf");
    }

    /// <summary>
    /// Generates the Invoice Thai SingleItem preview PDF for manual review.
    /// </summary>
    [Fact]
    public void Invoice_Thai_SingleItem()
    {
        var document = new InvoiceDocument(DocumentTestData.InvoiceThai_SingleItem);
        GenerateAndSave(document, "Invoice_Thai_SingleItem.pdf");
    }

    /// <summary>
    /// Generates the Invoice Thai ManyItems preview PDF for manual review.
    /// </summary>
    [Fact]
    public void Invoice_Thai_ManyItems()
    {
        var document = new InvoiceDocument(DocumentTestData.InvoiceThai_ManyItems);
        GenerateAndSave(document, "Invoice_Thai_ManyItems.pdf");
    }

    /// <summary>
    /// Generates the Invoice Mixed SingleItem preview PDF for manual review.
    /// </summary>
    [Fact]
    public void Invoice_Mixed_SingleItem()
    {
        var document = new InvoiceDocument(DocumentTestData.InvoiceMixed_SingleItem);
        GenerateAndSave(document, "Invoice_Mixed_SingleItem.pdf");
    }

    /// <summary>
    /// Generates the Invoice Mixed ManyItems preview PDF for manual review.
    /// </summary>
    [Fact]
    public void Invoice_Mixed_ManyItems()
    {
        var document = new InvoiceDocument(DocumentTestData.InvoiceMixed_ManyItems);
        GenerateAndSave(document, "Invoice_Mixed_ManyItems.pdf");
    }

    /// <summary>
    /// Generates the Invoice Thai Individual preview PDF for manual review.
    /// </summary>
    [Fact]
    public void Invoice_Thai_Individual()
    {
        var document = new InvoiceDocument(DocumentTestData.InvoiceThai_Individual);
        GenerateAndSave(document, "Invoice_Thai_Individual.pdf");
    }

    /// <summary>
    /// Generates the Invoice Thai IndividualNoId preview PDF for manual review.
    /// </summary>
    [Fact]
    public void Invoice_Thai_IndividualNoId()
    {
        var document = new InvoiceDocument(DocumentTestData.InvoiceThai_IndividualNoId);
        GenerateAndSave(document, "Invoice_Thai_IndividualNoId.pdf");
    }

    // ==================== QUOTATION TESTS ====================

    /// <summary>
    /// Generates the Quotation English SingleItem preview PDF for manual review.
    /// </summary>
    [Fact]
    public void Quotation_English_SingleItem()
    {
        var document = new QuotationDocument(DocumentTestData.QuotationEnglish_SingleItem);
        GenerateAndSave(document, "Quotation_English_SingleItem.pdf");
    }

    /// <summary>
    /// Generates the Quotation English ManyItems preview PDF for manual review.
    /// </summary>
    [Fact]
    public void Quotation_English_ManyItems()
    {
        var document = new QuotationDocument(DocumentTestData.QuotationEnglish_ManyItems);
        GenerateAndSave(document, "Quotation_English_ManyItems.pdf");
    }

    /// <summary>
    /// Generates the Quotation Thai SingleItem preview PDF for manual review.
    /// </summary>
    [Fact]
    public void Quotation_Thai_SingleItem()
    {
        var document = new QuotationDocument(DocumentTestData.QuotationThai_SingleItem);
        GenerateAndSave(document, "Quotation_Thai_SingleItem.pdf");
    }

    /// <summary>
    /// Generates the Quotation Thai ManyItems preview PDF for manual review.
    /// </summary>
    [Fact]
    public void Quotation_Thai_ManyItems()
    {
        var document = new QuotationDocument(DocumentTestData.QuotationThai_ManyItems);
        GenerateAndSave(document, "Quotation_Thai_ManyItems.pdf");
    }

    /// <summary>
    /// Generates the Quotation Mixed SingleItem preview PDF for manual review.
    /// </summary>
    [Fact]
    public void Quotation_Mixed_SingleItem()
    {
        var document = new QuotationDocument(DocumentTestData.QuotationMixed_SingleItem);
        GenerateAndSave(document, "Quotation_Mixed_SingleItem.pdf");
    }

    /// <summary>
    /// Generates the Quotation Mixed ManyItems preview PDF for manual review.
    /// </summary>
    [Fact]
    public void Quotation_Mixed_ManyItems()
    {
        var document = new QuotationDocument(DocumentTestData.QuotationMixed_ManyItems);
        GenerateAndSave(document, "Quotation_Mixed_ManyItems.pdf");
    }

    // ==================== RECEIPT TESTS ====================

    /// <summary>
    /// Generates the Receipt English Cash preview PDF for manual review.
    /// </summary>
    [Fact]
    public void Receipt_English_Cash()
    {
        var document = new ReceiptDocument(DocumentTestData.ReceiptEnglish_Cash);
        GenerateAndSave(document, "Receipt_English_Cash.pdf");
    }

    /// <summary>
    /// Generates the Receipt English Transfer preview PDF for manual review.
    /// </summary>
    [Fact]
    public void Receipt_English_Transfer()
    {
        var document = new ReceiptDocument(DocumentTestData.ReceiptEnglish_Transfer);
        GenerateAndSave(document, "Receipt_English_Transfer.pdf");
    }

    /// <summary>
    /// Generates the Receipt English CreditCard preview PDF for manual review.
    /// </summary>
    [Fact]
    public void Receipt_English_CreditCard()
    {
        var document = new ReceiptDocument(DocumentTestData.ReceiptEnglish_CreditCard);
        GenerateAndSave(document, "Receipt_English_CreditCard.pdf");
    }

    /// <summary>
    /// Generates the Receipt Thai Cash preview PDF for manual review.
    /// </summary>
    [Fact]
    public void Receipt_Thai_Cash()
    {
        var document = new ReceiptDocument(DocumentTestData.ReceiptThai_Cash);
        GenerateAndSave(document, "Receipt_Thai_Cash.pdf");
    }

    /// <summary>
    /// Generates the Receipt Thai Transfer preview PDF for manual review.
    /// </summary>
    [Fact]
    public void Receipt_Thai_Transfer()
    {
        var document = new ReceiptDocument(DocumentTestData.ReceiptThai_Transfer);
        GenerateAndSave(document, "Receipt_Thai_Transfer.pdf");
    }

    /// <summary>
    /// Generates the Receipt Mixed Cash preview PDF for manual review.
    /// </summary>
    [Fact]
    public void Receipt_Mixed_Cash()
    {
        var document = new ReceiptDocument(DocumentTestData.ReceiptMixed_Cash);
        GenerateAndSave(document, "Receipt_Mixed_Cash.pdf");
    }

    // ==================== DELIVERY NOTE TESTS ====================

    /// <summary>
    /// Generates the DeliveryNote English SingleItem preview PDF for manual review.
    /// </summary>
    [Fact]
    public void DeliveryNote_English_SingleItem()
    {
        var document = new DeliveryNoteDocument(DocumentTestData.DeliveryNoteEnglish_SingleItem);
        GenerateAndSave(document, "DeliveryNote_English_SingleItem.pdf");
    }

    /// <summary>
    /// Generates the DeliveryNote English ManyItems preview PDF for manual review.
    /// </summary>
    [Fact]
    public void DeliveryNote_English_ManyItems()
    {
        var document = new DeliveryNoteDocument(DocumentTestData.DeliveryNoteEnglish_ManyItems);
        GenerateAndSave(document, "DeliveryNote_English_ManyItems.pdf");
    }

    /// <summary>
    /// Generates the DeliveryNote Thai SingleItem preview PDF for manual review.
    /// </summary>
    [Fact]
    public void DeliveryNote_Thai_SingleItem()
    {
        var document = new DeliveryNoteDocument(DocumentTestData.DeliveryNoteThai_SingleItem);
        GenerateAndSave(document, "DeliveryNote_Thai_SingleItem.pdf");
    }

    /// <summary>
    /// Generates the DeliveryNote Thai ManyItems preview PDF for manual review.
    /// </summary>
    [Fact]
    public void DeliveryNote_Thai_ManyItems()
    {
        var document = new DeliveryNoteDocument(DocumentTestData.DeliveryNoteThai_ManyItems);
        GenerateAndSave(document, "DeliveryNote_Thai_ManyItems.pdf");
    }

    /// <summary>
    /// Generates the DeliveryNote Mixed SingleItem preview PDF for manual review.
    /// </summary>
    [Fact]
    public void DeliveryNote_Mixed_SingleItem()
    {
        var document = new DeliveryNoteDocument(DocumentTestData.DeliveryNoteMixed_SingleItem);
        GenerateAndSave(document, "DeliveryNote_Mixed_SingleItem.pdf");
    }

    /// <summary>
    /// Generates the DeliveryNote English MultiPage preview PDF for manual review.
    /// </summary>
    [Fact]
    public void DeliveryNote_English_MultiPage()
    {
        var document = new DeliveryNoteDocument(DocumentTestData.DeliveryNoteEnglish_MultiPage);
        GenerateAndSave(document, "DeliveryNote_English_MultiPage.pdf");
    }

    // ==================== JOB TICKET TESTS ====================

    /// <summary>
    /// Generates the JobTicket FDM English preview PDF for manual review.
    /// </summary>
    [Fact]
    public void JobTicket_FDM_English()
    {
        var document = new JobTicketDocument(DocumentTestData.JobTicketFdm_English);
        GenerateAndSave(document, "JobTicket_FDM_English.pdf");
    }

    /// <summary>
    /// Generates the JobTicket CNC Thai preview PDF for manual review.
    /// </summary>
    [Fact]
    public void JobTicket_CNC_Thai()
    {
        var document = new JobTicketDocument(DocumentTestData.JobTicketCnc_Thai);
        GenerateAndSave(document, "JobTicket_CNC_Thai.pdf");
    }

    /// <summary>
    /// Generates the JobTicket SLA Mixed preview PDF for manual review.
    /// </summary>
    [Fact]
    public void JobTicket_SLA_Mixed()
    {
        var document = new JobTicketDocument(DocumentTestData.JobTicketSla_Mixed);
        GenerateAndSave(document, "JobTicket_SLA_Mixed.pdf");
    }

    // ==================== FINANCIAL REPORT TESTS ====================

    /// <summary>
    /// Generates the FinancialReport English Simple preview PDF for manual review.
    /// </summary>
    [Fact]
    public void FinancialReport_English_Simple()
    {
        var document = new FinancialReportDocument(DocumentTestData.ReportEnglish_Simple);
        GenerateAndSave(document, "FinancialReport_English_Simple.pdf");
    }

    /// <summary>
    /// Generates the FinancialReport English Complex preview PDF for manual review.
    /// </summary>
    [Fact]
    public void FinancialReport_English_Complex()
    {
        var document = new FinancialReportDocument(DocumentTestData.ReportEnglish_Complex);
        GenerateAndSave(document, "FinancialReport_English_Complex.pdf");
    }

    /// <summary>
    /// Generates the FinancialReport Thai Simple preview PDF for manual review.
    /// </summary>
    [Fact]
    public void FinancialReport_Thai_Simple()
    {
        var document = new FinancialReportDocument(DocumentTestData.ReportThai_Simple);
        GenerateAndSave(document, "FinancialReport_Thai_Simple.pdf");
    }

    /// <summary>
    /// Generates the FinancialReport Thai Complex preview PDF for manual review.
    /// </summary>
    [Fact]
    public void FinancialReport_Thai_Complex()
    {
        var document = new FinancialReportDocument(DocumentTestData.ReportThai_Complex);
        GenerateAndSave(document, "FinancialReport_Thai_Complex.pdf");
    }

    /// <summary>
    /// Generates the FinancialReport Mixed Simple preview PDF for manual review.
    /// </summary>
    [Fact]
    public void FinancialReport_Mixed_Simple()
    {
        var document = new FinancialReportDocument(DocumentTestData.ReportMixed_Simple);
        GenerateAndSave(document, "FinancialReport_Mixed_Simple.pdf");
    }

    private static void GenerateAndSave(IDocument document, string fileName)
    {
        var bytes = document.GeneratePdf();
        var filePath = Path.Combine(OutputDirectory, fileName);
        File.WriteAllBytes(filePath, bytes);
        Console.WriteLine($"Generated: {filePath}");
    }

    private static void PrepareOutputDirectory()
    {
        Directory.CreateDirectory(OutputDirectory);

        foreach (var file in Directory.EnumerateFiles(OutputDirectory, "*.pdf"))
        {
            File.Delete(file);
        }
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
