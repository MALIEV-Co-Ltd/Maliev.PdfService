using Maliev.PdfService.Domain.Entities;
using QuestPDF.Infrastructure;

namespace Maliev.PdfService.Api.Services;

/// <summary>
/// Factory for creating QuestPDF document implementations based on document type.
/// </summary>
public interface IDocumentFactory
{
    /// <summary>
    /// Creates a document instance for the specified type and data.
    /// </summary>
    /// <param name="type">The type of document to create.</param>
    /// <param name="data">The data to bind to the document.</param>
    /// <returns>A QuestPDF IDocument instance.</returns>
    IDocument CreateDocument(DocumentType type, object data);
}
