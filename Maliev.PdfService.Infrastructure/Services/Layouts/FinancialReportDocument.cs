using Maliev.PdfService.Api.Models.Data;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.IO;

namespace Maliev.PdfService.Api.Services.Layouts;

/// <summary>
/// QuestPDF layout for Financial Report documents.
/// </summary>
public class FinancialReportDocument : IDocument
{
    /// <summary>The report data.</summary>
    public FinancialReportData Data { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="FinancialReportDocument"/> class.
    /// </summary>
    public FinancialReportDocument(FinancialReportData data)
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
            page.Size(PageSizes.A4);
            page.Margin(40);
            page.DefaultTextStyle(x => x.FontFamily("Roboto", "Noto Sans Thai").FontSize(10));

            page.Foreground().Column(fg =>
            {
                fg.Item().PaddingTop(99, Unit.Millimetre).Width(8).LineHorizontal(1).LineColor(Colors.Black);
                fg.Item().PaddingTop(99, Unit.Millimetre).Width(8).LineHorizontal(1).LineColor(Colors.Black);
            });

            page.Header().Column(header =>
            {
                header.Item().Row(headerRow =>
                {
                    headerRow.RelativeItem().Column(col =>
                    {
                        col.Item().Height(20).Width(60).Svg(DocumentResources.ReadText("MALIEV_BLACK.svg"));
                        col.Item().PaddingTop(3).Text("MALIEV Co., Ltd. (Head Office) | สำนักงานใหญ่").FontSize(8).Bold();
                        col.Item().Text("36/1 Moo 3, Khlong Khoi, Pak Kret, Nonthaburi 11120").FontSize(8).FontColor(Colors.Grey.Darken1);
                        col.Item().Text("บริษัท มาลีฟ จำกัด 36/1 หมู่ 3 ต.คลองขอย อ.ปากเกร็ด จ.นนทบุรี 11120").FontSize(8).FontColor(Colors.Grey.Darken1);
                        col.Item().Text("www.maliev.com | info@maliev.com | Tax ID: 0125561001573").FontSize(8).FontColor(Colors.Grey.Darken1);
                    });

                    headerRow.ConstantItem(200).AlignRight().Column(col =>
                    {
                        col.Item().AlignRight().Text("FINANCIAL REPORT").FontSize(16).Bold();
                        col.Item().AlignRight().Text("รายงานทางการเงิน").FontSize(14);
                        col.Item().PaddingTop(2).AlignRight().Text($"{Data.ReportNumber}").FontSize(11).Bold();
                    });
                });

                header.Item().PaddingTop(8).LineHorizontal(1);
            });

            page.Content().PaddingVertical(15).Column(content =>
            {
                foreach (var section in Data.Sections)
                {
                    var isFirstSection = Data.Sections.IndexOf(section) == 0;

                    content.Item().PaddingBottom(10).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(3);
                            columns.ConstantColumn(130);
                        });

                        table.Cell().PaddingBottom(3).AlignMiddle().Text(section.SectionTitle).Bold().FontSize(12).FontColor(Colors.Black);
                        table.Cell().PaddingBottom(3).AlignMiddle().AlignRight().Text(
                            isFirstSection
                                ? $"Period: {Data.PeriodStart:dd/MM/yyyy} - {Data.PeriodEnd:dd/MM/yyyy}"
                                : ""
                        ).FontSize(8).FontColor(Colors.Grey.Darken1);

                        foreach (var lineItem in section.LineItems)
                        {
                            table.Cell().Padding(3).Text(lineItem.Description);
                            table.Cell().Padding(3).AlignRight().Text(lineItem.Amount.ToString("N2"));
                        }

                        table.Cell().Padding(3);
                        table.Cell().Padding(3).AlignRight().Text(section.SectionTotal.ToString("N2")).Bold();
                    });
                }

                content.Item().PaddingTop(20).LineHorizontal(1).LineColor(Colors.Grey.Medium);

                content.Item().PaddingTop(10).Table(summaryTable =>
                {
                    summaryTable.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(3);
                        columns.ConstantColumn(130);
                    });

                    summaryTable.Cell().Padding(3).Text("Total Revenue:").Bold();
                    summaryTable.Cell().Padding(3).AlignRight().Text(Data.TotalRevenue.ToString("N2")).Bold().FontColor(Colors.Black);

                    summaryTable.Cell().Padding(3).Text("Total Expenses:").Bold();
                    summaryTable.Cell().Padding(3).AlignRight().Text(Data.TotalExpenses.ToString("N2")).Bold().FontColor(Colors.Black);

                    var profitColor = Colors.Black;
                    summaryTable.Cell().Padding(3).PaddingTop(8).Text("Net Profit/Loss:").Bold().FontSize(12);
                    summaryTable.Cell().Padding(3).PaddingTop(8).AlignRight().Text($"{Data.Currency} {Data.NetProfit:N2}").Bold().FontSize(12).FontColor(profitColor);
                });
            });

            page.Footer().Row(row =>
            {
                row.RelativeItem().Text("MALIEV Co., Ltd.").FontSize(7);
                row.RelativeItem().AlignCenter().Text($"{Data.ReportDate:dd MMM yyyy}").FontSize(7);
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
}
