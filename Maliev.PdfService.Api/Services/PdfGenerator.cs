using Maliev.PdfService.Data.Entities;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using QuestPDF.Previewer;

namespace Maliev.PdfService.Api.Services;

public interface IPdfGenerator
{
    Task<byte[]> GeneratePdfAsync(DocumentType type, dynamic data, string? configJson = null);
}

public class PdfGenerator : IPdfGenerator
{
    private readonly IDocumentFactory _documentFactory;
    private readonly IWebHostEnvironment _env;

    public PdfGenerator(IDocumentFactory documentFactory, IWebHostEnvironment env)
    {
        _documentFactory = documentFactory;
        _env = env;
    }

    public async Task<byte[]> GeneratePdfAsync(DocumentType type, object data, string? configJson = null)
    {
        IDocument document = _documentFactory.CreateDocument(type, data);

        if (_env.IsDevelopment())
        {
            // Show in companion for hot-reload preview during development
            // This is non-blocking
            // _ = Task.Run(() => document.ShowInCompanion());
        }

        return document.GeneratePdf();
    }
}
