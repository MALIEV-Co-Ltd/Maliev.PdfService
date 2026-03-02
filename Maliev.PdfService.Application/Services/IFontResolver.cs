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
