# Sports Booking API

A complete **backend-only REST API** for a Sports Field Booking application built with:

- **ASP.NET Core Web API** (.NET 10)
- **C#**
- **SQL Server + Entity Framework Core** (Code First, Migrations)
- **ASP.NET Core Identity** (UserManager, roles, lockout, email confirmation)
- **JWT Authentication** (short-lived access token + refresh tokens)
- **SMTP email** via MailKit (confirm email, reset password)
- **Swagger / OpenAPI** (with Authorize button)
- **FluentValidation**
- **Clean Architecture**

## 📁 Solution Structure

```
SportsBooking.sln
├── src/
│   ├── SportsBooking.Domain/          # Entities, Enums, Exceptions (no dependencies)
│   ├── SportsBooking.Application/     # DTOs, Validators, Interfaces, Options, Services
│   ├── SportsBooking.Infrastructure/  # EF Core, Repositories, JWT, Seed Data, Migrations
│   └── SportsBooking.API/             # Controllers, Middleware, Swagger, DI
└── tests/
    └── SportsBooking.Tests/           # xUnit unit tests
```

**Dependency rules (enforced by architecture tests):**
- `Domain` → no dependencies (no EF Core, no ASP.NET)
- `Application` → depends only on `Domain`
- `Infrastructure` → depends on `Application` + `Domain`
- `API` → depends on `Application` + `Infrastructure`

## 🏛 Main Entities

| Entity | Description |
|---|---|
| `User` | Identity-backed customer accounts (`IdentityUser<int>`) with profile + location + role |
| `RefreshToken` | Hashed refresh tokens used to rotate JWT access tokens |
| `Payment` | Payment records tied to bookings (mock provider by default) |
| `Location` | Geographic area (city, governorate, lat/lon) |
| `Sport` | Football, Padel, Basketball, Tennis, Handball |
| `Field` | Playable field with day/night pricing + images |
| `FieldImage` | Field images (primary + gallery) |
| `FieldAmenity` | Field facilities (floodlights, parking, etc.) |
| `FieldAvailability` | Date-based open/close times |
| `Booking` | Confirmed bookings with concurrency stamp |
| `Review` | 1–5 star ratings tied to completed bookings |
| `Favorite` | User's bookmarked fields (unique per user+field) |

## 🔐 Authentication

Full ASP.NET Core Identity flow:

- **POST** `/api/auth/register` — create account (password hashed by Identity, user is added to the `Customer` role)
- **POST** `/api/auth/login` — obtain access + refresh tokens (email must be confirmed)
- **POST** `/api/auth/refresh` — exchange a valid refresh token for a new token pair (rotation)
- **POST** `/api/auth/logout` — revoke the current refresh token
- **POST** `/api/auth/confirm-email` — confirm email with the token sent at registration
- **POST** `/api/auth/resend-confirmation` — resend the confirmation email
- **POST** `/api/auth/forgot-password` — sends a password-reset email (always 200 for security)
- **POST** `/api/auth/reset-password` — set a new password with the reset token
- **POST** `/api/auth/change-password` — change password while logged in (revokes all refresh tokens)

**Access tokens** are short-lived (`Jwt:AccessTokenMinutes`, default 15). **Refresh tokens** are stored **hashed** in the `RefreshTokens` table (`Jwt:RefreshTokenExpiryDays`, default 7) and are rotated on each use; logging out or changing your password revokes them.

Use the access token via the Swagger **Authorize** button (`Authorization: Bearer {token}`).

> ⚠️ Email confirmation is **required** before login. In development you can call `/api/auth/confirm-email` with the generated token, or configure SMTP so emails are actually sent.

## 📡 API Endpoints

### Auth
| Method | Endpoint | Auth |
|---|---|---|
| POST | `/api/auth/register` | Public |
| POST | `/api/auth/login` | Public |
| POST | `/api/auth/refresh` | Public |
| POST | `/api/auth/logout` | JWT |
| POST | `/api/auth/confirm-email` | Public |
| POST | `/api/auth/resend-confirmation` | Public |
| POST | `/api/auth/forgot-password` | Public |
| POST | `/api/auth/reset-password` | Public |
| POST | `/api/auth/change-password` | JWT |

