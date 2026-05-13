using Maliev.PdfService.Api.Models.Data;
using QRCoder;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.IO;
using System.IO;

namespace Maliev.PdfService.Api.Services.Layouts;

/// <summary>
/// QuestPDF layout for Job Ticket documents — shop floor manufacturing work orders.
/// Inspired by Jobboss-style job tickets; shows all information operators need at the machine.
/// </summary>
public class JobTicketDocument : IDocument
{
    /// <summary>The job ticket data.</summary>
    public JobTicketData Data { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="JobTicketDocument"/> class.
    /// </summary>
    public JobTicketDocument(JobTicketData data)
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
            page.Margin(35);
            page.DefaultTextStyle(x => x.FontFamily("Roboto", "Noto Sans Thai").FontSize(9));

            page.Header().Column(header =>
            {
                // ─── Top Bar ─────────────────────────────────────────────
                header.Item().Row(row =>
                {
                    // Company name + label
                    row.RelativeItem().Column(col =>
                    {
                        col.Item().Height(20).Width(60).Svg(DocumentResources.ReadText("MALIEV_BLACK.svg"));
                    });

                    // Priority badge + job number
                    row.ConstantItem(180).AlignRight().Column(col =>
                    {
                        col.Item().AlignRight().Text($"#{Data.JobTicketNumber}").FontSize(14).Bold().FontColor(Colors.Black);
                        var priorityColor = Data.Priority <= 2 ? Colors.Red.Medium : Data.Priority <= 4 ? Colors.Orange.Medium : Colors.Grey.Darken1;
                        var priorityLabel = Data.Priority <= 2 ? "HIGH PRIORITY" : Data.Priority <= 4 ? "MEDIUM" : "NORMAL";
                        col.Item().PaddingTop(3).AlignRight().Text(priorityLabel).FontSize(9).Bold().FontColor(priorityColor);
                    });

                    // QR Code in header - rightmost
                    row.ConstantItem(60).AlignRight().Column(col =>
                    {
                        col.Item().AlignRight().Width(50).Height(50).Image(GenerateQrCode(Data.JobId));
                    });
                });

                header.Item().PaddingTop(6).LineHorizontal(1).LineColor(Colors.Black);
            });

            page.Content().PaddingVertical(10).Column(content =>
            {
                // ─── Job Overview Box ─────────────────────────────────────
                content.Item().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(8).Column(box =>
                {
                    box.Item().Row(row =>
                    {
                        // Left Column: Part / Order Info
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Text("ชื่อชิ้นงาน / PART NAME").FontSize(7).FontColor(Colors.Grey.Darken1);
                            col.Item().Text(Data.PartName).FontSize(13).Bold();
                            col.Item().PaddingTop(6).Text("ลูกค้า / CUSTOMER").FontSize(7).FontColor(Colors.Grey.Darken1);
                            col.Item().Text(Data.CustomerName).FontSize(10).Bold();
                        });

                        // Center: Quantities & Deadline
                        row.ConstantItem(150).Column(col =>
                        {
                            col.Item().Text("จำนวน / QTY").FontSize(7).FontColor(Colors.Grey.Darken1);
                            col.Item().Text(Data.Quantity.ToString("N0")).FontSize(18).Bold().FontColor(Colors.Black);
                            if (Data.DeliveryDeadline.HasValue)
                            {
                                col.Item().PaddingTop(6).Text("กำหนดส่ง / DEADLINE").FontSize(7).FontColor(Colors.Grey.Darken1);
                                col.Item().Text(Data.DeliveryDeadline.Value.ToString("dd MMM yyyy")).FontSize(11).Bold().FontColor(Colors.Black);
                            }
                        });

                        // Right: Job IDs
                        row.ConstantItem(160).Column(col =>
                        {
                            col.Item().Text("ออกวันที่ / ISSUED").FontSize(7).FontColor(Colors.Grey.Darken1);
                            col.Item().Text(Data.IssuedDate.ToString("dd MMM yyyy")).FontSize(9);
                            col.Item().PaddingTop(4).Text("Job ID").FontSize(7).FontColor(Colors.Grey.Darken1);
                            col.Item().Text(Data.JobId).FontSize(8).FontColor(Colors.Grey.Darken2);
                            if (!string.IsNullOrEmpty(Data.OrderReference))
                            {
                                col.Item().PaddingTop(4).Text("Order Ref").FontSize(7).FontColor(Colors.Grey.Darken1);
                                col.Item().Text(Data.OrderReference).FontSize(8);
                            }
                        });
                    });
                });

                // ─── Material & Process ───────────────────────────────────
                content.Item().PaddingTop(8).Row(row =>
                {
                    // Material block
                    row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(8).Column(col =>
                    {
                        col.Item().Text("วัสดุ / MATERIAL").FontSize(7).Bold().FontColor(Colors.Grey.Darken1);
                        col.Item().PaddingTop(4).Text(Data.MaterialName).FontSize(11).Bold();
                        if (!string.IsNullOrEmpty(Data.ColorName))
                        {
                            col.Item().PaddingTop(2).Text($"Color: {Data.ColorName}").FontSize(9);
                        }
                        if (!string.IsNullOrEmpty(Data.SurfaceFinishing))
                        {
                            col.Item().PaddingTop(2).Text($"Finish: {Data.SurfaceFinishing}").FontSize(9);
                        }
                    });

                    row.ConstantItem(8);

                    // Technology block
                    row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten2).Background(Colors.Grey.Lighten4).Padding(8).Column(col =>
                    {
                        col.Item().Text("กระบวนการ / PROCESS").FontSize(7).Bold().FontColor(Colors.Black);
                        col.Item().PaddingTop(4).Text(Data.Technology).FontSize(14).Bold().FontColor(Colors.Black);
                        if (!string.IsNullOrEmpty(Data.AssignedMachine))
                        {
                            col.Item().PaddingTop(4).Text($"Machine: {Data.AssignedMachine}").FontSize(9).Bold();
                        }
                        if (Data.VolumeCm3.HasValue)
                        {
                            col.Item().Text($"Volume: {Data.VolumeCm3:N2} cm³").FontSize(8).FontColor(Colors.Grey.Darken1);
                        }
                        if (Data.EstimatedMinutes.HasValue)
                        {
                            var hrs = Data.EstimatedMinutes / 60;
                            var mins = Data.EstimatedMinutes % 60;
                            col.Item().Text($"Est. Time: {hrs}h {mins:D2}m").FontSize(8).FontColor(Colors.Grey.Darken1);
                        }
                    });
                });

