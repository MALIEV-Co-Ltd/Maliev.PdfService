using System.Text.RegularExpressions;
using Maliev.PdfService.Api.Models.Data;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Maliev.PdfService.Api.Services.Layouts;

/// <summary>
/// Renders a Make Studio customer assistant transcript as a chronological timeline.
/// </summary>
/// <param name="data">The chat transcript data.</param>
public class ChatTranscriptDocument(ChatTranscriptData data) : IDocument
{
    private static readonly string BrandColor = Colors.Black;
    private static readonly string BodyText = Colors.Grey.Darken4;
    private static readonly string MutedText = Colors.Grey.Darken1;
    private static readonly string Hairline = Colors.Grey.Lighten2;
    private static readonly string DatePill = Colors.Grey.Lighten5;
    private static readonly string CodeBackground = Colors.Grey.Lighten5;

    private static readonly Regex OrderedListRegex = new(@"^\d+\.\s+(.+)$", RegexOptions.Compiled);
    private static readonly Regex BoldRegex = new(@"\*\*(.+?)\*\*", RegexOptions.Compiled);
    private static readonly Regex ItalicAsteriskRegex = new(@"(?<!\*)\*(?!\*)(.+?)(?<!\*)\*(?!\*)", RegexOptions.Compiled);
    private static readonly Regex ItalicUnderscoreRegex = new(@"(?<!_)_(?!_)(.+?)(?<!_)_(?!_)", RegexOptions.Compiled);

    /// <summary>Gets the underlying chat transcript data used to compose the PDF.</summary>
    public ChatTranscriptData Data { get; } = data;

    /// <inheritdoc />
    public DocumentMetadata GetMetadata() => new()
    {
        Title = $"Chat Transcript - {Data.SessionId}",
        Author = "MALIEV Make Studio",
        Subject = $"Customer assistant chat transcript from {Data.GeneratedAt:yyyy-MM-dd HH:mm}",
        Keywords = "MALIEV, Make Studio, chat transcript, customer assistant"
    };

    /// <inheritdoc />
    public void Compose(IDocumentContainer container)
    {
        container.Page(page =>
        {
            page.Size(PageSizes.A4);
            page.Margin(40);
            page.DefaultTextStyle(x => x.FontFamily("Roboto", "Noto Sans Thai").FontSize(10).FontColor(BodyText));

            page.Header().Element(ComposeHeader);
            page.Content().Element(ComposeContent);
            page.Footer().Element(ComposeFooter);
        });
    }

    private void ComposeHeader(IContainer container)
    {
        container.Column(header =>
        {
            header.Item().Row(row =>
            {
                row.RelativeItem().Width(120).Svg(DocumentResources.ReadText("MALIEV_BLACK.svg")).FitWidth();
                row.ConstantItem(220).AlignRight().Column(col =>
                {
                    col.Item().AlignRight().Text("Chat Transcript").FontSize(16).Bold().FontColor(BrandColor);
                    col.Item().AlignRight().Text("Make Studio Customer Assistant").FontSize(10).FontColor(MutedText);
                });
            });

            header.Item().PaddingTop(8).LineHorizontal(1).LineColor(Hairline);

            header.Item().PaddingTop(8).Row(info =>
            {
                info.RelativeItem().Text($"Session: {Data.SessionId}").FontSize(8).FontColor(MutedText);
                info.RelativeItem().AlignRight().Text($"Generated: {Data.GeneratedAt.ToLocalTime():dd MMM yyyy HH:mm}").FontSize(8).FontColor(MutedText);
            });

            if (!string.IsNullOrWhiteSpace(Data.CustomerName) || !string.IsNullOrWhiteSpace(Data.CustomerEmail))
            {
                header.Item().PaddingTop(2).Row(info =>
                {
                    info.RelativeItem().Text($"Customer: {Data.CustomerName ?? Data.CustomerEmail}").FontSize(8).FontColor(MutedText);
                    if (!string.IsNullOrWhiteSpace(Data.CustomerEmail) && !string.IsNullOrWhiteSpace(Data.CustomerName))
                    {
                        info.RelativeItem().AlignRight().Text(Data.CustomerEmail).FontSize(8).FontColor(MutedText);
                    }
                });
            }

            header.Item().PaddingTop(8).LineHorizontal(1).LineColor(Hairline);
        });
    }

