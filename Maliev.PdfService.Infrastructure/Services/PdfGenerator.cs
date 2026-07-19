using Maliev.PdfService.Domain.Entities;
using Microsoft.AspNetCore.Hosting;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace Maliev.PdfService.Api.Services;

/// <summary>
/// Default implementation of the PDF generator.
/// </summary>
public class PdfGenerator : IPdfGenerator
{
    private readonly IDocumentFactory _documentFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="PdfGenerator"/> class.
    /// </summary>
    /// <param name="documentFactory">The document factory.</param>
    /// <param name="env">The host environment (unused, preserved for DI compatibility).</param>
    public PdfGenerator(IDocumentFactory documentFactory, IWebHostEnvironment env)
    {
        _documentFactory = documentFactory;
        _ = env;
    }

    /// <inheritdoc/>
    public async Task<byte[]> GeneratePdfAsync(DocumentType type, object data, string? templateCode = null)
    {
        _ = templateCode;
        IDocument document = _documentFactory.CreateDocument(type, data);
        return await Task.Run(document.GeneratePdf);
    }

    /// <inheritdoc/>
    public string GetStoragePath(DocumentType type, string referenceId, string fileName)
    {
        return $"pdfs/{type.ToString().ToLowerInvariant()}/{referenceId}/{fileName}";
    }
}
