using System.Net;
using System.Net.Http.Json;
using Maliev.PdfService.Api.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using Xunit;

namespace Maliev.PdfService.Tests.Unit;

/// <summary>
/// Unit tests for the UploadService HTTP client integration used by PDF generation.
/// </summary>
public class UploadServiceClientTests
{
    private readonly Mock<HttpMessageHandler> _handlerMock = new();
    private readonly Mock<ILogger<UploadServiceClient>> _loggerMock = new();
    private readonly HttpClient _httpClient;
    private readonly UploadServiceClient _client;

    /// <summary>
    /// Initializes a new instance of the <see cref="UploadServiceClientTests"/> class.
    /// </summary>
    public UploadServiceClientTests()
    {
        _httpClient = new HttpClient(_handlerMock.Object)
        {
            BaseAddress = new Uri("http://upload-service/")
        };
        _client = new UploadServiceClient(_httpClient, _loggerMock.Object);
    }

    /// <summary>
    /// Verifies that a completed upload returns the signed URL when UploadService provides one.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Fact]
    public async Task UploadFileAsync_ReturnsSignedUrl_OnSuccess()
    {
        // Arrange
        var uploadId = Guid.NewGuid().ToString();
        var expectedPath = "folder/file.pdf";
        var expectedSignedUrl = "https://storage.example.com/folder/file.pdf?signature=test";
        _handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync((HttpRequestMessage request, CancellationToken _) =>
            {
                if (request.Method == HttpMethod.Post && request.RequestUri?.PathAndQuery == "/upload/v1/uploads/resumable")
                {
                    return new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                        Content = JsonContent.Create(new
                        {
                            uploadId = uploadId,
                            sessionUri = "http://upload-session/upload",
                            expiresAt = DateTime.UtcNow.AddMinutes(15),
                            totalSize = 1
                        })
                    };
                }

                if (request.Method == HttpMethod.Put && request.RequestUri?.AbsoluteUri == "http://upload-session/upload")
                {
                    return new HttpResponseMessage(HttpStatusCode.OK);
                }

                if (request.Method == HttpMethod.Post && request.RequestUri?.PathAndQuery == $"/upload/v1/uploads/resumable/{uploadId}/complete")
                {
                    return new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                        Content = JsonContent.Create(new { storagePath = expectedPath, signedUrl = expectedSignedUrl })
                    };
                }

                return new HttpResponseMessage(HttpStatusCode.NotFound);
            });

        // Act
        var result = await _client.UploadFileAsync("file.pdf", new byte[] { 1 }, "application/pdf", "folder/file.pdf");

        // Assert
        Assert.Equal(expectedSignedUrl, result);
    }

    /// <summary>
    /// Verifies that a completed upload falls back to the storage path when no signed URL is returned.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Fact]
    public async Task UploadFileAsync_ReturnsStoragePath_WhenSignedUrlIsMissing()
    {
        // Arrange
        var uploadId = Guid.NewGuid().ToString();
        var expectedPath = "folder/file.pdf";
        _handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync((HttpRequestMessage request, CancellationToken _) =>
            {
                if (request.Method == HttpMethod.Post && request.RequestUri?.PathAndQuery == "/upload/v1/uploads/resumable")
                {
                    return new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                        Content = JsonContent.Create(new
                        {
                            uploadId = uploadId,
                            sessionUri = "http://upload-session/upload",
                            expiresAt = DateTime.UtcNow.AddMinutes(15),
                            totalSize = 1
                        })
                    };
                }

                if (request.Method == HttpMethod.Put && request.RequestUri?.AbsoluteUri == "http://upload-session/upload")
                {
                    return new HttpResponseMessage(HttpStatusCode.OK);
                }

                if (request.Method == HttpMethod.Post && request.RequestUri?.PathAndQuery == $"/upload/v1/uploads/resumable/{uploadId}/complete")
                {
                    return new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                        Content = JsonContent.Create(new { storagePath = expectedPath })
                    };
                }

                return new HttpResponseMessage(HttpStatusCode.NotFound);
            });

        // Act
        var result = await _client.UploadFileAsync("file.pdf", new byte[] { 1 }, "application/pdf", "folder/file.pdf");

        // Assert
        Assert.Equal(expectedPath, result);
    }

    /// <summary>
    /// Verifies that upload failures are surfaced as invalid operation exceptions.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Fact]
    public async Task UploadFileAsync_Throws_OnFailure()
    {
        // Arrange
        _handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.BadRequest,
                Content = new StringContent("Error")
            });

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _client.UploadFileAsync("file.pdf", new byte[] { 1 }, "application/pdf", "folder/file.pdf"));
    }
}
