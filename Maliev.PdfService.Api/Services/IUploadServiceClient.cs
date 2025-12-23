using System.Net.Http.Headers;

namespace Maliev.PdfService.Api.Services;

public interface IUploadServiceClient
{
    Task<string> UploadFileAsync(string fileName, byte[] content, string contentType, string storagePath, CancellationToken cancellationToken = default);
}

public class UploadServiceClient : IUploadServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<UploadServiceClient> _logger;

    public UploadServiceClient(HttpClient httpClient, ILogger<UploadServiceClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<string> UploadFileAsync(string fileName, byte[] content, string contentType, string storagePath, CancellationToken cancellationToken = default)
    {
        using var requestContent = new MultipartFormDataContent();
        
        var fileContent = new ByteArrayContent(content);
        fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse(contentType);
        requestContent.Add(fileContent, "File", fileName);
        
        requestContent.Add(new StringContent(storagePath), "Path");
        requestContent.Add(new StringContent("PdfService"), "ServiceName");
        requestContent.Add(new StringContent("true"), "Overwrite");

        var response = await _httpClient.PostAsync("upload/v1/uploads", requestContent, cancellationToken);
        
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError("Failed to upload file to UploadService. Status: {StatusCode}, Error: {Error}", response.StatusCode, error);
            throw new InvalidOperationException($"Failed to upload file to UploadService: {response.StatusCode}");
        }

        var result = await response.Content.ReadFromJsonAsync<UploadResponse>(cancellationToken: cancellationToken);
        return result?.StoragePath ?? throw new InvalidOperationException("UploadService returned empty storage path");
    }

    private record UploadResponse([property: System.Text.Json.Serialization.JsonPropertyName("storagePath")] string StoragePath);
}
