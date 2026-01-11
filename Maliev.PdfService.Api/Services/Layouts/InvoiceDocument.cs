using Maliev.PdfService.Api.Models.Data;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Maliev.PdfService.Api.Services.Layouts;

/// <summary>
/// QuestPDF layout for Invoice documents.
/// </summary>
public class InvoiceDocument : IDocument
{
    /// <summary>The invoice data.</summary>
    public InvoiceData Data { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="InvoiceDocument"/> class.
    /// </summary>
    public InvoiceDocument(InvoiceData data)
    {
        Data = data;
    }

    /// <inheritdoc/>
    public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

    /// <inheritdoc/>
    public void Compose(IDocumentContainer container)
    {
        container.Page(page =>
        {
            page.Margin(50);
            page.DefaultTextStyle(x => x.FontFamily("Kanit"));

            page.Header().Row(row =>
            {
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text("INVOICE").FontSize(20).SemiBold().FontColor(Colors.Blue.Medium);
                    col.Item().Text($"Invoice #: {Data.InvoiceNumber}");
                });
            });

            page.Content().PaddingVertical(20).Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(30);
                    columns.RelativeColumn();
                    columns.ConstantColumn(50);
                    columns.ConstantColumn(80);
                });

                table.Header(header =>
                {
                    header.Cell().Text("#");
                    header.Cell().Text("Description");
                    header.Cell().Text("Qty");
                    header.Cell().Text("Total");
                });

                foreach (var item in Data.Items)
                {
                    table.Cell().Text(item.Index.ToString());
                    table.Cell().Text(item.Description);
                    table.Cell().Text(item.Quantity.ToString());
                    table.Cell().Text(item.TotalPrice.ToString("C"));
                }
            });

            page.Footer().AlignCenter().Text(x =>
            {
                x.Span("Page ");
                x.CurrentPageNumber();
            });
        });
    }
}
