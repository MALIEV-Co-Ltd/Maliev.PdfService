using Maliev.PdfService.Domain.Entities;

namespace Maliev.PdfService.Api.Services;

/// <summary>
/// Service contract for generating PDF documents.
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
