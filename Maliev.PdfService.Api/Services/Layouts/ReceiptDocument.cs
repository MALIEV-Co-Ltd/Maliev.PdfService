using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Maliev.PdfService.Api.Services.Layouts;

public class ReceiptDocument : IDocument
{
    public dynamic Data { get; }

    public ReceiptDocument(dynamic data)
    {
        Data = data;
    }

    public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

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
