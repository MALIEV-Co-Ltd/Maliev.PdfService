using Maliev.PdfService.Api.Models.Data;
using QRCoder;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Maliev.PdfService.Api.Services.Layouts;

/// <summary>
/// QuestPDF layout for customer-facing material datasheets.
/// </summary>
public sealed class MaterialDatasheetDocument(MaterialDatasheetData data) : IDocument
{
    private const string QuoteUrl = "https://quote.maliev.com/projects/new";
    private const string MaterialsUrl = "https://www.maliev.com/materials";
    private const string LineUrl = "https://page.line.me/maliev";
    private const string FacebookUrl = "https://www.facebook.com/maliev.manufacturing/";
    private const string YouTubeUrl = "https://www.youtube.com/channel/UCCosquPSUed6UPlMcRCq0Ig";
    private const string InstagramUrl = "https://www.instagram.com/maliev.manufacturing/";

    private static readonly string AccentBlue = Colors.Blue.Darken2;
    private static readonly string BodyText = Colors.Grey.Darken3;
    private static readonly string DarkPanel = Colors.Grey.Darken4;
    private static readonly string Hairline = Colors.Grey.Lighten2;
    private static readonly string SoftPanel = Colors.Grey.Lighten5;
    private static readonly string WarmPanel = Colors.Grey.Lighten4;
    private static readonly (string Channel, string Label, string Url)[] SocialLinks =
    [
        ("LINE", "Official Account @maliev", LineUrl),
        ("Facebook", "maliev.manufacturing", FacebookUrl),
        ("YouTube", "MALIEV channel", YouTubeUrl),
        ("Instagram", "@maliev.manufacturing", InstagramUrl)
    ];

    /// <summary>Gets the material datasheet data.</summary>
    public MaterialDatasheetData Data { get; } = data;

    private string MaterialPageUrl => string.IsNullOrWhiteSpace(Data.PublicUrl)
        ? $"https://www.maliev.com/materials/{Uri.EscapeDataString(Data.Slug)}"
        : Data.PublicUrl;

    /// <inheritdoc/>
    public DocumentMetadata GetMetadata()
    {
        return new DocumentMetadata
        {
            Title = $"{Data.Name} - MALIEV Material Datasheet",
            Author = "MALIEV Co., Ltd.",
            Subject = Data.Family,
            Keywords = "MALIEV, manufacturing, material datasheet, material properties, quotation, DFM",
            Language = Data.CultureName.Equals("th-TH", StringComparison.OrdinalIgnoreCase) ? "th-TH" : "en-US"
        };
    }

    /// <inheritdoc/>
    public DocumentSettings GetSettings()
    {
        return new DocumentSettings
        {
            PDFA_Conformance = PDFA_Conformance.PDFA_3A,
            PDFUA_Conformance = PDFUA_Conformance.PDFUA_1,
            ImageCompressionQuality = ImageCompressionQuality.High,
            ImageRasterDpi = 180
        };
    }

    /// <inheritdoc/>
    public void Compose(IDocumentContainer container)
    {
        ComposeDatasheet(container);
        ComposeCompanyBackPage(container);
    }

    private static TextStyle TextStyle(TextStyle style)
    {
        return style.FontFamily("Roboto", "Noto Sans Thai", "Segoe UI", "Arial").FontSize(10).FontColor(BodyText);
    }

    private static void ConfigureStandardPage(PageDescriptor page)
    {
        page.Size(PageSizes.A4);
        page.Margin(42);
        page.DefaultTextStyle(TextStyle);
    }

