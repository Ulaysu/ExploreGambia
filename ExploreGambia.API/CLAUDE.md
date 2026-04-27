# CLAUDE.md — ExploreGambia API Agent Contract

This file defines **strict implementation rules** for AI coding agents working in this repository.

If a request conflicts with these rules, ask for clarification before coding.

---

## 1) Architecture Rules (Clean Architecture + Layering)

### 1.1 Mandatory layers
All new code **must** follow this layered model:

1. **Presentation (`Controllers/`)**
   - HTTP concerns only: routing, model binding, auth attributes, status codes.
   - No EF Core queries, no business calculations, no persistence logic.
2. **Application (`Services/` or `UseCases/`)**
   - Business orchestration and use-case logic.
   - Depends on abstractions only (interfaces), not concrete infrastructure.
3. **Infrastructure (`Repositories/`, `Data/`)**
   - EF Core, external I/O, repository implementations.
4. **Domain (`Models/Domain/`)**
   - Core entities, enums, domain invariants.

### 1.2 Dependency direction
- Outer layers can depend on inner layers.
- Inner layers must **never** depend on outer layers.
- Controllers must depend on service abstractions, not directly on repositories.

### 1.3 Separation rules
- DTOs are transport contracts only; do not place business rules in DTO classes.
- Repositories should not return DTOs.
- Controllers should not construct domain entities manually except trivial pass-through mapping.

### 1.4 Startup composition
- `Program.cs` is the only composition root.
- All DI registrations belong in startup/extension wiring code.
- Do not instantiate repositories/services with `new` inside controllers.

---

## 2) Naming Conventions

### 2.1 General
- Use **PascalCase** for types, methods, public properties.
- Use **camelCase** for local variables/parameters.
- Use descriptive names; avoid abbreviations except well-known acronyms (`Dto`, `Api`, `Id`).

### 2.2 Types and files
- One public type per file.
- File name must match the type name.
- Interface names must start with `I` (e.g., `ITourService`).

### 2.3 Suffix conventions
- Domain entities: no suffix (`Tour`, `Booking`).
- Request/response contracts: `*RequestDto`, `*ResponseDto`, or resource `*Dto`.
- Repository implementations: `*Repository`.
- Service implementations: `*Service`.
- Custom exceptions: `*Exception`.

### 2.4 Async
- Async methods must end with `Async`.
- Do not mix sync-over-async (`.Result`, `.Wait()`).

---

## 3) API Design Standards (REST + Versioning)

### 3.1 Versioning
- All endpoints must be versioned via URL segment: `api/v{version:apiVersion}/...`.
- New controllers must declare `[ApiVersion("1.0")]` (or requested version).
- Breaking contract changes require a new API version.

### 3.2 REST semantics
- Use nouns for routes (`/tours`, `/bookings/{id}`), not verbs.
- Correct HTTP methods:
  - `GET` read
  - `POST` create
  - `PUT` full update
  - `PATCH` partial update
  - `DELETE` remove
- `POST` create must return `201 Created` with `Location` header (`CreatedAtAction`).
- `DELETE` should return `204 No Content` unless explicit payload is needed.

### 3.3 Response contracts
- Controllers must return DTOs, never EF entities.
- Collections must support pagination inputs (`pageNumber`, `pageSize`) and deterministic sorting.
- Do not return raw strings for errors.

### 3.4 OpenAPI discipline
- Every endpoint should have clear response annotations (`ProducesResponseType`) when added/changed.
- Keep request/response schema stable for existing versions.

---

## 4) Error Handling Strategy

### 4.1 Centralized exception handling
- Use global middleware for unhandled exceptions.
- Controllers/services should throw typed exceptions for business/resource errors.

### 4.2 Standard error shape
All non-success responses should use a consistent JSON envelope:

```json
{
  "errorId": "guid",
  "code": "resource_not_found",
  "message": "Tour with id '...' was not found.",
  "details": []
}
```

### 4.3 Status code mapping
- Validation errors → `400 Bad Request`
- Resource missing → `404 Not Found`
- Auth failures → `401 Unauthorized`
- Forbidden access → `403 Forbidden`
- Conflict/state issues → `409 Conflict`
- Unexpected errors → `500 Internal Server Error`

