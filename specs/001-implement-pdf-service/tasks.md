# Tasks: PDF Generation Service

**Input**: Design documents from `specs/001-implement-pdf-service/`
**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/

**Tests**: Test-First development is REQUIRED per Maliev Constitution. Tests will be written using standard xUnit and Testcontainers.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Project initialization and basic structure

- [x] T001 Create solution and project structure (Api, Data, Tests) at repository root
- [x] T002 Configure `Maliev.PdfService.sln` and project references
- [x] T003 [P] Add NuGet dependencies (QuestPDF, MassTransit, EF Core, GCS, Aspire ServiceDefaults) to project files
- [x] T004 [P] Configure `nuget.config` with GitHub Packages source in repository root
- [x] T005 [P] Setup standard logging, LogLevel configuration in `appsettings.json`, and API versioning in `Maliev.PdfService.Api/Program.cs`
- [x] T005b [P] Create `.github/CODEOWNERS` with `@MALIEV-Co-Ltd/core-developers`
---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core infrastructure that MUST be complete before ANY user story can be implemented

- [x] T006 Implement `PdfDbContext` with Postgres configuration in `Maliev.PdfService.Data/Data/PdfDbContext.cs`
- [x] T007 Create `PdfTemplate` entity and configuration in `Maliev.PdfService.Data/Entities/PdfTemplate.cs`
- [x] T008 Create `GenerationRequest` entity and configuration in `Maliev.PdfService.Data/Entities/GenerationRequest.cs`
- [x] T009 Create initial EF Core migration for `pdf-app-db` in `Maliev.PdfService.Data/Migrations/`
- [x] T010 [P] Implement `IGcsService` for Google Cloud Storage interactions in `Maliev.PdfService.Api/Services/GcsService.cs`
- [x] T011 [P] Implement `PdfIAMRegistrationService` hosted service for permission registration in `Maliev.PdfService.Api/Services/PdfIAMRegistrationService.cs`
- [x] T012 Configure `AddServiceDefaults()` and `AddMassTransitWithRabbitMq()` in `Maliev.PdfService.Api/Program.cs`
- [x] T012b Implement business metrics (DocsGeneratedTotal, GenerationDuration) in `Maliev.PdfService.Api/Metrics/PdfMetrics.cs` per Constitution XII

**Checkpoint**: Foundation ready - user story implementation can now begin

---

## Phase 3: User Story 1 - Generate On-Demand Documents (Priority: P1) 🎯 MVP



**Goal**: Support synchronous and asynchronous PDF generation via REST API for Invoices and Quotations.



**Independent Test**: Send a JSON payload to `/pdf/v1/generate` and receive a Public URL to a valid PDF file stored in GCS.



### Tests for User Story 1



- [x] T013 [P] [US1] Create integration test for synchronous generation in `Maliev.PdfService.Tests/Integration/SyncGenerationTests.cs`

- [x] T014 [P] [US1] Create integration test for asynchronous generation in `Maliev.PdfService.Tests/Integration/AsyncGenerationTests.cs`



### Implementation for User Story 1



- [x] T015 [P] [US1] Implement `IPdfGenerator` that applies `PdfTemplate` visual settings and QuestPDF engine in `Maliev.PdfService.Api/Services/PdfGenerator.cs`

- [x] T016 [P] [US1] Implement `DocumentFactory` for document strategy resolution in `Maliev.PdfService.Api/Services/DocumentFactory.cs`

- [x] T017 [P] [US1] Implement `InvoiceDocument` layout using QuestPDF `IDocument` in `Maliev.PdfService.Api/Services/Layouts/InvoiceDocument.cs`

- [x] T018 [P] [US1] Implement `QuotationDocument` layout in `Maliev.PdfService.Api/Services/Layouts/QuotationDocument.cs`

- [x] T018b [P] [US1] Implement `ReceiptDocument` layout in `Maliev.PdfService.Api/Services/Layouts/ReceiptDocument.cs`

- [x] T018c [P] [US1] Implement `FinancialReportDocument` layout in `Maliev.PdfService.Api/Services/Layouts/FinancialReportDocument.cs`

- [x] T019 [US1] Implement `GenerationController` with sync and async actions in `Maliev.PdfService.Api/Controllers/GenerationController.cs`

- [x] T020 [US1] Add data validation using DataAnnotations in `Maliev.PdfService.Api/Models/Requests/GeneratePdfRequest.cs`