    private void ComposeContent(IContainer container)
    {
        container.PaddingVertical(12).Column(content =>
        {
            if (Data.Messages.Count == 0)
            {
                content.Item().PaddingTop(40).AlignCenter().Text("No messages in this session.").FontSize(12).FontColor(MutedText);
                return;
            }

            var lastDate = DateOnly.MinValue;
            foreach (var message in Data.Messages.OrderBy(message => message.Timestamp))
            {
                var localTimestamp = message.Timestamp.ToLocalTime();
                var messageDate = DateOnly.FromDateTime(localTimestamp.DateTime);
                if (messageDate != lastDate)
                {
                    lastDate = messageDate;
                    content.Item().PaddingVertical(10).Element(c => ComposeDateDivider(c, messageDate));
                }

                content.Item().PaddingBottom(12).Element(c => ComposeMessage(c, message, localTimestamp));
            }
        });
    }

    private static void ComposeDateDivider(IContainer container, DateOnly date)
    {
        container.Row(row =>
        {
            row.RelativeItem().AlignMiddle().LineHorizontal(1).LineColor(Hairline);
            row.ConstantItem(104).AlignCenter().Background(DatePill).PaddingVertical(4)
                .Text(date.ToString("dd MMM yyyy")).FontSize(8).Bold().FontColor(BrandColor);
            row.RelativeItem().AlignMiddle().LineHorizontal(1).LineColor(Hairline);
        });
    }

    private void ComposeMessage(IContainer container, ChatMessageData message, DateTimeOffset localTimestamp)
    {
        var isUser = message.Role.Equals("user", StringComparison.OrdinalIgnoreCase);
        var roleLabel = isUser
            ? FirstNonEmpty(Data.CustomerName, "You")
            : "MALIEV Assistant";
        var content = SanitizeMessageContent(message.Content);

        if (string.IsNullOrWhiteSpace(content))
        {
            return;
        }

        container.Row(row =>
        {
            row.ConstantItem(46).PaddingTop(2).Text(localTimestamp.ToString("HH:mm:ss")).FontSize(8).FontColor(MutedText);
            row.ConstantItem(18).PaddingLeft(8).BorderLeft(1).BorderColor(Hairline);
            row.RelativeItem().Column(col =>
            {
                col.Item().Text(roleLabel).FontSize(9).SemiBold().FontColor(BrandColor);
                col.Item().PaddingTop(5).Element(c => ComposeMarkdown(c, content));
            });
        });
    }

