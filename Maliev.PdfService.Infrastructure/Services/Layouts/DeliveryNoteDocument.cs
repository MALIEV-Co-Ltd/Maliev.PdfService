using Maliev.PdfService.Api.Models.Data;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

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
            page.Margin(40);
            page.DefaultTextStyle(x => x.FontFamily("Kanit").FontSize(10));

            page.Header().Column(header =>
            {
                // Bilingual Title
                header.Item().AlignCenter().Text(text =>
                {
                    text.Span("ใบส่งของ / DELIVERY NOTE").FontSize(20).Bold().FontColor(Colors.Blue.Medium);
                });

                header.Item().PaddingTop(10).Row(row =>
                {
                    // Left: Delivery Note Number
                    row.RelativeItem().Column(col =>
                    {
                        col.Item().Text($"Delivery Note #: {Data.DeliveryNoteNumber}").Bold();
                        if (!string.IsNullOrEmpty(Data.OrderNumber))
                        {
                            col.Item().Text($"Order #: {Data.OrderNumber}").FontSize(9);
                        }
                    });

                    // Right: Date
                    row.RelativeItem().AlignRight().Column(col =>
                    {
                        col.Item().Text($"Date: {Data.DeliveryDate:dd MMM yyyy}");
                        if (!string.IsNullOrEmpty(Data.TrackingNumber))
                        {
                            col.Item().Text($"Tracking: {Data.TrackingNumber}").FontSize(9);
                        }
                    });
                });

                header.Item().PaddingTop(5).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
            });

            page.Content().PaddingVertical(15).Column(content =>
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
                    });
                });

                content.Item().PaddingTop(15).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

                // Items Table
                content.Item().PaddingTop(10).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(30);  // #
                        columns.ConstantColumn(80);  // Product Code
                        columns.RelativeColumn(3);   // Product Name
                        columns.ConstantColumn(60);  // Ordered
                        columns.ConstantColumn(70);  // Manufactured
                        columns.ConstantColumn(60);  // Delivered
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
                        var rowColor = isPartialDelivery ? Colors.Orange.Lighten4 : Colors.White;

                        table.Cell().Element(cell => DataCellStyle(cell, rowColor)).Text(index.ToString());
                        table.Cell().Element(cell => DataCellStyle(cell, rowColor)).Text(item.ProductCode);
                        table.Cell().Element(cell => DataCellStyle(cell, rowColor)).Text(item.ProductName);
                        table.Cell().Element(cell => DataCellStyle(cell, rowColor)).AlignRight().Text(item.QuantityOrdered.ToString("N2"));
                        table.Cell().Element(cell => DataCellStyle(cell, rowColor)).AlignRight().Text(item.QuantityManufactured.ToString("N2"));
                        table.Cell().Element(cell => DataCellStyle(cell, rowColor)).AlignRight().Text(item.QuantityDelivered.ToString("N2")).Bold();
                        table.Cell().Element(cell => DataCellStyle(cell, rowColor)).Text(item.UnitOfMeasure);

                        index++;
                    }
                });

                // Notes Section
                if (!string.IsNullOrEmpty(Data.Notes))
                {
                    content.Item().PaddingTop(15).Column(col =>
                    {
                        col.Item().Text("หมายเหตุ / NOTES:").Bold().FontSize(9).FontColor(Colors.Grey.Darken1);
                        col.Item().PaddingTop(3).Text(Data.Notes).FontSize(9);
                    });
                }

                // Signature Section
                content.Item().PaddingTop(30).Row(row =>
                {
                    // Delivered By
                    row.RelativeItem().Column(col =>
                    {
                        col.Item().Text("ผู้ส่งของ / Delivered by").FontSize(9).FontColor(Colors.Grey.Darken1);
                        col.Item().PaddingTop(30).LineHorizontal(1).LineColor(Colors.Grey.Medium);
                        col.Item().PaddingTop(3).AlignCenter().Text("Signature & Date").FontSize(8);
                    });

                    row.ConstantItem(40); // Spacer

                    // Received By
                    row.RelativeItem().Column(col =>
                    {
                        col.Item().Text("ผู้รับของ / Received by").FontSize(9).FontColor(Colors.Grey.Darken1);
                        col.Item().PaddingTop(30).LineHorizontal(1).LineColor(Colors.Grey.Medium);
                        col.Item().PaddingTop(3).AlignCenter().Text("Signature & Date").FontSize(8);
                    });
                });
            });

            page.Footer().AlignCenter().Column(footer =>
            {
                footer.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten3);
                footer.Item().PaddingTop(5).Text(text =>
                {
                    text.Span("Page ");
                    text.CurrentPageNumber();
                    text.Span(" of ");
                    text.TotalPages();
                    text.Span(" • Generated by Maliev Platform");
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
            .BorderColor(Colors.Grey.Lighten2)
            .Background(Colors.Grey.Lighten3)
            .Padding(5)
            .DefaultTextStyle(x => x.Bold().FontSize(9));
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
}
