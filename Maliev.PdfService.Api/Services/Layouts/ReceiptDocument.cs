using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace Maliev.PdfService.Api.Services.Layouts;

/// <summary>
/// QuestPDF layout for Receipt documents.
/// </summary>
public class ReceiptDocument : IDocument
{
    /// <summary>The receipt data.</summary>
    public dynamic Data { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ReceiptDocument"/> class.
    /// </summary>
    public ReceiptDocument(dynamic data)
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
            page.Margin(50);
            page.Header().Text("RECEIPT").FontSize(20).SemiBold();
            page.Content().Text("Receipt details would go here.");
        });
    }
}