    private static void ComposeMarkdown(IContainer container, string markdown)
    {
        container.Column(column =>
        {
            var lines = markdown.Replace("\r\n", "\n", StringComparison.Ordinal).Split('\n');
            var paragraph = new List<string>();
            var list = new List<string>();
            var orderedList = false;

            void FlushParagraph()
            {
                if (paragraph.Count == 0)
                {
                    return;
                }

                var text = CleanInlineMarkdown(string.Join(' ', paragraph));
                column.Item().PaddingBottom(5).Text(text).FontSize(10).LineHeight(1.35f);
                paragraph.Clear();
            }

            void FlushList()
            {
                if (list.Count == 0)
                {
                    return;
                }

                for (var itemIndex = 0; itemIndex < list.Count; itemIndex++)
                {
                    var marker = orderedList
                        ? $"{itemIndex + 1}."
                        : "\u2022";
                    column.Item().PaddingBottom(2).Row(row =>
                    {
                        row.ConstantItem(14).Text(marker).FontSize(10).FontColor(BrandColor);
                        row.RelativeItem().Text(CleanInlineMarkdown(list[itemIndex])).FontSize(10).LineHeight(1.3f);
                    });
                }

                column.Item().PaddingBottom(3);
                list.Clear();
            }

            for (var index = 0; index < lines.Length; index++)
            {
                var trimmed = lines[index].Trim();
                if (string.IsNullOrWhiteSpace(trimmed))
                {
                    FlushParagraph();
                    FlushList();
                    continue;
                }

                if (trimmed.StartsWith("```", StringComparison.Ordinal))
                {
                    FlushParagraph();
                    FlushList();
                    var code = new List<string>();
                    index++;
                    while (index < lines.Length && !lines[index].Trim().StartsWith("```", StringComparison.Ordinal))
                    {
                        code.Add(lines[index]);
                        index++;
                    }

                    column.Item().PaddingBottom(6).Background(CodeBackground).Border(1).BorderColor(Hairline)
                        .Padding(6).Text(string.Join('\n', code).TrimEnd()).FontSize(8).LineHeight(1.25f);
                    continue;
                }

                if (TryReadTable(lines, index, out var headers, out var rows, out var consumedRows))
                {
                    FlushParagraph();
                    FlushList();
                    ComposeTable(column, headers, rows);
                    index += consumedRows - 1;
                    continue;
                }

                var headingLevel = GetHeadingLevel(trimmed);
                if (headingLevel > 0)
                {
                    FlushParagraph();
                    FlushList();
                    var heading = CleanInlineMarkdown(trimmed[(headingLevel + 1)..].Trim());
                    var fontSize = headingLevel == 1 ? 13 : headingLevel == 2 ? 12 : 11;
                    column.Item().PaddingTop(headingLevel == 1 ? 4 : 2).PaddingBottom(4)
                        .Text(heading).FontSize(fontSize).SemiBold().FontColor(BrandColor);
                    continue;
                }

                if (TryGetListItem(trimmed, out var itemText, out var isOrdered))
                {
                    FlushParagraph();
                    if (list.Count > 0 && orderedList != isOrdered)
                    {
                        FlushList();
                    }

                    orderedList = isOrdered;
                    list.Add(itemText);
                    continue;
                }

                FlushList();
                paragraph.Add(trimmed);
            }

            FlushParagraph();
            FlushList();
        });
    }

