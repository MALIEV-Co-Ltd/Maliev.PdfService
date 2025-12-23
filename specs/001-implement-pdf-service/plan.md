# Implementation Plan: PDF Generation Service

**Branch**: `001-implement-pdf-service` | **Date**: 2025-12-23 | **Spec**: [specs/001-implement-pdf-service/spec.md](spec.md)
**Input**: Feature specification from `/specs/001-implement-pdf-service/spec.md`

## Summary

Implement a high-performance .NET 10 microservice for PDF generation using QuestPDF. The service will support multi-language (Thai/English) document generation for Invoices, Quotations, Receipts, and Financial Reports. It integrates with Google Cloud Storage for persistent file storage and provides public signed/static URLs for document retrieval. Architectural alignment follows the Maliev microservices pattern with Clean Architecture, Aspire integration, and MassTransit messaging.

## Technical Context

**Language/Version**: .NET 10  
**Primary Dependencies**: QuestPDF, MassTransit, EntityFramework Core, Maliev.Aspire.ServiceDefaults, Google.Cloud.Storage.V1  
**Storage**: PostgreSQL (Metadata & Logging), Google Cloud Storage (Persistent PDF storage)  
**Testing**: xUnit (Standard Assertions), Testcontainers (Postgres, RabbitMQ, GCS Emulator/Real)  
**Target Platform**: Linux (Docker/Kubernetes) via Maliev.Aspire  
**Project Type**: Microservice (Api, Data, Tests)  
**Performance Goals**: < 2s sync generation for standard documents; support 50+ concurrent requests  
**Constraints**: 
- Route prefix: `/pdf` (No `/api`)
- NO FluentValidation
- NO FluentAssertions
- NO AutoMapper
- QuestPDF Companion enabled in Development
**Scale/Scope**: Ecosystem-wide document generation service (Quotation, Invoice, Receipt, Report)

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

- [x] **Service Autonomy**: Owns `pdf-app-db` and GCS buckets.
- [x] **Explicit Contracts**: OpenAPI/Scalar documentation.
- [x] **Test-First**: Real infrastructure testing via Testcontainers.
- [x] **Observability**: AddServiceDefaults() and Business Metrics included.
- [x] **Security**: JWT Auth + Google Secret Manager.
- [x] **Zero Warnings**: Build configured to treat warnings as errors.
- [x] **Docker Standards**: Dockerfile in API project, non-root `app` user.
- [x] **Structure**: Flat structure (Maliev.PdfService.Api, Maliev.PdfService.Data, Maliev.PdfService.Tests).
- [x] **No Forbidden Libraries**: No FluentValidation, No FluentAssertions, No AutoMapper.

## Project Structure

### Documentation (this feature)

```text
specs/001-implement-pdf-service/
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
├── contracts/           # Phase 1 output
└── tasks.md             # Phase 2 output
```

### Source Code (repository root)

```text
Maliev.PdfService.Api/
├── Controllers/
├── Services/            # DocumentFactory, PdfGenerator, FontResolver
├── Consumers/           # MassTransit event consumers
├── Middleware/
├── Models/
├── Properties/
├── appsettings.json
├── Dockerfile
└── Program.cs

Maliev.PdfService.Data/
├── Data/
│   ├── PdfDbContext.cs
│   └── Interceptors/
├── Entities/            # PdfTemplate, GenerationRequest
├── Migrations/
└── Maliev.PdfService.Data.csproj

Maliev.PdfService.Tests/
├── Integration/         # Testcontainers based tests
├── Unit/                # Layout and mapping tests
└── Maliev.PdfService.Tests.csproj

Maliev.PdfService.sln
nuget.config
```

**Structure Decision**: Flat structure as per Constitution XV, with Api, Data, and Tests projects named with full company prefix.

## Complexity Tracking

> **Fill ONLY if Constitution Check has violations that must be justified**

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| None | N/A | N/A |