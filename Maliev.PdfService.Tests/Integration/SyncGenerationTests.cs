using Maliev.PdfService.Tests.Testing;
using Maliev.PdfService.Data.Data;
using System.Net.Http.Json;
using Xunit;
using Moq;
using System.Text.Json;
using Maliev.PdfService.Api.Models.Requests;
using Maliev.PdfService.Api.Models.Data;
using Maliev.PdfService.Api.Services;
using Maliev.PdfService.Api.Authorization;
using Maliev.PdfService.Data.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.TestHost;
using PdfProgram = Maliev.PdfService.Api.Program;

namespace Maliev.PdfService.Tests.Integration;

public class SyncGenerationTests : IClassFixture<BaseIntegrationTestFactory<PdfProgram, PdfDbContext>>
{
    private readonly BaseIntegrationTestFactory<PdfProgram, PdfDbContext> _factory;
    private readonly Mock<IUploadServiceClient> _uploadServiceMock = new();

    public SyncGenerationTests(BaseIntegrationTestFactory<PdfProgram, PdfDbContext> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Generate_ReturnsOkWithUrl()
    {
        // Arrange
        _uploadServiceMock.Setup(x => x.UploadFileAsync(It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("https://storage.googleapis.com/mock/test.pdf");

        var factory = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                services.AddScoped(_ => _uploadServiceMock.Object);
            });
        });

        var client = _factory.CreateAuthenticatedClient(factory, permissions: [PdfPermissions.GenerationCreate]);

        var request = new GeneratePdfRequest
        {
            TemplateCode = "INV-STD-01",
            ReferenceId = "INV-2025-001",
            DocumentType = DocumentType.Invoice,
            Data = JsonSerializer.Deserialize<JsonElement>(JsonSerializer.Serialize(new InvoiceData
            {
                InvoiceNumber = "INV-2025-001",
                Items = new List<InvoiceItemData> { new InvoiceItemData { Index = 1, Description = "Test Item", Quantity = 1, TotalPrice = 100.0 } }
            }))
        };

        // Act
        var response = await client.PostAsJsonAsync("/pdf/v1/generations/generate", request);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
    }
}
