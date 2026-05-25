# CampusHub.ConfigCenter

**ASP.NET Core (.NET 9) — Practical Work #6**

A diagnostic web application that demonstrates ASP.NET Core configuration system: merging multiple configuration sources, custom provider, options pattern, and middleware.

---

## What This Project Demonstrates

| Topic | Where |
|---|---|
| `IConfiguration` indexer and DI | `/config/raw` |
| Non-file providers: env vars, commandLineArgs, in-memory | `/config/effective` |
| File providers: JSON, XML, INI | All routes |
| Merging sources and key conflict resolution | `/config/effective` |
| `GetSection()`, `GetChildren()`, `GetConnectionString()` | `/config/section/portal`, `/config/tree`, `/config/connection` |
| `IConfigurationRoot.Providers` | `/config/providers` |
| Custom configuration provider | `/config/custom` |
| Binding to classes via `Get<T>()` | `/config/bind` |
| `IOptions<T>` in endpoints and middleware | `/config/options`, headers |

---

## Project Structure

```
CampusHub.ConfigCenter/
├── Program.cs                          # App entry point, all routes
├── Configuration/
│   ├── TextConfigurationProvider.cs    # Custom provider implementation
│   ├── TextConfigurationSource.cs      # IConfigurationSource adapter
│   └── TextConfigurationExtensions.cs  # AddTextFile() extension method
├── Middleware/
│   └── PortalHeaderMiddleware.cs       # Adds X-Portal-Title / X-Portal-Semester headers
├── Models/
│   ├── PortalOptions.cs                # Nested options with Admin and Modules
│   ├── AdminOptions.cs
│   └── NotificationOptions.cs
├── Properties/
│   └── launchSettings.json             # env vars + commandLineArgs for conflict demo
├── appsettings.json                    # Base configuration
├── appsettings.Development.json        # Dev overrides (Portal:Title, Portal:Semester)
├── portal.xml                          # XML provider (CampusName, Dean, Building)
├── notifications.ini                   # INI provider (Notifications section)
└── customsettings.txt                  # Custom text provider (key/value pairs)
```

---

## Configuration Sources (in priority order, lowest → highest)

1. `appsettings.json`
2. `appsettings.Development.json`
3. `portal.xml` — via `AddXmlFile()`
4. `notifications.ini` — via `AddIniFile()`
5. `AddInMemoryCollection()` — hardcoded fallback values
6. `customsettings.txt` — via custom `TextConfigurationProvider`
7. Environment variables (`Portal__SupportEmail`, `Notifications__Sender`)
8. Command-line arguments (`Portal:Title=CampusHub-Portal-CLI`)

**Last source wins.** Three intentional conflicts are demonstrated at `/config/effective`.

---

## Custom Provider (`TextConfigurationProvider`)

Reads `customsettings.txt` where each key and value occupy separate lines:

```
Custom:AppCode
CAMPUSHUB-2025
Custom:MaintenanceMode
false
```

Registered via the `AddTextFile()` extension method on `IConfigurationBuilder`.

---

## Endpoints

| Route | Description |
|---|---|
| `GET /` | Home page with links to all test routes |
| `GET /config/raw` | Raw values via `IConfiguration` indexer and DI |
| `GET /config/section/portal` | Portal section via `GetSection()` |
| `GET /config/tree` | Recursive tree of Portal section via `GetChildren()` |
| `GET /config/connection` | Default connection string via `GetConnectionString()` |
| `GET /config/providers` | List of active providers from `IConfigurationRoot.Providers` |
| `GET /config/custom` | Values read by the custom `TextConfigurationProvider` |
| `GET /config/bind` | `PortalOptions` object bound via `Get<PortalOptions>()` |
| `GET /config/options` | `IOptions<PortalOptions>` and `IOptions<NotificationOptions>` |
| `GET /config/effective` | Final values of conflicting keys with source explanation |

Every response includes `X-Portal-Title` and `X-Portal-Semester` headers added by `PortalHeaderMiddleware`.

---

## Running the Project

```bash
dotnet run
```

The app starts at `http://localhost:5152`. Open the root `/` for a linked overview of all routes.

---

## Key Conflicts Demonstrated

| Key | Sources | Winner |
|---|---|---|
| `Portal:Title` | `appsettings.json` → `appsettings.Development.json` → commandLineArgs | `commandLineArgs` |
| `Portal:SupportEmail` | `appsettings.json` → `portal.xml` → in-memory → env vars | env vars |
| `Notifications:Sender` | `appsettings.json` → `notifications.ini` → in-memory → env vars | env vars |

---

## Requirements

- .NET 9 SDK
- No database or external services required
