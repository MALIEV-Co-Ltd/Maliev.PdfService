using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Logging;

namespace Maliev.PdfService.Api.Services;

/// <summary>
/// Default implementation of the Upload Service client.
/// </summary>
public class UploadServiceClient : IUploadServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<UploadServiceClient> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="UploadServiceClient"/> class.
    /// </summary>
    public UploadServiceClient(HttpClient httpClient, ILogger<UploadServiceClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<string> UploadFileAsync(string fileName, byte[] content, string contentType, string storagePath, CancellationToken cancellationToken = default)
    {
        var initiateRequest = new InitiateResumableUploadRequest(
            Path: storagePath,
            FileName: fileName,
            ServiceName: "PdfService",
            ContentType: contentType,
            TotalSize: content.LongLength,
            Overwrite: true);

        var initiateResponse = await _httpClient.PostAsJsonAsync("upload/v1/uploads/resumable", initiateRequest, cancellationToken);
        if (!initiateResponse.IsSuccessStatusCode)
        {
            var error = await initiateResponse.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError("Failed to initiate UploadService upload. Status: {StatusCode}, Error: {Error}", initiateResponse.StatusCode, error);
            throw new InvalidOperationException($"Failed to initiate UploadService upload: {initiateResponse.StatusCode}");
        }

        var session = await initiateResponse.Content.ReadFromJsonAsync<InitiateResumableUploadResponse>(cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException("UploadService returned an empty resumable session");

        using var fileContent = new ByteArrayContent(content);
        fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse(contentType);
        fileContent.Headers.ContentLength = content.LongLength;
        fileContent.Headers.ContentRange = new ContentRangeHeaderValue(0, content.LongLength - 1, content.LongLength);

        var gcsResponse = await _httpClient.PutAsync(
            $"upload/v1/uploads/resumable/{Uri.EscapeDataString(session.UploadId)}",
            fileContent,
            cancellationToken);
        if (!gcsResponse.IsSuccessStatusCode)
        {
            var error = await gcsResponse.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError("Failed to upload file through UploadService resumable proxy. Status: {StatusCode}, Error: {Error}", gcsResponse.StatusCode, error);
            throw new InvalidOperationException($"Failed to upload file through UploadService proxy: {gcsResponse.StatusCode}");
        }

        var completeResponse = await _httpClient.PostAsJsonAsync(
            $"upload/v1/uploads/resumable/{session.UploadId}/complete",
            new { },
            cancellationToken);

        if (!completeResponse.IsSuccessStatusCode)
        {
            var error = await completeResponse.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError("Failed to complete UploadService upload. Status: {StatusCode}, Error: {Error}", completeResponse.StatusCode, error);
            throw new InvalidOperationException($"Failed to complete UploadService upload: {completeResponse.StatusCode}");
        }

        var result = await completeResponse.Content.ReadFromJsonAsync<UploadResponse>(cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException("UploadService returned empty upload response");

        return result.SignedUrl ?? result.StoragePath ?? throw new InvalidOperationException("UploadService returned empty signed URL and storage path");
    }

    private sealed record UploadResponse(
        [property: System.Text.Json.Serialization.JsonPropertyName("storagePath")] string? StoragePath,
        [property: System.Text.Json.Serialization.JsonPropertyName("signedUrl")] string? SignedUrl);

    private sealed record InitiateResumableUploadRequest(
        string Path,
        string FileName,
        string ServiceName,
        string ContentType,
        long TotalSize,
        bool Overwrite);

    private sealed record InitiateResumableUploadResponse(
        string UploadId,
        string SessionUri,
        DateTime ExpiresAt,
        long TotalSize);
}
