# Maliev.PdfService Development Guidelines

This document provides essential instructions for AI agents and developers working on the Maliev.PdfService repository.

> **Workspace root** `B:\maliev` contains **41 independent git repos**. Each `Maliev.*` folder and `maliev-gitops` is its own repo. There is no single repo at the workspace root. Always work within the target service directory.

---

## Build, Test & Lint Commands

### .NET Service (C# — .NET 10)

All commands run from within this service directory (`B:\maliev\Maliev.PdfService`).

```powershell
# Build (treats warnings as errors — all must be fixed)
dotnet build Maliev.PdfService.slnx

# Run all tests
dotnet test Maliev.PdfService.slnx --verbosity normal

# Run a single test method
dotnet test --filter "FullyQualifiedName~PdfGeneratorTests.GeneratePdfAsync_ReturnsPdfBytes"

# Run all tests in a class
dotnet test --filter "FullyQualifiedName~PdfGeneratorTests"

# Run with code coverage
dotnet test Maliev.PdfService.slnx --collect:"XPlat Code Coverage"

# Format check
dotnet format Maliev.PdfService.slnx

# EF Core migrations (Infrastructure project only)
dotnet ef migrations add <Name> --project Maliev.PdfService.Infrastructure --startup-project Maliev.PdfService.Infrastructure
```

---

## Code Style & Conventions

### Workspace Structure
```
Maliev.PdfService/
├── Maliev.PdfService.Api/           # Controllers, Consumers, Middleware
├── Maliev.PdfService.Application/   # Use cases, DTOs, Interfaces, Handlers
├── Maliev.PdfService.Domain/        # Entities, value objects, domain interfaces
├── Maliev.PdfService.Infrastructure/ # EF Core DbContext, repositories, HTTP clients
├── Maliev.PdfService.Tests/         # Unit + Integration tests (xUnit)
├── Directory.Build.props            # Central package versioning
└── Maliev.PdfService.slnx          # Solution file (.slnx preferred over .sln)
```

### C# Naming & Formatting
- **Namespaces**: File-scoped (`namespace Maliev.PdfService.Domain.Entities;`)
- **Classes/Methods/Properties**: `PascalCase`
- **Private fields**: `_camelCase` (underscore prefix)
- **Parameters/locals**: `camelCase`
- **Async methods**: Suffix with `Async` (e.g., `GeneratePdfAsync`)
- **Interfaces**: Prefix with `I` (e.g., `IPdfGenerator`)
- **Permissions**: GCP-style `{domain}.{plural-resource}.{action}` as `public const string` in a `Permissions` static class
  - Valid: `pdf.documents.create`, `pdf.templates.list`
  - Invalid: `pdf.document.create` (singular), `pdf.create` (missing resource)
- **XML docs**: Required on ALL public methods and properties
- **Nullable**: Enabled (`<Nullable>enable</Nullable>`). Use `?` explicitly
- **Imports**: System first, then third-party, then local. Alphabetize within groups. Remove unused `using`
- **Braces**: Allman style (new line) for methods and control structures. Expression-bodied for properties/accessors
- **Indentation**: 4 spaces, LF line endings, UTF-8, trim trailing whitespace

### C# Patterns
- **DI**: Constructor injection with `private readonly` fields
- **Controllers**: `[ApiController]`, `[ApiVersion("1")]`, `[Route("pdf/v{version:apiVersion}")]`
- **Logging**: `ILogger<T>` with structured placeholders (never interpolate): `_logger.LogInformation("Processing {FileId}", fileId)`
- **Error handling**: Global exception middleware. Return `ProblemDetails` / `ErrorResponse` DTOs. Never expose stack traces
- **JSON**: Snake_case_lower for Auth service (`JsonNamingPolicy.SnakeCaseLower`); other services may vary — check existing conventions
- **Manual mapping**: Static extension methods (`ToDto()`, `ToEntity()`). AutoMapper is banned
- **Validation**: `System.ComponentModel.DataAnnotations` on DTOs. FluentValidation is banned

---

## Banned Libraries (Build Will Fail)

| Banned | Use Instead |
|--------|-------------|
| AutoMapper | Manual mapping extensions |
| FluentValidation | DataAnnotations or manual validation |
| FluentAssertions | Standard xUnit `Assert.*` |
| Swashbuckle/Swagger | Scalar (at `/{service}/scalar`) |
| InMemoryDatabase (EF Core) | Testcontainers with real PostgreSQL |

---

## Testing Rules

- **Framework**: xUnit with standard `Assert` (`Assert.Equal`, `Assert.NotNull`, etc.)
- **Naming**: `MethodName_StateUnderTest_ExpectedBehavior` or `HTTP_METHOD_Path_Scenario_ExpectedStatus`
- **Coverage**: Minimum 80% per service
- **Integration tests**: `BaseIntegrationTestFactory<TProgram, TDbContext>` with Testcontainers (PostgreSQL, Redis, RabbitMQ). Never InMemoryDatabase
- **System tests** (Tier 3): `AspireTestFixture` with `[Collection("AspireDomainTests")]` — shared AppHost, never one per class
- **Eventual consistency**: Use `TestHelpers.WaitForAsync`. Never `Task.Delay`
- **MassTransit consumers**: Must have consumer tests using `AddMassTransitTestHarness()`

### Testing Strategy (4-Tier Pyramid Context)

This service's tests cover **Tier 1 (Unit)** and **Tier 2 (Service Integration)** of the Maliev testing pyramid:

| Tier | What to Test | Infrastructure |
|------|-------------|---------------|
| **Unit** | Business logic, domain models, service methods with mocked dependencies | None (mocks only) |
| **Service Integration** | API endpoints, database persistence, permission enforcement, input validation | `BaseIntegrationTestFactory` + Testcontainers (Postgres/Redis/RabbitMQ) |

**Tier 3 (System Integration)** — cross-service workflows and event chains — is tested in `Maliev.Aspire.Tests/`.

> Full ecosystem test strategy: `Maliev.Aspire.Tests/TEST_PLAN.md`

---

## Mandatory Rules

- **`TreatWarningsAsErrors = true`**: Zero warnings allowed. No suppression
- **`[RequirePermission("domain.resources.action")]`**: On all endpoints, not plain `[Authorize]`
- **API versioning**: All routes versioned (`v1/`)
- **Service prefix**: Routes prefixed with service domain (e.g., `/pdf`)
- **Scalar docs**: Configured at `/pdf/scalar`
- **Secrets**: Never hardcoded. Use GCP Secret Manager or environment variables
- **Async/await**: All the way down. Pass `CancellationToken`
- **EF Core Design package**: Only in Infrastructure project, never in Api
- **PostgreSQL xmin**: Shadow property only — `entity.Property<uint>("xmin").HasColumnType("xid").IsRowVersion()`. Never add entity property
- **Temporary files**: Generate in `/temp` folder, clean up afterwards

---

## Architecture & Libraries

- **Framework:** .NET 10 / ASP.NET Core
- **PDF Generation:** QuestPDF
- **Messaging:** MassTransit (RabbitMQ/ServiceBus)
- **Data Access:** Entity Framework Core (`PdfDbContext`)
- **Testing:** xUnit, Moq

---

## Git Rules

- Each `Maliev.*` folder is an independent git repo. `cd` into it before git commands
- **Commit early and often** after every meaningful unit of work. Do not accumulate changes
- **Never use `git checkout` to restore files** — commit first, then `git revert` or `git reset --soft`
- Feature branches merged to `develop` via PR. Do not push without being asked
