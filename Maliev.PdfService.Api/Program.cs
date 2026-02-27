#pragma warning disable CA1848 // For improved performance, use the LoggerMessage delegates

using Maliev.Aspire.ServiceDefaults;
using Maliev.PdfService.Api.Metrics;
using Maliev.PdfService.Api.Services;
using Maliev.PdfService.Data.Data;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Infrastructure;

// Initialize bootstrap logging
using var loggerFactory = LoggerFactory.Create(logBuilder => logBuilder.AddConsole());
var bootstrapLogger = loggerFactory.CreateLogger("Program");

try
{
    Maliev.PdfService.Api.Program.Log.StartingHost(bootstrapLogger, "PDF Service");

    var builder = WebApplication.CreateBuilder(args);

    // --- Maliev Service Defaults ---
    builder.AddServiceDefaults();
    builder.AddJwtAuthentication();

    // --- Database ---
    builder.AddPostgresDbContext<PdfDbContext>("PdfDbContext");

    // --- Messaging ---
    builder.AddMassTransitWithRabbitMq(x =>
    {
        x.AddConsumer<Maliev.PdfService.Api.Consumers.InvoiceFinalizedConsumer>();
        x.AddConsumer<Maliev.PdfService.Api.Consumers.FileDeletedEventConsumer>();
        x.AddConsumer<Maliev.PdfService.Api.Consumers.PdfGenerationRequestedConsumer>();
        x.AddConsumer<Maliev.PdfService.Api.Consumers.ReceiptPdfRequestedConsumer>();
        x.AddConsumer<Maliev.PdfService.Api.Consumers.DeliveryNotePdfRequestedConsumer>();
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
    builder.AddServiceClient<IInvoiceServiceClient, InvoiceServiceClient>("InvoiceService");

    // Add DeliveryService HTTP client for fetching delivery note data
    builder.Services.AddHttpClient("DeliveryService", client =>
    {
        var baseUrl = builder.Configuration.GetConnectionString("deliveryservice")
            ?? builder.Configuration["Services:DeliveryService:BaseUrl"];
        if (!string.IsNullOrEmpty(baseUrl))
        {
            client.BaseAddress = new Uri(baseUrl);
        }
    });

    builder.Services.AddScoped<IDocumentFactory, DocumentFactory>();
    builder.Services.AddScoped<IPdfGenerator, PdfGenerator>();
    builder.Services.AddSingleton<IFontResolver, FontResolver>();
    builder.AddIAMServiceClient("pdf");
    builder.Services.AddIAMRegistration<PdfIAMRegistrationService>("pdf");

    builder.Services.AddPermissionAuthorization();

    var app = builder.Build();
    var logger = app.Services.GetRequiredService<ILogger<Maliev.PdfService.Api.Program>>();

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
    if (!app.Environment.IsDevelopment())
    {
        app.UseHttpsRedirection();
    }
    app.UseAuthentication();
    app.UseAuthorization();

    // Map Aspire default endpoints
    app.MapDefaultEndpoints("pdf");

    // Map OpenAPI and Scalar documentation (dev/staging only)
    app.MapApiDocumentation(servicePrefix: "pdf");

    // Run migrations on startup
    await app.MigrateDatabaseAsync<PdfDbContext>();

    // Map endpoints with /pdf prefix
    app.MapControllers();

    Maliev.PdfService.Api.Program.Log.ServiceStarted(logger, "PDF Service");
    await app.RunAsync();
}
catch (Exception ex)
{
    Maliev.PdfService.Api.Program.Log.HostTerminated(bootstrapLogger, ex, "PDF Service");
    throw;
}
finally
{
    loggerFactory.Dispose();
}

namespace Maliev.PdfService.Api
{
    /// <summary>
    /// Entry point for the PDF Service API.
    /// </summary>
    public partial class Program
    {
        internal static partial class Log
        {
            [LoggerMessage(Level = LogLevel.Information, Message = "Starting {ServiceName} host")]
            public static partial void StartingHost(ILogger logger, string serviceName);

            [LoggerMessage(Level = LogLevel.Critical, Message = "{ServiceName} host terminated unexpectedly during startup")]
            public static partial void HostTerminated(ILogger logger, Exception ex, string serviceName);

            [LoggerMessage(Level = LogLevel.Information, Message = "{ServiceName} started successfully")]
            public static partial void ServiceStarted(ILogger logger, string serviceName);
        }
    }
}
