using Maliev.MessagingContracts.Contracts.Uploads;
using Maliev.PdfService.Infrastructure.Data;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace Maliev.PdfService.Api.Consumers;

/// <summary>
/// Consumes FileDeletedEvent to clean up local storage URLs in Pdf Service.
/// </summary>
public class FileDeletedEventConsumer : IConsumer<FileDeletedEvent>
{
    private readonly PdfDbContext _dbContext;
    private readonly ILogger<FileDeletedEventConsumer> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="FileDeletedEventConsumer"/> class.
    /// </summary>
    public FileDeletedEventConsumer(PdfDbContext dbContext, ILogger<FileDeletedEventConsumer> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task Consume(ConsumeContext<FileDeletedEvent> context)
    {
        var payload = context.Message.Payload;

        _logger.LogInformation("Processing FileDeletedEvent for FileId: {FileId}, StoragePath: {StoragePath}",
            payload.FileId, payload.StoragePath);

        // Find generation requests referencing this storage path or file ID
        // Use EF.Functions.Like for case-insensitive/provider-specific matching if needed,
        // but here we just need to avoid client-side evaluation issues if any.
        var fileIdPattern = $"%{payload.FileId}%";
        var storagePathPattern = $"%{payload.StoragePath}%";

        var requests = await _dbContext.GenerationRequests
            .Where(r =>
                (r.StorageUrl != null && (EF.Functions.Like(r.StorageUrl, fileIdPattern) || EF.Functions.Like(r.StorageUrl, storagePathPattern))) ||
                (r.StoragePath != null && EF.Functions.Like(r.StoragePath, storagePathPattern)))
            .ToListAsync();

        if (requests.Any())
        {
            foreach (var req in requests)
            {
                req.StorageUrl = null;
                req.StoragePath = null;
            }

            await _dbContext.SaveChangesAsync();
            _logger.LogInformation("Cleared PDF storage artifacts for {Count} generation requests.", requests.Count);
        }
    }
}
