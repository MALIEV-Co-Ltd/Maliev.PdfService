using Maliev.PdfService.Api.Models.Data;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Maliev.PdfService.Api.Services.Layouts;

public class ChatTranscriptDocument(ChatTranscriptData data) : IDocument
{
    private static readonly string UserBubbleBg = Colors.Blue.Lighten5;
    private static readonly string AssistantBubbleBg = Colors.Grey.Lighten4;
    private static readonly string BrandColor = Colors.Black;
    private static readonly string BodyText = Colors.Grey.Darken3;
    private static readonly string MutedText = Colors.Grey.Darken1;
    private static readonly string Hairline = Colors.Grey.Lighten2;

    public ChatTranscriptData Data { get; } = data;

    public DocumentMetadata GetMetadata() => new()
    {
        Title = $"Chat Transcript — {Data.SessionId}",
        Author = "MALIEV Make Studio",
        Subject = $"Customer assistant chat transcript from {Data.GeneratedAt:yyyy-MM-dd HH:mm}",
        Keywords = "MALIEV, Make Studio, chat transcript, customer assistant"
    };

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
                row.ConstantItem(200).AlignRight().Column(col =>
                {
                    col.Item().AlignRight().Text("Chat Transcript").FontSize(16).Bold();
                    col.Item().AlignRight().Text("Make Studio Customer Assistant").FontSize(10).FontColor(MutedText);
                });
            });

            header.Item().PaddingTop(8).LineHorizontal(1).LineColor(Hairline);

            header.Item().PaddingTop(8).Row(info =>
            {
                info.RelativeItem().Text($"Session: {Data.SessionId}").FontSize(8).FontColor(MutedText);
                info.RelativeItem().AlignRight().Text($"Generated: {Data.GeneratedAt:dd MMM yyyy HH:mm}").FontSize(8).FontColor(MutedText);
            });

            if (!string.IsNullOrWhiteSpace(Data.CustomerName) || !string.IsNullOrWhiteSpace(Data.CustomerEmail))
            {
                header.Item().PaddingTop(2).Row(info =>
                {
                    info.RelativeItem().Text($"Customer: {Data.CustomerName ?? Data.CustomerEmail}").FontSize(8).FontColor(MutedText);
                    if (!string.IsNullOrWhiteSpace(Data.CustomerEmail) && Data.CustomerName is not null)
                        info.RelativeItem().AlignRight().Text(Data.CustomerEmail).FontSize(8).FontColor(MutedText);
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
            foreach (var message in Data.Messages)
            {
                var messageDate = DateOnly.FromDateTime(message.Timestamp.DateTime);
                if (messageDate != lastDate)
                {
                    lastDate = messageDate;
                    content.Item().PaddingVertical(6).AlignCenter().Text(messageDate.ToString("dd MMM yyyy"))
                        .FontSize(8).Bold().FontColor(MutedText).Background(Colors.Grey.Lighten5).PaddingHorizontal(8).PaddingVertical(3);
                }

                content.Item().PaddingBottom(8).Element(c => ComposeMessage(c, message));
            }
        });
    }

    private void ComposeMessage(IContainer container, ChatMessageData message)
    {
        var isUser = message.Role.Equals("user", StringComparison.OrdinalIgnoreCase);
        var bgColor = isUser ? UserBubbleBg : AssistantBubbleBg;
        var roleLabel = isUser ? "You" : "MALIEV Assistant";
        var alignment = isUser ? 0.25f : 0f;

        container.PaddingLeft(isUser ? 80 : 0).PaddingRight(isUser ? 0 : 80).Column(col =>
        {
            col.Item().Row(row =>
            {
                row.RelativeItem().Text(roleLabel).FontSize(7).Bold().FontColor(MutedText);
                row.RelativeItem().AlignRight().Text(message.Timestamp.ToLocalTime().ToString("HH:mm")).FontSize(7).FontColor(MutedText);
            });

            col.Item().PaddingTop(3).Background(bgColor).Border(1).BorderColor(Hairline).Padding(10).Text(message.Content)
                .FontSize(9).LineHeight(1.35f);
        });
    }

    private void ComposeFooter(IContainer container)
    {
        container.BorderTop(1).BorderColor(Hairline).PaddingTop(8).Row(row =>
        {
            row.RelativeItem().Text("MALIEV's Make Studio").FontSize(7).FontColor(MutedText);
            row.RelativeItem().AlignRight().Text(text =>
            {
                text.Span("Generated from MALIEV's Make Studio — ").FontSize(7).FontColor(MutedText);
                text.Span("Page ").FontSize(7).FontColor(MutedText);
                text.CurrentPageNumber().FontSize(7).FontColor(MutedText);
                text.Span(" of ").FontSize(7).FontColor(MutedText);
                text.TotalPages().FontSize(7).FontColor(MutedText);
            });
        });
    }
}
