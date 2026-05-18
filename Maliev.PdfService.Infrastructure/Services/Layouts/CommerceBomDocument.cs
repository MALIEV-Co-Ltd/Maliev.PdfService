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

                content.Item().PaddingTop(14).Row(row =>
                {
                    AddSummaryCard(row.RelativeItem(), "BOM value", $"{Data.Currency} {Data.TotalCost:N2}");
                    AddSummaryCard(row.RelativeItem(), "Items", Data.Items.Count.ToString("N0"));
                    AddSummaryCard(row.RelativeItem(), "Sourcing time", FormatDays(Data.SourcingTimeDays));
                });
                content.Item().PaddingTop(4).Text($"Sourcing time: {FormatDays(Data.SourcingTimeDays)}").FontSize(8).FontColor(Colors.Grey.Darken1);

                content.Item().PaddingTop(14).Column(items =>
                {
                    if (Data.Items.Count == 0)
                    {
                        items.Item().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(10).AlignCenter().Text("No BOM items configured.").FontColor(Colors.Grey.Darken1);
                    }
                    else
                    {
                        foreach (var item in Data.Items)
                        {
                            items.Item().PaddingBottom(8).Element(container => ComposeBomItem(container, item));
                        }
                    }
                });
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

    private static void AddSummaryCard(IContainer container, string label, string value)
    {
        container
            .PaddingRight(6)
            .Border(1)
            .BorderColor(Colors.Grey.Lighten2)
            .Background(Colors.Grey.Lighten5)
            .Padding(8)
            .Column(col =>
            {
                col.Item().Text(label).FontSize(8).FontColor(Colors.Grey.Darken1);
                col.Item().Text(value).FontSize(11).Bold();
            });
    }

    private static void ComposeBomItem(IContainer container, CommerceBomItemData item)
    {
        container
            .Border(1)
            .BorderColor(Colors.Grey.Lighten2)
            .Padding(8)
            .Column(card =>
            {
                card.Item().Row(row =>
                {
                    row.RelativeItem().Text(text =>
                    {
                        text.Span($"{item.Index}. {item.ItemName}").Bold();
                        if (!string.IsNullOrWhiteSpace(item.PartNumber))
                        {
                            text.EmptyLine();
                            text.Span($"Part no: {item.PartNumber}").FontSize(8).FontColor(Colors.Grey.Darken1);
                        }
                    });

                    row.ConstantItem(120).AlignRight().Text($"{item.Currency} {item.LineTotal:N2}").Bold();
                });

                card.Item().PaddingTop(4).Text($"Specification: {FirstNonEmpty(item.Specification, "-")}").FontSize(8);
                card.Item().PaddingTop(3).Text($"Assembly: {FirstNonEmpty(item.AssemblyName, "-")}").FontSize(8);
                card.Item().Text($"Subassembly: {FirstNonEmpty(item.SubassemblyName, "-")}").FontSize(8);
                card.Item().Text($"Supplier: {FirstNonEmpty(item.SupplierName, "-")}").FontSize(8);
                card.Item().Text($"Supplier URL: {FirstNonEmpty(ShortenUrl(item.SupplierUrl), "-")}").FontSize(8);
                card.Item().Text($"Drawing: {FirstNonEmpty(ShortenUrl(item.DrawingUrl), "-")}").FontSize(8);
                card.Item().Text($"Image: {FirstNonEmpty(ShortenUrl(item.ImageUrl), "-")}").FontSize(8);
                card.Item().PaddingTop(4).Text($"Qty: {item.Quantity:N4} {item.Unit} | Unit cost: {item.Currency} {item.UnitCost:N2} | Lead time: {FormatDays(item.LeadTimeDays)} | Sourcing time: {FormatDays(item.SourcingTimeDays)}").FontSize(8);

                if (!string.IsNullOrWhiteSpace(item.Notes))
                {
                    card.Item().PaddingTop(4).Text($"Notes: {item.Notes}").FontSize(8).FontColor(Colors.Grey.Darken1);
                }
            });
    }

    private static string FirstNonEmpty(params string?[] values)
    {
        return values.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value)) ?? string.Empty;
    }

    private static string FormatDays(int? days)
    {
        var value = days.GetValueOrDefault();
        return value <= 0 ? "Not set" : $"{value:N0} days";
    }

    private static string ShortenUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return string.Empty;
        }

        return url.Replace("https://", string.Empty, StringComparison.OrdinalIgnoreCase)
            .Replace("http://", string.Empty, StringComparison.OrdinalIgnoreCase);
    }
}
