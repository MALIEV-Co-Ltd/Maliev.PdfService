namespace Maliev.PdfService.Api.Services;

/// <summary>
/// Client for interacting with the Upload Service.
/// </summary>
public interface IUploadServiceClient
{
    /// <summary>
    /// Uploads a file to the central storage via the Upload Service.
    /// </summary>
    /// <param name="fileName">The name of the file to upload.</param>
    /// <param name="content">The file content bytes.</param>
    /// <param name="contentType">The MIME type of the file.</param>
    /// <param name="storagePath">The target storage path.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The resulting storage path or URL.</returns>
    Task<string> UploadFileAsync(string fileName, byte[] content, string contentType, string storagePath, CancellationToken cancellationToken = default);
}
