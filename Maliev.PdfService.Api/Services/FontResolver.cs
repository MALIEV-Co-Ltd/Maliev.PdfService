using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using QuestPDF.Drawing;

namespace Maliev.PdfService.Api.Services;

public interface IFontResolver
{
    void RegisterFonts();
}

public class FontResolver : IFontResolver
{
    private readonly IWebHostEnvironment _env;

    public FontResolver(IWebHostEnvironment env)
    {
        _env = env;
    }

    public void RegisterFonts()
    {
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
    }
}
