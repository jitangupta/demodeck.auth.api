# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build & Run Commands

```bash
dotnet build                     # Build the project
dotnet run                       # Run locally (http://localhost:5130)
dotnet publish -c Release        # Publish for production
docker build .                   # Build Docker image
```

There is no test project currently. Manual API testing can be done via `test-auth.http` or `DemoDeck.Auth.Api.http`.

## Architecture

This is a **multi-tenant JWT authentication microservice** for the DemoDeck SaaS platform, built with ASP.NET Core 8.0 / C# 12.

### Request Flow

`POST /api/auth/token` with `LoginRequest` (username, password, tenantName) →
AuthController → AuthService (validates tenant via external Tenant API call, looks up user, verifies BCrypt password) → JwtService (generates JWT with tenant/user claims) → returns `LoginResponse`

### Key Design Decisions

- **External tenant validation:** AuthService makes an HTTP call to a separate Tenant API (`TenantUrl` in appsettings) to verify tenant existence before authenticating users.
- **In-memory user store:** `InMemoryTenantUserRepository` holds seed data for 4 test tenants. This is a placeholder—no real database is connected yet.
- **Interface-based DI:** All services use interfaces (`IAuthService`, `IJwtService`, `ITenantUserRepository`) registered in `Program.cs`.
- **OpenTelemetry tracing:** Configured with OTLP exporter targeting a Kubernetes Tempo instance. The `/health` endpoint is excluded from traces.
- **Dynamic CORS:** Allowed origins are loaded from `appsettings.json` under `Cors:AllowedOrigins`, with wildcard subdomain support for `*.demodeck.xyz`.

### Endpoints

- `POST /api/auth/token` — Generate JWT token (main endpoint)
- `GET /health` — Health check
- `GET /swagger/` — Swagger UI (Development environment only)

### JWT Claims

Tokens include: NameIdentifier, Name, Email, tenant name, tenant ID, user role. Configured via `JwtSettings` section in appsettings (60-minute expiry).

## CI/CD

GitHub Actions workflow (`.github/workflows/docker-image.yml`) is manually triggered via `workflow_dispatch`, accepts a tag input, and pushes a Docker image to GitHub Container Registry (`ghcr.io`).
