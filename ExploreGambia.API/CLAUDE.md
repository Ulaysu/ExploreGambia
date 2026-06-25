# CLAUDE.md — ExploreGambia API Agent Contract

This file defines rules for AI coding agents working in this repository.

It intentionally describes the repository **as it exists today** and distinguishes current requirements from future direction. Do not pretend the target architecture already exists.

If a user request conflicts with a **MUST FOLLOW** rule, ask for clarification before coding. If you must preserve a **LEGACY PATTERN** while making a narrow change, document the reason and the safer follow-up refactor.

---

## Architecture Decisions

The following decisions are intentional:

- Repository pattern is used for persistence access.
- AutoMapper is the standard mapping mechanism.
- API Versioning uses URL segments.
- Identity is separated into ExploreGambiaAuthDbContext.
- Payments are persisted before provider workflows begin.


## Forbidden New Architectural Patterns

Unless explicitly requested:

Do not introduce:

- MediatR
- CQRS
- Event Sourcing
- Generic Repository abstractions
- Unit of Work abstractions
- Vertical Slice Architecture
- Domain Events

New features should follow existing repository and service patterns.

## New Feature Workflow

Before implementation:

1. Find an existing similar feature.
2. Reuse the same patterns.
3. Reuse DTO conventions.
4. Reuse authorization conventions.
5. Reuse validation conventions.

Do not invent a new pattern if an existing one already solves the problem.

## 0) Rule Labels

Every rule in this document uses one of these labels:

- **MUST FOLLOW** — Required for new or changed code unless the user explicitly approves an exception.
- **SHOULD FOLLOW** — Strongly preferred. Existing legacy constraints may justify a local exception, but do not expand the exception.
- **LEGACY PATTERN** — A pattern that exists in the current codebase. Preserve only when necessary for narrow compatibility; do not copy into new code.

---

## 1) Current Repository Architecture

### 1.1 Current shape

- **MUST FOLLOW:** This is currently a single-project ASP.NET Core API centered on `ExploreGambia.API/`.
- **MUST FOLLOW:** Do not rewrite the repository into a multi-project solution unless the user explicitly asks for that migration.
- **MUST FOLLOW:** Current key folders are:
  - `Controllers/` — HTTP endpoints and API entry points.
  - `Services/` — business orchestration and provider workflows where they already exist.
  - `Repositories/` — persistence abstractions and EF Core repository implementations.
  - `Data/` — EF Core `DbContext`, Identity context, design-time factories, and seeding.
  - `Models/Domain/` — current entity classes and enums.
  - `Models/DTOs/` — request/response contracts.
  - `Mapping/` — AutoMapper profile definitions.
  - `Middleware/` — cross-cutting request pipeline behavior.
  - `Exceptions/` — custom exceptions.
  - `Validations/` and `CustomActionFilters/` — input validation helpers and filters.

### 1.2 Current layering reality

- **MUST FOLLOW:** Controllers must not query EF Core directly.
- **MUST FOLLOW:** Controllers must not instantiate repositories or services with `new`; use dependency injection.
- **MUST FOLLOW:** `Program.cs` is the current composition root. DI registrations, middleware wiring, authentication configuration, payment provider configuration, EF Core configuration, and startup seeding belong there or in startup extension methods called from there.
- **SHOULD FOLLOW:** New business orchestration should be added to `Services/` behind service interfaces.
- **SHOULD FOLLOW:** New controllers should depend on service abstractions rather than repository abstractions.
- **LEGACY PATTERN:** Several existing controllers inject repositories directly. This is current reality, not the preferred pattern for new code.
- **LEGACY PATTERN:** Some existing controllers contain ownership checks and small business decisions. New or substantially changed ownership/business logic should move toward services or authorization policies.
- **LEGACY PATTERN:** `PaymentService` directly depends on `ExploreGambiaDbContext` for explicit transactions. This is acceptable until a transaction/unit-of-work abstraction exists; do not introduce additional direct service-to-DbContext dependencies unless needed for atomic multi-entity writes.

### 1.3 Target direction without restructuring

- **SHOULD FOLLOW:** Trend toward stricter separation inside the existing project:
  - Controllers: HTTP concerns only.
  - Services: use-case orchestration, business validation, ownership decisions, provider workflows.
  - Repositories/Data: EF Core persistence and database details.
  - Domain models: core state and enums.
