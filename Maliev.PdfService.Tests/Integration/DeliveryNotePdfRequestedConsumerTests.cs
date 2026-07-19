using System.Net;
using System.Text.Json;
using Maliev.MessagingContracts.Contracts.Delivery;
using Maliev.MessagingContracts.Contracts.Pdf;
using Maliev.PdfService.Api.Consumers;
using Maliev.PdfService.Api.Services;
using Maliev.PdfService.Domain.Entities;
using Maliev.PdfService.Infrastructure.Data;
using Maliev.PdfService.Tests.Fixtures;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Maliev.PdfService.Tests.Integration;

/// <summary>
/// Integration tests for delivery note PDF request processing.
/// </summary>
public class DeliveryNotePdfRequestedConsumerTests : IClassFixture<PdfServiceTestFactory>
{
    private readonly PdfServiceTestFactory _factory;

    /// <summary>
    /// Initializes a new instance of the <see cref="DeliveryNotePdfRequestedConsumerTests"/> class.
    /// </summary>
    /// <param name="factory">The PDF service test factory.</param>
    public DeliveryNotePdfRequestedConsumerTests(PdfServiceTestFactory factory)
    {
        _factory = factory;
    }

    /// <summary>
    /// Verifies that delivery note PDF generation publishes a completed event and persists the generation log.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Fact]
    public async Task Consume_DeliveryNotePdfRequestedEvent_PublishesCompletedEvent()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<PdfDbContext>();
        var pdfGeneratorMock = new Mock<IPdfGenerator>();
        var uploadServiceMock = new Mock<IUploadServiceClient>();
        var publishEndpointMock = new Mock<IPublishEndpoint>();
        var httpClientFactory = CreateDeliveryHttpClientFactory();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<DeliveryNotePdfRequestedConsumer>>();