### Users
| Method | Endpoint | Auth |
|---|---|---|
| GET | `/api/users/me` | JWT |
| PUT | `/api/users/me` | JWT |
| PUT | `/api/users/me/location` | JWT |

### Sports
| Method | Endpoint | Auth |
|---|---|---|
| GET | `/api/sports` | Public |
| GET | `/api/sports/{id}` | Public |

### Fields
| Method | Endpoint | Auth |
|---|---|---|
| GET | `/api/fields` | Public |
| GET | `/api/fields/{id}` | Public |
| GET | `/api/fields/search` | Public |
| GET | `/api/fields/nearby` | Public |
| GET | `/api/fields/top-rated` | Public |
| GET | `/api/fields/{id}/availability` | Public |
| GET | `/api/fields/{id}/reviews` | Public |

### Bookings
| Method | Endpoint | Auth |
|---|---|---|
| POST | `/api/bookings` | JWT |
| POST | `/api/bookings/preview` | JWT |
| GET | `/api/bookings/my-bookings` | JWT |
| GET | `/api/bookings/{id}` | JWT |
| POST | `/api/bookings/{id}/cancel` | JWT |

### Payments
| Method | Endpoint | Auth |
|---|---|---|
| POST | `/api/payments` | JWT |
| GET | `/api/payments/{id}` | JWT |
| GET | `/api/payments/{id}/status` | JWT |
| POST | `/api/payments/{id}/refund` | JWT |
| POST | `/api/payments/webhook` | Public (provider callback) |

### Reviews
| Method | Endpoint | Auth |
|---|---|---|
| POST | `/api/reviews` | JWT |

### Favorites
| Method | Endpoint | Auth |
|---|---|---|
| GET | `/api/favorites` | JWT |
| POST | `/api/favorites/{fieldId}` | JWT |
| DELETE | `/api/favorites/{fieldId}` | JWT |

## ⚙️ Business Rules

- **Nearby fields** use latitude/longitude + radius with **Haversine** distance (`GeoCalculator`).
- **Top-rated fields** filter by radius **before** sorting by rating.
- **Search** supports: sport, field type, city, max price, min rating, date, location, radius, sorting (distance/rating/price/review count) and pagination.
- Fields have **day/night pricing** and multiple images (primary + gallery).
- **Day/night periods are configurable** via `Pricing:DayStartHour` / `Pricing:NightStartHour` (default day 08:00 → night 18:00).
- **Email confirmation is mandatory** — unconfirmed users get `403 EMAIL_NOT_CONFIRMED` on login.
- **Refresh tokens are rotated** (old token is revoked & replaced on every refresh).
- **Backend calculates the final price** — the client never sends a price.
- **Availability is date-based** and considers existing (non-cancelled) bookings.
- **Variable booking duration** with configurable maximum (`Booking:MaxDurationHours`, default 4).
- **Only consecutive available slots** can be booked — the availability endpoint reports `MaxConsecutiveHours` per slot.
- **Overlapping/double bookings prevented** with `SERIALIZABLE` transactions + EF Core optimistic concurrency (`ConcurrencyStamp` row version).
- **409 Conflict** returned when a slot becomes unavailable.
- **Booking flow**: booking is created as `PendingPayment` → a payment is created → on success the booking becomes `Confirmed`.
- **Payment amount is always computed server-side** from the booking total — the client never sends a price.
- **Payments go through `IPaymentProvider`** (mock provider by default, configured in `Payment:Mock`); the webhook endpoint is provider-facing and never trusts the frontend to confirm payment.
- **Payment failures return `402 PAYMENT_FAILED`** and the booking remains `PendingPayment` so the customer can retry.
- **Cancelled/Expired bookings do not occupy a slot**; `PendingPayment`/`Confirmed` bookings do.
- Users can only access **their own** bookings.
- Reviews allowed **only after a booking is completed** and **only once per booking** (unique DB index on `BookingId`).
- Rating must be **1–5**.
- Field `AverageRating` / `ReviewCount` **recalculated** after each review.
- **Duplicate favorites prevented** with business check + unique DB index on `(UserId, FieldId)`.

## 🗄 Database