- **SHOULD FOLLOW:** Significant new features should be designed so they can later move into separate Application/Domain/Infrastructure projects, but do not perform that move unless requested.

---

## 2) Dependency and Layering Rules

### 2.1 Controllers

- **MUST FOLLOW:** Controllers handle routing, model binding, authorization attributes, status codes, and response shaping.
- **MUST FOLLOW:** Controllers must not contain EF Core queries.
- **MUST FOLLOW:** Controllers must not return EF/domain entities directly as response bodies.
- **SHOULD FOLLOW:** Controllers should delegate business decisions to services.
- **SHOULD FOLLOW:** Controllers should use AutoMapper or service-returned DTOs for response mapping.
- **LEGACY PATTERN:** Existing direct repository usage in controllers may be preserved for narrow edits, but do not introduce new direct repository dependencies in a new controller if a service abstraction is appropriate.

### 2.2 Services

- **MUST FOLLOW:** Business validation belongs in services or domain/application logic, not only in DTO attributes.
- **MUST FOLLOW:** Payment and booking state transitions must be implemented in services, not controllers.
- **SHOULD FOLLOW:** Services should depend on repository/service/provider abstractions.
- **SHOULD FOLLOW:** Services should throw typed exceptions for business and resource errors.
- **LEGACY PATTERN:** Direct `DbContext` usage in `PaymentService` exists for transaction boundaries. Prefer a future transaction abstraction rather than spreading this pattern.

### 2.3 Repositories and data access

- **MUST FOLLOW:** Repositories contain EF Core persistence logic.
- **MUST FOLLOW:** Repositories must use async EF Core APIs such as `ToListAsync`, `FirstOrDefaultAsync`, `CountAsync`, and `SaveChangesAsync`.
- **MUST FOLLOW:** Server-side filtering, sorting, and paging must be applied before materialization for list endpoints.
- **SHOULD FOLLOW:** Use `AsNoTracking()` for read-only queries.
- **SHOULD FOLLOW:** Repositories should return domain entities or persistence results, not API DTOs.
- **LEGACY PATTERN:** `BookingRepository.GetAllBookingsAsync` currently returns admin booking DTOs. Do not copy this pattern; move DTO projection upward when refactoring that area.

---

## 3) Naming and Style Rules

### 3.1 C# naming

- **MUST FOLLOW:** Use PascalCase for types, methods, and public properties.
- **MUST FOLLOW:** Use camelCase for local variables and parameters.
- **SHOULD FOLLOW:** Use descriptive names; avoid abbreviations except well-known acronyms such as `Dto`, `Api`, and `Id`.

### 3.2 Types and files

- **MUST FOLLOW:** Interface names must start with `I`.
- **MUST FOLLOW:** Repository implementations must end with `Repository`.
- **MUST FOLLOW:** Service implementations must end with `Service`.
- **MUST FOLLOW:** Custom exceptions must end with `Exception`.
- **MUST FOLLOW:** Request/response contracts should use `*RequestDto`, `*ResponseDto`, or resource `*Dto` names.
- **SHOULD FOLLOW:** Prefer one public type per file and file names matching public type names.
- **LEGACY PATTERN:** Some domain files currently colocate entity classes with related enums, such as booking/payment status enums. Do not expand this pattern if adding unrelated public types.

### 3.3 Async

- **MUST FOLLOW:** New async methods must end with `Async`.
- **MUST FOLLOW:** Do not use sync-over-async patterns such as `.Result` or `.Wait()`.
- **LEGACY PATTERN:** Some existing async repository methods do not end with `Async`. Preserve names only for narrow compatibility; prefer `Async` suffix for new methods.

---

## 4) API Design Rules

### 4.1 Versioning

- **MUST FOLLOW:** All endpoints must be versioned with a URL segment: `api/v{version:apiVersion}/...`.
- **MUST FOLLOW:** New controllers must declare `[ApiVersion("1.0")]` unless another version is requested.
- **MUST FOLLOW:** Breaking response/request contract changes require a new API version or explicit approval.

### 4.2 Routes and HTTP semantics

