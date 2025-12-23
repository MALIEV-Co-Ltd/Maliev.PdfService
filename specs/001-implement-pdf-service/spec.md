# Feature Specification: PDF Generation Service

**Feature Branch**: `001-implement-pdf-service`  
**Created**: 2025-12-23  
**Status**: Draft  
**Input**: User description: "Implement PDF Service with QuestPDF"

## Clarifications
### Session 2025-12-23
- Q: Where should generated PDFs be stored? → A: Google Cloud Storage
- Q: How should the service return the generated PDF? → A: Public URL (Signed or Static)
- Q: What is the retention policy for generated files? → A: Persistent (No automatic deletion)
- Q: How are document numbers (e.g., INV-001) handled? → A: Received in Payload (Source service owns number)
- Q: What is the primary authorization model for generation? → A: System-to-System (Service Account) + Admin

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Generate On-Demand Documents (Priority: P1)

As a business user or integrated service, I want to generate a printable PDF document (e.g., Invoice, Quotation) instantly by providing raw data (including document numbers), so that I can provide professional documents to customers.

**Why this priority**: Core functionality of the service. Without on-demand generation, the service has no value.

**Independent Test**: Can be tested by sending a document request with data and receiving a valid, readable PDF file containing that data.

**Acceptance Scenarios**:

1. **Given** a valid set of invoice data with a specific document number, **When** I request an on-demand PDF generation, **Then** I should receive a Public URL that points to a visually correct PDF file containing that specific document number.
2. **Given** a request for a non-existent document template, **When** I request generation, **Then** I should receive a clear error message.

---

### User Story 2 - Automated Background Generation (Priority: P2)

As a system component (like InvoiceService), I want to trigger PDF generation automatically when a business event occurs (e.g., an invoice is finalized), so that documents are prepared without manual intervention using the document number provided in the event.

**Why this priority**: Enables seamless workflow integration across the Maliev ecosystem.

**Independent Test**: Can be tested by publishing a message to the service bus and verifying that a corresponding PDF record is created and stored.

**Acceptance Scenarios**:

1. **Given** a finalized invoice event (containing a document number) is published to the bus, **When** the PDF service consumes it, **Then** it should generate the PDF and store it in Google Cloud Storage, making a Public URL available for later retrieval.

---

### User Story 3 - Multi-Language Support (Thai/English) (Priority: P3)

As a Thai-based business, I want my documents to support both Thai and English text correctly, so that I can serve local and international customers.

**Why this priority**: Critical for regional compliance and usability in the Thai market.

**Independent Test**: Can be tested by generating a document containing Thai characters and verifying they are rendered correctly (no broken glyphs).

**Acceptance Scenarios**:

1. **Given** a document request containing Thai text, **When** the PDF is generated, **Then** the Thai characters must be rendered with a readable, professional font.

---

### Edge Cases

- **Data Volume**: Handling extremely long tables (e.g., an invoice with 100+ items) with proper pagination.
- **Missing Data**: Ensuring the document layout doesn't break if optional fields (like a secondary phone number) are missing.
- **Concurrency**: Handling multiple simultaneous generation requests without performance degradation.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST support the generation of Quotations, Invoices, Receipts, and Financial Reports.
- **FR-002**: System MUST correctly render Thai and English characters within the same document.
- **FR-003**: System MUST provide a list of available document templates.
- **FR-004**: System MUST support synchronous generation (immediate file return) and asynchronous generation (background processing).
- **FR-005**: System MUST allow configurable visual settings (e.g., logos, primary colors) per template.
- **FR-006**: System MUST log all generation requests for audit purposes.
- **FR-007**: System MUST strictly control access to generation endpoints via standard authorization using System-to-System (Service Account) and Admin roles.
- **FR-008**: System MUST store generated PDFs in Google Cloud Storage and provide Public URLs for retrieval.
- **FR-009**: System MUST ensure stored files are persistent and not automatically deleted.
- **FR-010**: System MUST use document identifiers (numbers) provided in the input payload rather than generating its own.

### Key Entities *(include if feature involves data)*

- **PdfTemplate**: Represents a document layout and its default settings (e.g., "Standard Invoice v1").
- **GenerationRequest**: Represents a specific task to create a PDF, tracking its status and the generated Public URL.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Documents are generated and returned within 2 seconds for standard-sized requests (up to 10 pages or 100 line items) in sync mode.
- **SC-002**: 100% of Thai characters are rendered correctly without missing glyphs.
- **SC-003**: Service handles up to 50 simultaneous active rendering sessions without increasing latency by more than 20%.
- **SC-004**: System uptime for PDF generation availability is at least 99.9%.
