# Maliev.PdfService

A .NET 10.0 Web API service responsible for generating PDF documents.

## Technology Stack

- **Framework:** .NET 10.0
- **PDF Generation:** QuestPDF
- **Messaging:** MassTransit (RabbitMQ)
- **Database:** Entity Framework Core
- **Cloud Storage:** Google Cloud Storage

## Project Structure

- **Maliev.PdfService.Api:** The entry point of the application, containing Controllers, Consumers, and Services.
- **Maliev.PdfService.Data:** Data access layer, Entity Framework Core context, and migrations.
- **Maliev.PdfService.Tests:** Unit and Integration tests.

## Getting Started

### Prerequisites

- .NET 10.0 SDK

### Build

```bash
dotnet build
```

### Run

```bash
dotnet run --project Maliev.PdfService.Api
```

### Test

```bash
dotnet test
```