- **EF Core Code First** with migrations.
- SQL Server with proper:
  - Relationships & cascade/restrict behaviors
  - Indexes (email unique, favorite unique, city, sport, booking-field-date, etc.)
  - Decimal precision (`decimal(18,2)` prices, `decimal(10,7)` coordinates)
  - RowVersion concurrency token on `Bookings`
- **Seed data** for Cairo, Giza, Mansoura and Alexandria (10 fields, 5 sports, 10 locations, amenities, images, 7 days of availability).

## ⚙️ Configuration (`appsettings.json`)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.;Database=SportsBookingDb;Trusted_Connection=True;TrustServerCertificate=True;"
  },
  "Jwt": {
    "Issuer": "SportsBooking",
    "Audience": "SportsBooking.Clients",
    "Key": "CHANGE_THIS...",
    "AccessTokenMinutes": 15,
    "RefreshTokenExpiryDays": 7
  },
  "Pricing": { "DayStartHour": 8, "NightStartHour": 18 },
  "Booking": { "MaxDurationHours": 4, "MinDurationHours": 1 },
  "Location": { "DefaultRadiusKm": 10, "MaxRadiusKm": 100 },
  "Smtp": {
    "Host": "",
    "Port": 587,
    "EnableSsl": true,
    "Username": "",
    "Password": "",
    "FromAddress": "",
    "FromName": "Sports Booking"
  },
  "App": {
    "ClientBaseUrl": "https://localhost:55814",
    "ConfirmEmailPath": "/confirm-email",
    "ResetPasswordPath": "/reset-password"
  },
  "Payment": {
    "Provider": "Mock",
    "Currency": "EGP",
    "Mock": { "AlwaysSucceed": true, "SimulatedFailureReason": "" }
  }
}
```

> ⚠️ Replace the JWT `Key` with a strong random value (e.g. from a secret store) before production.
> ⚠️ Fill in `Smtp` credentials to send real emails (confirmation & password reset). `App:ClientBaseUrl` is the front-end base URL used to build the email links.
> 💳 `Payment` uses a **mock provider** by default (`Payment:Mock:AlwaysSucceed`). Set it to `false` to simulate failed payments. Swap `IPaymentProvider` for a real gateway later.

## 🧪 Tests

Run with:

```bash
dotnet test
```

Coverage:
- **Booking**: day/night pricing, duration limits, overlapping slot conflict, valid creation
- **Pricing**: day vs night rates, line-item breakdown
- **Availability**: closed days, booking-aware slots
- **Concurrency**: conflict detection inside transaction returns 409
- **Payments**: successful payment confirms booking, failed payment returns 402, webhook, refunds, ownership checks
- **Reviews**: completed-booking only, one review per booking, rating range, field rating recalc
- **Nearby search**: radius filtering, top-rated radius-before-sort
- **Favorites**: duplicate prevention, not-found handling
- **Validation**: FluentValidation rules for register/login/booking/review
- **Architecture**: Domain has no EF/ASP.NET refs, Application has no Infrastructure refs
- **Tokens**: refresh token generation + hashing

## 🚀 Getting Started

1. **Prerequisites**: .NET 10 SDK, SQL Server (LocalDB or full instance).
2. **Set the connection string** in `src/SportsBooking.API/appsettings.json`.
3. **Build**:
   ```bash
   dotnet build SportsBooking.sln
   ```
4. **Run**:
   ```bash
   dotnet run --project src/SportsBooking.API
   ```
5. The seeder runs automatically in **Development** mode:
   - Applies migrations
   - Creates the Identity roles (`Customer`, `Owner`, `Admin`)
   - Seeds sports, locations, fields, images, amenities, availability
6. Open **Swagger UI**: `https://localhost:5001/swagger`
7. Register a user → confirm the email (via `/api/auth/confirm-email` with the emailed token) → login → copy the token → click **Authorize** → paste as `Bearer {token}`.

## 🌍 Sample Coordinates (Seed)

| City | Lat | Lon |
|---|---|---|
| Cairo (Nasr City) | 30.049 | 31.240 |
| Giza (Sheikh Zayed) | 30.050 | 30.950 |
| Mansoura | 31.040 | 31.380 |
| Alexandria (Smouha) | 31.200 | 29.950 |