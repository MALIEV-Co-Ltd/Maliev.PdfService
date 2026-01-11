using Maliev.PdfService.Api.Models.Data;
using Maliev.PdfService.Api.Services;
using Maliev.PdfService.Api.Services.Layouts;
using Maliev.PdfService.Data.Entities;
using Xunit;

namespace Maliev.PdfService.Tests.Unit;

public class DocumentFactoryTests
{
    private readonly DocumentFactory _factory = new();

    [Fact]
    public void CreateDocument_Throws_OnUnsupportedType()
    {
        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => _factory.CreateDocument((DocumentType)999, new { }));
    }

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
        Assert.Single(invoiceDoc.Data.Items);
    }
}
