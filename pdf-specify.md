# Specification for Maliev.PdfService

**Context:**
You are a Senior .NET Architect. You are implementing a new service in the Maliev ecosystem. You must adhere to the established patterns found in `Maliev.AccountingService` and `Maliev.InvoiceService`.

**Goal:**
Generate a .NET 10 microservice for PDF generation using QuestPDF.

---

# Service Specification: Maliev.PdfService

## 1. Project Structure
Create the following projects within `B:\maliev\Maliev.PdfService`:
*   **Maliev.PdfService.Api**: ASP.NET Core Web API (Host).
*   **Maliev.PdfService.Data**: Class Library (EF Core, PostgreSQL).
*   **Maliev.PdfService.Tests**: xUnit Test Project.

## 2. Technical Requirements
*   **Target Framework**: .NET 10.
*   **PDF Engine**: QuestPDF.
*   **Primary Font**: Kanit (must be registered in `Program.cs` for Thai support).
*   **Routing**: All routes MUST start with `/pdf` (e.g., `/pdf/v1/generate`). DO NOT use `/api`.
*   **Validation**: Use `System.ComponentModel.DataAnnotations`. **DO NOT use FluentValidation**.
*   **Assertions**: Use `Xunit.Assert`. **DO NOT use FluentAssertions**.
*   **Dev Tools**: Call `document.ShowInCompanion()` in `Development` environment.

## 3. Infrastructure & Integration
*   **ServiceDefaults**: Use `builder.AddServiceDefaults()` from `Maliev.Aspire.ServiceDefaults`.
*   **Database**: Use `builder.AddPostgresDbContext<PdfDbContext>()`.
*   **Messaging**: Use `builder.AddMassTransitWithRabbitMq()`.
*   **IAM**: 
    *   Implement `PdfIAMRegistrationService : IHostedService` to register permissions (e.g., `pdf:generate`, `pdf:view`).
    *   Use `builder.AddJwtAuthentication()`.

## 4. Domain Entities (`Maliev.PdfService.Data`)
*   **PdfTemplate**: `Id`, `Code` (unique), `Name`, `LayoutClass`, `ConfigJson`.
*   **PdfGenerationLog**: `Id`, `ReferenceId`, `DocumentType`, `Status`, `CreatedAt`.

## 5. QuestPDF Implementation (`Maliev.PdfService.Api/Services`)
*   Implement a `DocumentFactory` that returns an `IDocument` based on `DocumentType`:
    *   `QuotationDocument`: Includes valid-until date, signature lines.
    *   `InvoiceDocument`: Itemized table with Tax/Total calculation.
    *   `ReceiptDocument`: Payment method details.
    *   `FinancialReportDocument`: Aggregated data tables.
*   **Localization**: Implement a `FontResolver` that ensures Thai glyphs render correctly using the embedded Kanit font.

## 6. API Endpoints
*   `POST /pdf/v1/generate`: (Sync) Generates and returns PDF file stream.
*   `POST /pdf/v1/generate/async`: (Async) Enqueues generation and returns a tracking ID.
*   `GET /pdf/v1/templates`: Lists available document templates.

## 7. Development Mode
In `Program.cs`:
```csharp
if (app.Environment.IsDevelopment())
{
    QuestPDF.Settings.License = LicenseType.Community;
    // The Generator service should call document.ShowInCompanion() when rendering
}
```

## 8. Naming Conventions
*   Database: `pdf-app-db` (Postgres).
*   Containers: `maliev-pdfservice-api`.
*   Namespace: `Maliev.PdfService`.

---
