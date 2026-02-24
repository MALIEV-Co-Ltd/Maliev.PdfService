using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Maliev.PdfService.Data.Data;

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
            ?? "Host=localhost;Database=pdf-app-db;Username=postgres;Password=postgres";

        optionsBuilder.UseNpgsql(connectionString);

        return new PdfDbContext(optionsBuilder.Options);
    }
}
