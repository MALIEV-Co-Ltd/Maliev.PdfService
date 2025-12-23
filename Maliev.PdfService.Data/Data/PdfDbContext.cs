using Maliev.PdfService.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Maliev.PdfService.Data.Data;

public class PdfDbContext : DbContext
{
    public PdfDbContext(DbContextOptions<PdfDbContext> options) : base(options)
    {
    }

    public DbSet<PdfTemplate> PdfTemplates => Set<PdfTemplate>();
    public DbSet<GenerationRequest> GenerationRequests => Set<GenerationRequest>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<PdfTemplate>(entity =>
        {
            entity.HasIndex(e => e.Code).IsUnique();
        });

        modelBuilder.Entity<GenerationRequest>(entity =>
        {
            entity.Property(e => e.DocumentType).HasConversion<string>();
            entity.Property(e => e.Status).HasConversion<string>();
        });
    }
}
