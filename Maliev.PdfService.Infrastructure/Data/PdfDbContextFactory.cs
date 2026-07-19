using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Maliev.PdfService.Infrastructure.Data;

/// <summary>
/// Design-time factory for the <see cref="PdfDbContext"/>.
/// </summary>
public class PdfDbContextFactory : IDesignTimeDbContextFactory<PdfDbContext>
{
    /// <inheritdoc/>
    public PdfDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<PdfDbContext>();
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__PdfDbContext")
            ?? throw new InvalidOperationException(
                "Set ConnectionStrings__PdfDbContext for design-time EF operations.");

        optionsBuilder.UseNpgsql(connectionString);

        return new PdfDbContext(optionsBuilder.Options);
    }
}
