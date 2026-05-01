using System.Net;
using System.Net.Http.Json;
using Maliev.PdfService.Api.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using Xunit;

namespace Maliev.PdfService.Tests.Unit;

public class UploadServiceClientTests
{
    private readonly Mock<HttpMessageHandler> _handlerMock = new();
    private readonly Mock<ILogger<UploadServiceClient>> _loggerMock = new();
    private readonly HttpClient _httpClient;
    private readonly UploadServiceClient _client;

    public UploadServiceClientTests()
    {
        _httpClient = new HttpClient(_handlerMock.Object)
        {
            BaseAddress = new Uri("http://upload-service/")
        };
        _client = new UploadServiceClient(_httpClient, _loggerMock.Object);
    }

    [Fact]
    public async Task UploadFileAsync_ReturnsStoragePath_OnSuccess()
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
