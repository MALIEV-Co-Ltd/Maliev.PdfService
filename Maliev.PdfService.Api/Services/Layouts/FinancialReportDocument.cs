using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Maliev.PdfService.Api.Services.Layouts;

public class FinancialReportDocument : IDocument
{
    public dynamic Data { get; }

    public FinancialReportDocument(dynamic data)
    {
        Data = data;
    }

    public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

    public void Compose(IDocumentContainer container)
    {
        container.Page(page =>
        {
            page.Margin(50);
            page.Header().Text("FINANCIAL REPORT").FontSize(20).SemiBold();
            page.Content().Text("Financial report details would go here.");
        });
    }
}
