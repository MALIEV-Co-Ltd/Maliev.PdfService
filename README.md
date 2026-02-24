# Maliev PDF Service

[![Build Status](https://img.shields.io/badge/Build-Passing-success)](https://github.com/ORGANIZATION/Maliev.PdfService)
[![.NET Version](https://img.shields.io/badge/.NET-10.0-blue)](https://dotnet.microsoft.com/download/dotnet/10.0)
[![Database](https://img.shields.io/badge/Database-PostgreSQL%2018-blue)](https://www.postgresql.org/)

Professional document generation service specializing in high-fidelity business PDFs for the Maliev ecosystem.

**Role in MALIEV Architecture**: The authoritative engine for document rendering. It transforms raw business data (Invoices, Quotations, Reports) into professional, industry-standard PDF documents with support for corporate branding and digital signatures.

---

## 🏗️ Architecture & Tech Stack

- **Framework**: ASP.NET Core 10.0 (C# 13)
- **Engine**: QuestPDF (High-performance rendering kernel)
- **Database**: PostgreSQL 18 with Entity Framework Core 10.x
- **Distributed Cache**: Redis 7.x (Template & asset caching)
- **Messaging**: RabbitMQ via MassTransit (Asynchronous generation queue)
- **Object Storage**: Google Cloud Storage (PDF archival)
- **API Documentation**: OpenAPI 3.1 + Scalar UI

---

## ⚖️ Constitution Rules

This service strictly adheres to the platform development mandates:

### Banned Libraries
To maintain high performance and low complexity, the following are **NOT** used:
- ❌ **AutoMapper**: Explicit manual mapping only.
- ❌ **FluentValidation**: Standard Data Annotations (`[Required]`, `[EmailAddress]`) only.
- ❌ **FluentAssertions**: Standard xUnit `Assert` methods only.
- ❌ **In-memory Test DB**: All integration tests use **Testcontainers** with real PostgreSQL 18.

### Mandatory Practices
- ✅ **TreatWarningsAsErrors**: Enabled in all `.csproj` files.
- ✅ **XML Documentation**: Required on all public methods and properties.
- ✅ **No Secrets in Code**: All sensitive configuration injected via environment variables.
- ✅ **No Test Config in Program.cs**: Test configuration in test fixtures only.
- ✅ **IAM Integration**: Self-registers permissions with the IAM Service using GCP-style naming: `{service}.{resource}.{action}`.

---

## ✨ Key Features

- **High-Performance Rendering**: Industrial-grade PDF creation with sub-second generation for complex documents.
- **Async Job Orchestration**: Reliable background generation for batch processes or high-resource documents.
- **Template Management**: Centralized repository for document templates with versioned styling.
- **Signed URL Access**: Native integration with Google Cloud Storage for secure, temporary document download links.
- **Multi-Category Generators**: Specialized rendering logic for Invoices, Quotations, Receipts, and detailed Operational Reports.

---

## 🚀 Quick Start

### Prerequisites
- .NET 10.0 SDK
- Docker Desktop (for infrastructure)
- PostgreSQL 18 (Alpine)

### Local Development Setup

1. **Clone the repository**
```bash
git clone https://github.com/ORGANIZATION/Maliev.PdfService.git
cd Maliev.PdfService
```

2. **Spin up Infrastructure**
```bash
docker run --name pdf-db -e POSTGRES_PASSWORD=YOUR_PASSWORD -p 5432:5432 -d postgres:18-alpine
docker run --name pdf-redis -p 6379:6379 -d redis:7-alpine
```

3. **Configure Environment**
```powershell
# Windows PowerShell
$env:ConnectionStrings__PdfDbContext="YOUR_POSTGRES_CONNECTION_STRING"
$env:ConnectionStrings__Cache="YOUR_REDIS_CONNECTION_STRING"
```

4. **Apply Migrations & Run**
```bash
dotnet ef database update --project Maliev.PdfService.Data
dotnet run --project Maliev.PdfService.Api
```

The service will be available at `http://localhost:5000/pdf`. Access the interactive documentation at `http://localhost:5000/pdf/scalar`.

---

## 📡 API Endpoints

All endpoints are prefixed with `/pdf/v1/`.

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/generate/invoice` | Generate a synchronous invoice PDF |
| POST | `/async/batch` | Queue multiple documents for generation |
| GET | `/jobs/{id}` | Poll status of an asynchronous generation job |
| GET | `/templates` | List available document templates |

---

## 🏥 Health & Monitoring

Standardized health probes for Kubernetes orchestration:
- **Liveness**: `GET /pdf/liveness`
- **Readiness**: `GET /pdf/readiness` (Checks DB and Redis connectivity)
- **Metrics**: `GET /pdf/metrics` (Prometheus format)

---

## 🧪 Testing

We prioritize reliable tests over mock-heavy unit tests.

```bash
# Run all tests using Testcontainers
dotnet test --verbosity normal
```

- **Integration Tests**: Use real PostgreSQL 18 containers.
- **Contract Tests**: Ensure API stability for consumers.

---

## 📦 Deployment

Infrastructure management is handled via GitOps patterns.

- **Docker Image**: `REGION-docker.pkg.dev/PROJECT_ID/REPOSITORY/maliev-pdf-service:{sha}`
- **Environments**: Development, Staging, Production

---

## 📄 License

Proprietary - © 2025 MALIEV Co., Ltd. All rights reserved.