- **MUST FOLLOW:** Use correct HTTP methods:
  - `GET` for reads.
  - `POST` for creates or provider actions that start workflows.
  - `PUT` for full updates.
  - `PATCH` for partial state changes.
  - `DELETE` for removals.
- **MUST FOLLOW:** Create endpoints for addressable resources should return `201 Created` with `CreatedAtAction`.
- **SHOULD FOLLOW:** CRUD routes should use nouns and resource identifiers.
- **SHOULD FOLLOW:** Provider workflow routes may use action-style segments when they model external payment operations, such as checkout, intent creation, verification, or webhooks.
- **SHOULD FOLLOW:** `DELETE` should return `204 No Content` unless a stable response payload is intentionally part of the contract.
- **LEGACY PATTERN:** Some existing delete endpoints return `200 OK` with message objects. Do not copy this into new endpoints without a compatibility reason.

### 4.3 Response contracts

- **MUST FOLLOW:** Successful API responses should return DTOs or explicit response contracts, not EF/domain entities.
- **MUST FOLLOW:** Do not expose security-sensitive internal fields such as password hashes, raw refresh tokens, or provider secrets.
- **SHOULD FOLLOW:** Collection endpoints should support `pageNumber`, `pageSize`, deterministic default sorting, and server-side filtering where applicable.
- **LEGACY PATTERN:** Some current admin list endpoints return unpaged collections. Preserve only for compatibility; add pagination for new list endpoints.

### 4.4 OpenAPI

- **SHOULD FOLLOW:** Add clear `ProducesResponseType` annotations when adding or substantially changing endpoints.
- **SHOULD FOLLOW:** Keep schemas stable for existing API versions.

---

## 5) Authentication Rules

### 5.1 Current authentication model

- **MUST FOLLOW:** Authentication uses ASP.NET Core Identity with `ApplicationUser` as the user entity.
- **MUST FOLLOW:** `ApplicationUser` includes `FirstName`, `LastName`, `IsActive`, `RefreshToken`, `RefreshTokenExpiryTime`, and optional `TourGuide` navigation.
- **MUST FOLLOW:** JWT access tokens must be signed using `JWT_SECRET` and include:
  - `ClaimTypes.NameIdentifier` containing the user id.
  - `ClaimTypes.Email` containing the user email.
  - `ClaimTypes.Role` for each assigned role.
- **MUST FOLLOW:** JWT validation must validate issuer, audience, lifetime, and signing key.
- **MUST FOLLOW:** Access-token lifetime is currently one hour unless intentionally changed.

### 5.2 Refresh token behavior

- **MUST FOLLOW:** Refresh tokens are cryptographically random strings stored on the user record.
- **MUST FOLLOW:** Refresh tokens currently expire after 30 days.
- **MUST FOLLOW:** Refresh token use must rotate the refresh token and extend the expiry.
- **MUST FOLLOW:** Invalid, mismatched, or expired refresh tokens must be rejected.

### 5.3 User activation and registration

- **MUST FOLLOW:** Users with `IsActive == false` must not be allowed to log in.
- **MUST FOLLOW:** Registration creates an Identity user with email as username.
- **MUST FOLLOW:** Registration may assign roles supplied by the request after validation.
- **MUST FOLLOW:** If a registered user receives the `Guide` role, the current behavior is to create a linked `TourGuide` profile when one does not already exist.
- **SHOULD FOLLOW:** Registration side effects, especially role-to-profile creation, should remain in `AuthService` or a dedicated service, not controllers.

---

## 6) Authorization and Ownership Rules

### 6.1 Roles

- **MUST FOLLOW:** Current roles are `User`, `Admin`, and `Guide`.
- **MUST FOLLOW:** New non-public endpoints must explicitly use `[Authorize]`, `[Authorize(Roles = "...")]`, or a documented policy.
- **MUST FOLLOW:** New or changed endpoints must not leave commented-out authorization attributes.
- **MUST FOLLOW:** Public endpoints must be intentionally public. Use `[AllowAnonymous]` for anonymous provider webhooks or auth endpoints where appropriate.
- **LEGACY PATTERN:** Some sensitive current endpoints have commented-out or missing authorization attributes. Treat this as security debt; do not copy it.

### 6.2 Current ownership behavior

