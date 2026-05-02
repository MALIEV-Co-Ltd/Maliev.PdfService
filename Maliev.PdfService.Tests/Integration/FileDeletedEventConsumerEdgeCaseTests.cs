using Maliev.MessagingContracts;
using Maliev.MessagingContracts.Contracts.Uploads;
using Maliev.PdfService.Api.Consumers;
using Maliev.PdfService.Domain.Entities;
using Maliev.PdfService.Infrastructure.Data;
using Maliev.PdfService.Tests.Fixtures;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Maliev.PdfService.Tests.Integration;

/// <summary>
/// Integration tests for file deletion consumer edge cases.
/// </summary>
public class FileDeletedEventConsumerEdgeCaseTests : IClassFixture<PdfServiceTestFactory>
{
    private readonly PdfServiceTestFactory _factory;

    /// <summary>
    /// Initializes a new instance of the <see cref="FileDeletedEventConsumerEdgeCaseTests"/> class.
    /// </summary>
    /// <param name="factory">The PDF service test factory.</param>
    public FileDeletedEventConsumerEdgeCaseTests(PdfServiceTestFactory factory)
    {
        _factory = factory;
    }

    /// <summary>
    /// Verifies that deleting an unrelated file does not fail when no generation requests match.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Fact]
    public async Task Consume_FileDeletedEvent_NoMatchingRequests_DoesNotThrow()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<PdfDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<FileDeletedEventConsumer>>();

        var consumer = new FileDeletedEventConsumer(context, logger);

        var evt = new FileDeletedEvent(
            MessageId: Guid.NewGuid(),
            MessageName: "FileDeletedEvent",
            MessageType: MessageType.Event,
            MessageVersion: "1.0",
            PublishedBy: "Upload",
            ConsumedBy: new[] { "Pdf" },
            CorrelationId: Guid.NewGuid(),
            CausationId: null,
            OccurredAtUtc: DateTimeOffset.UtcNow,
            IsPublic: true,
            Payload: new FileDeletedEventPayload(
                FileId: "nonexistent-file-id",
                UploadId: "UP-999",
                ServiceId: "PDF",
                StoragePath: "nonexistent/path",
                DeletedAt: DateTimeOffset.UtcNow,
                DeletedBy: Guid.NewGuid().ToString(),
                Reason: "Test"
            )
        );

        var mockContext = new Mock<ConsumeContext<FileDeletedEvent>>();
        mockContext.Setup(m => m.Message).Returns(evt);

        await consumer.Consume(mockContext.Object);
    }

    /// <summary>
    /// Verifies that all matching generation requests are updated for a shared deleted file identifier.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Fact]
    public async Task Consume_FileDeletedEvent_MultipleMatchingRequests_UpdatesAll()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<PdfDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<FileDeletedEventConsumer>>();

        var fileId = "shared-file-123";

        var log1 = new GenerationRequest
        {
            Id = Guid.NewGuid(),
            ReferenceId = "REF-1",
            TemplateCode = "T1",
            DocumentType = DocumentType.Invoice,
            Status = GenerationStatus.Completed,
            StorageUrl = $"http://storage.com/{fileId}/invoice1.pdf",
            CreatedAt = DateTime.UtcNow
        };

        var log2 = new GenerationRequest
        {
            Id = Guid.NewGuid(),
            ReferenceId = "REF-2",
            TemplateCode = "T2",
            DocumentType = DocumentType.Receipt,
            Status = GenerationStatus.Completed,
            StorageUrl = $"http://storage.com/{fileId}/receipt1.pdf",
            CreatedAt = DateTime.UtcNow
        };

        context.GenerationRequests.AddRange(log1, log2);
        await context.SaveChangesAsync();

        var consumer = new FileDeletedEventConsumer(context, logger);

        var evt = new FileDeletedEvent(
            MessageId: Guid.NewGuid(),
            MessageName: "FileDeletedEvent",
            MessageType: MessageType.Event,
            MessageVersion: "1.0",
            PublishedBy: "Upload",
            ConsumedBy: new[] { "Pdf" },
            CorrelationId: Guid.NewGuid(),
            CausationId: null,
            OccurredAtUtc: DateTimeOffset.UtcNow,
            IsPublic: true,
            Payload: new FileDeletedEventPayload(
                FileId: fileId,
                UploadId: "UP-123",
                ServiceId: "PDF",
                StoragePath: "path/to/file",
                DeletedAt: DateTimeOffset.UtcNow,
                DeletedBy: Guid.NewGuid().ToString(),
                Reason: "Test"
            )
        );

        var mockContext = new Mock<ConsumeContext<FileDeletedEvent>>();
        mockContext.Setup(m => m.Message).Returns(evt);

        await consumer.Consume(mockContext.Object);

        var updatedLog1 = await context.GenerationRequests.FindAsync(log1.Id);
        var updatedLog2 = await context.GenerationRequests.FindAsync(log2.Id);

        Assert.Null(updatedLog1!.StorageUrl);
        Assert.Null(updatedLog2!.StorageUrl);
    }

    /// <summary>
    /// Verifies that deletion matching works when the event identifies the storage path.
    /// </summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Fact]
    public async Task Consume_FileDeletedEvent_MatchesByStoragePath()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<PdfDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<FileDeletedEventConsumer>>();

        var storagePath = "path/to/deleted/file";

        var log = new GenerationRequest
        {
            Id = Guid.NewGuid(),
            ReferenceId = "REF-PATH",
            TemplateCode = "T",
            DocumentType = DocumentType.DeliveryNote,
            Status = GenerationStatus.Completed,
            StorageUrl = $"http://storage.com/{storagePath}/deliverynote.pdf",
            CreatedAt = DateTime.UtcNow
        };

        context.GenerationRequests.Add(log);
        await context.SaveChangesAsync();

        var consumer = new FileDeletedEventConsumer(context, logger);

        var evt = new FileDeletedEvent(
            MessageId: Guid.NewGuid(),
            MessageName: "FileDeletedEvent",
            MessageType: MessageType.Event,
            MessageVersion: "1.0",
            PublishedBy: "Upload",
            ConsumedBy: new[] { "Pdf" },
            CorrelationId: Guid.NewGuid(),
            CausationId: null,
            OccurredAtUtc: DateTimeOffset.UtcNow,
            IsPublic: true,
            Payload: new FileDeletedEventPayload(
                FileId: "file-id-123",
                UploadId: "UP-123",
                ServiceId: "PDF",
                StoragePath: storagePath,
                DeletedAt: DateTimeOffset.UtcNow,
                DeletedBy: Guid.NewGuid().ToString(),
                Reason: "Test"
            )
        );

        var mockContext = new Mock<ConsumeContext<FileDeletedEvent>>();
        mockContext.Setup(m => m.Message).Returns(evt);

        await consumer.Consume(mockContext.Object);

        var updatedLog = await context.GenerationRequests.FindAsync(log.Id);
        Assert.Null(updatedLog!.StorageUrl);
    }
}
