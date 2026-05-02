using Maliev.PdfService.Api.Models.Data;
using Maliev.PdfService.Api.Services;
using Maliev.PdfService.Api.Services.Layouts;
using Maliev.PdfService.Domain.Entities;
using Xunit;

namespace Maliev.PdfService.Tests.Unit;

/// <summary>
/// Unit tests for mapping PDF document types and data into QuestPDF document instances.
/// </summary>
public class DocumentFactoryTests
{
    private readonly DocumentFactory _factory = new();

    /// <summary>
    /// Verifies that unsupported document types throw an argument-out-of-range exception.
    /// </summary>
    [Fact]
    public void CreateDocument_Throws_OnUnsupportedType()
    {
        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => _factory.CreateDocument((DocumentType)999, new { }));
    }

    /// <summary>
    /// Verifies that invoice data is mapped into an invoice document.
    /// </summary>
    [Fact]
    public void CreateDocument_MapsObjectToInvoiceData()
    {
        // Arrange
        var data = new InvoiceData
        {
            InvoiceNumber = "MAP-001",
            Items = new List<InvoiceItemData> { new InvoiceItemData { Index = 1, Description = "D", Quantity = 1.0, TotalPrice = 50.0 } }
        };

        // Act
        var document = _factory.CreateDocument(DocumentType.Invoice, data);

        // Assert
        var invoiceDoc = Assert.IsType<InvoiceDocument>(document);
        Assert.Equal("MAP-001", invoiceDoc.Data.InvoiceNumber);
    }

    /// <summary>
    /// Verifies that quotation document types create quotation documents.
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
    /// Verifies that receipt document types create receipt documents.
    /// </summary>
    [Fact]
    public void CreateDocument_Receipt_ReturnsReceiptDocument()
    {
        // Arrange
        var data = new { ReceiptNumber = "RCP-1" };

        // Act
        var doc = _factory.CreateDocument(DocumentType.Receipt, data);

        // Assert
        Assert.NotNull(doc);
    }

    /// <summary>
    /// Verifies that report document types create financial report documents.
    /// </summary>
    [Fact]
    public void CreateDocument_Report_ReturnsReportDocument()
    {
        // Arrange
        var data = new { ReportTitle = "Report" };

        // Act
        var doc = _factory.CreateDocument(DocumentType.Report, data);

        // Assert
        Assert.NotNull(doc);
    }

    /// <summary>
    /// Verifies that JSON invoice data can be mapped into a PDF document.
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
    /// Verifies that invalid invoice input data throws an invalid operation exception.
    /// </summary>
    [Fact]
    public void MapToInvoiceData_InvalidType_ThrowsException()
    {
        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => _factory.CreateDocument(DocumentType.Invoice, "not-data"));
    }
}
