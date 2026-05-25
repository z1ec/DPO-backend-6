# Practical Work #6 — ASP.NET Core Configuration

**Course:** Backend Development (ASP.NET Core / .NET)
**Topic:** Application configuration, configuration providers, merging sources, binding to classes and IOptions

---

## Project

### CampusHub.ConfigCenter

A diagnostic web application that acts as a configuration management center for a fictional college portal. The app demonstrates the full ASP.NET Core configuration pipeline: loading settings from multiple sources, resolving key conflicts by source priority, exposing a custom configuration provider, and injecting typed options into endpoints and middleware.

---

## What Was Implemented

### Configuration Sources (8 total, in priority order)

| Priority | Source | Method |
|---|---|---|
| 1 (lowest) | `appsettings.json` | `AddJsonFile()` |
| 2 | `appsettings.Development.json` | `AddJsonFile()` |
| 3 | `portal.xml` | `AddXmlFile()` |
| 4 | `notifications.ini` | `AddIniFile()` |
| 5 | In-memory collection | `AddInMemoryCollection()` |
| 6 | `customsettings.txt` | `AddTextFile()` — custom provider |
| 7 | Environment variables | `AddEnvironmentVariables()` |
| 8 (highest) | Command-line arguments | `AddCommandLine()` |

### Custom Configuration Provider

`TextConfigurationProvider` reads a plain-text file where each key and its value occupy separate lines:

```
Custom:AppCode
CAMPUSHUB-2025
Custom:MaintenanceMode
false
```

Registered via the `AddTextFile()` extension method on `IConfigurationBuilder`.

### Key Conflict Demonstration

Three intentional key conflicts show how the last registered source always wins:

| Key | Competing Sources | Winner |
|---|---|---|
| `Portal:Title` | appsettings.json → appsettings.Development.json → CLI args | CLI args |
| `Portal:SupportEmail` | appsettings.json → portal.xml → in-memory → env vars | env vars |
| `Notifications:Sender` | appsettings.json → notifications.ini → in-memory → env vars | env vars |

### Options Pattern

Configuration sections are bound to typed classes and injected via `IOptions<T>`:

- `PortalOptions` — nested object (`AdminOptions`) + collection (`Modules`)
- `NotificationOptions` — flat settings
- `PortalHeaderMiddleware` — consumes `IOptions<PortalOptions>` and adds `X-Portal-Title` / `X-Portal-Semester` response headers on every request

### API Endpoints

| Route | Description |
|---|---|
| `GET /` | Home page with links to all test routes |
| `GET /config/raw` | Raw values via `IConfiguration` indexer |
| `GET /config/section/portal` | Portal section via `GetSection()` |
| `GET /config/tree` | Recursive section tree via `GetChildren()` |
| `GET /config/connection` | Connection string via `GetConnectionString()` |
| `GET /config/providers` | Active providers list from `IConfigurationRoot` |
| `GET /config/custom` | Values from the custom `TextConfigurationProvider` |
| `GET /config/bind` | `PortalOptions` bound via `Get<PortalOptions>()` |
| `GET /config/options` | `IOptions<PortalOptions>` + `IOptions<NotificationOptions>` |
| `GET /config/effective` | Final values of all conflicting keys |

---

## Repository Structure

```
DPO-backend-6/
├── CampusHub.ConfigCenter/   # ASP.NET Core project
├── screnshoots_for_report/   # Screenshots for the lab report
├── Отчёт_Практика_6_CampusHub.docx
└── Практика_6.pdf            # Original assignment
```

---

## Running

```bash
cd CampusHub.ConfigCenter
dotnet run
```

App starts at `http://localhost:5152`. Open `/` for a linked overview of all routes.

---

## Stack

- .NET 9 / ASP.NET Core
- `Microsoft.Extensions.Configuration.Xml`
- `Microsoft.Extensions.Configuration.Ini`
- Custom `IConfigurationProvider` implementation
