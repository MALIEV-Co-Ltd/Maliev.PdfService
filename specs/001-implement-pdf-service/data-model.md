# Data Model: PDF Generation Service

## Entities

### PdfTemplate
Represents a reusable document layout strategy.

| Field | Type | Description | Constraints |
|-------|------|-------------|-------------|
| Id | Guid | Primary Key | Required |
| Code | string | Unique template identifier (e.g., "INV-STD-01") | Required, Unique, Max 50 |
| Name | string | Human-readable name | Required, Max 100 |
| LayoutClass | string | Full name of the QuestPDF IDocument implementation | Required |
| ConfigJson | string | JSON configuration (colors, logo URL, etc.) | Required (Default empty object) |
| CreatedAt | DateTime | Creation timestamp | Required |

### GenerationRequest
Tracks the lifecycle of a specific PDF generation task.

| Field | Type | Description | Constraints |
|-------|------|-------------|-------------|
| Id | Guid | Primary Key | Required |
| ReferenceId | string | The ID of the source document (e.g., InvoiceId) | Required, Max 100 |
| DocumentType | Enum | Quotation, Invoice, Receipt, Report | Required |
| Status | Enum | Pending, Processing, Completed, Failed | Required |
| StorageUrl | string | Public URL to the generated file in GCS | Optional |
| ErrorMessage | string | Failure details if status is Failed | Optional |
| CreatedAt | DateTime | Request timestamp | Required |
| CompletedAt | DateTime | Completion timestamp | Optional |

## State Transitions (GenerationRequest)
1. **Pending**: Request received (Async path) or initialization (Sync path).
2. **Processing**: Rendering/Uploading in progress.
3. **Completed**: File stored in GCS, URL available.
4. **Failed**: Error occurred during generation or storage.

## Validation Rules
- `PdfTemplate.Code` must be unique across the system.
- `GenerationRequest.ReferenceId` must be provided to link PDFs back to their source business records.
- Input data for generation must match the schema expected by the `LayoutClass`.