    private void ComposeDatasheet(IDocumentContainer container)
    {
        container.Page(page =>
        {
            ConfigureStandardPage(page);
            page.Content().Column(column =>
            {
                ComposeHeader(column);

                if (HasImage(Data.CoverImage))
                {
                    column.Item().PaddingTop(18).Element(item => ComposeCoverImage(item, Data.CoverImage!));
                }

                if (Data.Specs.Count > 0)
                {
                    column.Item().PaddingTop(20).Element(ComposeSpecificationsSection);
                }

                if (Data.Bands.Count > 0)
                {
                    column.Item().PaddingTop(20).Element(ComposeSelectionGuidanceSection);
                }

                if (!string.IsNullOrWhiteSpace(Data.Pros) || !string.IsNullOrWhiteSpace(Data.Cons))
                {
                    column.Item().PaddingTop(20).Row(row =>
                    {
                        row.RelativeItem().Element(ComposeProsPanel);
                        row.ConstantItem(12);
                        row.RelativeItem().Element(ComposeWatchPanel);
                    });
                }

                if (!string.IsNullOrWhiteSpace(Data.Disclaimer))
                {
                    column.Item()
                        .ExtendVertical()
                        .AlignBottom()
                        .PaddingTop(18)
                        .BorderTop(1)
                        .BorderColor(Hairline)
                        .PaddingTop(10)
                        .Text(Data.Disclaimer)
                        .FontSize(8)
                        .LineHeight(1.4f)
                        .FontColor(Colors.Grey.Darken1);
                }
            });
            ComposeFooter(page);
        });
    }

    private void ComposeHeader(ColumnDescriptor column)
    {
        column.Item().Row(row =>
        {
            row.RelativeItem().Width(150).Svg(DocumentResources.ReadText("MALIEV_BLACK.svg")).FitWidth();
            row.ConstantItem(160).AlignRight().Text("Material Datasheet").FontSize(10).Bold().FontColor(AccentBlue);
        });

        column.Item().PaddingTop(22).Row(row =>
        {
            row.RelativeItem().Text(Data.CategoryLabel.ToUpperInvariant()).FontSize(9).Bold().FontColor(AccentBlue);
            row.ConstantItem(140).AlignRight().Text(Data.ProcessLabel).FontSize(9).Bold().FontColor(Colors.Grey.Darken1);
        });

        column.Item().PaddingTop(8).SemanticHeader1().Text(Data.Name).FontSize(28).Bold().FontColor(DarkPanel);

        if (!string.IsNullOrWhiteSpace(Data.Family))
        {
            column.Item().PaddingTop(8).Text(Data.Family).FontSize(12).LineHeight(1.35f).FontColor(BodyText);
        }

        column.Item().PaddingTop(14).LineHorizontal(1).LineColor(Hairline);
    }

    private void ComposeCompanyBackPage(IDocumentContainer container)
    {
        container.Page(page =>
        {
            ConfigureStandardPage(page);
            page.Content().Column(column =>
            {
                ComposePageEyebrow(column, "About MALIEV");
                column.Item().SemanticHeader1().Text("Manufacturing support from prototype to usable parts").FontSize(22).Bold().FontColor(DarkPanel);
                column.Item().PaddingTop(10).Text("MALIEV helps engineering teams prepare manufacturable files, review material and process choices, quote production work, and keep each order traceable from upload through delivery.")
                    .FontSize(10)
                    .LineHeight(1.4f)
                    .FontColor(BodyText);

                column.Item().PaddingTop(20).Row(row =>
                {
                    row.RelativeItem().Element(item => ContactCard(item, "Get part price", QuoteUrl));
                    row.ConstantItem(12);
                    row.RelativeItem().Element(item => ContactCard(item, "Compare materials", MaterialsUrl));
                });

                column.Item().PaddingTop(18).Row(row =>
                {
                    row.RelativeItem().Element(CompanyInfoCard);
                    row.ConstantItem(12);
                    row.RelativeItem().Element(ComposeSocialMediaCard);
                });

                column.Item()
                    .ExtendVertical()
                    .AlignBottom()
                    .PaddingBottom(28)
                    .Text("This datasheet lists typical values for material selection, not final manufacturing acceptance criteria. Confirm the exact grade, condition, and inspection requirements with your quote request.")
                    .FontSize(8)
                    .LineHeight(1.4f)
                    .FontColor(Colors.Grey.Darken1);
            });
            ComposeFooter(page);
        });
    }

