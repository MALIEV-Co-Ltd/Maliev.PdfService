using Maliev.PdfService.Api.Services;
using Microsoft.AspNetCore.Hosting;
using Moq;
using Xunit;

namespace Maliev.PdfService.Tests.Unit;

public class FontResolverTests
{
    private readonly Mock<IWebHostEnvironment> _envMock = new();
    private readonly FontResolver _resolver;

    public FontResolverTests()
    {
        _resolver = new FontResolver(_envMock.Object);
    }

    [Fact]
    public void RegisterFonts_DoesNotThrow_WhenDirectoryMissing()
    {
        // Arrange
        _envMock.Setup(x => x.ContentRootPath).Returns(Path.GetTempPath());

        // Act & Assert
        _resolver.RegisterFonts();
    }

    [Fact]
    public void RegisterFonts_DoesNotThrow_WhenFontsMissing()
    {
        // Arrange
        var tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(Path.Combine(tempPath, "Resources", "Fonts"));
        _envMock.Setup(x => x.ContentRootPath).Returns(tempPath);

        // Act & Assert
        try
        {
            _resolver.RegisterFonts();
        }
        finally
        {
            Directory.Delete(tempPath, true);
        }
    }
}
