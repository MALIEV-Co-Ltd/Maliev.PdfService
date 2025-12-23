using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Maliev.PdfService.Data.Data;

public class PdfDbContextFactory : IDesignTimeDbContextFactory<PdfDbContext>
{
    public PdfDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<PdfDbContext>();
        optionsBuilder.UseNpgsql("Host=localhost;Database=pdf-app-db;Username=postgres;Password=postgres");

        return new PdfDbContext(optionsBuilder.Options);
    }
}