- **MUST FOLLOW:** A `User` may create bookings for themselves; booking ownership is stored in `Booking.UserId`.
- **MUST FOLLOW:** A user may pay for or verify payment for only their own booking unless they are an Admin.
- **MUST FOLLOW:** A `Guide` owns tours through the linked `TourGuide.UserId` and `Tour.TourGuideId` relationship.
- **MUST FOLLOW:** Guide-only private tour actions must verify that the current user owns the tour.
- **MUST FOLLOW:** Admin may bypass user booking ownership checks only where the endpoint/service explicitly allows Admin behavior.
- **SHOULD FOLLOW:** Ownership checks should live in services or authorization handlers.
- **LEGACY PATTERN:** Some ownership checks currently live in controllers. Preserve only during narrow edits; move to services when refactoring.

### 6.3 Authorization-sensitive endpoints

- **MUST FOLLOW:** Payment creation/confirmation/admin-style payment management must be protected or intentionally documented as public provider integration.
- **MUST FOLLOW:** Admin dashboard, user management, user activation, payment summaries, and global booking/payment lists should require Admin access when changed or secured.
- **MUST FOLLOW:** Webhook endpoints may be anonymous only because they authenticate using provider signatures.

---

## 7) Payment Workflow Rules

### 7.1 Payment domain model

- **MUST FOLLOW:** Payment records belong to bookings through `BookingId`.
- **MUST FOLLOW:** Payment amounts must use decimal precision suitable for money.
- **MUST FOLLOW:** Current payment statuses are `Pending`, `Processing`, `Succeeded`, `Failed`, and `Canceled`.
- **MUST FOLLOW:** Payment provider references are stored in `Payment.ProviderReference` when available.

### 7.2 Booking/payment invariants

- **MUST FOLLOW:** Payment amount must exactly match the booking total before payment creation or confirmation.
- **MUST FOLLOW:** Bookings with `Canceled` or `Completed` status cannot accept payment.
- **MUST FOLLOW:** A booking already `Confirmed` must not accept another successful payment.
- **MUST FOLLOW:** Confirming payment must also mark the booking as `Confirmed`.
- **MUST FOLLOW:** Payment confirmation must be idempotent when payment is already `Succeeded` and booking is already `Confirmed`.
- **MUST FOLLOW:** Payment confirmation that updates both payment and booking must use an explicit transaction.

### 7.3 Local/manual payment workflow

- **MUST FOLLOW:** Creating a local payment must:
  1. Load the booking.
  2. Verify the booking can accept payment.
  3. Verify amount equals booking total.
  4. Create a `Pending` payment.
- **MUST FOLLOW:** Confirming a local payment must:
  1. Load payment and booking.
  2. Re-check booking/payment invariants.
  3. Mark payment `Succeeded`.
  4. Store the provider/reference value when supplied.
  5. Mark booking `Confirmed`.
  6. Commit atomically.

### 7.4 ModemPay workflow

- **MUST FOLLOW:** ModemPay configuration is provided through `ModemPayOptions` from environment/configuration.
- **MUST FOLLOW:** ModemPay inline/intent creation must enforce booking ownership unless current user is Admin.
- **MUST FOLLOW:** ModemPay inline/intent creation creates or reuses a latest `ModemPayCard` payment when no active successful payment exists.
- **MUST FOLLOW:** ModemPay metadata must include local `paymentId` and `bookingId` when initiating provider workflows.
- **MUST FOLLOW:** ModemPay verification must retrieve the provider transaction through the ModemPay client.
- **MUST FOLLOW:** ModemPay verification/webhook processing must resolve a known local payment from metadata or provider reference.
- **MUST FOLLOW:** ModemPay verification must validate provider status, amount, currency, and ownership when a user context is present.
- **MUST FOLLOW:** ModemPay webhook requests must validate provider signature before processing.
- **MUST FOLLOW:** ModemPay success events confirm payment and booking; failure/cancel/expired events mark non-succeeded payments as failed or canceled.

### 7.5 Stripe workflow

