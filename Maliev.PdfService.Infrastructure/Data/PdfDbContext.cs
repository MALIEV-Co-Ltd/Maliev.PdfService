using Maliev.PdfService.Domain.Entities;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace Maliev.PdfService.Infrastructure.Data;

/// <summary>
/// Database context for the PDF service.
/// </summary>
public class PdfDbContext : DbContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PdfDbContext"/> class.
    /// </summary>
    public PdfDbContext(DbContextOptions<PdfDbContext> options) : base(options)
    {
    }

    /// <summary>The collection of PDF templates.</summary>
    public DbSet<PdfTemplate> PdfTemplates => Set<PdfTemplate>();
    /// <summary>The collection of generation requests.</summary>
    public DbSet<GenerationRequest> GenerationRequests => Set<GenerationRequest>();

    /// <inheritdoc/>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.AddInboxStateEntity();
        modelBuilder.AddOutboxMessageEntity();
        modelBuilder.AddOutboxStateEntity();

        modelBuilder.Entity<PdfTemplate>(entity =>
        {
            entity.HasIndex(e => e.Code).IsUnique();
        });

        modelBuilder.Entity<GenerationRequest>(entity =>
        {
            entity.Property(e => e.DocumentType).HasConversion<string>();
            entity.Property(e => e.Status).HasConversion<string>();
            entity.Property(e => e.StoragePath).HasMaxLength(1024);
        });
    }
}
