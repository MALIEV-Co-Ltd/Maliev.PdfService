using Maliev.PdfService.Api.Models.Data;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.IO;

namespace Maliev.PdfService.Api.Services.Layouts;

/// <summary>
/// QuestPDF layout for Receipt documents (ใบเสร็จรับเงิน).
/// Generates 2 pages: Original (ต้นฉบับ) and Copy (สำเนา).
/// </summary>
public class ReceiptDocument : IDocument
{
    /// <summary>The receipt data.</summary>
    public ReceiptData Data { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ReceiptDocument"/> class.
    /// </summary>
    public ReceiptDocument(ReceiptData data)
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
                        col.Item().AlignRight().Text("ต้นฉบับ").FontSize(12).Bold();
                        col.Item().PaddingTop(2).AlignRight().Text("ORIGINAL").FontSize(12).Bold();
                        col.Item().PaddingTop(8).AlignRight().Text("RECEIPT").FontSize(16).Bold();
                        col.Item().AlignRight().Text("ใบเสร็จรับเงิน").FontSize(14);
                        col.Item().PaddingTop(2).AlignRight().Text($"#{Data.ReceiptNumber}").FontSize(11).Bold();
                        col.Item().PaddingTop(2).AlignRight().Text($"{Data.ReceiptDate:dd MMM yyyy}").FontSize(9);
                    });
                });

                header.Item().PaddingTop(8).LineHorizontal(1);
            });

            ComposeReceiptContent(page);

            page.Footer().Row(row =>
            {
                row.RelativeItem().Text("Maliev Co., Ltd.").FontSize(7);
                row.RelativeItem().AlignCenter().Text(Data.ReceiptNumber).FontSize(7);
                row.RelativeItem().AlignRight().Text(text =>
                {
                    text.Span("Page ").FontSize(7);
                    text.CurrentPageNumber().FontSize(7);
                    text.Span(" of ").FontSize(7);
                    text.TotalPages().FontSize(7);
                });
            });
        });

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
                        col.Item().AlignRight().Text("สำเนา").FontSize(12).Bold();
                        col.Item().PaddingTop(2).AlignRight().Text("COPY").FontSize(12).Bold();
                        col.Item().PaddingTop(8).AlignRight().Text("RECEIPT").FontSize(16).Bold();
                        col.Item().AlignRight().Text("ใบเสร็จรับเงิน").FontSize(14);
                        col.Item().PaddingTop(2).AlignRight().Text($"{Data.ReceiptNumber}").FontSize(11).Bold();
                        col.Item().PaddingTop(2).AlignRight().Text($"{Data.ReceiptDate:dd MMM yyyy}").FontSize(9);
                    });
                });

                header.Item().PaddingTop(8).LineHorizontal(1);
            });

            ComposeReceiptContent(page);

            page.Footer().Row(row =>
            {
                row.RelativeItem().Text("Maliev Co., Ltd.").FontSize(7);
                row.RelativeItem().AlignCenter().Text(Data.ReceiptNumber).FontSize(7);
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

    private void ComposeReceiptContent(PageDescriptor page)
    {
        page.Content().PaddingVertical(15).Column(content =>
        {
            content.Item().Row(row =>
            {
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text("ผู้รับเงิน / RECEIVED FROM:").Bold().FontSize(9).FontColor(Colors.Grey.Darken1);
                    col.Item().PaddingTop(3);
                    if (Data.CustomerType == "Corporate")
                    {
                        var branch = string.IsNullOrEmpty(Data.CustomerBranch) ? "" : $" ({Data.CustomerBranch})";
                        col.Item().Text($"{Data.CustomerName}{branch}").FontSize(11).Bold();
                        if (!string.IsNullOrEmpty(Data.CustomerTaxId))
                            col.Item().Text($"เลขประจำตัวผู้เสียภาษี: {Data.CustomerTaxId}").FontSize(9);
                    }
                    else
                    {
                        col.Item().Text(Data.CustomerName).FontSize(11).Bold();
                        if (!string.IsNullOrEmpty(Data.CustomerTaxId))
                            col.Item().Text($"เลขประจำตัวประชาชน: {Data.CustomerTaxId}").FontSize(9);
                    }
                    if (!string.IsNullOrEmpty(Data.CustomerAddress))
                    {
                        col.Item().Text(Data.CustomerAddress).FontSize(9);
                    }
                });

                row.RelativeItem().PaddingLeft(20).Column(col =>
                {
                    col.Item().Text("ข้อมูลการชำระเงิน / PAYMENT INFO:").Bold().FontSize(9).FontColor(Colors.Grey.Darken1);
                    col.Item().PaddingTop(3).Text($"Method: {Data.PaymentMethod}").FontSize(10);
                    if (!string.IsNullOrEmpty(Data.ReferenceNumber))
                    {
                        col.Item().Text($"Ref: {Data.ReferenceNumber}").FontSize(9);
                    }
                });
            });

            content.Item().PaddingTop(15).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

            content.Item().PaddingTop(10).Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(30);
                    columns.RelativeColumn(3);
                    columns.ConstantColumn(50);
                    columns.ConstantColumn(60);
                    columns.ConstantColumn(80);
                });

                table.Header(tableHeader =>
                {
                    tableHeader.Cell().Element(HeaderCellStyle).Text("#");
                    tableHeader.Cell().Element(HeaderCellStyle).Text("Description");
                    tableHeader.Cell().Element(HeaderCellStyle).AlignRight().Text("Qty");
                    tableHeader.Cell().Element(HeaderCellStyle).AlignRight().Text("Unit Price");
                    tableHeader.Cell().Element(HeaderCellStyle).AlignRight().Text("Amount");
                });

                int index = 1;
                foreach (var item in Data.Items)
                {
                    table.Cell().Element(DataCellStyle).ShowEntire().Text(index.ToString());
                    table.Cell().Element(DataCellStyle).ShowEntire().Text(item.Description);
                    table.Cell().Element(DataCellStyle).ShowEntire().AlignRight().Text(item.Quantity.ToString("N2"));
                    table.Cell().Element(DataCellStyle).ShowEntire().AlignRight().Text(item.UnitPrice.ToString("N2"));
                    table.Cell().Element(DataCellStyle).ShowEntire().AlignRight().Text(item.TotalPrice.ToString("N2"));
                    index++;
                }
            });

            content.Item().PaddingTop(10).AlignRight().Width(200).Column(totalsCol =>
            {
                totalsCol.Item().Row(row =>
                {
                    row.RelativeItem().Text("Subtotal:");
                    row.ConstantItem(100).AlignRight().Text(Data.Subtotal.ToString("N2"));
                });

                if (Data.DiscountAmount > 0)
                {
                    totalsCol.Item().Row(row =>
                    {
                        row.RelativeItem().Text("Discount:");
                        row.ConstantItem(100).AlignRight().Text($"-{Data.DiscountAmount:N2}");
                    });
                }

                if (Data.TaxAmount > 0)
                {
                    totalsCol.Item().Row(row =>
                    {
                        row.RelativeItem().Text("Tax:");
                        row.ConstantItem(100).AlignRight().Text(Data.TaxAmount.ToString("N2"));
                    });
                }

                totalsCol.Item().PaddingTop(5).BorderTop(1).BorderColor(Colors.Grey.Medium).Row(row =>
                {
                    row.RelativeItem().Text("TOTAL:").Bold();
                    row.ConstantItem(100).AlignRight().Text($"{Data.Currency} {Data.TotalAmount:N2}").Bold().FontColor(Colors.Black);
                });
            });

            if (!string.IsNullOrEmpty(Data.Notes))
            {
                content.Item().PaddingTop(20).Column(col =>
                {
                    col.Item().Text("หมายเหตุ / NOTES:").Bold().FontSize(9).FontColor(Colors.Grey.Darken1);
                    col.Item().PaddingTop(3).Text(Data.Notes).FontSize(9);
                });
            }

            content.Item().PaddingBottom(60);

            content.Item().Row(row =>
            {
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text("ผู้รับเงิน / Received by").FontSize(9).FontColor(Colors.Grey.Darken1);
                    col.Item().PaddingTop(60).LineHorizontal(1).LineColor(Colors.Grey.Medium);
                    col.Item().PaddingTop(10).AlignCenter().Text("Signature & Date").FontSize(8);
                });

                row.ConstantItem(40);

                row.RelativeItem().Column(col =>
                {
                    col.Item().Text("ผู้จ่ายเงิน / Paid by").FontSize(9).FontColor(Colors.Grey.Darken1);
                    col.Item().PaddingTop(60).LineHorizontal(1).LineColor(Colors.Grey.Medium);
                    col.Item().PaddingTop(10).AlignCenter().Text("Signature & Date").FontSize(8);
                });
            });
        });
    }

    private static IContainer HeaderCellStyle(IContainer container)
    {
        return container
            .Border(1)
            .BorderColor(Colors.Black)
            .Background(Colors.Black)
            .Padding(5)
            .DefaultTextStyle(x => x.Bold().FontSize(9).FontColor(Colors.White));
    }

    private static IContainer DataCellStyle(IContainer container)
    {
        return container
            .Border(1)
            .BorderColor(Colors.Grey.Lighten2)
            .Padding(5)
            .DefaultTextStyle(x => x.FontSize(9));
    }
}
