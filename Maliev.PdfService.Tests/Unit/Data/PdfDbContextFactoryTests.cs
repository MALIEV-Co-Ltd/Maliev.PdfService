using Maliev.PdfService.Infrastructure.Data;
using Xunit;

namespace Maliev.PdfService.Tests.Unit.Data;

/// <summary>
/// Tests design-time PDF database context configuration.
/// </summary>
public sealed class PdfDbContextFactoryTests
{
    /// <summary>
    /// Verifies design-time EF operations require an explicit connection string.
    /// </summary>
    [Fact]
    public void CreateDbContext_MissingConnectionString_ThrowsConfigurationError()
    {
        var previousConnectionString = Environment.GetEnvironmentVariable("ConnectionStrings__PdfDbContext");
        Environment.SetEnvironmentVariable("ConnectionStrings__PdfDbContext", null);

        try
        {
            var factory = new PdfDbContextFactory();

            var exception = Assert.Throws<InvalidOperationException>(() => factory.CreateDbContext([]));

            Assert.Contains("ConnectionStrings__PdfDbContext", exception.Message);
        }
        finally
        {
            Environment.SetEnvironmentVariable("ConnectionStrings__PdfDbContext", previousConnectionString);
        }
    }

    /// <summary>
    /// Verifies design-time EF operations use the configured connection string.
    /// </summary>
    [Fact]
    public void CreateDbContext_ConfiguredConnectionString_CreatesContext()
    {
        var previousConnectionString = Environment.GetEnvironmentVariable("ConnectionStrings__PdfDbContext");
        Environment.SetEnvironmentVariable(
            "ConnectionStrings__PdfDbContext",
            "Host=localhost;Database=pdf_test;Username=test");

        try
        {
            var factory = new PdfDbContextFactory();

            using var context = factory.CreateDbContext([]);

            Assert.NotNull(context);
        }
        finally
        {
            Environment.SetEnvironmentVariable("ConnectionStrings__PdfDbContext", previousConnectionString);
        }
    }
}
