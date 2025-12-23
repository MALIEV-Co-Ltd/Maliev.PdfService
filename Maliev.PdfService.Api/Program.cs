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
builder.AddPostgresDbContext<PdfDbContext>(
    connectionStringName: "PdfDbContext",
    configureOptions: (Action<DbContextOptionsBuilder>?)null);

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
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

// --- API Versioning ---
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    options.ApiVersionReader = new UrlSegmentApiVersionReader();
}).AddMvc();

// --- PDF Engine Settings ---
QuestPDF.Settings.License = LicenseType.Community;

// --- Custom Services ---
builder.Services.AddSingleton<PdfMetrics>();
builder.Services.AddHttpClient<IUploadServiceClient, UploadServiceClient>(client =>
{
    var baseUrl = builder.Configuration["ExternalServices:UploadService:BaseUrl"] ?? "http://maliev-uploadservice-api:8080";
    client.BaseAddress = new Uri(baseUrl);
})
.AddStandardResilienceHandler();

builder.Services.AddScoped<IDocumentFactory, DocumentFactory>();
builder.Services.AddScoped<IPdfGenerator, PdfGenerator>();
builder.Services.AddSingleton<IFontResolver, FontResolver>();
builder.Services.AddHostedService<PdfIAMRegistrationService>();

var app = builder.Build();

// --- Initialize PDF Engine ---
using (var scope = app.Services.CreateScope())
{
    var fontResolver = scope.ServiceProvider.GetRequiredService<IFontResolver>();
    fontResolver.RegisterFonts();
}

// --- Middleware Pipeline ---
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthorization();

// Map endpoints with /pdf prefix
app.MapControllers();

// Map Aspire default endpoints
app.MapDefaultEndpoints("pdf");

app.Run();