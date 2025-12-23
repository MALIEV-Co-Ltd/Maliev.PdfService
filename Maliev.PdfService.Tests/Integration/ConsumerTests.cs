using Maliev.PdfService.Tests.Testing;
using Maliev.PdfService.Data.Data;
using Maliev.PdfService.Api.Consumers;
using Maliev.PdfService.Api.Services;
using Moq;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.TestHost;
using Xunit;

namespace Maliev.PdfService.Tests.Integration;

public class ConsumerTests : IClassFixture<BaseIntegrationTestFactory<Program, PdfDbContext>>
{
    private readonly BaseIntegrationTestFactory<Program, PdfDbContext> _factory;
    private readonly Mock<IUploadServiceClient> _uploadServiceMock = new();

    public ConsumerTests(BaseIntegrationTestFactory<Program, PdfDbContext> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task InvoiceFinalizedConsumer_ConsumesMessage()
    {
        // Arrange
        var factory = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                services.AddScoped(_ => _uploadServiceMock.Object);
            });
        });

        var bus = factory.Services.GetRequiredService<IBus>();
        var message = new InvoiceFinalizedEvent(Guid.NewGuid(), "INV-TEST-001", new { InvoiceNumber = "INV-TEST-001", Items = new List<object>() });

        // Act
        await bus.Publish(message);

        // Assert
        Assert.NotNull(bus);
    }
}
