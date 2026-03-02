using Maliev.PdfService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Maliev.PdfService.Tests.Infrastructure;

/// <summary>Integrity tests.</summary>
public class ModelIntegrityTests
{
    /// <summary>Check for pending migrations.</summary>
    [Fact]
    public void Model_ShouldNotHavePendingChanges()
    {
        var options = new DbContextOptionsBuilder<PdfDbContext>()
            .UseNpgsql("Host=localhost;Database=ModelCheck")
            .Options;

        using var context = new PdfDbContext(options);
        var hasChanges = context.Database.HasPendingModelChanges();

        Assert.False(hasChanges, "Run 'dotnet ef migrations add <Name> --project Maliev.PdfService.Data --startup-project Maliev.PdfService.Api'");
    }
}
