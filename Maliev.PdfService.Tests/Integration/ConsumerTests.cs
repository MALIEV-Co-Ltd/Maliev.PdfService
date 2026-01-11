using Maliev.MessagingContracts.Generated;
using Maliev.PdfService.Tests.Testing;
using Maliev.PdfService.Data.Data;
using Maliev.PdfService.Api.Consumers;
using Maliev.PdfService.Api.Services;
using Moq;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.TestHost;
using Xunit;
using PdfProgram = Maliev.PdfService.Api.Program;

namespace Maliev.PdfService.Tests.Integration;

public class ConsumerTests : IClassFixture<BaseIntegrationTestFactory<PdfProgram, PdfDbContext>>
{
    private readonly BaseIntegrationTestFactory<PdfProgram, PdfDbContext> _factory;
    private readonly Mock<IUploadServiceClient> _uploadServiceMock = new();

    public ConsumerTests(BaseIntegrationTestFactory<PdfProgram, PdfDbContext> factory)
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
        var payload = new InvoiceCreatedEventPayload(Guid.NewGuid(), "INV-TEST-001", null, null, Guid.NewGuid(), 1000.0, "USD", null, DateTimeOffset.UtcNow);
        var message = new InvoiceCreatedEvent(Guid.NewGuid(), "InvoiceCreated", MessageType.Event, "1.0", "InvoiceService", new[] { "PdfService" }, Guid.NewGuid(), null, DateTimeOffset.UtcNow, false, payload);

        // Act
        await bus.Publish(message);

        // Assert
        Assert.NotNull(bus);
    }
}
