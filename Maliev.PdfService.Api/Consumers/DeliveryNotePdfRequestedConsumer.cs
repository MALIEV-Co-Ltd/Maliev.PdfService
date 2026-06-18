using Maliev.MessagingContracts.Contracts.Delivery;
using Maliev.MessagingContracts.Contracts.Pdf;
using Maliev.MessagingContracts.Contracts.Shared;
using Maliev.PdfService.Api.Models.Data;
using Maliev.PdfService.Api.Services;
using Maliev.PdfService.Infrastructure.Data;
using Maliev.PdfService.Domain.Entities;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace Maliev.PdfService.Api.Consumers;

/// <summary>
/// Consumes DeliveryNotePdfRequestedEvent to generate PDF for delivery notes.
/// </summary>
public class DeliveryNotePdfRequestedConsumer : IConsumer<DeliveryNotePdfRequestedEvent>
{
    private readonly IPdfGenerator _pdfGenerator;
    private readonly IUploadServiceClient _uploadService;
    private readonly PdfDbContext _dbContext;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<DeliveryNotePdfRequestedConsumer> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="DeliveryNotePdfRequestedConsumer"/> class.
    /// </summary>
    public DeliveryNotePdfRequestedConsumer(
        IPdfGenerator pdfGenerator,
        IUploadServiceClient uploadService,
        PdfDbContext dbContext,
        IPublishEndpoint publishEndpoint,
        IHttpClientFactory httpClientFactory,
        ILogger<DeliveryNotePdfRequestedConsumer> logger)
    {
        _pdfGenerator = pdfGenerator;
        _uploadService = uploadService;
        _dbContext = dbContext;
        _publishEndpoint = publishEndpoint;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task Consume(ConsumeContext<DeliveryNotePdfRequestedEvent> context)
    {
        var message = context.Message;
        var payload = message.Payload;
        if (payload is null)
        {
            _logger.LogWarning("Ignoring DeliveryNotePdfRequestedEvent without payload");
            return;
        }

        if (!IsRoutedToPdfService(message))
        {
            _logger.LogDebug(
                "Skipping delivery note PDF request {DeliveryNoteId} because it is routed to {ConsumedBy}",
                payload.DeliveryNoteId,
                message.ConsumedBy is null ? "(none)" : string.Join(",", message.ConsumedBy));
            return;
        }

        _logger.LogInformation("Processing PDF generation for delivery note: {DeliveryNoteId}", payload.DeliveryNoteId);

        var log = new GenerationRequest
        {
            ReferenceId = payload.DeliveryNoteId,
            TemplateCode = "DN-STD-01", // Standard delivery note template
            DocumentType = DocumentType.DeliveryNote,
            Status = GenerationStatus.Processing,
            DataJson = System.Text.Json.JsonSerializer.Serialize(message),
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.GenerationRequests.Add(log);
        await _dbContext.SaveChangesAsync();

        try
        {
            // Fetch delivery note data from DeliveryService
            var deliveryNoteData = await FetchDeliveryNoteDataAsync(payload.DeliveryNoteId, context.CancellationToken);

            // Generate PDF
            var pdfBytes = await _pdfGenerator.GeneratePdfAsync(DocumentType.DeliveryNote, deliveryNoteData, log.TemplateCode);

            // Upload to storage
            var fileName = $"DeliveryNote_{payload.DeliveryNoteId}_{Guid.NewGuid()}.pdf";
            var storagePath = _pdfGenerator.GetStoragePath(DocumentType.DeliveryNote, payload.DeliveryNoteId, fileName);
            var url = await _uploadService.UploadFileAsync(fileName, pdfBytes, "application/pdf", storagePath, context.CancellationToken);

            // Update generation log
            log.Status = GenerationStatus.Completed;
            log.StorageUrl = url;
            log.StoragePath = storagePath;
            log.CompletedAt = DateTime.UtcNow;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate PDF for delivery note: {DeliveryNoteId}", payload.DeliveryNoteId);

            log.Status = GenerationStatus.Failed;
            log.ErrorMessage = ex.Message;
            log.CompletedAt = DateTime.UtcNow;

            await _publishEndpoint.Publish(new PdfGenerationFailedEvent(
                MessageId: Guid.NewGuid(),
                MessageName: nameof(PdfGenerationFailedEvent),
                MessageType: MessageType.Event,
                MessageVersion: "1.0.0",
                PublishedBy: "PdfService",
                ConsumedBy: ["DeliveryService"],
                CorrelationId: context.CorrelationId ?? Guid.NewGuid(),
                CausationId: context.MessageId,
                OccurredAtUtc: DateTimeOffset.UtcNow,
                IsPublic: false,
                Payload: new PdfGenerationFailedEventPayload(
                    RequestId: log.Id.ToString(),
                    ReferenceId: payload.DeliveryNoteId,
                    DocumentType: DocumentType.DeliveryNote.ToString(),
                    ErrorMessage: ex.Message,
                    FailedAt: DateTimeOffset.UtcNow
                )
            ), context.CancellationToken);

            await _dbContext.SaveChangesAsync(context.CancellationToken);
            return;
        }

        await _publishEndpoint.Publish(new PdfGenerationCompletedEvent(
            MessageId: Guid.NewGuid(),
            MessageName: nameof(PdfGenerationCompletedEvent),
            MessageType: MessageType.Event,
            MessageVersion: "1.0.0",
            PublishedBy: "PdfService",
            ConsumedBy: ["DeliveryService"],
            CorrelationId: context.CorrelationId ?? Guid.NewGuid(),
            CausationId: context.MessageId,
            OccurredAtUtc: DateTimeOffset.UtcNow,
            IsPublic: false,
            Payload: new PdfGenerationCompletedEventPayload(
                RequestId: log.Id.ToString(),
                ReferenceId: payload.DeliveryNoteId,
                DocumentType: DocumentType.DeliveryNote.ToString(),
                StorageUrl: log.StorageUrl!,
                CompletedAt: DateTimeOffset.UtcNow
            )
        ), context.CancellationToken);

        await _dbContext.SaveChangesAsync(context.CancellationToken);

        _logger.LogInformation("Successfully generated and uploaded PDF for delivery note: {DeliveryNoteId}, URL: {Url}", payload.DeliveryNoteId, log.StorageUrl);
    }

    /// <summary>
    /// Fetches delivery note data from DeliveryService via HTTP client.
    /// </summary>
    private async Task<DeliveryNoteData> FetchDeliveryNoteDataAsync(string deliveryNoteId, CancellationToken cancellationToken)
    {
        var httpClient = _httpClientFactory.CreateClient("DeliveryService");

        var response = await httpClient.GetAsync($"/delivery/v1/delivery-notes/{deliveryNoteId}", cancellationToken);
        response.EnsureSuccessStatusCode();

        var deliveryNote = await response.Content.ReadFromJsonAsync<DeliveryNoteResponse>(cancellationToken: cancellationToken);
        if (deliveryNote == null)
        {
            throw new InvalidOperationException($"Failed to fetch delivery note {deliveryNoteId} from DeliveryService");
        }

        // Map to PDF data model
        return new DeliveryNoteData
        {
            DeliveryNoteNumber = deliveryNote.DeliveryNoteId,
            OrderNumber = deliveryNote.OrderId,
            CustomerName = deliveryNote.CustomerName ?? "N/A",
            CustomerAddress = FormatAddress(deliveryNote),
            DeliveryDate = deliveryNote.DeliveryDate,
            TrackingNumber = deliveryNote.TrackingNumber,
            CarrierName = deliveryNote.CarrierName,
            DeliveryContact = deliveryNote.DeliveryContactName,
            DeliveryPhone = deliveryNote.DeliveryContactPhone,
            Items = deliveryNote.Items.Select(item => new DeliveryNoteItemData
            {
                ProductCode = item.ProductCode ?? "N/A",
                ProductName = item.ProductName ?? "N/A",
                QuantityOrdered = item.QuantityOrdered,
                QuantityManufactured = item.QuantityManufactured,
                QuantityDelivered = item.QuantityDelivered,
                UnitOfMeasure = item.UnitOfMeasure
            }).ToList(),
            Notes = deliveryNote.DeliveryInstructions
        };
    }

    /// <summary>
    /// Formats the shipping address from delivery note fields.
    /// </summary>
    private static string FormatAddress(DeliveryNoteResponse note)
    {
        var parts = new List<string>();

        if (!string.IsNullOrEmpty(note.ShippingAddressLine1))
            parts.Add(note.ShippingAddressLine1);

        if (!string.IsNullOrEmpty(note.ShippingAddressLine2))
            parts.Add(note.ShippingAddressLine2);

        if (!string.IsNullOrEmpty(note.ShippingCity))
            parts.Add(note.ShippingCity);

        if (!string.IsNullOrEmpty(note.ShippingProvince))
            parts.Add(note.ShippingProvince);

        if (!string.IsNullOrEmpty(note.ShippingPostalCode))
            parts.Add(note.ShippingPostalCode);

        if (!string.IsNullOrEmpty(note.ShippingCountry))
            parts.Add(note.ShippingCountry);

        return parts.Count > 0 ? string.Join(", ", parts) : "N/A";
    }

    private static bool IsRoutedToPdfService(DeliveryNotePdfRequestedEvent message)
    {
        return message.ConsumedBy?.Any(consumer =>
            string.Equals(consumer, "PdfService", StringComparison.OrdinalIgnoreCase)) == true;
    }
}

/// <summary>
/// DTO for delivery note response from DeliveryService.
/// This is a minimal mapping of the actual DeliveryNoteResponse from DeliveryService.
/// </summary>
internal class DeliveryNoteResponse
{
    public string DeliveryNoteId { get; set; } = string.Empty;
    public string? OrderId { get; set; }
    public string? CustomerName { get; set; }
    public DateTime DeliveryDate { get; set; }
    public string? TrackingNumber { get; set; }
    public string? CarrierName { get; set; }
    public string? DeliveryContactName { get; set; }
    public string? DeliveryContactPhone { get; set; }
    public string? DeliveryInstructions { get; set; }
    public string? ShippingAddressLine1 { get; set; }
    public string? ShippingAddressLine2 { get; set; }
    public string? ShippingCity { get; set; }
    public string? ShippingProvince { get; set; }
    public string? ShippingPostalCode { get; set; }
    public string? ShippingCountry { get; set; }
    public List<DeliveryNoteItemResponse> Items { get; set; } = new();
}

/// <summary>
/// DTO for delivery note item from DeliveryService.
/// </summary>
internal class DeliveryNoteItemResponse
{
    public string? ProductCode { get; set; }
    public string? ProductName { get; set; }
    public decimal QuantityOrdered { get; set; }
    public decimal QuantityManufactured { get; set; }
    public decimal QuantityDelivered { get; set; }
    public string UnitOfMeasure { get; set; } = string.Empty;
}