    private static void ComposeTable(ColumnDescriptor column, IReadOnlyList<string> headers, IReadOnlyList<string[]> rows)
    {
        column.Item().PaddingBottom(8).Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                foreach (var _ in headers)
                {
                    columns.RelativeColumn();
                }
            });

            foreach (var header in headers)
            {
                table.Cell().Element(HeaderCellStyle).Text(CleanInlineMarkdown(header)).FontSize(8).SemiBold();
            }

            foreach (var row in rows)
            {
                for (var index = 0; index < headers.Count; index++)
                {
                    var value = index < row.Length ? row[index] : string.Empty;
                    table.Cell().Element(BodyCellStyle).Text(CleanInlineMarkdown(value)).FontSize(8).LineHeight(1.2f);
                }
            }
        });
    }

    private static IContainer HeaderCellStyle(IContainer container)
    {
        return container.Border(1).BorderColor(Hairline).Background(Colors.Grey.Lighten5).Padding(4);
    }

    private static IContainer BodyCellStyle(IContainer container)
    {
        return container.Border(1).BorderColor(Hairline).Padding(4);
    }

    private void ComposeFooter(IContainer container)
    {
        container.BorderTop(1).BorderColor(Hairline).PaddingTop(8).AlignCenter().Column(column =>
        {
            column.Item().AlignCenter().Text("Thank you for using Make Studio Customer Assistant").FontSize(7).FontColor(MutedText);
            column.Item().AlignCenter().Text("MALIEV Co., Ltd.").FontSize(8).FontColor(MutedText);
        });
    }

    private static bool TryReadTable(
        string[] lines,
        int startIndex,
        out string[] headers,
        out List<string[]> rows,
        out int consumedRows)
    {
        headers = [];
        rows = [];
        consumedRows = 0;

        if (startIndex + 1 >= lines.Length)
        {
            return false;
        }

        var headerLine = lines[startIndex].Trim();
        var separatorLine = lines[startIndex + 1].Trim();
        if (!IsTableRow(headerLine) || !IsTableSeparator(separatorLine))
        {
            return false;
        }

        headers = SplitTableRow(headerLine);
        var index = startIndex + 2;
        while (index < lines.Length && IsTableRow(lines[index].Trim()))
        {
            rows.Add(SplitTableRow(lines[index].Trim()));
            index++;
        }

        consumedRows = index - startIndex;
        return headers.Length > 0 && rows.Count > 0;
    }

    private static bool IsTableRow(string line)
    {
        return line.Contains('|', StringComparison.Ordinal) && SplitTableRow(line).Length > 1;
    }

    private static bool IsTableSeparator(string line)
    {
        var cells = SplitTableRow(line);
        return cells.Length > 1 && cells.All(cell =>
        {
            var value = cell.Trim().Trim(':');
            return value.Length >= 3 && value.All(ch => ch == '-');
        });
    }

    private static string[] SplitTableRow(string row)
    {
        return row.Trim().Trim('|').Split('|', StringSplitOptions.TrimEntries);
    }

    private static int GetHeadingLevel(string trimmed)
    {
        if (!trimmed.StartsWith('#'))
        {
            return 0;
        }

        var count = 0;
        while (count < trimmed.Length && trimmed[count] == '#')
        {
            count++;
        }

        return count is >= 1 and <= 3 && trimmed.Length > count && trimmed[count] == ' ' ? count : 0;
    }

    private static bool TryGetListItem(string trimmed, out string itemText, out bool ordered)
    {
        ordered = false;
        itemText = string.Empty;

        if (trimmed.Length > 2 && (trimmed[0] == '-' || trimmed[0] == '*' || trimmed[0] == '+') && trimmed[1] == ' ')
        {
            itemText = trimmed[2..].Trim();
            return true;
        }

        var match = OrderedListRegex.Match(trimmed);
        if (!match.Success)
        {
            return false;
        }

        ordered = true;
        itemText = match.Groups[1].Value.Trim();
        return true;
    }

    private static string CleanInlineMarkdown(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        var cleaned = value.Replace("`", string.Empty, StringComparison.Ordinal);
        cleaned = BoldRegex.Replace(cleaned, "$1");
        cleaned = ItalicAsteriskRegex.Replace(cleaned, "$1");
        cleaned = ItalicUnderscoreRegex.Replace(cleaned, "$1");
        return cleaned;
    }

    private static string SanitizeMessageContent(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return string.Empty;
        }

        var lines = content.Replace("\r\n", "\n", StringComparison.Ordinal).Split('\n');
        var sanitized = new List<string>(lines.Length);
        var skippingToolPayload = false;

        foreach (var line in lines)
        {
            var trimmed = line.Trim();
            if (IsToolTraceHeader(trimmed) || IsToolTraceResult(trimmed))
            {
                skippingToolPayload = true;
                continue;
            }

            if (skippingToolPayload)
            {
                if (string.IsNullOrWhiteSpace(trimmed) || LooksLikeToolPayloadLine(trimmed))
                {
                    continue;
                }

                skippingToolPayload = false;
            }

            if (sanitized.Count == 0 &&
                (string.IsNullOrWhiteSpace(trimmed) || LooksLikeToolPayloadLine(trimmed)))
            {
                continue;
            }

            sanitized.Add(line);
        }

        return sanitized.Count == 0 ? string.Empty : string.Join('\n', sanitized).Trim();
    }

    private static bool IsToolTraceHeader(string trimmed)
    {
        return trimmed.StartsWith("Tool Call:", StringComparison.OrdinalIgnoreCase) ||
            trimmed.StartsWith("Calling ", StringComparison.OrdinalIgnoreCase) ||
            trimmed.StartsWith("Arguments:", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsToolTraceResult(string trimmed)
    {
        return trimmed.StartsWith("Tool Result:", StringComparison.OrdinalIgnoreCase) ||
            trimmed.StartsWith("Got Result From ", StringComparison.OrdinalIgnoreCase);
    }

    private static bool LooksLikeToolPayloadLine(string trimmed)
    {
        if (string.IsNullOrWhiteSpace(trimmed))
        {
            return true;
        }

        return trimmed[0] is '{' or '}' or '[' or ']' or '"' ||
            trimmed.EndsWith("\",", StringComparison.Ordinal) ||
            trimmed.EndsWith('}') ||
            trimmed.EndsWith(']');
    }

    private static string FirstNonEmpty(params string?[] values)
    {
        return values.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value)) ?? string.Empty;
    }
}
