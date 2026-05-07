# F1Net

Personal F1 analytics web app: live + historical telemetry ingestion, lap anomaly detection, and a dashboard for standings and per-driver pace.

## Stack

- ASP.NET Core 8 (MVC + Web API + Razor Pages)
- SQL Server LocalDB (Windows) or SQL Server in Docker (cross-platform) + EF Core 8
- ASP.NET Core Identity + OpenIddict 5 (OIDC) with Google external login
- Chart.js for charts (vendored under `wwwroot/lib`, no npm)
- ML.NET (Randomized PCA) for unsupervised lap anomaly detection
- Serilog for logging
- Polly for HTTP resilience

## Solution layout

```
src/
  F1Net.Domain          // Entities, enums, base types — no dependencies
  F1Net.Application     // MediatR handlers, DTOs, abstractions (depends on Domain)
  F1Net.Infrastructure  // EF Core, ML.NET, OpenF1/Ergast clients, BackgroundService
  F1Net.Auth            // OpenIddict + Identity + Google federation wiring
  F1Net.Web             // Razor Pages + MVC + Web API host (composition root)
tests/
  F1Net.Domain.Tests
  F1Net.Application.Tests
  F1Net.Infrastructure.Tests
scripts/
  Sync-F1Data.ps1       // Weekly scheduled sync via client-credentials OIDC token
```

## Prerequisites

- .NET 8 SDK (`brew install --cask dotnet-sdk` or https://dot.net)
- A SQL Server instance:
  - Windows: SQL Server LocalDB (default connection string)
  - macOS/Linux: `docker run -e ACCEPT_EULA=Y -e SA_PASSWORD=Your_password123 -p 1433:1433 -d mcr.microsoft.com/mssql/server:2022-latest` and update the connection string

## Configuration secrets (user-secrets, never committed)

```
dotnet user-secrets set "Authentication:Google:ClientId"     "..." --project src/F1Net.Web
dotnet user-secrets set "Authentication:Google:ClientSecret" "..." --project src/F1Net.Web
dotnet user-secrets set "OpenF1:ApiKey"                      "..." --project src/F1Net.Web
```

## Run

```
dotnet restore
dotnet ef database update --project src/F1Net.Infrastructure --startup-project src/F1Net.Web
dotnet run --project src/F1Net.Web
```

## Phases

This project is being built phase-by-phase. See task list in conversation; current phase: 1 (skeleton).
