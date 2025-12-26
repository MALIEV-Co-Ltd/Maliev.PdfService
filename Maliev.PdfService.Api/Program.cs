using Maliev.PdfService.Api.Metrics;
using Maliev.PdfService.Api.Services;
using Maliev.PdfService.Data.Data;
using Maliev.Aspire.ServiceDefaults;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Asp.Versioning;
using QuestPDF.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// --- Maliev Service Defaults ---
builder.AddServiceDefaults();

// --- IAM Integration ---
builder.Services.AddIAMClient(builder.Configuration, "PdfService");

// --- Database ---
builder.AddPostgresDbContext<PdfDbContext>("PdfDbContext");

// --- Messaging ---
builder.AddMassTransitWithRabbitMq(x =>
{
    x.AddConsumer<Maliev.PdfService.Api.Consumers.InvoiceFinalizedConsumer>();
});

// --- API Configuration ---
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });

builder.AddStandardOpenApi(
    title: "MALIEV PDF Service API",
    description: "PDF generation service for the Maliev platform. Handles document rendering from templates, font resolution, and secure delivery of generated documents.");

// --- API Versioning ---
builder.AddDefaultApiVersioning();

// --- PDF Engine Settings ---
QuestPDF.Settings.License = LicenseType.Community;

// --- Custom Services ---
builder.Services.AddSingleton<PdfMetrics>();
builder.AddServiceClient<IUploadServiceClient, UploadServiceClient>("UploadService");

builder.Services.AddScoped<IDocumentFactory, DocumentFactory>();
builder.Services.AddScoped<IPdfGenerator, PdfGenerator>();
builder.Services.AddSingleton<IFontResolver, FontResolver>();
builder.Services.AddIAMRegistration<PdfIAMRegistrationService>();

var app = builder.Build();

// --- Maliev Standard Middleware ---
app.UseStandardMiddleware();
app.UseCors();

// --- Initialize PDF Engine ---
using (var scope = app.Services.CreateScope())
{
    var fontResolver = scope.ServiceProvider.GetRequiredService<IFontResolver>();
    fontResolver.RegisterFonts();
}

// --- Middleware Pipeline ---
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

// Map Aspire default endpoints
app.MapDefaultEndpoints("pdf");

// Map OpenAPI and Scalar documentation (dev/staging only)
app.MapApiDocumentation(servicePrefix: "pdf");

// Run migrations on startup
try
{
    await app.MigrateDatabaseAsync<PdfDbContext>();
}
catch (Exception ex)
{
    Console.WriteLine($"Error applying migrations: {ex.Message}");
}

// Map endpoints with /pdf prefix
app.MapControllers();

app.Run();