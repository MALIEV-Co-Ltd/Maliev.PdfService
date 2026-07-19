using System.ComponentModel.DataAnnotations;

namespace Maliev.PdfService.Api.Models.Data;

/// <summary>
/// Data contract for a customer-facing blog practical note PDF.
/// </summary>
public sealed class BlogPracticalNoteData
{
    /// <summary>Gets or sets the source blog post slug.</summary>
    [Required]
    public string Slug { get; set; } = string.Empty;

    /// <summary>Gets or sets the requested culture name.</summary>
    [Required]
    public string CultureName { get; set; } = "en";

    /// <summary>Gets or sets the localized blog post title.</summary>
    [Required]
    public string Title { get; set; } = string.Empty;

    /// <summary>Gets or sets the localized blog post summary.</summary>
    [Required]
    public string Summary { get; set; } = string.Empty;

    /// <summary>Gets or sets the localized blog post category.</summary>
    [Required]
    public string Category { get; set; } = string.Empty;

    /// <summary>Gets or sets the canonical public HTTPS URL for the blog post.</summary>
    [Required]
    [Url]
    public string PublicUrl { get; set; } = string.Empty;

    /// <summary>Gets or sets the optional cover image.</summary>
    public BlogPracticalNoteImageData? CoverImage { get; set; }

    /// <summary>Gets or sets the localized article sections.</summary>
    public List<BlogPracticalNoteSectionData> Sections { get; set; } = [];

    /// <summary>Gets or sets the localized takeaway checklist items.</summary>
    public List<string> Takeaways { get; set; } = [];
}

/// <summary>
/// Data contract for one section in a blog practical note PDF.
/// </summary>
public sealed class BlogPracticalNoteSectionData
{
    /// <summary>Gets or sets the localized section title.</summary>
    [Required]
    public string Title { get; set; } = string.Empty;

    /// <summary>Gets or sets the localized section body.</summary>
    [Required]
    public string Body { get; set; } = string.Empty;

    /// <summary>Gets or sets localized supporting points for the section.</summary>
    public List<string> Items { get; set; } = [];

    /// <summary>Gets or sets the optional section image.</summary>
    public BlogPracticalNoteImageData? Image { get; set; }
}

/// <summary>
/// Data contract for an embedded blog practical note image.
/// </summary>
public sealed class BlogPracticalNoteImageData
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
