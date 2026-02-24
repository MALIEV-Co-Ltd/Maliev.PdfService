# Quickstart: PDF Generation Service

## Prerequisites
- .NET 10 SDK
- Docker (for Testcontainers and QuestPDF Companion)
- Access to Google Cloud Storage (or Emulator)

## Initial Setup
1. **Clone and Restore**:
   ```bash
   dotnet restore
   ```
2. **Database Migration**:
   ```bash
   dotnet ef database update --project Maliev.PdfService.Data --startup-project Maliev.PdfService.Api
   ```

## Local Development (QuestPDF Companion)
To design and preview templates in real-time:
1. Ensure the QuestPDF Companion app is running on your machine.
2. Run the API project in `Development` mode:
   ```bash
   $env:ASPNETCORE_ENVIRONMENT="Development"
   dotnet run --project Maliev.PdfService.Api
   ```
3. Trigger a generation via `POST /pdf/v1/generate`. The document will automatically appear in the Companion.

## Running Tests
Tests use **Testcontainers** to spin up real PostgreSQL and RabbitMQ instances.
```bash
dotnet test
```

## Integration Example (C#)
```csharp
var client = new HttpClient();
var request = new {
    templateCode = "INV-STD-01",
    referenceId = "INV-1001",
    documentType = "Invoice",
    data = new {
        CustomerName = "Alice",
        Items = new[] { new { Description = "Widget", Price = 100 } }
    }
};

var response = await client.PostAsJsonAsync("https://localhost:8080/pdf/v1/generate", request);
var result = await response.Content.ReadFromJsonAsync<GenerationResult>();
Console.WriteLine($"PDF available at: {result.StorageUrl}");
```
