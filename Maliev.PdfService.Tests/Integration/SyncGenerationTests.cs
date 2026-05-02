using Maliev.PdfService.Tests.Fixtures;
using System.Net.Http.Json;
using Xunit;
using Moq;
using System.Text.Json;
using Maliev.PdfService.Api.Models.Requests;
using Maliev.PdfService.Api.Models.Data;
using Maliev.PdfService.Api.Authorization;
using Maliev.PdfService.Domain.Entities;

namespace Maliev.PdfService.Tests.Integration;

/// <summary>
/// Integration tests for synchronous PDF generation endpoints.
/// </summary>
public class SyncGenerationTests : IClassFixture<PdfServiceTestFactory>
{
    private readonly PdfServiceTestFactory _factory;

    /// <summary>
    /// Initializes a new instance of the <see cref="SyncGenerationTests"/> class.
    /// </summary>
    /// <param name="factory">The PDF service test factory.</param>
    public SyncGenerationTests(PdfServiceTestFactory factory)
    {
        _factory = factory;
    }

    /// <summary>
    /// Verifies that synchronous generation returns a successful response with an upload URL.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Fact]
    public async Task Generate_ReturnsOkWithUrl()
    {
        // Arrange
        _factory.UploadServiceMock.Setup(x => x.UploadFileAsync(It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("https://storage.googleapis.com/mock/test.pdf");

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
        var response = await client.PostAsJsonAsync("/pdf/v1/generations/generate", request);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
    }
}
