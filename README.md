# Maliev PDF Service

Professional PDF document generation service for the MALIEV platform, providing on-demand and asynchronous PDF creation for invoices, quotations, reports, receipts, and other business documents with full IAM integration.

## Service Description

The PDF Service handles all PDF document generation across the MALIEV platform. It provides both synchronous (immediate) and asynchronous (queued) PDF generation with support for templates, custom fonts, watermarks, and digital signatures. Generated PDFs can be stored in Google Cloud Storage or returned directly to clients.

## Architecture Overview

### Project Structure
```
Maliev.PdfService/
├── Maliev.PdfService.Api/              # Presentation layer
│   ├── Controllers/                    # REST API endpoints
│   ├── Services/                       # PDF generation services
│   ├── Templates/                      # Document templates
│   └── Consumers/                      # RabbitMQ event consumers
├── Maliev.PdfService.Data/             # Data access layer
│   ├── Entities/                       # Generation history
│   └── Migrations/                     # Database migrations
└── Maliev.PdfService.Tests/            # Integration tests
```

## Technologies Used

- **.NET 10.0** - Runtime and framework
- **ASP.NET Core** - Web API framework
- **QuestPDF** - Modern PDF generation library
- **Entity Framework Core** - ORM with PostgreSQL provider
- **PostgreSQL 18** - PDF generation history and templates
- **Google Cloud Storage** - PDF file storage
- **RabbitMQ** - Message queue via MassTransit for async generation
- **Redis** - Caching for templates and frequently generated documents
- **OpenTelemetry** - Observability

## Dependencies

### Databases
- **PostgreSQL**: Generation history, job status, template metadata
- **Redis**: Template caching, generation queue status

### Storage
- **Google Cloud Storage**: Long-term PDF storage with signed URLs

### Messaging
- **RabbitMQ**: Async PDF generation requests from other services

### External Services
- **IAM Service**: Authentication and authorization
- **Invoice Service**: Invoice data for PDF generation
- **Quotation Service**: Quote data for PDF generation
- **Receipt Service**: Receipt data for PDF generation
- **Order Service**: Order confirmation PDFs

## IAM Integration

### Required Permissions
- `pdf.generate` - Generate PDF documents
- `pdf.generate.async` - Submit async generation jobs
- `pdf.read` - Download/view generated PDFs
- `pdf.templates.read` - View PDF templates
- `pdf.templates.write` - Create/modify templates (admin)
- `pdf.history.read` - View generation history

### Predefined Roles
- **User**: Generate and view own PDFs
- **Service Account**: Async PDF generation from other services
- **PDF Admin**: Manage templates and view all generation history

## API Endpoints

### Synchronous Generation
- `POST /v1/pdf/generate/invoice` - Generate invoice PDF (immediate)
- `POST /v1/pdf/generate/quotation` - Generate quotation PDF
- `POST /v1/pdf/generate/receipt` - Generate receipt PDF
- `POST /v1/pdf/generate/order-confirmation` - Generate order confirmation
- `POST /v1/pdf/generate/custom` - Generate from custom template

### Asynchronous Generation
- `POST /v1/pdf/async/invoice` - Queue invoice PDF generation
- `POST /v1/pdf/async/batch` - Batch PDF generation
- `GET /v1/pdf/jobs/{jobId}` - Get job status
- `GET /v1/pdf/jobs/{jobId}/download` - Download when ready

### Template Management
- `GET /v1/pdf/templates` - List available templates
- `GET /v1/pdf/templates/{id}` - Get template details
- `POST /v1/pdf/templates` - Create custom template
- `PUT /v1/pdf/templates/{id}` - Update template
- `DELETE /v1/pdf/templates/{id}` - Delete template

### History
- `GET /v1/pdf/history` - View generation history
- `GET /v1/pdf/history/{id}` - Get specific generation details

## Configuration

### appsettings.json
```json
{
  "ConnectionStrings": {
    "PdfDatabase": "Host=postgres;Port=5432;Database=maliev_pdf;Username=app;Password=secret",
    "Redis": "redis:6379"
  },
  "RabbitMQ": {
    "Host": "rabbitmq",
    "Username": "guest",
    "Password": "guest"
  },
  "Jwt": {
    "Key": "base64-encoded-key",
    "Issuer": "maliev-pdf-service",
    "Audience": "maliev-services"
  },
  "ExternalServices": {
    "IAM": {
      "BaseUrl": "http://iam-service:8080"
    }
  },
  "GoogleCloudStorage": {
    "BucketName": "maliev-pdfs",
    "ProjectId": "maliev-platform",
    "CredentialsPath": "/secrets/gcs-key.json"
  },
  "PdfGeneration": {
    "MaxConcurrentJobs": 10,
    "DefaultDPI": 300,
    "CompressionEnabled": true,
    "WatermarkEnabled": false
  }
}
```

