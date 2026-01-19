using Maliev.PdfService.Api.Models.Data;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace Maliev.PdfService.Api.Services.Layouts;

/// <summary>
/// QuestPDF layout for Quotation documents.
/// </summary>
public class QuotationDocument : IDocument
{
    /// <summary>
    /// The quotation data.
    /// </summary>
    public QuotationData Data { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="QuotationDocument"/> class.
    /// </summary>
    public QuotationDocument(QuotationData data)
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
            page.DefaultTextStyle(x => x.FontFamily("Kanit"));

            page.Header().Row(row =>
            {
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text("QUOTATION").FontSize(20).SemiBold();
                    col.Item().Text($"Quotation #: {Data.QuotationNumber}");
                });
            });

            page.Content().PaddingVertical(20).Column(col =>
            {
                col.Item().Text($"Customer: {Data.CustomerName}");
                col.Item().Text($"Date: {Data.QuotationDate:yyyy-MM-dd}");

                col.Item().PaddingTop(10).Text("Quotation details would go here.");
            });
        });
    }
}
