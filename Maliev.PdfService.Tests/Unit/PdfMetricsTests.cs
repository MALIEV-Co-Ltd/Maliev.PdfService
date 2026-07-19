using Maliev.PdfService.Api.Metrics;
using Moq;
using System.Diagnostics.Metrics;
using Xunit;

namespace Maliev.PdfService.Tests.Unit;

/// <summary>
/// Unit tests for PDF service metrics recording.
/// </summary>
public class PdfMetricsTests
{
    private readonly Mock<IMeterFactory> _meterFactoryMock = new();
    private readonly PdfMetrics _metrics;

    /// <summary>
    /// Initializes a new instance of the <see cref="PdfMetricsTests"/> class.
    /// </summary>
    public PdfMetricsTests()
    {
        _meterFactoryMock.Setup(x => x.Create(It.IsAny<MeterOptions>()))
            .Returns(new Meter("TestMeter"));
        _metrics = new PdfMetrics(_meterFactoryMock.Object);
    }

    /// <summary>
    /// Verifies that recording a generated document metric does not throw.
    /// </summary>
    [Fact]
    public void RecordDocGenerated_DoesNotThrow()
    {
        _metrics.RecordDocGenerated("Invoice");
    }

    /// <summary>
    /// Verifies that recording a PDF generation duration metric does not throw.
    /// </summary>
    [Fact]
    public void RecordGenerationDuration_DoesNotThrow()
    {
        _metrics.RecordGenerationDuration("Invoice", 1.5);
    }
}
