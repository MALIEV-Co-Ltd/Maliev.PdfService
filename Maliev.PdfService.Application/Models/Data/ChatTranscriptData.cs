namespace Maliev.PdfService.Api.Models.Data;

public class ChatTranscriptData
{
    public string SessionId { get; set; } = string.Empty;

    public string? CustomerName { get; set; }

    public string? CustomerEmail { get; set; }

    public string Language { get; set; } = "en";

    public DateTimeOffset GeneratedAt { get; set; } = DateTimeOffset.UtcNow;

    public string? GeneratedBy { get; set; }

    public required List<ChatMessageData> Messages { get; set; }
}

public class ChatMessageData
{
    public string Role { get; set; } = "assistant";

    public string Content { get; set; } = string.Empty;

    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
}