                // ─── Specifications ───────────────────────────────────────
                var hasSpecs = Data.ThreadTapRequired || Data.InsertRequired || Data.PartMarking
                    || !string.IsNullOrEmpty(Data.Tolerance)
                    || !string.IsNullOrEmpty(Data.SurfaceRoughness)
                    || !string.IsNullOrEmpty(Data.InspectionType);

                if (hasSpecs)
                {
                    content.Item().PaddingTop(8).Border(1).BorderColor(Colors.Grey.Lighten2).Padding(8).Column(col =>
                    {
                        col.Item().Text("ข้อกำหนด / SPECIFICATIONS").FontSize(8).Bold().FontColor(Colors.Grey.Darken1);
                        col.Item().PaddingTop(6).Row(row =>
                        {
                            // 3D Printing checkboxes
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Row(r =>
                                {
                                    r.ConstantItem(14).Border(1).BorderColor(Colors.Black).Padding(1).AlignCenter().AlignMiddle().Text(Data.ThreadTapRequired ? "X" : "").FontSize(8).FontColor(Colors.Black);
                                    r.RelativeItem().PaddingLeft(6).Text("Thread Tapping (ทำเกลียว)").FontSize(9);
                                });
                                c.Item().PaddingTop(3).Row(r =>
                                {
                                    r.ConstantItem(14).Border(1).BorderColor(Colors.Black).Padding(1).AlignCenter().AlignMiddle().Text(Data.InsertRequired ? "X" : "").FontSize(8).FontColor(Colors.Black);
                                    r.RelativeItem().PaddingLeft(6).Text("Heat-set Inserts (ฝังอินเสิร์ต)").FontSize(9);
                                });
                                c.Item().PaddingTop(3).Row(r =>
                                {
                                    r.ConstantItem(14).Border(1).BorderColor(Colors.Black).Padding(1).AlignCenter().AlignMiddle().Text(Data.PartMarking ? "X" : "").FontSize(8).FontColor(Colors.Black);
                                    r.RelativeItem().PaddingLeft(6).Text("Part Marking (ตีตราชิ้นงาน)").FontSize(9);
                                });
                            });

                            // CNC specs
                            row.RelativeItem().Column(c =>
                            {
                                if (!string.IsNullOrEmpty(Data.Tolerance))
                                {
                                    c.Item().Row(r =>
                                    {
                                        r.ConstantItem(80).Text("Tolerance:").FontSize(8).Bold();
                                        r.RelativeItem().Text(Data.Tolerance).FontSize(9);
                                    });
                                }
                                if (!string.IsNullOrEmpty(Data.SurfaceRoughness))
                                {
                                    c.Item().PaddingTop(3).Row(r =>
                                    {
                                        r.ConstantItem(80).Text("Surface Ra:").FontSize(8).Bold();
                                        r.RelativeItem().Text(Data.SurfaceRoughness).FontSize(9);
                                    });
                                }
                                if (!string.IsNullOrEmpty(Data.InspectionType))
                                {
                                    c.Item().PaddingTop(3).Row(r =>
                                    {
                                        r.ConstantItem(80).Text("Inspection:").FontSize(8).Bold();
                                        r.RelativeItem().Text(Data.InspectionType).FontSize(9);
                                    });
                                }
                            });
                        });
                    });
                }

                // ─── Reference Images ─────────────────────────────────────────

                var placeholderSvg = DocumentResources.ReadText("placeholder.svg");

                content.Item().PaddingTop(10).Table(table =>
                {
                    table.ColumnsDefinition(cols =>
                    {
                        for (int i = 0; i < 6; i++)
                        {
                            cols.RelativeColumn();
                        }
                    });

                    string[] labels = { "FRONT", "LEFT", "RIGHT", "BACK", "TOP", "BOTTOM" };
                    string[] sideKeys = { "Front", "Left", "Right", "Back", "Top", "Bottom" };

                    for (int i = 0; i < 6; i++)
                    {
                        string label = labels[i];
                        string sideKey = sideKeys[i];
                        bool hasImage = Data.PreviewImages != null && Data.PreviewImages.TryGetValue(sideKey, out var url) && !string.IsNullOrEmpty(url);

                        table.Cell().AlignCenter().Column(imgCol =>
                        {
                            imgCol.Item().Text(label).FontSize(6).FontColor(Colors.Grey.Darken1).AlignCenter();
                            if (hasImage)
                            {
                                imgCol.Item().PaddingTop(2).Height(80).Width(80).Image(Data.PreviewImages![sideKey]!);
                            }
                            else
                            {
                                imgCol.Item().PaddingTop(2).Height(80).Width(80).Svg(placeholderSvg);
                            }
                        });
                    }
                });

                // ─── Customer Requirements / Notes ────────────────────────
                if (!string.IsNullOrEmpty(Data.Requirements) || !string.IsNullOrEmpty(Data.Notes))
                {
                    content.Item().Column(col =>
                    {
                        if (!string.IsNullOrEmpty(Data.Requirements))
                        {
                            col.Item().PaddingTop(10).Text("ความต้องการลูกค้า / REQUIREMENTS:").FontSize(8).Bold().FontColor(Colors.Grey.Darken1);
                            col.Item().PaddingTop(3).Text(Data.Requirements).FontSize(9);
                        }
                        if (!string.IsNullOrEmpty(Data.Notes))
                        {
                            col.Item().PaddingTop(10).Text("หมายเหตุ / NOTES:").FontSize(8).Bold().FontColor(Colors.Grey.Darken1);
                            col.Item().PaddingTop(3).Text(Data.Notes).FontSize(9);
                        }
                    });
                }

                // Signatures moved to footer
            });

            page.Footer().Column(footer =>
            {
                footer.Item().BorderTop(1).BorderColor(Colors.Grey.Lighten2).PaddingTop(10).Row(row =>
                {
                    void SignBox(string label)
                    {
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Text(label).FontSize(8).FontColor(Colors.Grey.Darken1);
                            col.Item().PaddingTop(15).LineHorizontal(1).LineColor(Colors.Grey.Medium);
                            col.Item().PaddingTop(2).AlignCenter().Text("Name & Date").FontSize(7).FontColor(Colors.Grey.Medium);
                        });
                    }

                    SignBox("ผู้วางแผน / Planner");
                    row.ConstantItem(20);
                    SignBox("ผู้ผลิต / Operator");
                    row.ConstantItem(20);
                    SignBox("ตรวจสอบคุณภาพ / QC");
                });

                footer.Item().PaddingTop(8).Row(row =>
                {
                    row.RelativeItem().Text("Maliev Co., Ltd.").FontSize(7);
                    row.RelativeItem().AlignCenter().Text(Data.JobTicketNumber).FontSize(7);
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

    private static byte[] GenerateQrCode(string jobId)
    {
        var url = $"https://app.maliev.com/jobs/{jobId}";
        using var qrGenerator = new QRCodeGenerator();
        using var qrCodeData = qrGenerator.CreateQrCode(url, QRCodeGenerator.ECCLevel.L);
        using var qrCode = new PngByteQRCode(qrCodeData);
        return qrCode.GetGraphic(30);
    }
}
