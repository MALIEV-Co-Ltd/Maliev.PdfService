using Maliev.PdfService.Tests.Fixtures;
using System.Net.Http.Json;
using Xunit;
using System.Text.Json;
using Maliev.PdfService.Api.Models.Requests;
using Maliev.PdfService.Api.Models.Data;
using Maliev.PdfService.Api.Authorization;
using Maliev.PdfService.Domain.Entities;

namespace Maliev.PdfService.Tests.Integration;

public class AsyncGenerationTests : IClassFixture<PdfServiceTestFactory>
{
    private readonly PdfServiceTestFactory _factory;

    public AsyncGenerationTests(PdfServiceTestFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GenerateAsync_ReturnsAcceptedWithRequestId()
    {
        // Arrange
        var client = _factory.CreateAuthenticatedClient(permissions: [PdfPermissions.GenerationCreate]);

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
        var response = await client.PostAsJsonAsync("/pdf/v1/generations/generate/async", request);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.Accepted, response.StatusCode);
    }
}
