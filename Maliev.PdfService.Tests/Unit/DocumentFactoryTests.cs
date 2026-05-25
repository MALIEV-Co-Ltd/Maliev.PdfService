using Maliev.PdfService.Api.Models.Data;
using Maliev.PdfService.Api.Services;
using Maliev.PdfService.Api.Services.Layouts;
using Maliev.PdfService.Domain.Entities;
using Xunit;

namespace Maliev.PdfService.Tests.Unit;

/// <summary>
/// Unit tests for DocumentFactory.
/// </summary>
public class DocumentFactoryTests
{
    private readonly DocumentFactory _factory = new();

    /// <summary>
    /// Tests that CreateDocument throws on unsupported type.
    /// </summary>
    [Fact]
    public void CreateDocument_Throws_OnUnsupportedType()
    {
        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => _factory.CreateDocument((DocumentType)999, new { }));
    }

    /// <summary>
    /// Tests that CreateDocument maps object to InvoiceData.
    /// </summary>
    [Fact]
    public void CreateDocument_MapsObjectToInvoiceData()
    {
        // Arrange
        var data = new InvoiceData
        {
            InvoiceNumber = "MAP-001",
            Items = new List<InvoiceItemData> { new InvoiceItemData { Index = 1, Description = "D", Quantity = 1m, UnitPrice = 50m, LineSubtotal = 50m, LineTotal = 50m } }
        };

        // Act
        var document = _factory.CreateDocument(DocumentType.Invoice, data);

        // Assert
        var invoiceDoc = Assert.IsType<InvoiceDocument>(document);
        Assert.Equal("MAP-001", invoiceDoc.Data.InvoiceNumber);
    }

    /// <summary>
    /// Tests that CreateDocument returns QuotationDocument.
    /// </summary>
    [Fact]
    public void CreateDocument_Quotation_ReturnsQuotationDocument()
    {
        // Arrange
        var data = new QuotationData { QuotationNumber = "QUO-1" };

        // Act
        var doc = _factory.CreateDocument(DocumentType.Quotation, data);

        // Assert
        Assert.NotNull(doc);
    }

    /// <summary>
    /// Tests that CreateDocument returns ReceiptDocument.
    /// </summary>
    [Fact]
    public void CreateDocument_Receipt_ReturnsReceiptDocument()
    {
        // Arrange
        var data = new ReceiptData { ReceiptNumber = "RCP-1" };

        // Act
        var doc = _factory.CreateDocument(DocumentType.Receipt, data);

        // Assert
        Assert.NotNull(doc);
    }

    /// <summary>
    /// Tests that CreateDocument returns ReportDocument.
    /// </summary>
    [Fact]
    public void CreateDocument_Report_ReturnsReportDocument()
    {
        // Arrange
        var data = new FinancialReportData { ReportTitle = "Report" };

        // Act
        var doc = _factory.CreateDocument(DocumentType.Report, data);

        // Assert
        Assert.NotNull(doc);
    }

    /// <summary>
    /// Tests that CreateDocument returns CommerceBomDocument.
    /// </summary>
    [Fact]
    public void CreateDocument_CommerceBom_ReturnsCommerceBomDocument()
    {
        // Arrange
        var data = new CommerceBomData { ProductTitle = "Starter machine", ProductHandle = "starter-machine" };

        // Act
        var doc = _factory.CreateDocument(DocumentType.CommerceBom, data);

        // Assert
        Assert.IsType<CommerceBomDocument>(doc);
    }

    /// <summary>
    /// Tests that CreateDocument returns BlogPracticalNoteDocument.
    /// </summary>
    [Fact]
    public void CreateDocument_BlogPracticalNote_ReturnsBlogPracticalNoteDocument()
    {
        // Arrange
        var data = new BlogPracticalNoteData
        {
            Slug = "silicone-master-preparation",
            Title = "Silicone casting starts with the master",
            Summary = "The mold repeats the quality and defects of the master pattern.",
            Category = "Casting",
            PublicUrl = "https://www.maliev.com/blog/silicone-master-preparation",
            Sections =
            [
                new BlogPracticalNoteSectionData
                {
                    Title = "Review the mold behavior before the part",
                    Body = "Casting and molding decisions are affected by draft and wall thickness.",
                    Items = ["A better master makes a better batch."]
                }
            ],
            Takeaways = ["A better master makes a better batch."]
        };

        // Act
        var doc = _factory.CreateDocument(DocumentType.BlogPracticalNote, data);

        // Assert
        Assert.IsType<BlogPracticalNoteDocument>(doc);
    }

    /// <summary>
    /// Tests that MapToInvoiceData returns data from JsonElement.
    /// </summary>
    [Fact]
    public void MapToInvoiceData_JsonElement_ReturnsData()
    {
        // Arrange
        var json = "{\"InvoiceNumber\":\"INV-2\"}";
        var element = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>(json);

        // Act
        var doc = _factory.CreateDocument(DocumentType.Invoice, element);

        // Assert
        Assert.NotNull(doc);
    }

    /// <summary>
    /// Tests that MapToInvoiceData throws exception on invalid type.
    /// </summary>
    [Fact]
    public void MapToInvoiceData_InvalidType_ThrowsException()
    {
        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => _factory.CreateDocument(DocumentType.Invoice, "not-data"));
    }
}
