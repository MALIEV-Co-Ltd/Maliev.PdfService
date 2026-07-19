using Maliev.PdfService.Api.Models.Data;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Maliev.PdfService.Api.Services.Layouts;

/// <summary>
/// QuestPDF layout for Invoice documents (ใบกำกับภาษี / ใบแจ้งหนี้).
/// Supports Tax Invoice and Standard Invoice types.
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
        var isTaxInvoice = Data.DocumentType == "TaxInvoice";

        container.Page(page =>
        {
            page.Margin(50);
            page.DefaultTextStyle(x => x.FontFamily("Roboto", "Noto Sans Thai"));

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

                    headerRow.ConstantItem(160).AlignRight().Column(col =>
                    {
                        var docTitle = isTaxInvoice ? "TAX INVOICE" : "INVOICE";
                        var docTitleThai = isTaxInvoice ? "ใบกำกับภาษี" : "ใบแจ้งหนี้";
                        col.Item().AlignRight().Text(docTitle).FontSize(16).Bold();
                        col.Item().AlignRight().Text(docTitleThai).FontSize(14);
                        col.Item().PaddingTop(2).AlignRight().Text($"{Data.InvoiceNumber}").FontSize(11).Bold();
                    });
                });

                header.Item().PaddingTop(8).LineHorizontal(1);
            });

            page.Content().Column(content =>
            {
                content.Item().PaddingTop(15).Row(row =>
                {
                    row.RelativeItem().Column(col =>
                    {
                        col.Item().Text("ผู้ซื้อ / BILL TO:").Bold().FontSize(8).FontColor(Colors.Grey.Darken1);
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
                        if (!string.IsNullOrEmpty(Data.BillingAddress))
                            col.Item().Text(Data.BillingAddress).FontSize(8);
                    });

                    row.ConstantItem(200).Column(col =>
                    {
                        col.Item().Row(r =>
                        {
                            r.ConstantItem(100).Text("Issue Date:").FontSize(9).Bold();
                            r.RelativeItem().AlignRight().Text(Data.IssueDate.ToString("dd MMM yyyy")).FontSize(9);
                        });
                        if (Data.DueDate.HasValue)
                        {
                            col.Item().PaddingTop(4).Row(r =>
                            {
                                r.ConstantItem(100).Text("Due Date:").FontSize(9).Bold();
                                r.RelativeItem().AlignRight().Text(Data.DueDate.Value.ToString("dd MMM yyyy")).FontSize(9);
                            });
                        }
                        if (!string.IsNullOrEmpty(Data.PoNumber))
                        {
                            col.Item().PaddingTop(4).Row(r =>
                            {
                                r.ConstantItem(100).Text("P.O. #:").FontSize(9).Bold();
                                r.RelativeItem().AlignRight().Text(Data.PoNumber).FontSize(9);
                            });
                        }
                        if (!string.IsNullOrEmpty(Data.QuotationReference))
                        {
                            col.Item().PaddingTop(4).Row(r =>
                            {
                                r.ConstantItem(100).Text("Quotation Ref:").FontSize(9).Bold();
                                r.RelativeItem().AlignRight().Text(Data.QuotationReference).FontSize(9);
                            });
                        }
                        if (Data.PaymentTermsDays.HasValue && !isTaxInvoice)
                        {
                            col.Item().PaddingTop(4).Row(r =>
                            {
                                r.ConstantItem(100).Text("Payment Terms:").FontSize(9).Bold();
                                r.RelativeItem().AlignRight().Text($"Net {Data.PaymentTermsDays} days").FontSize(9);
                            });
                        }
                    });
                });

                content.Item().PaddingTop(8).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(28);
                        columns.ConstantColumn(65);
                        columns.RelativeColumn(3);
                        columns.ConstantColumn(40);
                        columns.ConstantColumn(40);
                        columns.ConstantColumn(85);
                        columns.ConstantColumn(50);
                        columns.ConstantColumn(85);
                    });

                    table.Header(h =>
                    {
                        h.Cell().Element(HeaderCell).AlignCenter().Text("#");
                        h.Cell().Element(HeaderCell).Text("Code");
                        h.Cell().Element(HeaderCell).Text("Description");
                        h.Cell().Element(HeaderCell).AlignRight().Text("Qty");
                        h.Cell().Element(HeaderCell).Text("Unit");
                        h.Cell().Element(HeaderCell).AlignRight().Text("Unit Price");
                        h.Cell().Element(HeaderCell).AlignRight().Text("Disc %");
                        h.Cell().Element(HeaderCell).AlignRight().Text("Amount");
                    });

                    foreach (var item in Data.Items)
                    {
                        var bg = item.Index % 2 == 0 ? Colors.Grey.Lighten5 : Colors.White;
                        table.Cell().Element(c => DataCell(c, bg).ShowEntire()).AlignCenter().Text(item.Index.ToString());
                        table.Cell().Element(c => DataCell(c, bg).ShowEntire()).Text(item.ItemCode ?? "").FontSize(8);
                        table.Cell().Element(c => DataCell(c, bg).ShowEntire()).Text(item.Description);
                        table.Cell().Element(c => DataCell(c, bg).ShowEntire()).AlignRight().Text(item.Quantity.ToString("N0"));
                        table.Cell().Element(c => DataCell(c, bg).ShowEntire()).Text(item.Unit);
                        table.Cell().Element(c => DataCell(c, bg).ShowEntire()).AlignRight().Text(item.UnitPrice.ToString("N2"));
                        table.Cell().Element(c => DataCell(c, bg).ShowEntire()).AlignRight().Text(
                            item.DiscountPercentage > 0 ? $"{item.DiscountPercentage:N1}%" : "-"
                        );
                        table.Cell().Element(c => DataCell(c, bg).ShowEntire()).AlignRight().Text(item.LineSubtotal.ToString("N2")).Bold();
                    }
                });

                if (!isTaxInvoice && Data.LateFeePercentage.HasValue && Data.LateFeePercentage > 0)
                {
                    content.Item().PaddingTop(6).Text(
                        $"Late payment fee: {Data.LateFeePercentage:N1}% per month will be charged on overdue amounts."
                    ).FontSize(7).Italic();
                }

                var hasPaymentInfo = !string.IsNullOrEmpty(Data.BankName) || !string.IsNullOrEmpty(Data.BankAccountName) || !string.IsNullOrEmpty(Data.BankAccountNumber) || !string.IsNullOrEmpty(Data.BankBranch);

                if (!isTaxInvoice && hasPaymentInfo)
                {
                    content.Item().PaddingTop(8).Row(row =>
                    {
                        row.RelativeItem().Column(payment =>
                        {
                            var paymentInfo = new List<string>();
                            if (!string.IsNullOrEmpty(Data.BankName))
                                paymentInfo.Add($"Bank: {Data.BankName}");
                            if (!string.IsNullOrEmpty(Data.BankAccountName))
                                paymentInfo.Add($"Account Name: {Data.BankAccountName}");
                            if (!string.IsNullOrEmpty(Data.BankAccountNumber))
                                paymentInfo.Add($"Account No: {Data.BankAccountNumber}");
                            if (!string.IsNullOrEmpty(Data.BankBranch))
                                paymentInfo.Add($"Branch: {Data.BankBranch}");

                            payment.Item().Text("ข้อมูลการชำระเงิน / PAYMENT DETAILS:").Bold().FontSize(8).FontColor(Colors.Grey.Darken1);
                            foreach (var info in paymentInfo)
                            {
                                payment.Item().PaddingTop(3).Text(info).FontSize(8);
                            }
                        });

                        row.ConstantItem(40);

                        row.RelativeItem().AlignRight().Column(totals =>
                        {
                            totals.Item().PaddingTop(5).Row(r =>
                            {
                                r.RelativeItem().Text("Subtotal:").FontSize(9);
                                r.ConstantItem(100).AlignRight().Text(Data.Subtotal.ToString("N2")).FontSize(9);
                            });

                            if (Data.TotalDiscountAmount.HasValue && Data.TotalDiscountAmount > 0)
                            {
                                totals.Item().PaddingTop(5).Row(r =>
                                {
                                    r.RelativeItem().Text("Discount:").FontSize(9);
                                    r.ConstantItem(100).AlignRight().Text($"−{Data.TotalDiscountAmount:N2}").FontSize(9);
                                });
                            }

                            if (Data.TaxAmount > 0)
                            {
                                totals.Item().PaddingTop(5).Row(r =>
                                {
                                    r.RelativeItem().Text("VAT (7%):").FontSize(9);
                                    r.ConstantItem(100).AlignRight().Text(Data.TaxAmount.ToString("N2")).FontSize(9);
                                });
                            }

                            if (Data.WithholdingTaxAmount > 0)
                            {
                                var whPercent = Data.WithholdingTaxPercentage.HasValue ? $"{Data.WithholdingTaxPercentage:N1}%" : "";
                                totals.Item().PaddingTop(5).Row(r =>
                                {
                                    r.RelativeItem().Text($"Withholding Tax ({whPercent}):").FontSize(9);
                                    r.ConstantItem(100).AlignRight().Text($"−{Data.WithholdingTaxAmount:N2}").FontSize(9);
                                });
                            }

                            totals.Item().PaddingTop(10).Row(r =>
                            {
                                r.RelativeItem().Text($"TOTAL ({Data.Currency}):").Bold().FontSize(11).LineHeight(1.3f);
                                r.ConstantItem(100).AlignRight().Text(Data.GrandTotal.ToString("N2")).Bold().FontSize(11).LineHeight(1.3f);
                            });
                        });
                    });
                }
                else
                {
                    content.Item().PaddingTop(8).AlignRight().Width(260).Column(totals =>
                    {
                        totals.Item().PaddingTop(5).Row(r =>
                        {
                            r.RelativeItem().Text("Subtotal:").FontSize(9);
                            r.ConstantItem(100).AlignRight().Text(Data.Subtotal.ToString("N2")).FontSize(9);
                        });

                        if (Data.TotalDiscountAmount.HasValue && Data.TotalDiscountAmount > 0)
                        {
                            totals.Item().PaddingTop(5).Row(r =>
                            {
                                r.RelativeItem().Text("Discount:").FontSize(9);
                                r.ConstantItem(100).AlignRight().Text($"−{Data.TotalDiscountAmount:N2}").FontSize(9);
                            });
                        }

                        if (Data.TaxAmount > 0)
                        {
                            totals.Item().PaddingTop(5).Row(r =>
                            {
                                r.RelativeItem().Text("VAT (7%):").FontSize(9);
                                r.ConstantItem(100).AlignRight().Text(Data.TaxAmount.ToString("N2")).FontSize(9);
                            });
                        }

                        if (Data.WithholdingTaxAmount > 0)
                        {
                            var whPercent = Data.WithholdingTaxPercentage.HasValue ? $"{Data.WithholdingTaxPercentage:N1}%" : "";
                            totals.Item().PaddingTop(5).Row(r =>
                            {
                                r.RelativeItem().Text($"Withholding Tax ({whPercent}):").FontSize(9);
                                r.ConstantItem(100).AlignRight().Text($"−{Data.WithholdingTaxAmount:N2}").FontSize(9);
                            });
                        }

                        totals.Item().PaddingTop(10).Row(r =>
                        {
                            r.RelativeItem().Text($"TOTAL ({Data.Currency}):").Bold().FontSize(11).LineHeight(1.3f);
                            r.ConstantItem(100).AlignRight().Text(Data.GrandTotal.ToString("N2")).Bold().FontSize(11).LineHeight(1.3f);
                        });
                    });
                }

                if (!string.IsNullOrEmpty(Data.Notes))
                {
                    content.Item().PaddingTop(8).Column(col =>
                    {
                        col.Item().Text("หมายเหตุ / NOTES:").Bold().FontSize(8).FontColor(Colors.Grey.Darken1);
                        col.Item().PaddingTop(3).Text(Data.Notes).FontSize(8);
                    });
                }

                content.Item().PaddingBottom(60);

                content.Item().ShowEntire().Row(row =>
                {
                    row.RelativeItem().Column(col =>
                    {
                        col.Item().Text("ผู้มีอำนาจลงนาม / Authorized Signatory").FontSize(9).FontColor(Colors.Grey.Darken1);
                        col.Item().PaddingTop(60).LineHorizontal(1).LineColor(Colors.Grey.Medium);
                        col.Item().PaddingTop(10).AlignCenter().Text("Signature & Date").FontSize(8);
                    });

                    row.ConstantItem(40);

                    row.RelativeItem().Column(col =>
                    {
                        col.Item().Text("ผู้รับสินค้า / Received by").FontSize(9).FontColor(Colors.Grey.Darken1);
                        col.Item().PaddingTop(60).LineHorizontal(1).LineColor(Colors.Grey.Medium);
                        col.Item().PaddingTop(10).AlignCenter().Text("Signature & Date").FontSize(8);
                    });
                });
            });

            page.Footer().Row(row =>
            {
                row.RelativeItem().Text("Maliev Co., Ltd.").FontSize(7);
                row.RelativeItem().AlignCenter().Text(Data.InvoiceNumber).FontSize(7);
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
