using Maliev.PdfService.Api.Models.Data;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.IO;

namespace Maliev.PdfService.Api.Services.Layouts;

/// <summary>
/// QuestPDF layout for Quotation documents (ใบเสนอราคา).
/// Shows materials, manufacturing process, validity period, pricing, and terms.
/// </summary>
public class QuotationDocument : IDocument
{
    /// <summary>
    /// The quotation data.
    /// </summary>
    public QuotationData Data { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="QuotationDocument"/> class.
    /// </summary>
    public QuotationDocument(QuotationData data)
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
                        col.Item().Height(20).Width(60).Svg(File.ReadAllText("Resources/MALIEV_BLACK.svg"));
                        col.Item().PaddingTop(3).Text("MALIEV Co., Ltd. (Head Office) | สำนักงานใหญ่").FontSize(8).Bold();
                        col.Item().Text("36/1 Moo 3, Khlong Khoi, Pak Kret, Nonthaburi 11120").FontSize(8).FontColor(Colors.Grey.Darken1);
                        col.Item().Text("บริษัท มาลีฟ จำกัด 36/1 หมู่ 3 ต.คลองขอย อ.ปากเกร็ด จ.นนทบุรี 11120").FontSize(8).FontColor(Colors.Grey.Darken1);
                        col.Item().Text("www.maliev.com | info@maliev.com | Tax ID: 0125561001573").FontSize(8).FontColor(Colors.Grey.Darken1);
                    });

                    headerRow.ConstantItem(200).AlignRight().Column(col =>
                    {
                        col.Item().AlignRight().Text("QUOTATION").FontSize(16).Bold();
                        col.Item().AlignRight().Text("ใบเสนอราคา").FontSize(14);
                        col.Item().PaddingTop(2).AlignRight().Text($"{Data.QuotationNumber}").FontSize(11).Bold();
                        if (Data.VersionNumber > 1)
                            col.Item().AlignRight().Text($"Revision {Data.VersionNumber}").FontSize(9).FontColor(Colors.Grey.Darken1);
                    });
                });

                header.Item().PaddingTop(8).LineHorizontal(1);
            });

            page.Content().PaddingVertical(12).Column(content =>
            {
                content.Item().PaddingBottom(10).Row(row =>
                {
                    row.RelativeItem().Column(col =>
                    {
                        col.Item().Text("เสนอให้ / QUOTE TO:").Bold().FontSize(8).FontColor(Colors.Grey.Darken1);
                        col.Item().PaddingTop(3);
                        if (Data.CustomerType == "Corporate")
                        {
                            var branch = string.IsNullOrEmpty(Data.CustomerBranch) ? "" : $" ({Data.CustomerBranch})";
                            col.Item().Text($"{Data.CustomerName}{branch}").FontSize(11).Bold();
                            if (!string.IsNullOrEmpty(Data.CustomerTaxId))
                                col.Item().Text($"เลขประจำตัวผู้เสียภาษี: {Data.CustomerTaxId}").FontSize(8);
                        }
                        else
                        {
                            col.Item().Text(Data.CustomerName).FontSize(11).Bold();
                            if (!string.IsNullOrEmpty(Data.CustomerTaxId))
                                col.Item().Text($"เลขประจำตัวประชาชน: {Data.CustomerTaxId}").FontSize(8);
                        }
                        if (!string.IsNullOrEmpty(Data.CustomerAddress))
                            col.Item().Text(Data.CustomerAddress).FontSize(8);
                        if (!string.IsNullOrEmpty(Data.ContactPerson))
                            col.Item().Text($"Attn: {Data.ContactPerson}").FontSize(8);
                    });

                    row.ConstantItem(180).Column(col =>
                    {
                        col.Item().Table(dateTable =>
                        {
                            dateTable.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(70);
                                columns.RelativeColumn(1);
                            });

                            dateTable.Cell().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(4).Text("Issue Date").FontSize(8).Bold();
                            dateTable.Cell().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(4).AlignRight().Text(Data.QuotationDate.ToString("dd MMM yyyy")).FontSize(8);

                            dateTable.Cell().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(4).Text("Valid From").FontSize(8).Bold();
                            dateTable.Cell().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(4).AlignRight().Text(Data.ValidityStart.ToString("dd MMM yyyy")).FontSize(8);

                            dateTable.Cell().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(4).Text("Valid Until").FontSize(8).Bold();
                            dateTable.Cell().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(4).AlignRight().Text(Data.ValidityEnd.ToString("dd MMM yyyy")).FontSize(8).Bold();
                        });
                    });
                });

                content.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

                content.Item().PaddingTop(8).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(28);
                        columns.RelativeColumn(2);
                        columns.RelativeColumn(1);
                        columns.ConstantColumn(45);
                        columns.ConstantColumn(40);
                        columns.ConstantColumn(90);
                        columns.ConstantColumn(90);
                    });

                    table.Header(h =>
                    {
                        h.Cell().Element(HeaderCell).AlignCenter().Text("#");
                        h.Cell().Element(HeaderCell).Text("Material / บริการ");
                        h.Cell().Element(HeaderCell).Text("Process");
                        h.Cell().Element(HeaderCell).AlignRight().Text("Qty");
                        h.Cell().Element(HeaderCell).Text("Unit");
                        h.Cell().Element(HeaderCell).AlignRight().Text("Unit Price");
                        h.Cell().Element(HeaderCell).AlignRight().Text("Total");
                    });

                    foreach (var item in Data.Items)
                    {
                        var bg = item.Index % 2 == 0 ? Colors.Grey.Lighten5 : Colors.White;
                        table.Cell().Element(c => DataCell(c, bg).ShowEntire()).AlignCenter().Text(item.Index.ToString());
                        table.Cell().Element(c => DataCell(c, bg).ShowEntire()).Column(col =>
                        {
                            col.Item().Text(item.MaterialName).Bold();
                            if (!string.IsNullOrEmpty(item.Notes))
                                col.Item().Text(item.Notes).FontSize(7).FontColor(Colors.Grey.Darken1);
                        });
                        table.Cell().Element(c => DataCell(c, bg).ShowEntire()).Text(item.ManufacturingProcess ?? "");
                        table.Cell().Element(c => DataCell(c, bg).ShowEntire()).AlignRight().Text(item.Quantity.ToString("N0"));
                        table.Cell().Element(c => DataCell(c, bg).ShowEntire()).Text(item.QuantityUnit);
                        table.Cell().Element(c => DataCell(c, bg).ShowEntire()).AlignRight().Text(item.UnitPrice.ToString("N2"));
                        table.Cell().Element(c => DataCell(c, bg).ShowEntire()).AlignRight().Text(item.LineTotal.ToString("N2")).Bold();
                    }
                });

                if (Data.Discounts.Count > 0)
                {
                    content.Item().PaddingTop(8).Column(discCol =>
                    {
                        discCol.Item().Text("ส่วนลด / DISCOUNTS:").Bold().FontSize(8).FontColor(Colors.Grey.Darken1);
                        foreach (var discount in Data.Discounts)
                        {
                            discCol.Item().PaddingTop(2).Text(text =>
                            {
                                var valueStr = discount.DiscountType == "Percentage"
                                    ? $"{discount.DiscountValue:N1}%"
                                    : $"{Data.Currency} {discount.DiscountValue:N2}";
                                text.Span($"  • {discount.DiscountType}: {valueStr}").FontSize(9);
                                if (!string.IsNullOrEmpty(discount.Conditions))
                                    text.Span($" — {discount.Conditions}").FontSize(8).FontColor(Colors.Grey.Darken1);
                            });
                        }
                    });
                }

                content.Item().PaddingTop(8).AlignRight().Width(260).Column(totals =>
                {
                    if (Data.TotalDiscount > 0)
                    {
                        totals.Item().Row(r =>
                        {
                            r.RelativeItem().Text("Subtotal:").FontSize(9);
                            r.ConstantItem(100).AlignRight().Text(Data.SubtotalBeforeDiscount.ToString("N2")).FontSize(9);
                        });
                        totals.Item().Row(r =>
                        {
                            r.RelativeItem().Text("Discount:").FontSize(9);
                            r.ConstantItem(100).AlignRight().Text($"−{Data.TotalDiscount:N2}").FontSize(9);
                        });
                    }

                    totals.Item().Row(r =>
                    {
                        r.RelativeItem().Text("Subtotal (ex. VAT):").FontSize(9);
                        r.ConstantItem(100).AlignRight().Text(Data.Subtotal.ToString("N2")).FontSize(9);
                    });

                    if (Data.TaxAmount > 0)
                    {
                        totals.Item().Row(r =>
                        {
                            r.RelativeItem().Text("VAT (7%):").FontSize(9);
                            r.ConstantItem(100).AlignRight().Text(Data.TaxAmount.ToString("N2")).FontSize(9);
                        });
                    }

                    totals.Item().PaddingTop(4).BorderTop(1).Row(r =>
                    {
                        r.RelativeItem().Text($"TOTAL ({Data.Currency}):").Bold().FontSize(11);
                        r.ConstantItem(100).AlignRight().Text(Data.TotalAmount.ToString("N2")).Bold().FontSize(11);
                    });
                });

                content.Item().PaddingTop(12).LineHorizontal(1).LineColor(Colors.Grey.Lighten3);

                if (!string.IsNullOrEmpty(Data.DeliveryExpectations))
                {
                    content.Item().PaddingTop(6).Row(r =>
                    {
                        r.ConstantItem(100).Text("Lead Time:").Bold().FontSize(9);
                        r.RelativeItem().Text(Data.DeliveryExpectations).FontSize(9);
                    });
                }

                if (!string.IsNullOrEmpty(Data.SpecialTerms))
                {
                    content.Item().PaddingTop(6).Column(col =>
                    {
                        col.Item().Text("เงื่อนไขพิเศษ / SPECIAL TERMS:").Bold().FontSize(8).FontColor(Colors.Grey.Darken1);
                        col.Item().PaddingTop(3).Text(Data.SpecialTerms).FontSize(9);
                    });
                }

                if (!string.IsNullOrEmpty(Data.ChangeSummary))
                {
                    content.Item().PaddingTop(6).Row(r =>
                    {
                        r.ConstantItem(100).Text("Changes:").Bold().FontSize(8).FontColor(Colors.Grey.Darken1);
                        r.RelativeItem().Text(Data.ChangeSummary).FontSize(8).FontColor(Colors.Grey.Darken1);
                    });
                }

                content.Item().PaddingTop(15).Column(disclaimerCol =>
                {
                    disclaimerCol.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten3);
                    disclaimerCol.Item().PaddingTop(6).Text(
                        "This quotation is valid until the specified date. Prices are subject to change after the validity period. " +
                        "E&OE. (Errors and Omissions Excepted)"
                    ).FontSize(7).FontColor(Colors.Grey.Darken1).Italic();
                });

                // Signatures follow content naturally (not pushed to absolute page bottom)
                content.Item().PaddingBottom(60);

                content.Item().Row(row =>
                {
                    row.RelativeItem().Column(col =>
                    {
                        col.Item().Text("ผู้เสนอราคา / Quoted by").FontSize(9).FontColor(Colors.Grey.Darken1);
                        col.Item().PaddingTop(60).LineHorizontal(1).LineColor(Colors.Grey.Medium);
                        col.Item().PaddingTop(10).AlignCenter().Text("Signature & Date").FontSize(8);
                    });

                    row.ConstantItem(40);

                    row.RelativeItem().Column(col =>
                    {
                        col.Item().Text("ผู้อนุมัติ / Approved by").FontSize(9).FontColor(Colors.Grey.Darken1);
                        col.Item().PaddingTop(60).LineHorizontal(1).LineColor(Colors.Grey.Medium);
                        col.Item().PaddingTop(10).AlignCenter().Text("Signature & Date").FontSize(8);
                    });
                });
            });

            page.Footer().Row(row =>
            {
                row.RelativeItem().Text("Maliev Co., Ltd.").FontSize(7);
                row.RelativeItem().AlignCenter().Text(Data.QuotationNumber).FontSize(7);
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

    private static IContainer HeaderCell(IContainer container)
    {
        return container
            .Background(Colors.Black)
            .Padding(5)
            .DefaultTextStyle(x => x.Bold().FontSize(8).FontColor(Colors.White));
    }

    private static IContainer DataCell(IContainer container, string backgroundColor)
    {
        return container
            .BorderBottom(1)
            .BorderColor(Colors.Grey.Lighten3)
            .Background(backgroundColor)
            .Padding(5)
            .DefaultTextStyle(x => x.FontSize(9));
    }
}