**Checkpoint**: User Story 1 is functional (MVP)

---

## Phase 4: User Story 2 - Automated Background Generation (Priority: P2)



**Goal**: Trigger PDF generation automatically via MassTransit consumers when business events occur.



**Independent Test**: Publish an `InvoiceFinalizedEvent` to RabbitMQ and verify a `GenerationRequest` is created and a PDF is stored.



### Tests for User Story 2



- [x] T021 [P] [US2] Create integration test for `InvoiceFinalizedConsumer` in `Maliev.PdfService.Tests/Integration/ConsumerTests.cs`



### Implementation for User Story 2



- [x] T022 [US2] Implement `InvoiceFinalizedConsumer` to handle messaging triggers in `Maliev.PdfService.Api/Consumers/InvoiceFinalizedConsumer.cs`

- [x] T023 [US2] Register consumers in MassTransit configuration in `Maliev.PdfService.Api/Program.cs`

---

## Phase 5: User Story 3 - Multi-Language Support (Thai/English) (Priority: P3)



**Goal**: Correctly render Thai glyphs using the Kanit font for localized documents.



**Independent Test**: Generate a document with Thai text and verify no missing glyphs (squares) are present in the output.



### Tests for User Story 3



- [x] T024 [P] [US3] Create unit tests for Thai character rendering in `Maliev.PdfService.Tests/Unit/LocalizationTests.cs`



### Implementation for User Story 3



- [x] T025 [P] [US3] Embed `Kanit-Regular.ttf` and `Kanit-Bold.ttf` as resources in `Maliev.PdfService.Api/Resources/Fonts/`

- [x] T026 [US3] Implement `FontResolver` to register embedded fonts in `Maliev.PdfService.Api/Services/FontResolver.cs`

- [x] T027 [US3] Update `InvoiceDocument` and `QuotationDocument` to use the Kanit font for Thai content

---

## Phase N: Polish & Cross-Cutting Concerns

**Purpose**: Final touches, additional documents, and developer experience

- [x] T028 [P] Configure `ShowInCompanion()` call in `PdfGenerator` for `Development` environment
- [x] T029 [P] Implement `PdfTemplatesController` for `GET /pdf/v1/templates` in `Maliev.PdfService.Api/Controllers/PdfTemplatesController.cs`
- [x] T030 Create `Dockerfile` in `Maliev.PdfService.Api/` following Constitution standards
- [x] T031 Validate `quickstart.md` steps and project health

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies.
- **Foundational (Phase 2)**: Depends on T001-T005 completion.
- **User Stories (Phase 3+)**: All depend on Foundation (Phase 2).
- **Polish (Final Phase)**: Depends on core stories being complete.

### User Story Dependencies

- **User Story 1 (P1)**: Independent after Phase 2.
- **User Story 2 (P2)**: Independent after Phase 2, utilizes `PdfGenerator` from US1.
- **User Story 3 (P3)**: Independent after Phase 2, enhances US1 layouts.

### Parallel Opportunities

- T003-T005 (Setup)
- T010-T011 (Foundational Services)
- T013-T014 (US1 Tests)
- T015-T018 (US1 Implementation - Logic vs Layouts)
- T024-T025 (US3 Resource Preparation)

---

## Parallel Example: User Story 1

```bash
# Implement layouts in parallel
Task: "Implement InvoiceDocument layout in Maliev.PdfService.Api/Services/Layouts/InvoiceDocument.cs"
Task: "Implement QuotationDocument layout in Maliev.PdfService.Api/Services/Layouts/QuotationDocument.cs"

# Setup engine while layouts are being built
Task: "Implement IPdfGenerator abstraction and QuestPDF engine in Maliev.PdfService.Api/Services/PdfGenerator.cs"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Setup structure and dependencies.
2. Build DB context and GCS storage foundation.
3. Implement QuestPDF engine and the Invoice layout.
4. Expose the Sync endpoint.
5. **Validate**: Test with a real JSON payload.

### Incremental Delivery

1. Foundation -> Core Generation API (US1).
2. Add Async path and Messaging integration (US2).
3. Add Thai font support and localization (US3).
4. Polish (Companion, Templates API).

---

## Notes

- [P] tasks indicate no shared file modification or logical blocks.
- All projects follow the `Maliev.PdfService.*` naming convention.
- All routes start with `/pdf`.
- No AutoMapper, No FluentValidation, No FluentAssertions.
