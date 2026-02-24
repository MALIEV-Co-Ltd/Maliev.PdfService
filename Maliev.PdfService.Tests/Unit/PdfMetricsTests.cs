using Maliev.PdfService.Api.Metrics;
using Moq;
using System.Diagnostics.Metrics;
using Xunit;

namespace Maliev.PdfService.Tests.Unit;

public class PdfMetricsTests
{
    private readonly Mock<IMeterFactory> _meterFactoryMock = new();
    private readonly PdfMetrics _metrics;

    public PdfMetricsTests()
    {
        _meterFactoryMock.Setup(x => x.Create(It.IsAny<MeterOptions>()))
            .Returns(new Meter("TestMeter"));
        _metrics = new PdfMetrics(_meterFactoryMock.Object);
    }

    [Fact]
    public void RecordDocGenerated_DoesNotThrow()
    {
        _metrics.RecordDocGenerated("Invoice");
    }

    [Fact]
    public void RecordGenerationDuration_DoesNotThrow()
    {
        _metrics.RecordGenerationDuration("Invoice", 1.5);
    }
}