        pdfGeneratorMock
            .Setup(g => g.GeneratePdfAsync(DocumentType.DeliveryNote, It.IsAny<object>(), It.IsAny<string?>()))
            .ReturnsAsync([1, 2, 3]);
        pdfGeneratorMock
            .Setup(g => g.GetStoragePath(DocumentType.DeliveryNote, "DN-PDF-001", It.IsAny<string>()))
            .Returns("pdfs/deliverynote/DN-PDF-001/delivery-note.pdf");
        uploadServiceMock
            .Setup(s => s.UploadFileAsync(It.IsAny<string>(), It.IsAny<byte[]>(), "application/pdf", It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("https://storage.example/delivery-note.pdf");

        var consumer = new DeliveryNotePdfRequestedConsumer(
            pdfGeneratorMock.Object,
            uploadServiceMock.Object,
            context,
            publishEndpointMock.Object,
            httpClientFactory.Object,
            logger);
        var consumeContext = CreateConsumeContext("DN-PDF-001");

        await consumer.Consume(consumeContext.Object);

        var log = await context.GenerationRequests
            .AsNoTracking()
            .SingleAsync(r => r.ReferenceId == "DN-PDF-001");

        Assert.Equal(GenerationStatus.Completed, log.Status);
        Assert.Equal("https://storage.example/delivery-note.pdf", log.StorageUrl);
        publishEndpointMock.Verify(
            p => p.Publish(
                It.Is<PdfGenerationCompletedEvent>(e =>
                    e.Payload.ReferenceId == "DN-PDF-001" &&
                    e.Payload.DocumentType == DocumentType.DeliveryNote.ToString() &&
                    e.Payload.StorageUrl == "https://storage.example/delivery-note.pdf" &&
                    e.ConsumedBy.Contains("DeliveryService", StringComparer.OrdinalIgnoreCase)),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    /// <summary>
    /// Verifies delivery note PDF generation failures publish a failed event for DeliveryService.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Fact]
    public async Task Consume_DeliveryNotePdfRequestedEvent_GenerationFails_PublishesFailedEvent()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<PdfDbContext>();
        var pdfGeneratorMock = new Mock<IPdfGenerator>();
        var uploadServiceMock = new Mock<IUploadServiceClient>();
        var publishEndpointMock = new Mock<IPublishEndpoint>();
        var httpClientFactory = CreateDeliveryHttpClientFactory();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<DeliveryNotePdfRequestedConsumer>>();

        pdfGeneratorMock
            .Setup(g => g.GeneratePdfAsync(DocumentType.DeliveryNote, It.IsAny<object>(), It.IsAny<string?>()))
            .ThrowsAsync(new InvalidOperationException("delivery note render failed"));

        var consumer = new DeliveryNotePdfRequestedConsumer(
            pdfGeneratorMock.Object,
            uploadServiceMock.Object,
            context,
            publishEndpointMock.Object,
            httpClientFactory.Object,
            logger);
        var consumeContext = CreateConsumeContext("DN-PDF-GENERATE-FAIL");

        await consumer.Consume(consumeContext.Object);

        var log = await context.GenerationRequests
            .AsNoTracking()
            .SingleAsync(r => r.ReferenceId == "DN-PDF-GENERATE-FAIL");

        Assert.Equal(GenerationStatus.Failed, log.Status);
        Assert.Contains("delivery note render failed", log.ErrorMessage);
        publishEndpointMock.Verify(
            p => p.Publish(
                It.Is<PdfGenerationFailedEvent>(e =>
                    e.Payload.ReferenceId == "DN-PDF-GENERATE-FAIL" &&
                    e.Payload.DocumentType == DocumentType.DeliveryNote.ToString() &&
                    e.Payload.ErrorMessage == "delivery note render failed" &&
                    e.ConsumedBy.Contains("DeliveryService", StringComparer.OrdinalIgnoreCase)),
                It.IsAny<CancellationToken>()),
            Times.Once);
        publishEndpointMock.Verify(
            p => p.Publish(It.IsAny<PdfGenerationCompletedEvent>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    /// <summary>
    /// Verifies completed-event publish failures remain retryable and do not persist a completed terminal state.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Fact]
    public async Task Consume_DeliveryNotePdfRequestedEvent_CompletedEventPublishFails_KeepsRequestProcessing()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<PdfDbContext>();
        var pdfGeneratorMock = new Mock<IPdfGenerator>();
        var uploadServiceMock = new Mock<IUploadServiceClient>();
        var publishEndpointMock = new Mock<IPublishEndpoint>();
        var httpClientFactory = CreateDeliveryHttpClientFactory();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<DeliveryNotePdfRequestedConsumer>>();

        pdfGeneratorMock
            .Setup(g => g.GeneratePdfAsync(DocumentType.DeliveryNote, It.IsAny<object>(), It.IsAny<string?>()))
            .ReturnsAsync([1, 2, 3]);
        pdfGeneratorMock
            .Setup(g => g.GetStoragePath(DocumentType.DeliveryNote, "DN-PDF-PUBLISH-FAIL", It.IsAny<string>()))
            .Returns("pdfs/deliverynote/DN-PDF-PUBLISH-FAIL/delivery-note.pdf");
        uploadServiceMock
            .Setup(s => s.UploadFileAsync(It.IsAny<string>(), It.IsAny<byte[]>(), "application/pdf", It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("https://storage.example/delivery-note.pdf");
        publishEndpointMock
            .Setup(p => p.Publish(It.IsAny<PdfGenerationCompletedEvent>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("completion publish failed"));

        var consumer = new DeliveryNotePdfRequestedConsumer(
            pdfGeneratorMock.Object,
            uploadServiceMock.Object,
            context,
            publishEndpointMock.Object,
            httpClientFactory.Object,
            logger);
        var consumeContext = CreateConsumeContext("DN-PDF-PUBLISH-FAIL");

        await Assert.ThrowsAsync<InvalidOperationException>(() => consumer.Consume(consumeContext.Object));

        var log = await context.GenerationRequests
            .AsNoTracking()
            .SingleAsync(r => r.ReferenceId == "DN-PDF-PUBLISH-FAIL");

        Assert.Equal(GenerationStatus.Processing, log.Status);
        Assert.Null(log.ErrorMessage);
        publishEndpointMock.Verify(
            p => p.Publish(It.IsAny<PdfGenerationFailedEvent>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    /// <summary>
    /// Verifies delivery-note PDF requests routed to another service are ignored.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Fact]
    public async Task Consume_DeliveryNotePdfRequestedEvent_NotRoutedToPdfService_SkipsProcessing()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<PdfDbContext>();
        var pdfGeneratorMock = new Mock<IPdfGenerator>();
        var uploadServiceMock = new Mock<IUploadServiceClient>();
        var publishEndpointMock = new Mock<IPublishEndpoint>();
        var httpClientFactory = CreateDeliveryHttpClientFactory();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<DeliveryNotePdfRequestedConsumer>>();

        var consumer = new DeliveryNotePdfRequestedConsumer(
            pdfGeneratorMock.Object,
            uploadServiceMock.Object,
            context,
            publishEndpointMock.Object,
            httpClientFactory.Object,
            logger);
        var consumeContext = CreateConsumeContext("DN-PDF-UNROUTED", ["NotificationService"]);

        await consumer.Consume(consumeContext.Object);

        Assert.False(await context.GenerationRequests.AnyAsync(r => r.ReferenceId == "DN-PDF-UNROUTED"));
        pdfGeneratorMock.Verify(
            g => g.GeneratePdfAsync(It.IsAny<DocumentType>(), It.IsAny<object>(), It.IsAny<string?>()),
            Times.Never);
        uploadServiceMock.Verify(
            s => s.UploadFileAsync(
                It.IsAny<string>(),
                It.IsAny<byte[]>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
        publishEndpointMock.Verify(
            p => p.Publish(It.IsAny<PdfGenerationCompletedEvent>(), It.IsAny<CancellationToken>()),
            Times.Never);
        publishEndpointMock.Verify(
            p => p.Publish(It.IsAny<PdfGenerationFailedEvent>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    private static Mock<ConsumeContext<DeliveryNotePdfRequestedEvent>> CreateConsumeContext(
        string deliveryNoteId,
        string[]? consumedBy = null)
    {
        var message = new DeliveryNotePdfRequestedEvent(
            MessageId: Guid.NewGuid(),
            MessageName: nameof(DeliveryNotePdfRequestedEvent),
            MessageType: Maliev.MessagingContracts.Contracts.Shared.MessageType.Event,
            MessageVersion: "1.0.0",
            PublishedBy: "DeliveryService",
            ConsumedBy: consumedBy ?? ["PdfService"],
            CorrelationId: Guid.NewGuid(),
            CausationId: null,
            OccurredAtUtc: DateTimeOffset.UtcNow,
            IsPublic: false,
            Payload: new DeliveryNotePdfRequestedEventPayload(
                DeliveryNoteId: deliveryNoteId,
                RequestedBy: "test-user",
                RequestedAt: DateTimeOffset.UtcNow));

        var context = new Mock<ConsumeContext<DeliveryNotePdfRequestedEvent>>();
        context.Setup(c => c.Message).Returns(message);
        context.Setup(c => c.MessageId).Returns(message.MessageId);
        context.Setup(c => c.CorrelationId).Returns(message.CorrelationId);
        context.Setup(c => c.CancellationToken).Returns(CancellationToken.None);
        return context;
    }

    private static Mock<IHttpClientFactory> CreateDeliveryHttpClientFactory()
    {
        var body = JsonSerializer.Serialize(new
        {
            deliveryNoteId = "DN-PDF-001",
            orderId = "ORD-001",
            customerName = "Acme Manufacturing",
            deliveryDate = DateTime.UtcNow,
            trackingNumber = "TRK-001",
            carrierName = "Carrier",
            deliveryContactName = "Receiver",
            deliveryContactPhone = "+66810000000",
            shippingAddressLine1 = "88 Rama IX Road",
            shippingCity = "Bangkok",
            shippingProvince = "Bangkok",
            shippingPostalCode = "10310",
            shippingCountry = "TH",
            items = new[]
            {
                new
                {
                    productCode = "P1",
                    productName = "Product 1",
                    quantityOrdered = 1m,
                    quantityManufactured = 1m,
                    quantityDelivered = 1m,
                    unitOfMeasure = "pcs"
                }
            }
        });
        var client = new HttpClient(new StaticJsonHandler(body))
        {
            BaseAddress = new Uri("http://delivery-service")
        };
        var factory = new Mock<IHttpClientFactory>();
        factory.Setup(f => f.CreateClient("DeliveryService")).Returns(client);
        return factory;
    }

    private sealed class StaticJsonHandler : HttpMessageHandler
    {
        private readonly string _body;

        public StaticJsonHandler(string body)
        {
            _body = body;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(_body, System.Text.Encoding.UTF8, "application/json")
            });
        }
    }
}
