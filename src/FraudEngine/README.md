# Fraud Engine API

A real-time transaction fraud detection API built with .NET 10, FastEndpoints, and PostgreSQL. Transactions are scanned against a configurable set of fraud detection rules, and alerts are raised when suspicious activity is detected.

## Architecture

The solution follows Clean Architecture with three projects:

| Project | Role |
|---|---|
| **FE.API** | Web host, endpoints, middleware, authentication |
| **FE.Core** | Domain entities, enums, interfaces, CQRS commands/handlers/validators |
| **FE.Infrastructure** | EF Core data access, fraud detection rules, services, resilience |

**Key technologies:** FastEndpoints (REPR pattern), Entity Framework Core, Keycloak (JWT Bearer), Serilog, Polly (resilience), PostgreSQL.

## Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Docker](https://docs.docker.com/get-docker/)

### 1. Start the infrastructure

From the solution root (`src/FraudEngine/`):

```bash
docker compose up -d
```

This starts:

| Service | Port | Credentials |
|---|---|---|
| **Keycloak** | [http://localhost:8080](http://localhost:8080) | admin / admin |
| Keycloak Postgres | 5433 | keycloak / keycloak |
| Fraud Engine Postgres | 5432 | postgres / Password123 |

Keycloak auto-imports the `fraud-engine` realm with all clients, scopes, roles, and a test user.

### 2. Run the API

```bash
dotnet run --project src/FraudEngine/FE.API
```

Pending database migrations are applied automatically on startup — no manual `dotnet ef database update` step is needed.

### 3. Open Swagger

Navigate to the Swagger UI (the exact URL is printed in the console output). Click **Authorize**, select the **KeycloakOAuth** option, and click **Authorize** to be redirected to Keycloak's login page. Log in with `admin` / `admin`.

## API Endpoints

All endpoints require a valid JWT token (except health check).

### Transaction Scanning

| Method | Route | Description |
|---|---|---|
| POST | `/transaction/scan` | Scan a transaction against all applicable fraud rules |

**Request body:**

```json
{
  "referenceId": "TXN-001",
  "accountNumber": "ACC-12345",
  "customerName": "John Doe",
  "amount": 55000,
  "currency": "ZAR",
  "country": "ZA",
  "paymentChannel": "Online",
  "paymentTiming": "Immediate",
  "merchantName": "Acme Corp",
  "merchantId": "MERCH-001",
  "beneficiaryAccountNumber": "ACC-99999",
  "category": "Electronics"
}

```

The scan is **idempotent** — submitting the same `referenceId` + `accountNumber` combination returns the original result without re-processing.

**Response:** Returns the `referenceId` and a list of triggered rules, each with a `ruleName`, `severity`, and `description`.

### Fraud Alerts

| Method | Route | Description |
|---|---|---|
| POST | `/alerts/search` | Search alerts by transaction IDs, severities, or rule names (paginated) |

At least one filter (`transactionIds`, `severities`, or `ruleNames`) is required. Page size is capped at 100.

### Watchlist Management

| Method | Route | Description |
|---|---|---|
| POST | `/watchlist/entry` | Add a merchant or beneficiary to the watchlist |
| GET | `/watchlist/beneficiary/{identifier}` | Check if a beneficiary is on the watchlist |
| GET | `/watchlist/merchant/{identifier}` | Check if a merchant is on the watchlist |
| PATCH | `/watchlist/{id}/deactivate` | Soft-delete a watchlist entry |

### Health Check

| Method | Route | Description |
|---|---|---|
| GET | `/health` | Liveness probe (no auth required) |

## Fraud Detection Rules

When a transaction is scanned, it is evaluated against all rules applicable to its payment channel. Each triggered rule produces an alert with a severity level: **Low**, **Medium**, **High**, or **Critical**.

| Rule | Description | Channels | Configurable |
|---|---|---|---|
| **Threshold** | Flags transactions exceeding per-currency amount limits | All | `FraudRules:Threshold:Limits` |
| **Velocity** | Flags accounts with too many transactions in a time window | All | `FraudRules:Velocity` (Window, MaxTransactions) |
| **Structuring** | Detects amounts suspiciously just below reporting thresholds (anti-smurfing) | All | `FraudRules:Structuring` (Thresholds, ProximityPercentage) |
| **Behavioral Deviation** | Flags amounts that deviate significantly from the customer's channel average | All | `FraudRules:BehavioralDeviation:DeviationMultiplier` |
| **Watchlist** | Flags transactions involving watchlisted merchants | All | Managed via watchlist endpoints |
| **Geographic** | Detects impossible travel between countries | CardPresent | `FraudRules:GeographicRule:MinTimeBetweenCountries` |
| **Unknown Country** | Flags transactions from countries not previously seen for the customer | CardPresent | None |

### How Scanning Works

1. **Idempotency check** — if the transaction was already scanned, return the existing result
2. **Customer resolution** — get or create the customer; add the country to their known countries
3. **Context gathering** — fetch recent transactions (24h), watchlist entries, and the customer's channel average
4. **Rule evaluation** — run all applicable rules against the transaction and its context
5. **Persistence** — save the transaction, create fraud alerts for triggered rules, update the channel average
6. **Response** — return the list of triggered rules with their severity

## Configuration

All rule parameters are configurable in `appsettings.json` under `FraudRules`:

```json
{
  "FraudRules": {
    "Threshold": {
      "Limits": { "ZAR": 50000, "USD": 10000, "GBP": 8000, "EUR": 9000 }
    },
    "Velocity": {
      "Window": "00:10:00",
      "MaxTransactions": 5
    },
    "BehavioralDeviation": {
      "DeviationMultiplier": 3.0
    },
    "Structuring": {
      "Thresholds": { "ZAR": 25000, "USD": 10000 },
      "ProximityPercentage": 0.1
    },
    "GeographicRule": {
      "MinTimeBetweenCountries": "02:00:00"
    },
    "WatchlistCache": {
      "SlidingExpiration": "00:30:00",
      "NegativeCacheDuration": "00:05:00",
      "SizeLimit": 10000
    }
  }
}
```

### Keycloak Configuration

```json
{
  "Keycloak": {
    "Authority": "http://localhost:8080/realms/fraud-engine",
    "Audience": "fraud-engine-api",
    "RequireHttpsMetadata": false,
    "SwaggerClientId": "fraud-engine-swagger"
  }
}
```

`SwaggerClientId` enables the Swagger OAuth2 redirect flow in development. In production, `RequireHttpsMetadata` is `true` and the redirect flow is disabled — see [Authentication](#authentication).

## Authentication

The API uses **Keycloak** for authentication via JWT Bearer tokens.

### Development

Swagger exposes an **OAuth2 Authorization Code flow** with PKCE. Click **Authorize** in Swagger and you will be redirected to Keycloak's login page. You can also paste a token directly using the **Bearer** scheme.

### Production

The OAuth2 redirect flow is not available. Pass a Bearer token in the `Authorization` header. Obtain a token from Keycloak's token endpoint using client credentials or any other supported grant type.

Override the placeholder values in `appsettings.Production.json` (or via environment variables) for your deployment:

| Setting | Environment variable |
|---|---|
| Keycloak authority | `Keycloak__Authority` |
| Connection string | `ConnectionStrings__FraudEngine` |

### Keycloak Realm Setup

The `docker compose up` command auto-imports the realm with:

| Resource | Name | Details |
|---|---|---|
| Realm | `fraud-engine` | |
| API Client | `fraud-engine-api` | Confidential, secret: `fraud-engine-api-secret` |
| Swagger Client | `fraud-engine-swagger` | Public, PKCE, localhost redirects |
| Client Scope | `fraud-engine-api-audience` | Audience mapper for token validation |
| Roles | `fraud-analyst`, `fraud-admin` | |
| Test User | `admin` / `admin` | Has `fraud-admin` role |

## Resilience

Database operations use a Polly retry pipeline with exponential backoff:

- **Max retries:** 3
- **Backoff:** Exponential with jitter, starting at 200ms
- **Handles:** `DbUpdateException`, transient `NpgsqlException`, `TimeoutException`

## Watchlist Caching

Watchlist lookups use an in-memory cache to reduce database load:

- **Positive hits** are cached with a 30-minute sliding expiration
- **Negative hits** (entity not on watchlist) are cached for 5 minutes
- **Cache size** is capped at 10,000 entries
- Cache is invalidated on add and deactivate operations
