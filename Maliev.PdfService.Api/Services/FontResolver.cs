using QuestPDF.Drawing;

namespace Maliev.PdfService.Api.Services;

/// <summary>
/// Service for resolving and registering custom fonts for PDF generation.
/// </summary>
public interface IFontResolver
{
    /// <summary>
    /// Registers custom fonts with the QuestPDF FontManager.
    /// </summary>
    void RegisterFonts();
}

/// <summary>
/// Default implementation of the font resolver.
/// </summary>
public class FontResolver : IFontResolver
{
    private readonly IWebHostEnvironment _env;
    private static bool _fontsRegistered = false;
    private static readonly object _lock = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="FontResolver"/> class.
    /// </summary>
    public FontResolver(IWebHostEnvironment env)
    {
        _env = env;
    }

    /// <inheritdoc/>
    public void RegisterFonts()
    {
        if (_fontsRegistered) return;

        lock (_lock)
        {
            if (_fontsRegistered) return;

            // Path to embedded or local fonts
            var fontsPath = Path.Combine(_env.ContentRootPath, "Resources", "Fonts");

            if (Directory.Exists(fontsPath))
            {
                var regularPath = Path.Combine(fontsPath, "Kanit-Regular.ttf");
                var boldPath = Path.Combine(fontsPath, "Kanit-Bold.ttf");

                if (File.Exists(regularPath))
                {
                    using var stream = File.OpenRead(regularPath);
                    FontManager.RegisterFont(stream);
                }

                if (File.Exists(boldPath))
                {
                    using var stream = File.OpenRead(boldPath);
                    FontManager.RegisterFont(stream);
                }
            }

            _fontsRegistered = true;
        }
    }
}
