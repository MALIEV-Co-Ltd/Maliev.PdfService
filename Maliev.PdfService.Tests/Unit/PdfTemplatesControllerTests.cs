using Maliev.PdfService.Api.Controllers;
using Maliev.PdfService.Infrastructure.Data;
using Maliev.PdfService.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Maliev.PdfService.Tests.Unit;

public class PdfTemplatesControllerTests
{
    [Fact]
    public async Task GetTemplates_ReturnsEmptyList_WhenNoTemplates()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<PdfDbContext>()
            .UseInMemoryDatabase(databaseName: "PdfTemplatesControllerTests_Empty")
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
            .UseInMemoryDatabase(databaseName: "PdfTemplatesControllerTests_NotEmpty")
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
