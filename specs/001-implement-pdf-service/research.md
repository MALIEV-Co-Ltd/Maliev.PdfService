# Research: PDF Generation Service

## R-001: QuestPDF .NET 10 Compatibility
- **Decision**: Target .NET 10 with QuestPDF 2025.1+.
- **Rationale**: .NET 10 introduces runtime optimizations that benefit memory-intensive PDF generation. QuestPDF's reliance on SkiaSharp is fully compatible with .NET 10.
- **Alternatives considered**: Staying on .NET 9. Rejected due to the explicit requirement for .NET 10.

## R-002: Google Cloud Storage (GCS) Signed URLs
- **Decision**: Use `Google.Cloud.Storage.V1` library. Implement Signed URLs with V4 signature.
- **Rationale**: Signed URLs allow the service to return a direct, time-limited or permanent link to the GCS object without proxying the binary data through the API.
- **Alternatives considered**: Returning raw byte streams. Rejected to reduce API load and improve client-side caching.

## R-003: QuestPDF Companion & Aspire Networking
- **Decision**: Configure the Companion to listen on `0.0.0.0:12345` and ensure the Aspire `AppHost` exposes this port for the `PdfService` resource.
- **Rationale**: The Companion requires a TCP connection from the application. In a containerized Aspire environment, this requires explicit port mapping and networking configuration.
- **Alternatives considered**: Disabling Companion in Docker. Rejected as it blocks the "ShowInCompanion" feature required for rapid template development.

## R-004: Thai Font Registration (Kanit)
- **Decision**: Embed Kanit-Regular.ttf and Kanit-Bold.ttf as `EmbeddedResource` in the Data or Api project. Use `FontManager.RegisterFont` in `Program.cs`.
- **Rationale**: Ensuring consistent Thai rendering across Linux containers and local Windows dev environments requires bundled fonts. Kanit is the modern standard for Thai web/mobile apps.
- **Alternatives considered**: System fonts. Rejected as Docker images (Alpine/Debian) lack professional Thai fonts by default.
