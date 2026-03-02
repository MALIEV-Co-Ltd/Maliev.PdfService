using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace Maliev.PdfService.Api.Services.Layouts;

/// <summary>
/// QuestPDF layout for Financial Report documents.
/// </summary>
public class FinancialReportDocument : IDocument
{
    /// <summary>The report data.</summary>
    public dynamic Data { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="FinancialReportDocument"/> class.
    /// </summary>
    public FinancialReportDocument(dynamic data)
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
            page.Header().Text("FINANCIAL REPORT").FontSize(20).SemiBold();
            page.Content().Text("Financial report details would go here.");
        });
    }
}
