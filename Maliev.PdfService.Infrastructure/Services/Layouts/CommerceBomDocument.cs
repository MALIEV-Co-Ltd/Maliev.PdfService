using Maliev.PdfService.Api.Models.Data;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Maliev.PdfService.Api.Services.Layouts;

/// <summary>
/// QuestPDF layout for Commerce product bill of materials documents.
/// </summary>
public class CommerceBomDocument : IDocument
{
    /// <summary>Gets the BOM data.</summary>
    public CommerceBomData Data { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="CommerceBomDocument"/> class.
    /// </summary>
    /// <param name="data">The BOM data.</param>
    public CommerceBomDocument(CommerceBomData data)
    {
        Data = data;
    }

    /// <inheritdoc />
    public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

    /// <inheritdoc />
    public void Compose(IDocumentContainer container)
    {
        container.Page(page =>
        {
            page.Size(PageSizes.A4);
            page.Margin(36);
            page.DefaultTextStyle(style => style.FontFamily("Roboto", "Noto Sans Thai").FontSize(9));

            page.Header().Column(header =>
            {
                header.Item().Row(row =>
                {
                    row.RelativeItem().Column(col =>
                    {
                        col.Item().Height(20).Width(60).Svg(DocumentResources.ReadText("MALIEV_BLACK.svg"));
                        col.Item().PaddingTop(3).Text("MALIEV Co., Ltd.").FontSize(8).Bold();
                        col.Item().Text("Commerce product bill of materials").FontSize(8).FontColor(Colors.Grey.Darken1);
                    });

                    row.ConstantItem(220).AlignRight().Column(col =>
                    {
                        col.Item().AlignRight().Text("PRODUCT BOM").FontSize(16).Bold();
                        col.Item().AlignRight().Text(Data.ProductHandle).FontSize(10).FontColor(Colors.Grey.Darken1);
                    });
                });

                header.Item().PaddingTop(8).LineHorizontal(1);
            });

            page.Content().PaddingVertical(14).Column(content =>
            {
                content.Item().Row(row =>
                {
                    row.RelativeItem().Column(col =>
                    {
                        col.Item().Text("Product").FontSize(8).FontColor(Colors.Grey.Darken1);
                        col.Item().Text(Data.ProductTitle).FontSize(13).Bold();
                        col.Item().Text(FirstNonEmpty(Data.Brand, "-")).FontSize(9);
                    });
                    row.ConstantItem(240).AlignRight().Column(col =>
                    {
                        col.Item().Text($"Type: {FirstNonEmpty(Data.ProductType, "-")}").FontSize(9);
                        col.Item().Text($"Status: {FirstNonEmpty(Data.Status, "-")}").FontSize(9);
                        col.Item().Text($"Generated: {Data.GeneratedAt:dd MMM yyyy HH:mm}").FontSize(9);
                    });
                });

                content.Item().PaddingTop(14).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(28);
                        columns.RelativeColumn(2.6f);
                        columns.RelativeColumn(2.4f);
                        columns.ConstantColumn(70);
                        columns.ConstantColumn(82);
                        columns.ConstantColumn(82);
                    });

                    AddHeaderCell(table, "#");
                    AddHeaderCell(table, "Item");
                    AddHeaderCell(table, "Specification");
                    AddHeaderCell(table, "Qty");
                    AddHeaderCell(table, "Unit cost");
                    AddHeaderCell(table, "Line total");

                    if (Data.Items.Count == 0)
                    {
                        table.Cell().ColumnSpan(6).Padding(10).AlignCenter().Text("No BOM items configured.").FontColor(Colors.Grey.Darken1);
                    }
                    else
                    {
                        foreach (var item in Data.Items)
                        {
                            table.Cell().Element(CellStyle).AlignCenter().Text(item.Index.ToString());
                            table.Cell().Element(CellStyle).Text(text =>
                            {
                                text.Span(item.ItemName).Bold();
                                if (!string.IsNullOrWhiteSpace(item.Notes))
                                {
                                    text.EmptyLine();
                                    text.Span(item.Notes!).FontSize(8).FontColor(Colors.Grey.Darken1);
                                }
                            });
                            table.Cell().Element(CellStyle).Text(FirstNonEmpty(item.Specification, "-"));
                            table.Cell().Element(CellStyle).AlignRight().Text($"{item.Quantity:N4} {item.Unit}");
                            table.Cell().Element(CellStyle).AlignRight().Text($"{item.Currency} {item.UnitCost:N2}");
                            table.Cell().Element(CellStyle).AlignRight().Text($"{item.Currency} {item.LineTotal:N2}").Bold();
                        }
                    }
                });

                content.Item().PaddingTop(12).AlignRight().Text($"{Data.Currency} {Data.TotalCost:N2}").FontSize(13).Bold();
            });

            page.Footer().Row(row =>
            {
                row.RelativeItem().Text("MALIEV Commerce").FontSize(7);
                row.RelativeItem().AlignRight().Text(text =>
                {
                    text.Span("Page ").FontSize(7);
                    text.CurrentPageNumber().FontSize(7);
                    text.Span(" of ").FontSize(7);
                    text.TotalPages().FontSize(7);
                });
            });
        });
    }

    private static void AddHeaderCell(TableDescriptor table, string label)
    {
        table.Cell()
            .Background(Colors.Grey.Lighten3)
            .BorderBottom(1)
            .BorderColor(Colors.Grey.Medium)
            .Padding(5)
            .Text(label)
            .FontSize(8)
            .Bold();
    }

    private static IContainer CellStyle(IContainer container)
    {
        return container
            .BorderBottom(1)
            .BorderColor(Colors.Grey.Lighten2)
            .Padding(5);
    }

    private static string FirstNonEmpty(params string?[] values)
    {
        return values.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value)) ?? string.Empty;
    }
}
