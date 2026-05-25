using Maliev.PdfService.Api.Models.Data;
using QRCoder;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Maliev.PdfService.Api.Services.Layouts;

/// <summary>
/// QuestPDF layout for customer-facing blog practical note booklets.
/// </summary>
public sealed class BlogPracticalNoteDocument(BlogPracticalNoteData data) : IDocument
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

    /// <summary>Gets the practical note data.</summary>
    public BlogPracticalNoteData Data { get; } = data;

    private string BlogPostUrl => string.IsNullOrWhiteSpace(Data.PublicUrl)
        ? $"https://www.maliev.com/blog/{Uri.EscapeDataString(Data.Slug)}"
        : Data.PublicUrl;

    /// <inheritdoc/>
    public DocumentMetadata GetMetadata()
    {
        return new DocumentMetadata
        {
            Title = $"{Data.Title} - MALIEV Practical Note",
            Author = "MALIEV Co., Ltd.",
            Subject = Data.Summary,
            Keywords = "MALIEV, manufacturing, practical note, quotation, DFM",
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
        ComposeCover(container);
        ComposeTableOfContents(container);
        ComposeArticle(container);
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

    private void ComposeCover(IDocumentContainer container)
    {
        container.Page(page =>
        {
            page.Size(PageSizes.A4);
            page.Margin(38);
            page.DefaultTextStyle(TextStyle);

            page.Content()
                .Background(Colors.White)
                .Column(column =>
                {
                    column.Item().Row(row =>
                    {
                        row.RelativeItem().Width(150).Svg(DocumentResources.ReadText("MALIEV_BLACK.svg")).FitWidth();
                        row.ConstantItem(160).AlignRight().Text("Manufacturing Practical Note").FontSize(10).Bold().FontColor(AccentBlue);
                    });

                    column.Item().PaddingTop(30).Text(Data.Category.ToUpperInvariant()).FontSize(9).Bold().FontColor(AccentBlue);
                    column.Item().PaddingTop(8).SemanticHeader1().Text(Data.Title).FontSize(32).Bold().FontColor(DarkPanel);
                    column.Item().PaddingTop(12).Text(Data.Summary).FontSize(13).LineHeight(1.38f).FontColor(BodyText);

                    if (HasImage(Data.CoverImage))
                    {
                        column.Item().PaddingTop(22).Height(220).Background(WarmPanel).Image(Data.CoverImage!.Bytes).FitArea();
                    }

                    column.Item()
                        .ExtendVertical()
                        .AlignBottom()
                        .PaddingBottom(6)
                        .ShowEntire()
                        .Element(ComposeCoverContactBlock);
                });
        });
    }

    private void ComposeTableOfContents(IDocumentContainer container)
    {
        container.Page(page =>
        {
            ConfigureStandardPage(page);
            page.Content().Column(column =>
            {
                ComposePageEyebrow(column, "Contents");
                column.Item().SemanticHeader1().Text("Read before quoting").FontSize(22).Bold().FontColor(DarkPanel);
                column.Item().PaddingTop(8).Text(Data.Summary).FontSize(10).LineHeight(1.35f).FontColor(BodyText);

                column.Item().PaddingTop(18).SemanticTableOfContents().Column(toc =>
                {
                    for (var index = 0; index < Data.Sections.Count; index++)
                    {
                        var section = Data.Sections[index];
                        var sectionId = SectionId(index + 1);
                        toc.Item().PaddingBottom(8).SemanticTableOfContentsItem().SemanticLink(section.Title).SectionLink(sectionId).Row(row =>
                        {
                            row.ConstantItem(30).Element(item => NumberBadge(item, index + 1));
                            row.RelativeItem().PaddingLeft(8).BorderBottom(1).BorderColor(Hairline).PaddingBottom(8).Text(section.Title).FontSize(11).Bold().FontColor(DarkPanel);
                            row.ConstantItem(34).AlignRight().Text(text =>
                            {
                                text.Span("p. ").FontSize(8).FontColor(Colors.Grey.Darken1);
                                text.BeginPageNumberOfSection(sectionId).FontSize(8).Bold().FontColor(DarkPanel);
                            });
                        });
                    }
                });

                column.Item().PaddingTop(16).Element(ComposeTakeawayPanel);
            });
            ComposeFooter(page);
        });
    }

    private void ComposeArticle(IDocumentContainer container)
    {
        container.Page(page =>
        {
            ConfigureStandardPage(page);
            page.Content().Column(column =>
            {
                ComposePageEyebrow(column, "Article");

                for (var index = 0; index < Data.Sections.Count; index++)
                {
                    var section = Data.Sections[index];
                    column.Item().PaddingBottom(18).PreventPageBreak().Element(item => ComposeArticleSection(item, section, index + 1));
                }
            });
            ComposeFooter(page);
        });
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

                column.Item().ExtendVertical().AlignBottom().PaddingTop(24).BorderTop(1).BorderColor(Hairline).PaddingTop(10).Text("This booklet is a practical guide, not a final manufacturing acceptance document. Include drawings, material requirements, operating conditions, and inspection criteria with your quote request.")
                    .FontSize(8)
                    .LineHeight(1.35f)
                    .FontColor(Colors.Grey.Darken1);
            });
            ComposeFooter(page);
        });
    }

    private void ComposeCoverContactBlock(IContainer container)
    {
        container.BorderTop(1).BorderColor(Hairline).PaddingTop(16).Row(row =>
        {
            row.RelativeItem().Column(company =>
            {
                company.Item().Text("Prepared by MALIEV Co., Ltd.").FontSize(9).Bold().FontColor(DarkPanel);
                company.Item().Text("36/1 Moo 3, Khlong Khoi, Pak Kret, Nonthaburi 11120, Thailand").FontSize(8).FontColor(BodyText);
                company.Item().Text("www.maliev.com | info@maliev.com").FontSize(8).FontColor(BodyText);
                company.Item().PaddingTop(5).Text(text =>
                {
                    text.Span("Read online: ").FontSize(8).FontColor(Colors.Grey.Darken1);
                    text.Hyperlink(BlogPostUrl, BlogPostUrl).FontSize(8).Underline().FontColor(AccentBlue);
                });
            });
            row.ConstantItem(94).AlignRight().Hyperlink(BlogPostUrl).Element(item => ComposeQrCode(item, BlogPostUrl));
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

    private static void ComposeFooter(PageDescriptor page)
    {
        page.Footer().BorderTop(1).BorderColor(Hairline).PaddingTop(8).Row(row =>
        {
            row.RelativeItem().Text("MALIEV").FontSize(7).Bold().FontColor(Colors.Grey.Darken1);
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

    private void ComposeTakeawayPanel(IContainer container)
    {
        container.PreventPageBreak().Background(SoftPanel).Border(1).BorderColor(Hairline).Padding(12).Column(column =>
        {
            column.Item().Text("Before you upload").FontSize(13).Bold().FontColor(DarkPanel);
            column.Item().PaddingTop(8).Column(points =>
            {
                foreach (var takeaway in Data.Takeaways.Where(value => !string.IsNullOrWhiteSpace(value)))
                {
                    points.Item().PaddingBottom(6).Row(row =>
                    {
                        row.ConstantItem(10).Text("-").FontColor(AccentBlue).Bold();
                        row.RelativeItem().Text(takeaway).FontSize(9).LineHeight(1.25f).FontColor(BodyText);
                    });
                }
            });
        });
    }

    private void ComposeArticleSection(IContainer container, BlogPracticalNoteSectionData section, int number)
    {
        var sectionId = SectionId(number);
        container.SemanticSection().Section(sectionId).Column(column =>
        {
            column.Item().Row(row =>
            {
                row.ConstantItem(34).Element(item => NumberBadge(item, number));
                row.RelativeItem().PaddingLeft(10).SemanticHeader2().Text(section.Title).FontSize(16).Bold().FontColor(DarkPanel);
            });

            if (HasImage(section.Image))
            {
                column.Item().PaddingTop(10).Element(item => ComposeArticleImage(item, section.Image!));
            }

            column.Item().PaddingTop(8).Text(section.Body).FontSize(10).LineHeight(1.38f).FontColor(BodyText);

            if (section.Items.Count > 0)
            {
                column.Item().PaddingTop(10).Column(points =>
                {
                    foreach (var point in section.Items.Where(value => !string.IsNullOrWhiteSpace(value)))
                    {
                        points.Item().PaddingBottom(6).Background(SoftPanel).BorderLeft(3).BorderColor(AccentBlue).Padding(8).Text(point)
                            .FontSize(9)
                            .LineHeight(1.25f)
                            .FontColor(BodyText);
                    }
                });
            }
        });
    }

    private static void ComposeArticleImage(IContainer container, BlogPracticalNoteImageData image)
    {
        container.SemanticImage(image.Alt).Column(column =>
        {
            column.Item().Height(150).Background(WarmPanel).Image(image.Bytes).FitArea();
            if (!string.IsNullOrWhiteSpace(image.Caption))
            {
                column.Item().PaddingTop(5).SemanticCaption().Text(image.Caption).FontSize(8).FontColor(Colors.Grey.Darken1);
            }
        });
    }

    private static void NumberBadge(IContainer container, int number)
    {
        container
            .Background(AccentBlue)
            .PaddingVertical(5)
            .AlignCenter()
            .Text(number.ToString("00"))
            .FontSize(8)
            .Bold()
            .FontColor(Colors.White);
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
        container.Background(Colors.White).Border(1).BorderColor(Hairline).Padding(4).AspectRatio(1).Image(GenerateQrCode(url)).FitArea();
    }

    private static byte[] GenerateQrCode(string url)
    {
        using var qrGenerator = new QRCodeGenerator();
        using var qrCodeData = qrGenerator.CreateQrCode(url, QRCodeGenerator.ECCLevel.L);
        using var qrCode = new PngByteQRCode(qrCodeData);
        return qrCode.GetGraphic(20);
    }

    private static bool HasImage(BlogPracticalNoteImageData? image)
    {
        return image?.Bytes.Length > 0;
    }

    private static string SectionId(int number)
    {
        return $"section-{number:00}";
    }
}
