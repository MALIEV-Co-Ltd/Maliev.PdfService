using Maliev.PdfService.Infrastructure.Data;
using MassTransit.EntityFrameworkCoreIntegration;
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

    /// <summary>Ensures published PDF events are backed by the transactional bus outbox.</summary>
    [Fact]
    public void Model_ShouldIncludeMassTransitOutboxEntities()
    {
        var options = new DbContextOptionsBuilder<PdfDbContext>()
            .UseNpgsql("Host=localhost;Database=pdf_model_test;Username=postgres;Password=postgres")
            .Options;

        using var context = new PdfDbContext(options);

        Assert.NotNull(context.Model.FindEntityType(typeof(InboxState)));
        Assert.NotNull(context.Model.FindEntityType(typeof(OutboxMessage)));
        Assert.NotNull(context.Model.FindEntityType(typeof(OutboxState)));
    }
}
