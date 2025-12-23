using System.Diagnostics.Metrics;

namespace Maliev.PdfService.Api.Metrics;

public class PdfMetrics
{
    private readonly Counter<long> _docsGeneratedTotal;
    private readonly Histogram<double> _generationDuration;

    public PdfMetrics(IMeterFactory meterFactory)
    {
        var meter = meterFactory.Create("Maliev.PdfService");
        _docsGeneratedTotal = meter.CreateCounter<long>("pdf_docs_generated_total", "Number of documents generated");
        _generationDuration = meter.CreateHistogram<double>("pdf_generation_duration_seconds", "Duration of PDF generation");
    }

    public void RecordDocGenerated(string documentType)
    {
        _docsGeneratedTotal.Add(1, new KeyValuePair<string, object?>("document_type", documentType));
    }

    public void RecordGenerationDuration(string documentType, double durationSeconds)
    {
        _generationDuration.Record(durationSeconds, new KeyValuePair<string, object?>("document_type", documentType));
    }
}
