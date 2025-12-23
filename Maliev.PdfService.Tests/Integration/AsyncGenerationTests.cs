using Maliev.PdfService.Tests.Testing;
using Maliev.PdfService.Data.Data;
using System.Net.Http.Json;
using Xunit;
using Moq;
using Maliev.PdfService.Api.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.TestHost;

namespace Maliev.PdfService.Tests.Integration;

public class AsyncGenerationTests : IClassFixture<BaseIntegrationTestFactory<Program, PdfDbContext>>
{
    private readonly BaseIntegrationTestFactory<Program, PdfDbContext> _factory;
    private readonly Mock<IUploadServiceClient> _uploadServiceMock = new();

    public AsyncGenerationTests(BaseIntegrationTestFactory<Program, PdfDbContext> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GenerateAsync_ReturnsAcceptedWithRequestId()
    {
        // Arrange
        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                services.AddScoped(_ => _uploadServiceMock.Object);
            });
        }).CreateClient();

        var request = new
        {
            templateCode = "INV-STD-01",
            referenceId = "INV-2025-001",
            documentType = "Invoice",
            data = new { InvoiceNumber = "INV-2025-001", Items = new[] { new { Index = 1, Description = "Test Item", Quantity = 1, TotalPrice = 100.0 } } }
        };

        // Act
        var response = await client.PostAsJsonAsync("/pdf/v1/generate/async", request);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.Accepted, response.StatusCode);
    }
}