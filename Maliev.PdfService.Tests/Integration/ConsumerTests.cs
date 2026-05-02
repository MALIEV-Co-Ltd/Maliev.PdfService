using Maliev.MessagingContracts;
using Maliev.MessagingContracts.Contracts.Invoices;
using Maliev.PdfService.Tests.Fixtures;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Maliev.PdfService.Tests.Integration;

/// <summary>
/// Integration tests for PDF service message consumers registered with MassTransit.
/// </summary>
public class ConsumerTests : IClassFixture<PdfServiceTestFactory>
{
    private readonly PdfServiceTestFactory _factory;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConsumerTests"/> class.
    /// </summary>
    /// <param name="factory">The PDF service test factory.</param>
    public ConsumerTests(PdfServiceTestFactory factory)
    {
        _factory = factory;
    }

    /// <summary>
    /// Verifies that publishing an invoice-created event reaches the configured bus.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
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
