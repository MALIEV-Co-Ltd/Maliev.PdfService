using Maliev.PdfService.Api.Models.Data;
using QRCoder;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.IO;

namespace Maliev.PdfService.Api.Services.Layouts;

/// <summary>
/// QuestPDF layout for Delivery Note documents (ใบส่งของ).
/// </summary>
public class DeliveryNoteDocument : IDocument
{
    /// <summary>The delivery note data.</summary>
    public DeliveryNoteData Data { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="DeliveryNoteDocument"/> class.
    /// </summary>
    public DeliveryNoteDocument(DeliveryNoteData data)
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

                    headerRow.ConstantItem(160).AlignRight().Column(col =>
                    {
                        col.Item().AlignRight().Text("DELIVERY NOTE").FontSize(16).Bold();
                        col.Item().AlignRight().Text("ใบส่งของ").FontSize(14);
                        col.Item().PaddingTop(2).AlignRight().Text($"{Data.DeliveryNoteNumber}").FontSize(11).Bold();
                    });

                    headerRow.ConstantItem(70).AlignRight().Column(col =>
                    {
                        col.Item().PaddingLeft(10).Width(60).Height(60).Image(GenerateQrCode(Data.DeliveryNoteNumber));
                    });
                });

                header.Item().PaddingTop(8).LineHorizontal(1);
            });

            page.Content().PaddingTop(15).PaddingBottom(30).Column(content =>
            {
                // Customer & Delivery Information Section
                content.Item().Row(row =>
                {
                    // Left: Customer Info
                    row.RelativeItem().Column(col =>
                    {
                        col.Item().Text("ผู้รับ / SHIP TO:").Bold().FontSize(9).FontColor(Colors.Grey.Darken1);
                        col.Item().PaddingTop(3).Text(Data.CustomerName).FontSize(11).Bold();
                        col.Item().Text(Data.CustomerAddress).FontSize(9);
                    });

                    // Right: Delivery Contact Info
                    row.RelativeItem().PaddingLeft(20).Column(col =>
                    {
                        col.Item().Text("ผู้ติดต่อรับสินค้า / DELIVERY CONTACT:").Bold().FontSize(9).FontColor(Colors.Grey.Darken1);
                        var hasContact = !string.IsNullOrEmpty(Data.DeliveryContact) || !string.IsNullOrEmpty(Data.DeliveryPhone) || !string.IsNullOrEmpty(Data.CarrierName);
                        if (hasContact)
                        {
                            if (!string.IsNullOrEmpty(Data.DeliveryContact))
                            {
                                col.Item().PaddingTop(3).Text(Data.DeliveryContact).FontSize(10);
                            }
                            if (!string.IsNullOrEmpty(Data.DeliveryPhone))
                            {
                                col.Item().Text($"Tel: {Data.DeliveryPhone}").FontSize(9);
                            }
                            if (!string.IsNullOrEmpty(Data.CarrierName))
                            {
                                col.Item().PaddingTop(3).Text($"Carrier: {Data.CarrierName}").FontSize(9);
                            }
                        }
                        else
                        {
                            col.Item().PaddingTop(3).Text("-").FontSize(9);
                        }
                    });
                });

                var tracking = string.IsNullOrEmpty(Data.TrackingNumber) ? "n/a" : Data.TrackingNumber;

                // Table Caption: Delivery Date & Tracking
                content.Item().PaddingTop(10).Row(captionRow =>
                {
                    captionRow.RelativeItem().Text($"Delivery Date: {Data.DeliveryDate:dd MMM yyyy}").FontSize(8).FontColor(Colors.Grey.Darken1);
                    captionRow.RelativeItem().AlignRight().Text($"Tracking: {tracking}").FontSize(8).FontColor(Colors.Grey.Darken1);
                });

                // Items Table
                content.Item().PaddingTop(3).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(30);  // #
                        columns.ConstantColumn(80);  // Product Code
                        columns.RelativeColumn(3);   // Product Name
                        columns.ConstantColumn(70);  // Ordered
                        columns.ConstantColumn(80);  // Manufactured
                        columns.ConstantColumn(70);  // Delivered
                        columns.ConstantColumn(50);  // Unit
                    });

                    // Header Row
                    table.Header(tableHeader =>
                    {
                        tableHeader.Cell().Element(HeaderCellStyle).Text("#");
                        tableHeader.Cell().Element(HeaderCellStyle).Text("Product Code");
                        tableHeader.Cell().Element(HeaderCellStyle).Text("Product Name");
                        tableHeader.Cell().Element(HeaderCellStyle).AlignRight().Text("Ordered");
                        tableHeader.Cell().Element(HeaderCellStyle).AlignRight().Text("Manufactured");
                        tableHeader.Cell().Element(HeaderCellStyle).AlignRight().Text("Delivered");
                        tableHeader.Cell().Element(HeaderCellStyle).Text("Unit");
                    });

                    // Data Rows
                    int index = 1;
                    foreach (var item in Data.Items)
                    {
                        var isPartialDelivery = item.QuantityDelivered < item.QuantityManufactured;
                        var rowColor = isPartialDelivery ? Colors.Grey.Lighten4 : Colors.White;

                        table.Cell().ShowEntire().Element(cell => DataCellStyle(cell, rowColor)).Text(index.ToString());
                        table.Cell().ShowEntire().Element(cell => DataCellStyle(cell, rowColor)).Text(item.ProductCode);
                        table.Cell().ShowEntire().Element(cell => DataCellStyle(cell, rowColor)).Text(item.ProductName);
                        table.Cell().ShowEntire().Element(cell => DataCellStyle(cell, rowColor)).AlignRight().Text(item.QuantityOrdered.ToString("N2"));
                        table.Cell().ShowEntire().Element(cell => DataCellStyle(cell, rowColor)).AlignRight().Text(item.QuantityManufactured.ToString("N2"));
                        table.Cell().ShowEntire().Element(cell => DataCellStyle(cell, rowColor)).AlignRight().Text(item.QuantityDelivered.ToString("N2")).Bold();
                        table.Cell().ShowEntire().Element(cell => DataCellStyle(cell, rowColor)).Text(item.UnitOfMeasure);

                        index++;
                    }
                });

                // Notes Section - always visible
                content.Item().PaddingTop(15).Column(col =>
                {
                    col.Item().Text("หมายเหตุ / NOTES:").Bold().FontSize(9).FontColor(Colors.Grey.Darken1);
                    col.Item().PaddingTop(3).Text(string.IsNullOrEmpty(Data.Notes) ? "-" : Data.Notes).FontSize(9);
                });

                // Signature block — always at the end of the last page, no page breaks
                content.Item().ShowEntire().PaddingTop(30).Column(sigCol =>
                {
                    sigCol.Item().Row(sigRow =>
                    {
                        sigRow.RelativeItem().Column(col =>
                        {
                            col.Item().Text("Delivered by:").FontSize(9).FontColor(Colors.Grey.Darken1);
                            col.Item().PaddingTop(60).LineHorizontal(1).LineColor(Colors.Grey.Medium);
                            col.Item().PaddingTop(10).AlignCenter().Text("Signature & Date").FontSize(8);
                        });

                        sigRow.ConstantItem(40);

                        sigRow.RelativeItem().Column(col =>
                        {
                            col.Item().Text("Received by:").FontSize(9).FontColor(Colors.Grey.Darken1);
                            col.Item().PaddingTop(60).LineHorizontal(1).LineColor(Colors.Grey.Medium);
                            col.Item().PaddingTop(10).AlignCenter().Text("Signature & Date").FontSize(8);
                        });
                    });
                });
            });

            page.Footer().Row(footerRow =>
            {
                footerRow.RelativeItem().Text("MALIEV Co., Ltd.").FontSize(7);
                footerRow.RelativeItem().AlignCenter().Text(Data.DeliveryNoteNumber).FontSize(7);
                footerRow.RelativeItem().AlignRight().Text(text =>
                {
                    text.Span("Page ").FontSize(7);
                    text.CurrentPageNumber().FontSize(7);
                    text.Span(" of ").FontSize(7);
                    text.TotalPages().FontSize(7);
                });
            });
        });
    }

    /// <summary>
    /// Applies header cell styling.
    /// </summary>
    private static IContainer HeaderCellStyle(IContainer container)
    {
        return container
            .Border(1)
            .BorderColor(Colors.Black)
            .Background(Colors.Black)
            .Padding(5)
            .DefaultTextStyle(x => x.Bold().FontSize(9).FontColor(Colors.White));
    }

    /// <summary>
    /// Applies data cell styling with optional background color.
    /// </summary>
    private static IContainer DataCellStyle(IContainer container, string backgroundColor)
    {
        return container
            .Border(1)
            .BorderColor(Colors.Grey.Lighten2)
            .Background(backgroundColor)
            .Padding(5)
            .DefaultTextStyle(x => x.FontSize(9));
    }

    private static byte[] GenerateQrCode(string deliveryNoteNumber)
    {
        var url = $"https://app.maliev.com/delivery/{deliveryNoteNumber}";
        using var qrGenerator = new QRCodeGenerator();
        using var qrCodeData = qrGenerator.CreateQrCode(url, QRCodeGenerator.ECCLevel.L);
        using var qrCode = new PngByteQRCode(qrCodeData);
        return qrCode.GetGraphic(20);
    }
}
