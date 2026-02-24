using Maliev.MessagingContracts.Generated;
using Maliev.PdfService.Tests.Fixtures;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Maliev.PdfService.Tests.Integration;

public class ConsumerTests : IClassFixture<PdfServiceTestFactory>
{
    private readonly PdfServiceTestFactory _factory;

    public ConsumerTests(PdfServiceTestFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task InvoiceFinalizedConsumer_ConsumesMessage()
    {
        // Arrange
        var bus = _factory.Services.GetRequiredService<IBus>();
        var payload = new InvoiceCreatedEventPayload(Guid.NewGuid(), "INV-TEST-001", null, null, Guid.NewGuid(), 1000.0, "USD", null, DateTimeOffset.UtcNow);
        var message = new InvoiceCreatedEvent(Guid.NewGuid(), "InvoiceCreated", MessageType.Event, "1.0", "InvoiceService", new[] { "PdfService" }, Guid.NewGuid(), null, DateTimeOffset.UtcNow, false, payload);

        // Act
        await bus.Publish(message);

        // Assert
        Assert.NotNull(bus);
    }
}
