using Microsoft.AspNetCore.Hosting;
using QuestPDF.Drawing;

namespace Maliev.PdfService.Api.Services;

/// <summary>
/// Default implementation of the font resolver.
/// </summary>
public class FontResolver : IFontResolver
{
    private readonly IWebHostEnvironment _env;
    private static bool _fontsRegistered;
    private static readonly object LockObject = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="FontResolver"/> class.
    /// </summary>
    /// <param name="env">The web host environment.</param>
    public FontResolver(IWebHostEnvironment env)
    {
        _env = env;
    }

    /// <inheritdoc/>
    public void RegisterFonts()
    {
        if (_fontsRegistered)
        {
            return;
        }

        lock (LockObject)
        {
            if (_fontsRegistered)
            {
                return;
            }

            var fontsPath = Path.Combine(_env.ContentRootPath, "Resources", "Fonts");
            if (Directory.Exists(fontsPath))
            {
                RegisterFontIfExists(Path.Combine(fontsPath, "Kanit-Regular.ttf"));
                RegisterFontIfExists(Path.Combine(fontsPath, "Kanit-Bold.ttf"));
            }

            _fontsRegistered = true;
        }
    }

    private static void RegisterFontIfExists(string fontPath)
    {
        if (!File.Exists(fontPath))
        {
            return;
        }

        using var stream = File.OpenRead(fontPath);
        FontManager.RegisterFont(stream);
    }
}
