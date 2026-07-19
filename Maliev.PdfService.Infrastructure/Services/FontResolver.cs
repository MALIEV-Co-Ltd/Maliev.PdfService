using Microsoft.AspNetCore.Hosting;
using QuestPDF.Drawing;

namespace Maliev.PdfService.Api.Services;

/// <summary>
/// Default implementation of the font resolver.
/// Registers Roboto (Latin/English) and Noto Sans Thai for bilingual PDF generation.
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
                // Roboto — Latin/English (Regular, SemiBold, Bold)
                RegisterFontIfExists(Path.Combine(fontsPath, "Roboto-Regular.ttf"));
                RegisterFontIfExists(Path.Combine(fontsPath, "Roboto-SemiBold.ttf"));
                RegisterFontIfExists(Path.Combine(fontsPath, "Roboto-Bold.ttf"));

                // Noto Sans Thai — Thai script (Regular, SemiBold, Bold)
                RegisterFontIfExists(Path.Combine(fontsPath, "NotoSansThai-Regular.ttf"));
                RegisterFontIfExists(Path.Combine(fontsPath, "NotoSansThai-SemiBold.ttf"));
                RegisterFontIfExists(Path.Combine(fontsPath, "NotoSansThai-Bold.ttf"));
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
