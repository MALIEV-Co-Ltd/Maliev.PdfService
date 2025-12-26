using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Maliev.PdfService.Api.Services.Layouts;

public class QuotationDocument : IDocument
{
    public dynamic Data { get; }

    public QuotationDocument(dynamic data)
    {
        Data = data;
    }

    public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

    public void Compose(IDocumentContainer container)
    {
        container.Page(page =>
        {
            page.Margin(50);
            page.DefaultTextStyle(x => x.FontFamily("Kanit"));

            page.Header().Text("QUOTATION").FontSize(20).SemiBold();
            page.Content().Text("Quotation details would go here.");
        });
    }
}
