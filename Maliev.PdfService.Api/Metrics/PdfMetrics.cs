using System.Diagnostics.Metrics;

namespace Maliev.PdfService.Api.Metrics;

/// <summary>
/// Service for recording PDF generation metrics.
/// </summary>
public class PdfMetrics
{
    private readonly Counter<long> _docsGeneratedTotal;
    private readonly Histogram<double> _generationDuration;

    /// <summary>
    /// Initializes a new instance of the <see cref="PdfMetrics"/> class.
    /// </summary>
    public PdfMetrics(IMeterFactory meterFactory)
    {
        var meter = meterFactory.Create("Maliev.PdfService");
        _docsGeneratedTotal = meter.CreateCounter<long>("pdf_docs_generated_total", "Number of documents generated");
        _generationDuration = meter.CreateHistogram<double>("pdf_generation_duration_seconds", "Duration of PDF generation");
    }

    /// <summary>
    /// Records that a document has been generated.
    /// </summary>
    /// <param name="documentType">The type of document.</param>
    public void RecordDocGenerated(string documentType)
    {
        _docsGeneratedTotal.Add(1, new KeyValuePair<string, object?>("document_type", documentType));
    }

    /// <summary>
    /// Records the duration of a PDF generation.
    /// </summary>
    /// <param name="documentType">The type of document.</param>
    /// <param name="durationSeconds">The duration in seconds.</param>
    public void RecordGenerationDuration(string documentType, double durationSeconds)
    {
        _generationDuration.Record(durationSeconds, new KeyValuePair<string, object?>("document_type", documentType));
    }
}