- **MUST FOLLOW:** Stripe configuration is provided through `StripeOptions` and Stripe environment/configuration values.
- **MUST FOLLOW:** Stripe checkout creation must enforce that the current user owns the booking.
- **MUST FOLLOW:** Stripe checkout creation creates a local pending `Stripe` payment before creating the Stripe checkout session.
- **MUST FOLLOW:** Stripe checkout metadata must include local `paymentId`, `bookingId`, and `userId`.
- **MUST FOLLOW:** Stripe session id must be stored as provider reference when available.
- **MUST FOLLOW:** Stripe webhook requests must validate the Stripe signature using the configured webhook secret.
- **MUST FOLLOW:** Only paid `checkout.session.completed` events should confirm local payment.
- **MUST FOLLOW:** Stripe webhook handling must resolve local payment by provider reference before confirming it.

### 7.6 Payment migration guidance

- **SHOULD FOLLOW:** Add uniqueness/indexing for provider references when provider payment idempotency is hardened.
- **SHOULD FOLLOW:** Add a row-version or equivalent concurrency strategy for booking/payment state transitions.
- **SHOULD FOLLOW:** Replace generic exceptions in payment services with typed exceptions.
- **SHOULD FOLLOW:** Consider a transaction abstraction before adding more direct `DbContext` dependencies in services.

---

## 8) Media Upload Rules

### 8.1 Current workflow

- **MUST FOLLOW:** Media upload currently uses `IMediaService` with `CloudinaryMediaService`.
- **MUST FOLLOW:** Cloudinary configuration must include cloud name, API key, and API secret.
- **MUST FOLLOW:** Uploads currently go to the `explore-gambia` Cloudinary folder.
- **MUST FOLLOW:** Upload returns the Cloudinary secure URL.

### 8.2 Required hardening for new media work

- **SHOULD FOLLOW:** Add authorization to media upload endpoints unless a public upload is explicitly required.
- **SHOULD FOLLOW:** Validate file type/MIME type and file extension.
- **SHOULD FOLLOW:** Enforce maximum file size.
- **SHOULD FOLLOW:** Define cleanup behavior for orphaned uploads before adding delete/replace flows.
- **SHOULD FOLLOW:** Prefer stable response DTOs over anonymous upload response objects for new upload contracts.

---

## 9) Soft Delete and Destructive Operations

### 9.1 Current soft-delete behavior

- **MUST FOLLOW:** Tours have an `IsDeleted` flag.
- **MUST FOLLOW:** Public tour listing and public tour-by-id reads must exclude deleted tours.
- **MUST FOLLOW:** Admin can soft-delete and restore tours by toggling `IsDeleted`.
- **MUST FOLLOW:** Admin views may include deleted tours where moderation/restoration requires it.

### 9.2 Current hard-delete behavior

- **LEGACY PATTERN:** Guide tour delete currently hard-deletes the tour after checking that it belongs to the guide and has no active pending/confirmed bookings.
- **LEGACY PATTERN:** Booking delete currently hard-deletes a booking.
- **LEGACY PATTERN:** Payment delete currently hard-deletes a payment.
- **MUST FOLLOW:** Do not introduce new hard-delete behavior for business records without explicitly documenting why soft delete is insufficient.
- **SHOULD FOLLOW:** Prefer extending soft delete consistently for tours and other business records when refactoring destructive operations.

### 9.3 Delete response guidance

- **SHOULD FOLLOW:** New DELETE endpoints should return `204 No Content` unless a response body is intentionally part of the API contract.
- **LEGACY PATTERN:** Existing delete endpoints may return message payloads. Preserve only for compatibility.

---

## 10) DTO, Mapping, and Validation Rules

### 10.1 DTO boundaries

- **MUST FOLLOW:** External request and response payloads must use DTOs or explicit response contracts.
- **MUST FOLLOW:** Do not expose Identity security fields, provider secrets, connection strings, raw refresh tokens, or internal flags unless explicitly required for an admin contract.
- **MUST FOLLOW:** Use separate create/update/read DTOs when shapes differ.
- **SHOULD FOLLOW:** Avoid anonymous response types for stable public contracts.

### 10.2 Mapping

- **MUST FOLLOW:** Use AutoMapper profiles in `Mapping/` for entity-to-DTO and DTO-to-entity mapping where mapping is non-trivial or already follows that pattern.
- **SHOULD FOLLOW:** Keep computed-field mapping explicit.
- **SHOULD FOLLOW:** Add mapping tests when test infrastructure exists and mapping is complex.
- **LEGACY PATTERN:** Some DTO projection currently happens in repositories. Do not add new repository-owned API DTO projections.

