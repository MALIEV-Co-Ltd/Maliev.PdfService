# Maliev.PdfService Development Guidelines

This document provides essential instructions for AI agents and developers working on the Maliev.PdfService repository.

## 1. Build, Lint, and Test Commands

All commands should be run from the root directory unless specified otherwise.

### Build
- **Build Solution:**
  ```bash
  dotnet build
  ```
- **Clean Build:**
  ```bash
  dotnet clean && dotnet build
  ```

### Testing
This project uses **xUnit** for unit and integration tests.

- **Run All Tests:**
  ```bash
  dotnet test
  ```
- **Run a Single Test:**
  Use the `--filter` argument to run a specific test by its name.
  ```bash
  dotnet test --filter "DisplayName~NameOfYourTest"
  ```
  *Example:* `dotnet test --filter "DisplayName~GeneratePdfAsync_ReturnsPdfBytes"`

- **Run Tests in a Specific File (Class):**
  ```bash
  dotnet test --filter "FullyQualifiedName~Namespace.ClassName"
  ```
  *Example:* `dotnet test --filter "FullyQualifiedName~Maliev.PdfService.Tests.Unit.PdfGeneratorTests"`

### Linting & Formatting
- **Check Formatting:**
  ```bash
  dotnet format --verify-no-changes
  ```
- **Fix Formatting:**
  ```bash
  dotnet format
  ```

## 2. Code Style & Conventions

Adhere strictly to the following C# and .NET standards.

### General formatting
- **Namespaces:** Use **file-scoped namespaces** (C# 10+ feature).
  ```csharp
  // Good
  namespace Maliev.PdfService.Api.Services;

  // Avoid
  namespace Maliev.PdfService.Api.Services { ... }
  ```
- **Braces:** Use **Allman style** (braces on new lines) for all control structures and blocks.
  ```csharp
  if (condition)
  {
      // code
  }
  ```
- **Indentation:** Use 4 spaces. No tabs.

### Naming Conventions
- **Classes/Methods/Properties:** `PascalCase`.
- **Private Fields:** `_camelCase` (underscore prefix).
  ```csharp
  private readonly IPdfGenerator _pdfGenerator;
  ```
- **Interfaces:** Prefix with `I` (e.g., `IPdfGenerator`).
- **Async Methods:** End with `Async` suffix (e.g., `GeneratePdfAsync`).

### Dependencies & Imports
- **Usings:** Place `using` directives at the very top of the file.
- **Cleanup:** Remove unused `using` directives.
- **DI:** Use Constructor Injection. Ensure all services are registered in `Program.cs`.

### Types & Features
- Use `var` when the type is obvious from the right-hand side.
- Use nullable reference types (`string?`) where appropriate.
- Prefer `Task.Run` for CPU-bound work in async methods if blocking the main thread is a concern.

### Error Handling
- Use `try-catch` blocks in Controllers or top-level consumers.
- Log exceptions using `ILogger<T>` before re-throwing or returning error responses.
- Return appropriate HTTP status codes (e.g., 500 for unhandled exceptions, 400 for bad requests).

### Documentation
- Use XML documentation (`///`) for all public classes, interfaces, and methods.
- Describe parameters and return values.

## 3. Architecture & Libraries

- **Framework:** .NET 8 / ASP.NET Core.
- **PDF Generation:** QuestPDF.
- **Messaging:** MassTransit (RabbitMQ/ServiceBus).
- **Data Access:** Entity Framework Core (`PdfDbContext`).
- **Testing:** xUnit, Moq.

## 4. Cursor / Copilot Rules

*(No specific .cursor/rules/ or .github/copilot-instructions.md found. Follow the standard guidelines above.)*

---

## Git & Version Control — Mandatory Rules

### 🚨 CRITICAL: Always Commit Code Changes (Non-Negotiable)
- **You MUST commit your changes to the local repository after completing any meaningful unit of work.**
- **Never accumulate uncommitted changes.** Do not wait until end of session or until something breaks.
- **Commit early and often** — if a change is meaningful (even a small fix or refactor), commit it.
- **You do NOT need to push to remote** — local commits are sufficient to protect against accidental loss.
- **If you are unsure whether to commit, commit anyway.** Extra commits are harmless; lost work is irreversible.
- This rule applies even if you are just "testing" or "exploring" — use git branches to isolate experimental work and commit those changes too.

### 🚨 CRITICAL: Never Use `git checkout` to Restore Broken Files
- **NEVER use `git checkout` to restore or recover files.** This operation discards uncommitted changes permanently and will result in data loss.
- **To undo/recover from broken files: first commit your current changes, then use `git revert` or `git reset --soft` to safely undo.
