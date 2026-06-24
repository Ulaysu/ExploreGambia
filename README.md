# ExploreGambia API

ExploreGambia API is an ASP.NET Core backend for a tour booking platform. It manages tours, tour guides, bookings, authentication, and payments through Stripe and ModemPay.

The API is built with .NET 8, Entity Framework Core, PostgreSQL, ASP.NET Core Identity, JWT authentication, API versioning, Swagger, AutoMapper, and Serilog.

## Table Of Contents

- [Features](#features)
- [Architecture](#architecture)
- [Tech Stack](#tech-stack)
- [Project Structure](#project-structure)
- [Configuration](#configuration)
- [Local Setup](#local-setup)
- [Authentication](#authentication)
- [API Versioning](#api-versioning)
- [Core Workflows](#core-workflows)
- [Stripe Webhook Testing](#stripe-webhook-testing)
- [API Endpoints](#api-endpoints)
- [Data Model](#data-model)
- [Validation And Errors](#validation-and-errors)
- [Logging](#logging)
- [Deployment Notes](#deployment-notes)

## Features

- User registration, login, refresh tokens, and current-user profile lookup.
- Role support through ASP.NET Core Identity roles: `User`, `Admin`, and `Guide`.
- Tour guide management, including authenticated guide profile lookup and profile updates.
- Tour catalogue management with filtering, sorting, pagination, guide-owned tour views, availability updates, participant lists, and soft-delete/restore support for admins.
- Booking creation with duplicate active-booking prevention.
- Booking status lifecycle: `Pending`, `Confirmed`, `Canceled`, `Completed`.
- Payment lifecycle: `Pending`, `Processing`, `Succeeded`, `Failed`, `Canceled`.
- Stripe Checkout session creation.
- Stripe webhook confirmation for successful checkout payments.
- ModemPay inline/card payment preparation, verification, intent creation, and webhook handling.
- Admin dashboard, user status management, all-booking/all-payment views, payment summaries, and admin tour restore workflows.
- Cloudinary-backed media upload endpoint for tour images and other image assets.
- Swagger/OpenAPI documentation.
- PostgreSQL connection support for local and Railway-style deployments.
- Serilog console and rolling file logs.

## Architecture

The project follows a controller-service-repository structure:

- Controllers expose versioned HTTP endpoints.
- Services hold business workflows such as booking creation, payment confirmation, provider verification, and auth.
- Repositories handle data access through Entity Framework Core.
- Domain models represent persisted entities.
- DTOs define request and response payloads.
- AutoMapper maps between DTOs and domain models.
- `GlobalExceptionHandler` centralizes exception responses.

Two EF Core DbContexts are used:

- `ExploreGambiaDbContext`: tours, tour guides, bookings, and payments.
- `ExploreGambiaAuthDbContext`: Identity users, roles, and data protection keys.

Both contexts use the same PostgreSQL connection string but separate migration history tables:

- `__EFMigrationsHistory_App`
- `__EFMigrationsHistory_Auth`

## Tech Stack

- .NET 8
- ASP.NET Core Web API
- Entity Framework Core
- PostgreSQL with `Npgsql.EntityFrameworkCore.PostgreSQL`
- ASP.NET Core Identity
- JWT Bearer authentication
- Asp.Versioning
- AutoMapper
- Stripe.net
- ModemPay HTTP client integration
- CloudinaryDotNet
- Serilog
- Swagger / Swashbuckle

## Project Structure

```text
ExploreGambia.API/
  Controllers/          HTTP API controllers, including auth, admin, tours, guides, bookings, payments, and media
  CustomActionFilters/  Model validation filter
  Data/                 EF Core DbContexts and seeders
  Exceptions/           Domain-specific exceptions
  Mapping/              AutoMapper profiles
  Middleware/           Global exception handler
  Migrations/           EF Core migrations
  Models/
    Configurations/     Options classes
    Domain/             Entity models and enums
    DTOs/               Request and response DTOs
  Repositories/         Data access contracts and implementations
  Services/             Business logic, auth services, and Cloudinary media uploads
  Services/Payments/    Stripe and ModemPay integrations
  Validations/           Custom validation attributes
```

## Configuration

The API reads sensitive configuration primarily from environment variables. Some values may also be supplied through `appsettings.json` where supported by `Program.cs`.

### Required Environment Variables

| Variable | Purpose |
| --- | --- |
| `JWT_SECRET` | Signing key for JWT access tokens. |
| `STRIPE_SECRET_KEY` | Stripe secret API key. Required at startup. |
| `ConnectionStrings__DefaultConnection` | PostgreSQL connection string. |
| `CLOUDINARY_CLOUD_NAME` | Cloudinary cloud name used by media uploads. |
| `CLOUDINARY_API_KEY` | Cloudinary API key used by media uploads. |
| `CLOUDINARY_API_SECRET` | Cloudinary API secret used by media uploads. |

Instead of `ConnectionStrings__DefaultConnection`, the app can resolve PostgreSQL settings from:

- `DATABASE_URL`
- or `PGHOST`, `PGPORT`, `PGDATABASE`, `PGUSER`, `PGPASSWORD`

### Stripe Variables

| Variable | Purpose |
| --- | --- |
| `STRIPE_SECRET_KEY` | Stripe server-side API key. |
| `STRIPE_PUBLISHABLE_KEY` | Stripe publishable key for frontend usage. |
| `STRIPE_SUCCESS_URL` | Default Stripe Checkout success redirect URL. |
| `STRIPE_CANCEL_URL` | Default Stripe Checkout cancel redirect URL. |
| `STRIPE_WEBHOOK_SECRET` | Stripe webhook signing secret used to verify webhook requests. |

### ModemPay Variables

| Variable | Default | Purpose |
| --- | --- | --- |
| `MODEMPAY_PUBLIC_KEY` | empty | Public key used for inline payment setup. |
| `MODEMPAY_SECRET_KEY` | empty | Secret key used for provider API calls. |
| `MODEMPAY_WEBHOOK_SECRET` | empty | Webhook signature secret. |
| `MODEMPAY_CURRENCY` | `GMD` | Payment currency. |
| `MODEMPAY_BASE_URL` | `https://api.modempay.com` | Provider API base URL. |
| `MODEMPAY_TRANSACTION_PATH_TEMPLATE` | `/transactions/{transactionId}` | Transaction lookup path template. |

### Other Runtime Settings

| Variable | Purpose |
| --- | --- |
| `PORT` | If set, the app listens on `http://0.0.0.0:{PORT}`. Useful for hosted environments. |

## Local Setup

1. Clone the repository.

```powershell
git clone <repository-url>
cd ExploreGambia
```

2. Configure required local environment variables.

```powershell
setx JWT_SECRET "your-long-local-jwt-secret"
setx STRIPE_SECRET_KEY "sk_test_..."
setx STRIPE_PUBLISHABLE_KEY "pk_test_..."
setx STRIPE_SUCCESS_URL "http://localhost:5173/payment/success"
setx STRIPE_CANCEL_URL "http://localhost:5173/payment/cancel"
setx CLOUDINARY_CLOUD_NAME "your-cloudinary-cloud-name"
setx CLOUDINARY_API_KEY "your-cloudinary-api-key"
setx CLOUDINARY_API_SECRET "your-cloudinary-api-secret"
setx ConnectionStrings__DefaultConnection "Host=localhost;Port=5432;Database=ExploreGambia;Username=postgres;Password=your_password"
```

Restart Visual Studio, IIS Express, or the terminal after using `setx`.

3. Restore packages.

```powershell
dotnet restore
```

4. Apply EF Core migrations.

```powershell
dotnet ef database update --project ExploreGambia.API
```

5. Run the API.

```powershell
dotnet run --project ExploreGambia.API
```

6. Open Swagger.

```text
https://localhost:44331/swagger
```

The exact HTTPS port can vary by launch profile. Check `ExploreGambia.API/Properties/launchSettings.json` if your local port differs.

## Authentication

Authentication uses ASP.NET Core Identity plus JWT Bearer tokens.

Main auth endpoints:

- `POST /api/v1/auth/register`
- `POST /api/v1/auth/login`
- `POST /api/v1/auth/refresh-token`
- `GET /api/v1/auth/me`
- `PUT /api/v1/auth/me`

Include the JWT access token on protected requests:

```http
Authorization: Bearer <access-token>
```

Seeded roles:

- `User`
- `Admin`
- `Guide`

Password policy is intentionally relaxed for development and MVP usage:

- minimum length: `6`
- digit not required
- lowercase not required
- uppercase not required
- non-alphanumeric not required

## API Versioning

The API uses URL versioning as the primary route shape:

```text
/api/v1/...
```

Versioning can also be read from:

- `x-api-version` header
- `api-version` query string

Default version: `1.0`.

## Core Workflows

### Booking Flow

```text
User authenticates
  -> User creates booking for a tour
  -> API validates tour availability and participant count
  -> API prevents duplicate active booking for the same user and tour
  -> Booking starts as Pending
```

Booking totals are calculated server-side:

```text
booking.TotalAmount = tour.Price * numberOfPeople
```

### Stripe Payment Flow

```text
User creates booking
  -> Frontend requests Stripe Checkout session
  -> API creates local Payment with Pending status
  -> API creates Stripe Checkout Session
  -> API stores session.Id in payment.ProviderReference
  -> User pays on Stripe Checkout
  -> Stripe sends webhook to API
  -> API verifies Stripe signature
  -> API handles checkout.session.completed
  -> API finds Payment by ProviderReference
  -> API marks Payment as Succeeded
  -> API marks Booking as Confirmed
```

Important: the frontend success redirect is not trusted for payment confirmation. The Stripe webhook is the source of truth.

### ModemPay Flow

The API supports ModemPay through:

- inline payment preparation
- payment verification
- payment intent creation
- signed webhook processing

ModemPay successful transactions use the same provider-confirmation workflow that updates payment and booking state.

## Stripe Webhook Testing

Stripe webhooks are required for reliable payment confirmation. Users can close the browser, lose internet, or manually visit success URLs, so redirect pages must not mark bookings as confirmed.

Install and sign in to the Stripe CLI, then forward events to the local API:

```powershell
stripe listen --forward-to https://localhost:44331/api/v1/payments/stripe/webhook
```

The Stripe CLI prints a webhook signing secret:

```text
Ready! Your webhook signing secret is:
whsec_xxxxxxxxx
```

Set it locally:

```powershell
setx STRIPE_WEBHOOK_SECRET "whsec_xxxxxxxxx"
```

Restart Visual Studio, IIS Express, or the terminal after setting the environment variable.

Then create a Stripe Checkout payment and use the test card:

```text
4242 4242 4242 4242
```

Expected result:

- Stripe CLI receives `checkout.session.completed`.
- API returns `200 OK` from `/api/v1/payments/stripe/webhook`.
- Payment moves from `Pending` to `Succeeded`.
- Booking moves from `Pending` to `Confirmed`.

## API Endpoints

All endpoint paths below use version `v1`.


### Admin

| Method | Path | Auth | Purpose |
| --- | --- | --- | --- |
| `GET` | `/api/v1/admin/dashboard` | `Admin` | Get total users, guides, tours, bookings, and revenue. |
| `GET` | `/api/v1/admin/users` | `Admin` | List all users. |
| `GET` | `/api/v1/admin/users/{id}` | `Admin` | Get one user. |
| `PUT` | `/api/v1/admin/users/{id}/status` | `Admin` | Activate or deactivate a user account. |
| `GET` | `/api/v1/admin/tours` | `Admin` | List all tours for administration, including deleted tours where repository support applies. |
| `PATCH` | `/api/v1/admin/tours/{id}/delete` | `Admin` | Soft-delete a tour. |
| `PATCH` | `/api/v1/admin/tours/{id}/restore` | `Admin` | Restore a soft-deleted tour. |
| `GET` | `/api/v1/admin/bookings` | Currently public in controller | List all bookings with filters and pagination. |
| `GET` | `/api/v1/admin/payments` | Currently public in controller | List all payments with filters and pagination. |
| `GET` | `/api/v1/admin/payments/summary` | Currently public in controller | Get payment totals and summary values. |

### Auth

| Method | Path | Auth | Purpose |
| --- | --- | --- | --- |
| `POST` | `/api/v1/auth/register` | Public | Register a user. |
| `POST` | `/api/v1/auth/login` | Public | Login and receive tokens. |
| `POST` | `/api/v1/auth/refresh-token` | Public | Refresh auth tokens. |
| `GET` | `/api/v1/auth/me` | Bearer token | Get current authenticated user profile. |
| `PUT` | `/api/v1/auth/me` | Bearer token | Update the current user's first and last name. |

### Tours

| Method | Path | Purpose |
| --- | --- | --- |
| `GET` | `/api/v1/tours` | List tours with filters, sorting, and pagination. |
| `GET` | `/api/v1/tours/{id}` | Get one tour. |
| `POST` | `/api/v1/tours` | Create a tour for the authenticated guide (`Admin`, `Guide`). |
| `PUT` | `/api/v1/tours/{id}` | Update a guide-owned tour. |
| `PATCH` | `/api/v1/tours/{id}/availability` | Update availability for a guide-owned tour (`Guide`). |
| `GET` | `/api/v1/tours/{tourId}/participants` | Get participants for a guide-owned tour (`Guide`). |
| `GET` | `/api/v1/tours/my` | List tours owned by the current guide (`Guide`). |
| `GET` | `/api/v1/tours/my/{id}` | Get one current-guide-owned tour (`Guide`). |
| `DELETE` | `/api/v1/tours/{id}` | Delete a guide-owned tour (`Guide`). |

Supported tour query parameters include:

- `sortBy`
- `isAscending`
- `location`
- `minPrice`
- `maxPrice`
- `startDate`
- `endDate`
- `pageNumber`
- `pageSize`

### Tour Guides

| Method | Path | Purpose |
| --- | --- | --- |
| `GET` | `/api/v1/tour-guides` | List tour guides with search, sorting, and pagination. |
| `GET` | `/api/v1/tour-guides/{id}` | Get one tour guide. |
| `POST` | `/api/v1/tour-guides` | Create a tour guide. |
| `GET` | `/api/v1/tour-guides/me` | Get the current guide profile (`Guide`). |
| `PUT` | `/api/v1/tour-guides/me` | Update the current guide profile (`Guide`). |
| `PUT` | `/api/v1/tour-guides/{id}` | Update a tour guide. |
| `DELETE` | `/api/v1/tour-guides/{id}` | Delete a tour guide. |

Supported guide query parameters include:

- `sortBy`
- `isAscending`
- `searchTerm`
- `pageNumber`
- `pageSize`

### Bookings

Most booking endpoints require a bearer token.

| Method | Path | Auth | Purpose |
| --- | --- | --- | --- |
| `GET` | `/api/v1/bookings/my-bookings` | `User`, `Admin` | Get current user's bookings. |
| `GET` | `/api/v1/bookings` | `User`, `Admin` | List bookings with filters and pagination. |
| `GET` | `/api/v1/bookings/{id}` | `User`, `Admin` | Get one booking. |
| `POST` | `/api/v1/bookings` | `Admin`, `User` | Create a booking. |
| `PUT` | `/api/v1/bookings/{id}` | `User` | Update a booking. |
| `DELETE` | `/api/v1/bookings/{id}` | `Admin` | Delete a booking. |

Supported booking query parameters include:

- `status`
- `bookingDateFrom`
- `bookingDateTo`
- `sortBy`
- `isAscending`
- `pageNumber`
- `pageSize`

### Payments

| Method | Path | Auth | Purpose |
| --- | --- | --- | --- |
| `GET` | `/api/v1/payments` | Currently public in controller | List payments with filters and pagination. |
| `GET` | `/api/v1/payments/{id}` | Currently public in controller | Get one payment. |
| `POST` | `/api/v1/payments` | Currently public in controller | Create a manual payment record. |
| `PUT` | `/api/v1/payments/{id}` | Currently public in controller | Update a payment. |
| `POST` | `/api/v1/payments/{id}/confirm` | Currently public in controller | Confirm a payment manually. |
| `DELETE` | `/api/v1/payments/{id}` | Currently public in controller | Delete a payment. |
| `POST` | `/api/v1/payments/bookings/{bookingId}/stripe/checkout` | `User`, `Admin` | Create Stripe Checkout session. |
| `POST` | `/api/v1/payments/stripe/webhook` | Public webhook | Receive and verify Stripe webhook events. |
| `POST` | `/api/v1/payments/bookings/{bookingId}/modempay/inline` | `User`, `Admin` | Prepare ModemPay inline payment. |
| `POST` | `/api/v1/payments/modempay/verify` | `User`, `Admin` | Verify ModemPay transaction. |
| `POST` | `/api/v1/payments/bookings/{bookingId}/modempay/intent` | `User`, `Admin` | Create ModemPay payment intent. |
| `POST` | `/api/v1/payments/modempay/webhook` | Public webhook | Receive and verify ModemPay webhook events. |

Supported payment query parameters include:

- `paymentMethod`
- `paymentDateFrom`
- `paymentDateTo`
- `status`
- `sortBy`
- `isAscending`
- `pageNumber`
- `pageSize`


### Media

| Method | Path | Auth | Purpose |
| --- | --- | --- | --- |
| `POST` | `/api/v1/media/upload` | Currently public in controller | Upload an image to Cloudinary and return its secure URL. |

The upload service stores files in the `explore-gambia` Cloudinary folder.

## Data Model

### Tour

Represents a bookable tour.

Key fields:

- `TourId`
- `Title`
- `Description`
- `Location`
- `Price`
- `MaxParticipants`
- `StartDate`
- `EndDate`
- `ImageUrl`
- `IsAvailable`
- `TourGuideId`
- `IsDeleted`

Relationships:

- many tours belong to one tour guide
- one tour can have many bookings
- soft-deleted tours are marked with `IsDeleted` instead of being removed by admin delete/restore workflows

### TourGuide

Represents a guide who can lead tours.

Key fields:

- `TourGuideId`
- `FullName`
- `PhoneNumber`
- `Email`
- `Bio`
- `IsAvailable`

Relationship:

- one tour guide can have many tours

### Booking

Represents a user's tour reservation.

Key fields:

- `BookingId`
- `TourId`
- `UserId`
- `BookingDate`
- `NumberOfPeople`
- `TotalAmount`
- `Status`
- `StatusUpdatedAt`

Statuses:

- `Pending`
- `Confirmed`
- `Canceled`
- `Completed`

Relationships:

- one booking belongs to one tour
- one booking can have many payment attempts

### Payment

Represents a payment attempt or confirmed payment for a booking.

Key fields:

- `PaymentId`
- `BookingId`
- `PaymentMethod`
- `Amount`
- `PaymentDate`
- `Status`
- `ProviderReference`

Statuses:

- `Pending`
- `Processing`
- `Succeeded`
- `Failed`
- `Canceled`

`ProviderReference` stores provider identifiers such as Stripe Checkout Session IDs or ModemPay transaction references.

### ApplicationUser

Identity user model used by ASP.NET Core Identity. Bookings store `UserId` as the Identity user id.

## Validation And Errors

The API uses:

- data annotation validation on DTOs
- a global `ValidateModelAttribute`
- domain exceptions such as `BookingNotFoundException`, `TourNotFoundException`, `PaymentNotFoundException`, and `BusinessRuleException`
- `GlobalExceptionHandler` for consistent error responses

Business rules include:

- tours can only be updated, deleted, or have availability/participants viewed by the guide who owns them
- bookings cannot be created for unavailable tours
- booking participants cannot exceed a tour's maximum participants
- users cannot create duplicate active bookings for the same tour
- payment amount must match the booking total
- canceled or completed bookings cannot accept payments

## Logging

Serilog logs to:

- console
- `Logs/ExploreGambia_Log.txt`

File logs roll by minute according to the current configuration.

## Deployment Notes

- Configure `JWT_SECRET`, `STRIPE_SECRET_KEY`, Cloudinary credentials, and PostgreSQL connection settings before startup.
- In production, the app rejects database connection strings that point to `localhost` or `127.0.0.1`.
- Railway-style `DATABASE_URL` is supported and converted to an Npgsql connection string.
- Swagger is enabled in both Development and Production in the current configuration.
- CORS currently allows:
  - `http://localhost:5173`
  - `https://eg-frontend-pi.vercel.app`
- Stripe webhook endpoints must use a real `STRIPE_WEBHOOK_SECRET`; do not trust frontend redirects for payment confirmation.
