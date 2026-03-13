using Maliev.PdfService.Api.Controllers;
using Maliev.PdfService.Infrastructure.Data;
using Maliev.PdfService.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Testcontainers.PostgreSql;

namespace Maliev.PdfService.Tests.Unit;

public class PdfTemplatesControllerTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _dbContainer = 
                #pragma warning disable CS0618
        new PostgreSqlBuilder().WithImage("postgres:18-alpine")
        .Build();
#pragma warning restore CS0618

    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();

        var options = new DbContextOptionsBuilder<PdfDbContext>()
            .UseNpgsql(_dbContainer.GetConnectionString())
            .Options;

        using var context = new PdfDbContext(options);
        await context.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        await _dbContainer.StopAsync();
    }

    [Fact]
    public async Task GetTemplates_ReturnsEmptyList_WhenNoTemplates()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<PdfDbContext>()
            .UseNpgsql(_dbContainer.GetConnectionString())
            .Options;

        using var context = new PdfDbContext(options);
        var controller = new PdfTemplatesController(context);

        // Act
        var result = await controller.GetTemplates();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var templates = Assert.IsAssignableFrom<System.Collections.IEnumerable>(okResult.Value);
        Assert.Empty(templates);
    }

    [Fact]
    public async Task GetTemplates_ReturnsTemplates_WhenExist()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<PdfDbContext>()
            .UseNpgsql(_dbContainer.GetConnectionString())
            .Options;

        using var context = new PdfDbContext(options);
        context.PdfTemplates.Add(new PdfTemplate { Id = Guid.NewGuid(), Code = "T1", Name = "Template 1", LayoutClass = "L1" });
        await context.SaveChangesAsync();

        var controller = new PdfTemplatesController(context);

        // Act
        var result = await controller.GetTemplates();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var templates = Assert.IsAssignableFrom<System.Collections.IEnumerable>(okResult.Value);
        Assert.NotEmpty(templates);
    }
}




