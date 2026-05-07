using System.Buffers.Binary;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using Maliev.PdfService.Api.Models.Data;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Maliev.PdfService.Api.Services.Layouts;

/// <summary>
/// QuestPDF layout for Quotation documents (ใบเสนอราคา).
/// Shows materials, manufacturing process, validity period, pricing, and terms.
/// </summary>
public class QuotationDocument : IDocument
{
    private const int MaxThumbnailBytes = 2 * 1024 * 1024;
    private const int MaxThumbnailPixels = 1_000_000;
    private const byte PngColorTypeRgb = 2;
    private const byte PngColorTypeRgba = 6;
    private const byte PngFilterNone = 0;
    private const byte PngFilterSub = 1;
    private const byte PngFilterUp = 2;
    private const byte PngFilterAverage = 3;
    private const byte PngFilterPaeth = 4;
    private const byte TransparentAlpha = 0;
    private const byte OpaqueAlpha = 255;
    private const byte WhiteBackgroundThreshold = 245;
    private const string QuotationValidityNotice =
        "This quotation is valid until the specified date. Prices are subject to change after the validity period. " +
        "E&OE. (Errors and Omissions Excepted)";

    private static readonly byte[] PngSignature = [137, 80, 78, 71, 13, 10, 26, 10];

    private static readonly string[] ConfigurationDetailLabels =
    [
        "Bounding box",
        "Surface finish",
        "Tolerance",
        "Inspection",
        "Color",
        "Finish",
        "Infill",
        "Layer height",
        "Surface roughness"
    ];

    private static readonly HttpClient ThumbnailHttpClient = new()
    {
        Timeout = TimeSpan.FromSeconds(2)
    };

    private readonly Dictionary<string, byte[]?> _thumbnailImageCache = new(StringComparer.Ordinal);

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