    private void ComposeSpecificationsSection(IContainer container)
    {
        container.Column(column =>
        {
            ComposeSectionHeading(column, "Specifications");
            column.Item().PaddingTop(10).Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(2);
                    columns.RelativeColumn(3);
                });

                foreach (var spec in Data.Specs.Where(value => !string.IsNullOrWhiteSpace(value.Label)))
                {
                    table.Cell().BorderBottom(1).BorderColor(Hairline).PaddingVertical(7).PaddingRight(10)
                        .Text(spec.Label).FontSize(9).Bold().FontColor(DarkPanel);
                    table.Cell().BorderBottom(1).BorderColor(Hairline).PaddingVertical(7).Column(value =>
                    {
                        value.Item().Text(spec.Value).FontSize(9).FontColor(BodyText);
                        if (!string.IsNullOrWhiteSpace(spec.Note))
                        {
                            value.Item().PaddingTop(2).Text(spec.Note).FontSize(7).FontColor(Colors.Grey.Darken1);
                        }
                    });
                }
            });
        });
    }

    private void ComposeSelectionGuidanceSection(IContainer container)
    {
        container.Column(column =>
        {
            ComposeSectionHeading(column, "Selection guidance");
            column.Item().PaddingTop(10).Column(bands =>
            {
                foreach (var band in Data.Bands.Where(value => !string.IsNullOrWhiteSpace(value.Label)))
                {
                    bands.Item().PaddingBottom(8).Background(SoftPanel).BorderLeft(3).BorderColor(AccentBlue).Padding(8).Row(row =>
                    {
                        row.ConstantItem(120).Text(band.Label).FontSize(9).Bold().FontColor(DarkPanel);
                        row.RelativeItem().PaddingLeft(8).Text(band.Value).FontSize(9).LineHeight(1.25f).FontColor(BodyText);
                    });
                }
            });
        });
    }

    private void ComposeProsPanel(IContainer container)
    {
        container.Background(SoftPanel).Border(1).BorderColor(Hairline).Padding(12).Column(column =>
        {
            column.Item().Text("Pros").FontSize(13).Bold().FontColor(DarkPanel);
            column.Item().PaddingTop(8).Text(Data.Pros).FontSize(9).LineHeight(1.3f).FontColor(BodyText);
        });
    }

    private void ComposeWatchPanel(IContainer container)
    {
        container.Background(SoftPanel).Border(1).BorderColor(Hairline).Padding(12).Column(column =>
        {
            column.Item().Text("Watch / check before use").FontSize(13).Bold().FontColor(DarkPanel);
            column.Item().PaddingTop(8).Text(Data.Cons).FontSize(9).LineHeight(1.3f).FontColor(BodyText);
        });
    }

    private static void CompanyInfoCard(IContainer container)
    {
        container.Background(SoftPanel).Border(1).BorderColor(Hairline).Padding(14).Column(company =>
        {
            company.Item().Text("MALIEV Co., Ltd.").FontSize(13).Bold().FontColor(DarkPanel);
            company.Item().PaddingTop(5).Text("36/1 Moo 3, Khlong Khoi, Pak Kret, Nonthaburi 11120, Thailand").FontSize(9).FontColor(BodyText);
            company.Item().Text("www.maliev.com | info@maliev.com").FontSize(9).FontColor(BodyText);
            company.Item().Text("Weekdays 10:00-18:00").FontSize(9).FontColor(BodyText);
        });
    }

    private static void ComposeSocialMediaCard(IContainer container)
    {
        container.Background(SoftPanel).Border(1).BorderColor(Hairline).Padding(14).Column(social =>
        {
            social.Item().Text("Social media").FontSize(13).Bold().FontColor(DarkPanel);
            social.Item().PaddingTop(5).Text("Follow manufacturing updates and contact the team.").FontSize(8).LineHeight(1.25f).FontColor(Colors.Grey.Darken1);

            foreach (var (channel, label, url) in SocialLinks)
            {
                social.Item().PaddingTop(7).Text(text =>
                {
                    text.Span($"{channel}: ").FontSize(8).Bold().FontColor(DarkPanel);
                    text.Hyperlink(label, url).FontSize(8).Underline().FontColor(AccentBlue);
                });
            }
        });
    }

    private void ComposeFooter(PageDescriptor page)
    {
        page.Footer().BorderTop(1).BorderColor(Hairline).PaddingTop(8).Row(row =>
        {
            row.RelativeItem().Column(footer =>
            {
                footer.Item().Text("MALIEV").FontSize(7).Bold().FontColor(Colors.Grey.Darken1);
                footer.Item().Text(text => text.Hyperlink(MaterialPageUrl, MaterialPageUrl).FontSize(7).Underline().FontColor(AccentBlue));
            });
            row.RelativeItem().AlignRight().Text(text =>
            {
                text.Span("Page ").FontSize(7).FontColor(Colors.Grey.Darken1);
                text.CurrentPageNumber().FontSize(7).FontColor(Colors.Grey.Darken1);
                text.Span(" of ").FontSize(7).FontColor(Colors.Grey.Darken1);
                text.TotalPages().FontSize(7).FontColor(Colors.Grey.Darken1);
            });
        });
    }

    private static void ComposePageEyebrow(ColumnDescriptor column, string label)
    {
        column.Item().Text(label.ToUpperInvariant()).FontSize(8).Bold().FontColor(AccentBlue);
        column.Item().PaddingTop(4).PaddingBottom(14).LineHorizontal(1).LineColor(Hairline);
    }

    private static void ComposeSectionHeading(ColumnDescriptor column, string label)
    {
        column.Item().SemanticHeader2().Text(label).FontSize(16).Bold().FontColor(DarkPanel);
        column.Item().PaddingTop(6).LineHorizontal(1).LineColor(Hairline);
    }

    private void ComposeCoverImage(IContainer container, MaterialDatasheetImageData image)
    {
        container.SemanticImage(image.Alt).Hyperlink(MaterialPageUrl).Column(column =>
        {
            column.Item().Height(200).Background(WarmPanel).Image(image.Bytes).FitArea();
            if (!string.IsNullOrWhiteSpace(image.Caption))
            {
                column.Item().PaddingTop(5).SemanticCaption().Text(image.Caption).FontSize(8).FontColor(Colors.Grey.Darken1);
            }
        });
    }

    private static void ContactCard(IContainer container, string title, string url)
    {
        container.Hyperlink(url).Background(SoftPanel).Border(1).BorderColor(Hairline).Padding(12).Row(row =>
        {
            row.RelativeItem().Column(column =>
            {
                column.Item().Text(title).FontSize(9).Bold().FontColor(DarkPanel);
                column.Item().PaddingTop(5).Text(text => text.Hyperlink(url, url).FontSize(8).Underline().FontColor(AccentBlue));
            });
            row.ConstantItem(58).Element(item => ComposeQrCode(item, url));
        });
    }

    private static void ComposeQrCode(IContainer container, string url)
    {
        container.Background(Colors.White).Padding(3).AspectRatio(1).Image(GenerateQrCode(url)).FitArea();
    }

    private static byte[] GenerateQrCode(string url)
    {
        using var qrGenerator = new QRCodeGenerator();
        using var qrCodeData = qrGenerator.CreateQrCode(url, QRCodeGenerator.ECCLevel.L);
        using var qrCode = new PngByteQRCode(qrCodeData);
        return qrCode.GetGraphic(20);
    }

    private static bool HasImage(MaterialDatasheetImageData? image)
    {
        return image?.Bytes.Length > 0;
    }
}