### 4.4 Logging
- Log every unhandled exception with a correlation/error ID.
- Never leak stack traces, secrets, connection strings, or token values in API responses.

---

## 5) DTO and Mapping Rules

### 5.1 DTO boundaries
- Every externally exposed payload must be a DTO.
- Use separate DTOs for create/update/read when shapes differ.
- Never expose internal fields (e.g., security fields, internal flags) unless explicitly required.

### 5.2 Mapping
- Use AutoMapper profiles for entity↔DTO mapping.
- Keep all mappings in `Mapping/` profile classes.
- Complex computed fields should be mapped explicitly with unit-tested mapping config.

### 5.3 Validation
- DTO input validation must use data annotations and/or FluentValidation (if introduced).
- Business validation belongs in Application layer, not in attributes alone.

---

## 6) Database Practices (EF Core)

### 6.1 Query rules
- Repositories must use async EF APIs (`ToListAsync`, `FirstOrDefaultAsync`, etc.).
- Use `AsNoTracking()` for read-only queries.
- Always apply filtering/sorting/paging server-side before materialization.

### 6.2 Model configuration
- Configure relationships and constraints in `DbContext` fluent API or entity configurations.
- Use explicit precision for money/decimal fields.
- Use proper delete behavior; avoid accidental cascades.

### 6.3 Migrations
- Every schema change must include a migration.
- Migration names must be descriptive (`AddPaymentStatusIndex`).
- Do not edit historical migrations unless absolutely required and approved.

### 6.4 Transactions and consistency
- Multi-entity writes that must be atomic should use explicit transactions.
- Concurrency-sensitive updates should include concurrency handling strategy (row version or equivalent).

---

## 7) Testing Expectations

### 7.1 Minimum required test coverage for new features
- **Unit tests** for Application/Domain logic.
- **Integration tests** for repository behavior against a test database provider.
- **API tests** for controller contracts (status codes + response body).

### 7.2 What must be tested
- Happy paths.
- Validation failures.
- Not-found paths.
- Authorization-sensitive endpoints.
- Pagination/filtering behavior.

### 7.3 Quality gates before completion
Agents should run, at minimum:
1. `dotnet restore`
2. `dotnet build`
3. `dotnet test` (when tests exist)

If tests are absent, agents must explicitly state that and propose where to add them.

---

## 8) Folder Structure Explanation (Current + Target)

Current project centers on `ExploreGambia.API/` with these key areas:
- `Controllers/` — API endpoints and HTTP entry points.
- `Models/Domain/` — core entities.
- `Models/DTOs/` — request/response contracts.
- `Repositories/` — persistence abstractions + implementations.
- `Data/` — EF Core `DbContext` and seeding.
- `Mapping/` — AutoMapper profile definitions.
- `Middleware/` — cross-cutting request pipeline behavior.
- `Exceptions/` — custom exception types.
- `Validations/`, `CustomActionFilters/` — input validation and filter behaviors.

### Target evolution for stricter Clean Architecture
When adding significant new features, prefer this structure:

- `ExploreGambia.API/Controllers`
- `ExploreGambia.Application/Services|UseCases|Contracts`
- `ExploreGambia.Domain/Entities|ValueObjects|Enums`
- `ExploreGambia.Infrastructure/Persistence|Repositories|Identity|External`
- `tests/ExploreGambia.UnitTests`
- `tests/ExploreGambia.IntegrationTests`
- `tests/ExploreGambia.ApiTests`

Do not perform large structural moves unless requested, but all new code should trend toward this target.

---

## 9) Agent Operating Rules (Strict)

1. Do not introduce architectural shortcuts “just to make it work.”
2. Do not bypass mapping by returning domain entities directly from controllers.
3. Do not add hidden behavior in middleware without documenting it.
4. Do not couple controllers directly to EF `DbContext`.
5. Prefer small, reviewable commits with clear intent.
6. If a rule must be broken, document:
   - which rule,
   - why,
   - safer follow-up refactor.

This document is the default contract for AI-generated changes in this repository.
