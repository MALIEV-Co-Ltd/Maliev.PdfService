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
        var expectedPath = "folder/file.pdf";
        _handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = JsonContent.Create(new { StoragePath = expectedPath })
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
