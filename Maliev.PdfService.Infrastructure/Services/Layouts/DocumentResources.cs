namespace Maliev.PdfService.Api.Services.Layouts;

internal static class DocumentResources
{
    public static string ReadText(string fileName)
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Resources", fileName);
        return File.ReadAllText(path);
    }
}