            page.Header().Column(header =>
            {
                header.Item().Row(headerRow =>
                {
                    headerRow.RelativeItem().Column(col =>
                    {
                        col.Item().Height(20).Width(60).Svg(ReadResourceText("MALIEV_BLACK.svg"));
                        col.Item().PaddingTop(3).Text("MALIEV Co., Ltd. (Head Office) | สำนักงานใหญ่").FontSize(8).Bold();
                        col.Item().Text("36/1 Moo 3, Khlong Khoi, Pak Kret, Nonthaburi 11120").FontSize(8).FontColor(Colors.Grey.Darken1);
                        col.Item().Text("บริษัท มาลีฟ จำกัด 36/1 หมู่ 3 ต.คลองข่อย อ.ปากเกร็ด จ.นนทบุรี 11120").FontSize(8).FontColor(Colors.Grey.Darken1);
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
                content.Item().PaddingBottom(12).Row(row =>
                {
                    row.RelativeItem().Column(col =>
                    {
                        col.Item().Text("เสนอให้ / QUOTE TO:").Bold().FontSize(8).FontColor(Colors.Grey.Darken1);
                        col.Item().PaddingTop(3).Element(ComposeCustomerLines);

                        var billingAddressLines = GetLines(Data.BillingAddressLines, Data.BillingAddress ?? Data.CustomerAddress);
                        var shippingAddressLines = GetLines(Data.ShippingAddressLines, Data.ShippingAddress ?? Data.BillingAddress ?? Data.CustomerAddress);
                        if (shippingAddressLines.Count == 0 && billingAddressLines.Count > 0)
                        {
                            shippingAddressLines = billingAddressLines;
                        }

                        col.Item().PaddingTop(10).Row(addressRow =>
                        {
                            addressRow.RelativeItem().Element(container => ComposeAddressBlock(container, "Billing address", billingAddressLines));
                            addressRow.ConstantItem(22);
                            addressRow.RelativeItem().Element(container => ComposeAddressBlock(container, "Shipping address", shippingAddressLines));
                        });
                    });

                    row.ConstantItem(150).AlignRight().Table(dateTable =>
                    {
                        dateTable.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(64);
                            columns.RelativeColumn(1);
                        });

                        dateTable.Cell().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(4).Text("Issue Date").FontSize(8).Bold();
                        dateTable.Cell().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(4).AlignRight().Text(Data.QuotationDate.ToString("dd MMM yyyy")).FontSize(8);

                        dateTable.Cell().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(4).Text("Valid Until").FontSize(8).Bold();
                        dateTable.Cell().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(4).AlignRight().Text(Data.ValidityEnd.ToString("dd MMM yyyy")).FontSize(8).Bold();
                    });
                });

                content.Item()
                    .PaddingTop(6)
                    .PaddingBottom(4)
                    .AlignRight()
                    .Text(QuotationValidityNotice)
                    .FontSize(7)
                    .FontColor(Colors.Grey.Darken1);

                content.Item().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(22);
                        columns.RelativeColumn(4.3f);
                        columns.RelativeColumn(1.4f);
                        columns.ConstantColumn(28);
                        columns.ConstantColumn(26);
                        columns.ConstantColumn(68);
                        columns.ConstantColumn(70);
                    });

                    table.Header(h =>
                    {
                        h.Cell().Element(HeaderCell).AlignCenter().Text("#");
                        h.Cell().Element(HeaderCell).Text("Service / บริการ");
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
                            col.Item().Row(serviceRow =>
                            {
                                if (TryComposeThumbnail(serviceRow, item.ThumbnailUrl))
                                {
                                    serviceRow.ConstantItem(6);
                                }

                                serviceRow.RelativeItem().Column(serviceCol =>
                                {
                                    serviceCol.Item().Text(item.PartName ?? item.MaterialName).Bold();
                                    var detailLines = GetLines(item.DetailLines, null);
                                    var noteLines = GetLines([], item.Notes);
                                    var normalizedNoteLines = noteLines
                                        .Select(NormalizeManufacturingNoteText)
                                        .ToHashSet(StringComparer.OrdinalIgnoreCase);
                                    foreach (var detailLine in detailLines)
                                    {
                                        if (IsManufacturingNoteDetailLine(detailLine, normalizedNoteLines))
                                            continue;

                                        ComposeServiceDetailLine(serviceCol, detailLine);
                                    }

                                    var manufacturingNoteLines = noteLines
                                        .Where(IsManufacturingNoteLine)
                                        .Select(NormalizeManufacturingNoteText)
                                        .Distinct(StringComparer.OrdinalIgnoreCase)
                                        .ToList();
                                    ComposeManufacturingNoteLines(serviceCol, manufacturingNoteLines);
                                });
                            });
                        });
                        table.Cell().Element(c => DataCell(c, bg).ShowEntire()).Column(col =>
                        {
                            col.Item().Text(item.ManufacturingProcess ?? string.Empty);
                            if (!string.IsNullOrWhiteSpace(item.MaterialName))
                                col.Item().Text(item.MaterialName).FontSize(7).FontColor(Colors.Grey.Darken1);
                        });
                        table.Cell().Element(c => DataCell(c, bg).ShowEntire()).AlignRight().Text(item.Quantity.ToString("N0"));
                        table.Cell().Element(c => DataCell(c, bg).ShowEntire()).Text(item.QuantityUnit);
                        table.Cell().Element(c => DataCell(c, bg).ShowEntire()).AlignRight().Text(item.UnitPrice.ToString("N2"));
                        table.Cell().Element(c => DataCell(c, bg).ShowEntire()).AlignRight().Text(item.LineTotal.ToString("N2")).Bold();
                    }
                });

                content.Item().PaddingTop(8).Row(summary =>
                {
                    summary.RelativeItem().Column(leadTime =>
                    {
                        leadTime.Item().Text("Lead Time:").Bold().FontSize(8.5f);
                        leadTime.Item()
                            .PaddingTop(2)
                            .Text(string.IsNullOrWhiteSpace(Data.DeliveryExpectations)
                                ? "To be confirmed after project review"
                                : Data.DeliveryExpectations)
                            .FontSize(8.5f);
                    });

                    summary.ConstantItem(24);
                    summary.ConstantItem(190).Element(ComposeTotals);
                });

                var displayDiscounts = Data.Discounts
                    .Where(discount => !string.Equals(discount.Conditions, "Manual discount", StringComparison.OrdinalIgnoreCase))
                    .ToList();

                if (displayDiscounts.Count > 0)
                {
                    content.Item().PaddingTop(8).Column(discCol =>
                    {
                        discCol.Item().Text("ส่วนลด / DISCOUNTS:").Bold().FontSize(8).FontColor(Colors.Grey.Darken1);
                        foreach (var discount in displayDiscounts)
                        {
                            discCol.Item().PaddingTop(2).Text(text =>
                            {
                                var valueStr = discount.DiscountType == "Percentage"
                                    ? $"{discount.DiscountValue:N1}%"
                                    : $"{Data.Currency} {discount.DiscountValue:N2}";
                                text.Span($"  • {discount.DiscountType}: {valueStr}").FontSize(8);
                                if (!string.IsNullOrEmpty(discount.Conditions))
                                    text.Span($" — {discount.Conditions}").FontSize(8).FontColor(Colors.Grey.Darken1);
                            });
                        }
                    });
                }

                if (!string.IsNullOrEmpty(Data.SpecialTerms))
                {
                    content.Item()
                        .PaddingTop(12)
                        .ShowEntire()
                        .Column(col =>
                        {
                            col.Item().Text("เงื่อนไข / TERMS").Bold().FontSize(8).FontColor(Colors.Grey.Darken1);
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

                content.Item().ExtendVertical().AlignBottom().PaddingBottom(4).ShowEntire().Row(row =>
                {
                    row.RelativeItem().Element(QuotedByBlock);
                    row.ConstantItem(40);
                    row.RelativeItem().Element(container => SignatureBox(container, "ผู้อนุมัติ / Approved by"));
                });
            });

            page.Footer().Column(footer =>
            {
                footer.Item().Row(row =>
                {
                    row.RelativeItem().Text("Maliev Co., Ltd.").FontSize(7);
                    row.RelativeItem().AlignCenter().Text(string.Empty).FontSize(7);
                    row.RelativeItem().AlignRight().Text(text =>
                    {
                        text.Span("Page ").FontSize(7);
                        text.CurrentPageNumber().FontSize(7);
                        text.Span(" of ").FontSize(7);
                        text.TotalPages().FontSize(7);
                    });
                });
            });
        });
    }

    private static void ComposeServiceDetailLine(ColumnDescriptor serviceCol, string detailLine)
    {
        if (TryParseDrawingDetailLine(detailLine, out var drawings))
        {
            serviceCol.Item().Text("Drawings:").FontSize(7).Bold().FontColor(Colors.Grey.Darken1).LineHeight(1.25f);
            foreach (var drawing in drawings)
                serviceCol.Item().PaddingLeft(6).Text($"- {drawing}").FontSize(7).FontColor(Colors.Grey.Darken1).LineHeight(1.25f);

            return;
        }

        serviceCol.Item().Text(detailLine).FontSize(7).FontColor(Colors.Grey.Darken1).LineHeight(1.25f);
    }

    private static void ComposeManufacturingNoteLines(ColumnDescriptor serviceCol, IReadOnlyList<string> noteLines)
    {
        if (noteLines.Count == 0)
            return;

        serviceCol.Item().PaddingTop(4).Text("Note:").FontSize(7).Bold().FontColor(Colors.Grey.Darken2).LineHeight(1.25f);
        foreach (var noteLine in noteLines)
            serviceCol.Item().PaddingLeft(6).Text(noteLine).FontSize(7).FontColor(Colors.Grey.Darken2).LineHeight(1.25f);
    }

    private static bool IsManufacturingNoteDetailLine(string detailLine, HashSet<string> normalizedNoteLines) =>
        normalizedNoteLines.Contains(NormalizeManufacturingNoteText(detailLine)) &&
        IsManufacturingNoteLine(detailLine);

    private static bool IsManufacturingNoteLine(string noteLine) =>
        !IsConfigurationDetailLine(noteLine) &&
        !TryParseDrawingDetailLine(noteLine, out _);

    private static bool IsConfigurationDetailLine(string detailLine)
    {
        var normalizedLine = NormalizeManufacturingNoteText(detailLine);
        var separatorIndex = normalizedLine.IndexOf(':', StringComparison.Ordinal);
        if (separatorIndex <= 0)
            return false;

        var label = normalizedLine[..separatorIndex].Trim();
        return ConfigurationDetailLabels.Contains(label, StringComparer.OrdinalIgnoreCase);
    }

    private static string NormalizeManufacturingNoteText(string value)
    {
        var normalizedValue = value.Trim();
        normalizedValue = normalizedValue.StartsWith("Note:", StringComparison.OrdinalIgnoreCase)
            ? normalizedValue["Note:".Length..].Trim()
            : normalizedValue;

        return string.Join(' ', normalizedValue.Split(default(string[]), StringSplitOptions.RemoveEmptyEntries));
    }

    private static bool TryParseDrawingDetailLine(string detailLine, out List<string> drawings)
    {
        drawings = [];
        var normalizedLine = NormalizeManufacturingNoteText(detailLine);
        var separatorIndex = normalizedLine.IndexOf(':', StringComparison.Ordinal);
        if (separatorIndex <= 0)
            return false;

        var label = normalizedLine[..separatorIndex].Trim();
        if (!string.Equals(label, "Drawing", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(label, "Drawings", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        drawings = normalizedLine[(separatorIndex + 1)..]
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(drawing => !string.IsNullOrWhiteSpace(drawing))
            .ToList();

        return drawings.Count > 0;
    }

    private bool TryComposeThumbnail(RowDescriptor row, string? thumbnailReference)
    {
        var thumbnailBytes = TryGetThumbnailBytes(thumbnailReference);
        if (thumbnailBytes is not { Length: > 0 })
            return false;

        try
        {
            row.ConstantItem(34).Height(34).Image(thumbnailBytes);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private void ComposeTotals(IContainer container)
    {
        var discountAmount = Data.ManualDiscountAmount > 0 ? Data.ManualDiscountAmount : Data.TotalDiscount;
        var subtotal = Data.SubtotalBeforeDiscount > 0
            ? Data.SubtotalBeforeDiscount
            : Math.Max(0, Data.Subtotal + discountAmount - Data.ShippingCost);

        container.AlignRight().Width(190).Column(totals =>
        {
            AddTotalRow(totals, "Subtotal:", subtotal);
            AddTotalRow(totals, "Discount:", discountAmount, isDiscount: true);
            AddTotalRow(totals, "Shipping:", Data.ShippingCost);
            AddTotalRow(totals, "VAT (7%):", Data.TaxAmount);

            totals.Item().PaddingTop(2).DefaultTextStyle(x => x.Bold().FontSize(10)).Row(r =>
            {
                r.RelativeItem().Text($"TOTAL ({Data.Currency}):").LineHeight(1.35f);
                r.ConstantItem(82).AlignRight().Text(Data.TotalAmount.ToString("N2")).LineHeight(1.35f);
            });
        });
    }

    private static void AddTotalRow(ColumnDescriptor totals, string label, decimal amount, bool isDiscount = false)
    {
        var value = amount > 0 && isDiscount ? $"-{amount:N2}" : amount.ToString("N2");

        totals.Item().DefaultTextStyle(x => x.FontSize(8.5f)).Row(r =>
        {
            r.RelativeItem().Text(label).LineHeight(1.35f);
            r.ConstantItem(82).AlignRight().Text(value).LineHeight(1.35f);
        });
    }

    private byte[]? TryGetThumbnailBytes(string? thumbnailReference)
    {
        if (string.IsNullOrWhiteSpace(thumbnailReference))
            return null;

        if (_thumbnailImageCache.TryGetValue(thumbnailReference, out var cachedBytes))
            return cachedBytes;

        var bytes = LoadThumbnailBytes(thumbnailReference);
        if (bytes is not null)
            bytes = PrepareThumbnailBytes(bytes);

        _thumbnailImageCache[thumbnailReference] = bytes;
        return bytes;
    }

    private static byte[] PrepareThumbnailBytes(byte[] bytes) =>
        TryRemoveWhitePngBackground(bytes) ?? bytes;

    private static byte[]? LoadThumbnailBytes(string thumbnailReference)
    {
        try
        {
            if (Uri.TryCreate(thumbnailReference, UriKind.Absolute, out var uri)
                && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
            {
                return LoadRemoteThumbnailBytes(uri);
            }

            if (!File.Exists(thumbnailReference))
                return null;

            var fileInfo = new FileInfo(thumbnailReference);
            if (fileInfo.Length is <= 0 or > MaxThumbnailBytes)
                return null;

            return File.ReadAllBytes(thumbnailReference);
        }
        catch (Exception ex) when (ex is HttpRequestException
            or IOException
            or NotSupportedException
            or OperationCanceledException
            or UnauthorizedAccessException)
        {
            return null;
        }
    }

    private static byte[]? LoadRemoteThumbnailBytes(Uri uri)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, uri);
        using var response = ThumbnailHttpClient.Send(request, HttpCompletionOption.ResponseHeadersRead);
        if (!response.IsSuccessStatusCode)
            return null;

        var contentLength = response.Content.Headers.ContentLength;
        if (contentLength is <= 0 or > MaxThumbnailBytes)
            return null;

        using var stream = response.Content.ReadAsStream();
        using var buffer = new MemoryStream();
        var readBuffer = new byte[81920];

        int bytesRead;
        while ((bytesRead = stream.Read(readBuffer, 0, readBuffer.Length)) > 0)
        {
            if (buffer.Length + bytesRead > MaxThumbnailBytes)
                return null;

            buffer.Write(readBuffer, 0, bytesRead);
        }

        return buffer.Length > 0 ? buffer.ToArray() : null;
    }

    private static byte[]? TryRemoveWhitePngBackground(byte[] bytes)
    {
        try
        {
            return TryDecodePng(bytes, out var width, out var height, out var rgba)
                ? EncodePngWithTransparentBackground(width, height, rgba)
                : null;
        }
        catch (Exception ex) when (ex is InvalidDataException
            or IOException
            or NotSupportedException
            or OverflowException)
        {
            return null;
        }
    }

    private static bool TryDecodePng(byte[] bytes, out int width, out int height, out byte[] rgba)
    {
        width = 0;
        height = 0;
        rgba = [];

        if (bytes.Length < 33 || !bytes.AsSpan(0, PngSignature.Length).SequenceEqual(PngSignature))
            return false;

        byte bitDepth = 0;
        byte colorType = 0;
        byte interlaceMethod = 0;
        using var idat = new MemoryStream();

        var offset = PngSignature.Length;
        while (offset + 12 <= bytes.Length)
        {
            var length = BinaryPrimitives.ReadInt32BigEndian(bytes.AsSpan(offset, 4));
            if (length < 0 || offset + 12 + length > bytes.Length)
                return false;

            var type = bytes.AsSpan(offset + 4, 4);
            var dataOffset = offset + 8;

            if (type.SequenceEqual("IHDR"u8))
            {
                width = BinaryPrimitives.ReadInt32BigEndian(bytes.AsSpan(dataOffset, 4));
                height = BinaryPrimitives.ReadInt32BigEndian(bytes.AsSpan(dataOffset + 4, 4));
                bitDepth = bytes[dataOffset + 8];
                colorType = bytes[dataOffset + 9];
                interlaceMethod = bytes[dataOffset + 12];
            }
            else if (type.SequenceEqual("IDAT"u8))
            {
                idat.Write(bytes, dataOffset, length);
            }
            else if (type.SequenceEqual("IEND"u8))
            {
                break;
            }

            offset += length + 12;
        }

        if (width <= 0 || height <= 0 ||
            (long)width * height > MaxThumbnailPixels ||
            bitDepth != 8 || interlaceMethod != 0 ||
            colorType is not (PngColorTypeRgb or PngColorTypeRgba))
        {
            return false;
        }

        var bytesPerPixel = colorType == PngColorTypeRgba ? 4 : 3;
        var stride = checked(width * bytesPerPixel);
        var expectedLength = checked((stride + 1) * height);

        idat.Position = 0;
        using var zlib = new ZLibStream(idat, CompressionMode.Decompress);
        using var decompressed = new MemoryStream(expectedLength);
        zlib.CopyTo(decompressed);
        var decompressedBytes = decompressed.ToArray();
        if (decompressedBytes.Length != expectedLength)
            return false;

        var scanlines = UnfilterPngScanlines(decompressedBytes, width, height, bytesPerPixel);
        rgba = ConvertToRgba(scanlines, width, height, colorType);
        MakeEdgeConnectedWhitePixelsTransparent(rgba, width, height);

        return true;
    }

    private static byte[] UnfilterPngScanlines(byte[] decompressedBytes, int width, int height, int bytesPerPixel)
    {
        var stride = width * bytesPerPixel;
        var scanlines = new byte[stride * height];
        var sourceOffset = 0;

        for (var y = 0; y < height; y++)
        {
            var filter = decompressedBytes[sourceOffset++];
            var rowOffset = y * stride;
            for (var x = 0; x < stride; x++)
            {
                var raw = decompressedBytes[sourceOffset++];
                var left = x >= bytesPerPixel ? scanlines[rowOffset + x - bytesPerPixel] : 0;
                var up = y > 0 ? scanlines[rowOffset + x - stride] : 0;
                var upLeft = x >= bytesPerPixel && y > 0 ? scanlines[rowOffset + x - stride - bytesPerPixel] : 0;

                scanlines[rowOffset + x] = filter switch
                {
                    PngFilterNone => raw,
                    PngFilterSub => unchecked((byte)(raw + left)),
                    PngFilterUp => unchecked((byte)(raw + up)),
                    PngFilterAverage => unchecked((byte)(raw + ((left + up) / 2))),
                    PngFilterPaeth => unchecked((byte)(raw + PaethPredictor(left, up, upLeft))),
                    _ => throw new InvalidDataException("Unsupported PNG filter.")
                };
            }
        }

        return scanlines;
    }

    private static byte[] ConvertToRgba(byte[] scanlines, int width, int height, byte colorType)
    {
        var sourceBytesPerPixel = colorType == PngColorTypeRgba ? 4 : 3;
        var rgba = new byte[width * height * 4];
        var sourceOffset = 0;
        var targetOffset = 0;

        for (var i = 0; i < width * height; i++)
        {
            rgba[targetOffset++] = scanlines[sourceOffset++];
            rgba[targetOffset++] = scanlines[sourceOffset++];
            rgba[targetOffset++] = scanlines[sourceOffset++];
            rgba[targetOffset++] = sourceBytesPerPixel == 4 ? scanlines[sourceOffset++] : OpaqueAlpha;
        }

        return rgba;
    }

    private static byte PaethPredictor(int left, int up, int upLeft)
    {
        var estimate = left + up - upLeft;
        var leftDistance = Math.Abs(estimate - left);
        var upDistance = Math.Abs(estimate - up);
        var upLeftDistance = Math.Abs(estimate - upLeft);

        if (leftDistance <= upDistance && leftDistance <= upLeftDistance)
            return (byte)left;

        return upDistance <= upLeftDistance ? (byte)up : (byte)upLeft;
    }

    private static void MakeEdgeConnectedWhitePixelsTransparent(byte[] rgba, int width, int height)
    {
        var visited = new bool[width * height];
        var queue = new Queue<int>();

        for (var x = 0; x < width; x++)
        {
            EnqueueIfBackground(x, 0);
            EnqueueIfBackground(x, height - 1);
        }

        for (var y = 1; y < height - 1; y++)
        {
            EnqueueIfBackground(0, y);
            EnqueueIfBackground(width - 1, y);
        }

        while (queue.Count > 0)
        {
            var pixelIndex = queue.Dequeue();
            var x = pixelIndex % width;
            var y = pixelIndex / width;
            rgba[(pixelIndex * 4) + 3] = TransparentAlpha;

            EnqueueIfBackground(x - 1, y);
            EnqueueIfBackground(x + 1, y);
            EnqueueIfBackground(x, y - 1);
            EnqueueIfBackground(x, y + 1);
        }

        void EnqueueIfBackground(int x, int y)
        {
            if (x < 0 || y < 0 || x >= width || y >= height)
                return;

            var pixelIndex = (y * width) + x;
            if (visited[pixelIndex])
                return;

            visited[pixelIndex] = true;
            var offset = pixelIndex * 4;
            if (rgba[offset + 3] > TransparentAlpha &&
                rgba[offset] >= WhiteBackgroundThreshold &&
                rgba[offset + 1] >= WhiteBackgroundThreshold &&
                rgba[offset + 2] >= WhiteBackgroundThreshold)
            {
                queue.Enqueue(pixelIndex);
            }
        }
    }

    private static byte[] EncodePngWithTransparentBackground(int width, int height, byte[] rgba)
    {
        using var imageData = new MemoryStream();
        using (var zlib = new ZLibStream(imageData, CompressionLevel.Optimal, leaveOpen: true))
        {
            for (var y = 0; y < height; y++)
            {
                zlib.WriteByte(PngFilterNone);
                zlib.Write(rgba, y * width * 4, width * 4);
            }
        }

        using var png = new MemoryStream();
        png.Write(PngSignature);

        Span<byte> header = stackalloc byte[13];
        BinaryPrimitives.WriteInt32BigEndian(header, width);
        BinaryPrimitives.WriteInt32BigEndian(header[4..], height);
        header[8] = 8;
        header[9] = PngColorTypeRgba;
        WritePngChunk(png, "IHDR"u8, header);
        WritePngChunk(png, "IDAT"u8, imageData.ToArray());
        WritePngChunk(png, "IEND"u8, []);

        return png.ToArray();
    }

    private static void WritePngChunk(Stream stream, ReadOnlySpan<byte> type, ReadOnlySpan<byte> data)
    {
        Span<byte> length = stackalloc byte[4];
        BinaryPrimitives.WriteInt32BigEndian(length, data.Length);
        stream.Write(length);
        stream.Write(type);
        stream.Write(data);

        using var crcInput = new MemoryStream();
        crcInput.Write(type);
        crcInput.Write(data);

        Span<byte> crc = stackalloc byte[4];
        BinaryPrimitives.WriteUInt32BigEndian(crc, PngCrc32(crcInput.ToArray()));
        stream.Write(crc);
    }

    private static uint PngCrc32(byte[] bytes)
    {
        var crc = 0xffffffffu;
        foreach (var value in bytes)
        {
            crc ^= value;
            for (var bit = 0; bit < 8; bit++)
                crc = (crc & 1) == 1 ? (crc >> 1) ^ 0xedb88320u : crc >> 1;
        }

        return crc ^ 0xffffffffu;
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

    private void ComposeCustomerLines(IContainer container)
    {
        var lines = Data.CustomerDisplayLines.Count > 0
            ? Data.CustomerDisplayLines
            : BuildFallbackCustomerLines();

        container.Column(col =>
        {
            for (var i = 0; i < lines.Count; i++)
            {
                var text = col.Item().Text(lines[i]).FontSize(9);
                if (Data.CustomerType == "Corporate" && i == 0)
                    text.Bold();
                else
                    text.FontColor(Colors.Grey.Darken1);
            }
        });
    }

    private List<string> BuildFallbackCustomerLines()
    {
        var lines = new List<string>();
        var branch = string.IsNullOrEmpty(Data.CustomerBranch) ? string.Empty : $" ({Data.CustomerBranch})";

        lines.Add(Data.CustomerType == "Corporate" ? $"{Data.CustomerName}{branch}" : Data.CustomerName);

        if (!string.IsNullOrEmpty(Data.ContactPerson))
        {
            var contact = string.IsNullOrEmpty(Data.CustomerPhone)
                ? Data.ContactPerson
                : $"{Data.ContactPerson} ({Data.CustomerPhone})";
            lines.Add(Data.CustomerType == "Corporate" ? $"Attn: {contact}" : contact);
        }
        else if (!string.IsNullOrEmpty(Data.CustomerPhone))
        {
            lines.Add(Data.CustomerPhone);
        }

        return lines;
    }

    private static void ComposeAddressBlock(IContainer container, string title, IReadOnlyList<string> addressLines)
    {
        container.Column(col =>
        {
            col.Item().Text(title).FontSize(7).Bold().FontColor(Colors.Grey.Darken1);
            IReadOnlyList<string> lines = addressLines.Count == 0 ? ["-"] : addressLines;
            foreach (var line in lines)
                col.Item().Text(line).FontSize(8);
        });
    }

    private static List<string> GetLines(IReadOnlyList<string> lines, string? fallback)
    {
        if (lines.Count > 0)
            return lines.SelectMany(SplitTextLines).ToList();

        if (string.IsNullOrWhiteSpace(fallback))
            return [];

        return SplitTextLines(fallback).ToList();
    }

    private static IEnumerable<string> SplitTextLines(string value) =>
        value.Split(["\r\n", "\n", "\r", " | "], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(line => !string.IsNullOrWhiteSpace(line));

    private void QuotedByBlock(IContainer container)
    {
        var name = FirstNonEmpty(Data.QuotedByName, Data.QuotedByEmail, "MALIEV");

        container.Column(col =>
        {
            col.Item().Text("ผู้เสนอราคา / Quoted by").FontSize(9).FontColor(Colors.Grey.Darken1);
            col.Item().PaddingTop(8).Text(name).Bold().FontSize(10);

            if (!string.IsNullOrWhiteSpace(Data.QuotedByEmail) && !string.Equals(Data.QuotedByEmail, name, StringComparison.OrdinalIgnoreCase))
                col.Item().PaddingTop(2).Text(Data.QuotedByEmail).FontSize(8).FontColor(Colors.Grey.Darken1);

            if (!string.IsNullOrWhiteSpace(Data.QuotedByPhone))
                col.Item().PaddingTop(2).Text(Data.QuotedByPhone).FontSize(8).FontColor(Colors.Grey.Darken1);

            if (Data.QuotedAt.HasValue)
                col.Item().PaddingTop(2).Text($"Quoted on: {Data.QuotedAt.Value:dd MMM yyyy HH:mm}").FontSize(8).FontColor(Colors.Grey.Darken1);
        });
    }

    private static void SignatureBox(IContainer container, string title)
    {
        container.Column(col =>
        {
            col.Item().Text(title).FontSize(9).FontColor(Colors.Grey.Darken1);
            col.Item().PaddingTop(42).LineHorizontal(1).LineColor(Colors.Grey.Medium);
            col.Item().PaddingTop(6).AlignCenter().Text("Signature & Date").FontSize(8);
        });
    }

    private static string FirstNonEmpty(params string?[] values) =>
        values.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value)) ?? string.Empty;

    private static string ReadResourceText(string fileName)
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Resources", fileName);
        return File.ReadAllText(path);
    }
}