### 10.3 Validation

- **MUST FOLLOW:** DTO input validation must use data annotations, custom validation attributes, or FluentValidation if introduced.
- **MUST FOLLOW:** The global validation filter returns a standardized validation error payload for invalid model state.
- **MUST FOLLOW:** Business validation belongs in services or domain/application logic.
- **SHOULD FOLLOW:** Avoid duplicating manual `ModelState` checks in controllers unless endpoint-specific behavior is needed.

---

## 11) Error Handling and Logging

### 11.1 Standard error envelope

- **MUST FOLLOW:** New non-success API responses should use this standard shape:

```json
{
  "errorId": "guid",
  "code": "resource_not_found",
  "message": "Tour with id '...' was not found.",
  "details": []
}
```

- **MUST FOLLOW:** Validation errors should use code `validation_failed` and include field-level details.
- **LEGACY PATTERN:** Some existing controllers return raw strings, empty `NotFound()`, or ad hoc `{ Message = ... }` objects. Do not copy this pattern for new error responses.

### 11.2 Exception handling

- **MUST FOLLOW:** Use global middleware for unhandled exceptions.
- **MUST FOLLOW:** Resource missing errors should use typed not-found exceptions where possible.
- **MUST FOLLOW:** Business/state rule violations should use typed business exceptions where possible.
- **MUST FOLLOW:** Do not leak stack traces, secrets, connection strings, provider secrets, or token values in API responses.
- **SHOULD FOLLOW:** Replace generic `Exception` throws in service code with typed exceptions during refactors.

### 11.3 Status code mapping

- **MUST FOLLOW:** Validation errors map to `400 Bad Request`.
- **MUST FOLLOW:** Resource missing maps to `404 Not Found`.
- **MUST FOLLOW:** Authentication failures map to `401 Unauthorized`.
- **MUST FOLLOW:** Forbidden ownership/role failures map to `403 Forbidden`.
- **MUST FOLLOW:** Business conflict/state issues map to `409 Conflict`.
- **MUST FOLLOW:** Unexpected errors map to `500 Internal Server Error`.

### 11.4 Logging

- **MUST FOLLOW:** Log unhandled exceptions with an error/correlation id.
- **MUST FOLLOW:** Log payment/webhook processing events without sensitive values.
- **SHOULD FOLLOW:** Use structured logging placeholders rather than interpolated strings for new logs.

---

## 12) Database, EF Core, and Migrations

### 12.1 Current database contexts

- **MUST FOLLOW:** The API currently uses two EF Core contexts:
  - `ExploreGambiaDbContext` for app/domain data such as tours, bookings, tour guides, and payments.
  - `ExploreGambiaAuthDbContext` for ASP.NET Core Identity and data protection keys.
- **MUST FOLLOW:** Both contexts currently use PostgreSQL.
- **MUST FOLLOW:** App and auth contexts use separate migrations history tables.

### 12.2 Model configuration

- **MUST FOLLOW:** Configure relationships and delete behavior in `DbContext` fluent API or equivalent entity configurations.
- **MUST FOLLOW:** Money/decimal fields must use explicit precision.
- **SHOULD FOLLOW:** Document intentional cascades, especially payment deletion when bookings are deleted.

### 12.3 Query rules

- **MUST FOLLOW:** Use async EF Core APIs.
- **MUST FOLLOW:** Apply filters, sorting, paging, and limits before materialization.
- **SHOULD FOLLOW:** Clamp page sizes to a safe maximum for public/admin list endpoints.
- **SHOULD FOLLOW:** Use deterministic default ordering for paged collections.
- **SHOULD FOLLOW:** Use `AsNoTracking()` for read-only queries.

### 12.4 Migrations

- **MUST FOLLOW:** Every schema change must include an EF migration for the correct context.
- **MUST FOLLOW:** Migration names must be descriptive.
- **MUST FOLLOW:** Do not edit historical migrations unless explicitly approved.
- **MUST FOLLOW:** Be clear whether a migration belongs to the app context or auth context.

### 12.5 Transactions and consistency

- **MUST FOLLOW:** Multi-entity writes that must be atomic require explicit transactions.
- **SHOULD FOLLOW:** Add concurrency protection, such as row versions or database constraints, for payment/booking state transitions when refactoring.
- **SHOULD FOLLOW:** Use unique constraints or indexes for provider references and one-to-one identity mappings where appropriate.

