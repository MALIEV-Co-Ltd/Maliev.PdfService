using Maliev.PdfService.Data.Entities;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace Maliev.PdfService.Api.Services;

/// <summary>
/// Service for generating PDF documents using QuestPDF.
/// </summary>
public interface IPdfGenerator
{
    /// <summary>
    /// Generates a PDF byte array asynchronously.
    /// </summary>
    /// <param name="type">The type of document to generate.</param>
    /// <param name="data">The data to bind to the document.</param>
    /// <param name="templateCode">The specific template code to use.</param>
    /// <returns>A byte array containing the generated PDF.</returns>
    Task<byte[]> GeneratePdfAsync(DocumentType type, object data, string? templateCode = null);

    /// <summary>
    /// Constructs a standardized storage path for a generated document.
    /// </summary>
    /// <param name="type">The type of document.</param>
    /// <param name="referenceId">The business reference ID.</param>
    /// <param name="fileName">The name of the file.</param>
    /// <returns>A storage path string.</returns>
    string GetStoragePath(DocumentType type, string referenceId, string fileName);
}

/// <summary>
/// Default implementation of the PDF generator.
/// </summary>
public class PdfGenerator : IPdfGenerator
{
    private readonly IDocumentFactory _documentFactory;
    private readonly IWebHostEnvironment _env;

    /// <summary>
    /// Initializes a new instance of the <see cref="PdfGenerator"/> class.
    /// </summary>
    public PdfGenerator(IDocumentFactory documentFactory, IWebHostEnvironment env)
    {
        _documentFactory = documentFactory;
        _env = env;
    }

    /// <inheritdoc/>
    public async Task<byte[]> GeneratePdfAsync(DocumentType type, object data, string? templateCode = null)
    {
        // The templateCode can be used to select specific styling or layout variants in the factory
        IDocument document = _documentFactory.CreateDocument(type, data);

        // Offload the heavy synchronous GeneratePdf call to a background thread to prevent thread pool starvation
        return await Task.Run(() => document.GeneratePdf());
    }

    /// <inheritdoc/>
    public string GetStoragePath(DocumentType type, string referenceId, string fileName)
    {
        return $"pdfs/{type.ToString().ToLower()}/{referenceId}/{fileName}";
    }
}
