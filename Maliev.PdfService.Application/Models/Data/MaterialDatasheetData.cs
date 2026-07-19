using System.ComponentModel.DataAnnotations;

namespace Maliev.PdfService.Api.Models.Data;

/// <summary>
/// Data contract for a customer-facing material datasheet PDF.
/// </summary>
public sealed class MaterialDatasheetData
{
    /// <summary>Gets or sets the source material slug.</summary>
    [Required]
    public string Slug { get; set; } = string.Empty;

    /// <summary>Gets or sets the requested culture name.</summary>
    [Required]
    public string CultureName { get; set; } = "en";

    /// <summary>Gets or sets the material name (e.g. "PA12 nylon").</summary>
    [Required]
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets the localized material category label.</summary>
    [Required]
    public string CategoryLabel { get; set; } = string.Empty;

    /// <summary>Gets or sets the manufacturing process label (e.g. "MJF / SLS").</summary>
    [Required]
    public string ProcessLabel { get; set; } = string.Empty;

    /// <summary>Gets or sets the localized material family subtitle.</summary>
    [Required]
    public string Family { get; set; } = string.Empty;

    /// <summary>Gets or sets the canonical public HTTPS URL for the material detail page.</summary>
    [Required]
    [Url]
    public string PublicUrl { get; set; } = string.Empty;

    /// <summary>Gets or sets the localized typical-values disclaimer line.</summary>
    [Required]
    public string Disclaimer { get; set; } = string.Empty;

    /// <summary>Gets or sets the optional cover image.</summary>
    public MaterialDatasheetImageData? CoverImage { get; set; }

    /// <summary>Gets or sets the numeric/typical specification rows.</summary>
    public List<MaterialDatasheetSpecData> Specs { get; set; } = [];

    /// <summary>Gets or sets the qualitative selection-guidance rows.</summary>
    public List<MaterialDatasheetBandData> Bands { get; set; } = [];

    /// <summary>Gets or sets the localized pros / strengths summary.</summary>
    [Required]
    public string Pros { get; set; } = string.Empty;

    /// <summary>Gets or sets the localized watch / check-before-use summary.</summary>
    [Required]
    public string Cons { get; set; } = string.Empty;
}

/// <summary>
/// Data contract for one specification row in a material datasheet PDF.
/// </summary>
public sealed class MaterialDatasheetSpecData
{
    /// <summary>Gets or sets the localized specification label (e.g. "Tensile strength").</summary>
    [Required]
    public string Label { get; set; } = string.Empty;

    /// <summary>Gets or sets the localized specification value (e.g. "~48 MPa").</summary>
    [Required]
    public string Value { get; set; } = string.Empty;

    /// <summary>Gets or sets the optional secondary note (e.g. "ISO 527 (typical)").</summary>
    public string? Note { get; set; }
}

/// <summary>
/// Data contract for one selection-guidance band row in a material datasheet PDF.
/// </summary>
public sealed class MaterialDatasheetBandData
{
    /// <summary>Gets or sets the localized band label (e.g. "Best fit").</summary>
    [Required]
    public string Label { get; set; } = string.Empty;

    /// <summary>Gets or sets the localized band value.</summary>
    [Required]
    public string Value { get; set; } = string.Empty;
}

/// <summary>
/// Data contract for an embedded material datasheet image.
/// </summary>
public sealed class MaterialDatasheetImageData
{
    /// <summary>Gets or sets the source URL or path used by the caller.</summary>
    public string Url { get; set; } = string.Empty;

    /// <summary>Gets or sets the localized image alternative text.</summary>
    public string Alt { get; set; } = string.Empty;

    /// <summary>Gets or sets the localized image caption.</summary>
    public string Caption { get; set; } = string.Empty;

    /// <summary>Gets or sets the image content type.</summary>
    public string ContentType { get; set; } = "image/jpeg";

    /// <summary>Gets or sets the raw image bytes.</summary>
    public byte[] Bytes { get; set; } = [];
}