---

## 13) Testing and Quality Gates

### 13.1 Test reality

- **MUST FOLLOW:** Do not claim test projects exist unless they are present.
- **SHOULD FOLLOW:** When adding significant features, prefer adding tests in a future `tests/` structure rather than restructuring the production app.

### 13.2 Expected tests for new behavior

- **SHOULD FOLLOW:** Unit tests for service/domain logic.
- **SHOULD FOLLOW:** Integration tests for repository/database behavior.
- **SHOULD FOLLOW:** API tests for controller contracts.
- **MUST FOLLOW:** Auth, authorization, ownership, payment confirmation, webhook, and soft-delete changes should include tests when test infrastructure exists.
- **SHOULD FOLLOW:** Pagination/filtering behavior should be tested for list endpoints.

### 13.3 Quality gates before completion

- **MUST FOLLOW:** For code changes, run `dotnet restore` unless an environment limitation prevents it.
- **MUST FOLLOW:** For code changes, run `dotnet build` unless an environment limitation prevents it.
- **MUST FOLLOW:** Run `dotnet test` when test projects exist.
- **MUST FOLLOW:** If tests are absent or cannot run, explicitly state that and explain why.

---

## 14) Legacy Pattern Migration Guidance

### 14.1 General rule

- **MUST FOLLOW:** Do not expand legacy patterns when adding new functionality.
- **SHOULD FOLLOW:** When touching legacy code, make the smallest safe improvement toward the preferred pattern if it does not broaden the task.
- **MUST FOLLOW:** If preserving a legacy pattern, mention it in the final response and identify the safer follow-up refactor.

### 14.2 Controller-to-repository dependencies

- **LEGACY PATTERN:** Existing controllers may directly call repositories.
- **SHOULD FOLLOW:** For substantial changes, introduce or extend a service interface and move orchestration there.
- **SHOULD FOLLOW:** Avoid large controller refactors unless requested.

### 14.3 Repository DTO projection

- **LEGACY PATTERN:** Some repository methods return API DTOs.
- **SHOULD FOLLOW:** New repository methods return domain entities or persistence-specific results.
- **SHOULD FOLLOW:** Move DTO projection to services/controllers/mapping profiles during focused refactors.

### 14.4 Error response inconsistencies

- **LEGACY PATTERN:** Existing controllers may return raw strings or ad hoc objects.
- **SHOULD FOLLOW:** New errors should use the standard envelope or typed exceptions handled by middleware.
- **SHOULD FOLLOW:** Convert legacy direct error returns opportunistically when changing the endpoint.

### 14.5 Authorization gaps

- **LEGACY PATTERN:** Some current endpoints have missing or commented-out authorization attributes.
- **MUST FOLLOW:** Do not create new authorization gaps.
- **SHOULD FOLLOW:** When modifying a sensitive endpoint, add explicit authorization if doing so is within the task scope and does not break expected clients.

### 14.6 Delete behavior

- **LEGACY PATTERN:** Hard deletes exist for tours, bookings, and payments.
- **SHOULD FOLLOW:** Prefer soft delete for business records in new workflows.
- **SHOULD FOLLOW:** Before changing hard delete to soft delete, check API compatibility, admin workflows, provider/payment consequences, and migrations.

---

## 15) Agent Operating Rules

1. **MUST FOLLOW:** Do not introduce architectural shortcuts just to make code work.
2. **MUST FOLLOW:** Do not bypass mapping by returning domain entities directly from controllers.
3. **MUST FOLLOW:** Do not add hidden behavior in middleware without documenting it.
4. **MUST FOLLOW:** Do not couple controllers directly to EF `DbContext`.
5. **MUST FOLLOW:** Do not add new public endpoints without explicit public/protected intent.
6. **MUST FOLLOW:** Do not add or expose secrets in source code, logs, API responses, or docs.
7. **SHOULD FOLLOW:** Prefer small, reviewable commits with clear intent.
8. **MUST FOLLOW:** If a rule must be broken, document:
   - which rule,
   - why,
   - whether it follows a current legacy pattern,
   - safer follow-up refactor.

This document is the default contract for AI-generated changes in this repository.