## Database

**PostgreSQL 18** with Entity Framework Core migrations.

**Main Tables:**
- `PdfGenerationJobs` - Job tracking (status, progress, errors)
- `PdfTemplates` - Template definitions and metadata
- `PdfGenerationHistory` - Audit trail of all generations
- `PdfMetadata` - Generated PDF metadata (size, pages, URL)

## Running the Service

### Development
```bash
cd Maliev.PdfService.Api
dotnet run
```

**Access:**
- API: http://localhost:5000
- Health: http://localhost:5000/pdf/liveness
- Metrics: http://localhost:5000/pdf/metrics

### Docker
```bash
docker build -t maliev/pdf-service:latest .
docker run -p 8080:8080 \
  -v /path/to/gcs-key.json:/secrets/gcs-key.json \
  maliev/pdf-service:latest
```

### Tests
```bash
# Ensure Docker is running
docker ps

# Run tests
dotnet test
```

## Test Status

**From Test Summary (2025-12-24):**
- **Status**: FAILED (2 tests)
- **Critical Issue**: Database context initialization failure (NullReferenceException)
- **Location**: `DatabaseExtensions.cs:line 102`

**To Fix:**
1. Verify PostgreSQL connection string is properly configured
2. Check database configuration in ServiceDefaults
3. Ensure connection string is valid before DbContext initialization

## Key Features

### PDF Generation
- **High Quality**: 300 DPI, professional layouts
- **Templates**: Pre-built templates for common documents
- **Custom Fonts**: Support for corporate branding
- **Multi-Language**: Unicode support for international characters
- **Images & Logos**: Embedded images and company logos
- **Tables & Charts**: Complex data visualization

### Document Types
- **Invoices**: Tax invoices with line items, totals, VAT
- **Quotations**: Professional sales quotes
- **Receipts**: Payment receipts with transaction details
- **Reports**: Financial and operational reports
- **Order Confirmations**: Order summaries

### Advanced Features
- **Watermarks**: Draft, confidential, paid watermarks
- **Digital Signatures**: PDF signing (if configured)
- **Compression**: Optimized file sizes
- **Metadata**: PDF metadata for searchability
- **Accessibility**: PDF/UA compliant (optional)

### Performance
- **Async Processing**: Queue heavy jobs for background processing
- **Batch Generation**: Generate multiple PDFs in single request
- **Caching**: Template caching for faster generation
- **Streaming**: Large PDFs streamed directly to response

## Events Consumed

Via RabbitMQ:
- `InvoiceCreatedEvent` - Auto-generate invoice PDF
- `QuotationApprovedEvent` - Auto-generate quotation PDF
- `ReceiptIssuedEvent` - Auto-generate receipt PDF
- `ReportScheduledEvent` - Generate scheduled reports

## Events Published

- `PdfGeneratedEvent` - PDF generation completed
- `PdfGenerationFailedEvent` - PDF generation failed
- `BatchPdfCompletedEvent` - Batch job completed

## QuestPDF Usage

The service uses QuestPDF for modern, fluent PDF generation:

```csharp
Document.Create(container =>
{
    container.Page(page =>
    {
        page.Size(PageSizes.A4);
        page.Margin(2, Unit.Centimetre);
        page.Header().Text("Invoice").FontSize(20).Bold();
        page.Content().Column(column =>
        {
            column.Item().Text($"Invoice #: {invoice.Number}");
            column.Item().Table(table =>
            {
                // Invoice line items
            });
        });
        page.Footer().AlignCenter().Text(x =>
        {
            x.Span("Page ");
            x.CurrentPageNumber();
        });
    });
})
.GeneratePdf();
```

## Support

- Test Summary: `B:\maliev\all-services-test-summary.txt`
- ServiceDefaults: `B:\maliev\Maliev.Aspire\Maliev.Aspire.ServiceDefaults\README.md`
- QuestPDF Documentation: https://www.questpdf.com/

## License

Proprietary - Copyright 2025 MALIEV Co., Ltd. All rights reserved.

**QuestPDF License**: Commercial license required for commercial use.
