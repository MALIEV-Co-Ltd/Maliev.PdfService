using Maliev.PdfService.Api.Services;
using Microsoft.AspNetCore.Hosting;
using Moq;
using Xunit;

namespace Maliev.PdfService.Tests.Unit;

/// <summary>
/// Unit tests for PDF font registration behavior.
/// </summary>
public class FontResolverTests
{
    private readonly Mock<IWebHostEnvironment> _envMock = new();
    private readonly FontResolver _resolver;

    /// <summary>
    /// Initializes a new instance of the <see cref="FontResolverTests"/> class.
    /// </summary>
    public FontResolverTests()
    {
        _resolver = new FontResolver(_envMock.Object);
    }

    /// <summary>
    /// Verifies that font registration tolerates a missing resources directory.
    /// </summary>
    [Fact]
    public void RegisterFonts_DoesNotThrow_WhenDirectoryMissing()
    {
        // Arrange
        _envMock.Setup(x => x.ContentRootPath).Returns(Path.GetTempPath());

        // Act & Assert
        _resolver.RegisterFonts();
    }

    /// <summary>
    /// Verifies that font registration tolerates an empty fonts directory.
    /// </summary>
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
