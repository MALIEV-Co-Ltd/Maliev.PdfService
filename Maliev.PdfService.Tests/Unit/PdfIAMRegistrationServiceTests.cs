using Maliev.PdfService.Api.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace Maliev.PdfService.Tests.Unit;

public class PdfIAMRegistrationServiceTests
{
    private readonly Mock<IHttpClientFactory> _httpClientFactoryMock = new();
    private readonly Mock<ILogger<PdfIAMRegistrationService>> _loggerMock = new();
    private readonly Mock<HttpMessageHandler> _handlerMock = new();

    public PdfIAMRegistrationServiceTests()
    {
        var client = new HttpClient(_handlerMock.Object)
        {
            BaseAddress = new Uri("http://iam-service/")
        };
        _httpClientFactoryMock.Setup(x => x.CreateClient("IAMService")).Returns(client);
    }

    [Fact]
    public async Task RegisterAsync_CallsIAMRegister()
    {
        // Arrange
        _handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{}")
            });

        var service = new PdfIAMRegistrationService(_httpClientFactoryMock.Object, _loggerMock.Object);

        // Act
        await service.RegisterAsync(CancellationToken.None);

        // Assert
        _handlerMock.Protected().Verify(
            "SendAsync",
            Times.AtLeastOnce(),
            ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Post),
            ItExpr.IsAny<CancellationToken>());
    }

    [Fact]
    public async Task RegisterAsync_Throws_OnFailure()
    {
        // Arrange
        _handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new Exception("Network error"));

        var service = new PdfIAMRegistrationService(_httpClientFactoryMock.Object, _loggerMock.Object);

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => service.RegisterAsync(CancellationToken.None));
    }
}
