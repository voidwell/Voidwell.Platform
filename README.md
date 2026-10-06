# Voidwell.Platform

[![GitHub Workflow Status](https://img.shields.io/github/actions/workflow/status/voidwell/voidwell.platform/build-test.yml?branch=main&style=for-the-badge)](https://github.com/voidwell/voidwell.platform/actions/workflows/build-test.yml)
[![Latest Release](https://img.shields.io/github/v/release/voidwell/voidwell.platform?style=for-the-badge)](https://github.com/voidwell/voidwell.platform/releases/latest)
[![MIT License](https://img.shields.io/badge/License-MIT-green?style=for-the-badge)](LICENSE)

Backend API for Voidwell's platform features: the blog and custom game events. It stores data in PostgreSQL, caches with redis, and serves it over an ASP.NET Core HTTP API. It is called directly by clients, so endpoints enforce their own authorization.

## Requirements

| Dependency | Version | Purpose |
|---|---|---|
| [.NET SDK](https://dotnet.microsoft.com/download) | 10.0 (`net10.0`) | Build and run |
| PostgreSQL | any supported release | Primary data store (EF Core + Npgsql); migrations run automatically on startup |
| Redis | optional | Shared cache (FusionCache L2 + backplane). If `RedisConfiguration` is empty, caching is in-memory per instance |
| Voidwell auth server (`http://voidwellauth:5000`) | n/a | Validates incoming JWT / reference tokens, and issues the client-credentials tokens used to call Keycloak and Voidwell.DaybreakGames |
| Keycloak (Admin REST API) | n/a | Resolves blog author display names. Needs a service-account client with the realm-management `view-users` role |
| Voidwell.DaybreakGames API (`https://api.voidwell.com/ps2` by default) | n/a | Combat report and territory data for custom events |
| Docker | optional | Container build and deployment |

NuGet versions are managed centrally in [Directory.Packages.props](Directory.Packages.props). Authentication, caching, logging and Swagger setup come from the [Voidwell.Common](https://github.com/voidwell/voidwell.common) packages (`Voidwell.Common.Authentication`, `.Cache`, `.Logging` and `.Swagger`).

## Configuration

Settings are read from `appsettings.json`, then `appsettings.{Environment}.json` (for example `appsettings.Development.json`, optional), then environment variables. Environment variables override files; use `__` for nesting (for example `Auth__ClientSecret`).

| Key | Required | Description |
|---|---|---|
| `ConnectionString` | Yes | Npgsql connection string, e.g. `Server=localhost;Database=voidwell.platform;Username=...;Password=...` |
| `PoolSize` | No | `DbContext` pool size (default `5`) |
| `CommandTimeout` | No | Npgsql command timeout in seconds |
| `Auth:Authority` | Yes | Auth server address (`http://voidwellauth:5000` in `appsettings.json`) |
| `Auth:ClientId` | Yes | Client used for reference-token introspection (`voidwell-api` in `appsettings.json`) |
| `Auth:ClientSecret` | Yes | Secret for that client |
| `Auth:RoleClaimType` | No | Claim type that carries roles (`role` in `appsettings.json`) |
| `Keycloak:BaseUrl` | Yes | Keycloak server address, e.g. `https://auth.voidwell.com` |
| `Keycloak:Realm` | Yes | Realm that holds the users |
| `Keycloak:ClientId` | Yes | Service-account client used for the Admin API |
| `Keycloak:ClientSecret` | Yes | Secret for that client |
| `Keycloak:TokenServiceAddress` | Yes | OAuth2 token endpoint used to get the client-credentials token for the Admin API |
| `Keycloak:Scopes` | No | Comma-separated scopes to request |
| `DaybreakGames:BaseUrl` | No | Voidwell.DaybreakGames API address (default `https://api.voidwell.com/ps2`) |
| `DaybreakGames:ClientId` | Yes | Client used to authenticate to that API |
| `DaybreakGames:ClientSecret` | Yes | Secret for that client |
| `DaybreakGames:TokenServiceAddress` | Yes | OAuth2 token endpoint used to get the client-credentials token |
| `DaybreakGames:Scopes` | No | Comma-separated scopes to request |
| `RedisConfiguration` | No | StackExchange.Redis connection string. Empty keeps the cache in memory only |
| `OriginAddress` | No | Extra allowed CORS origin (`http://localhost:4200` is always allowed) |
| `ApplicationName` | No | Names the application (`Voidwell.Platform` in `appsettings.json`; defaults to the assembly name). It is the `Application` property on log events, the Swagger title and the cache key prefix |
| `Serilog` | No | Standard Serilog configuration section (levels and overrides) in `appsettings.json` |

Keycloak and DaybreakGames are validated when the app starts, so a missing required value stops it from starting. Both are called with client-credentials bearer tokens that are cached until they expire; a `401` response discards the token and retries the request once.

Example `appsettings.Development.json` (placed in `src/Voidwell.Platform.Api/`; it is gitignored, so keep real secrets there):

```json
{
  "ConnectionString": "Server=localhost;Database=voidwell.platform;Username=postgres;Password=postgres",
  "Auth": {
    "ClientSecret": "dev-secret"
  },
  "Keycloak": {
    "ClientSecret": "dev-secret",
    "TokenServiceAddress": "https://auth.voidwell.com/realms/voidwell/protocol/openid-connect/token"
  },
  "DaybreakGames": {
    "ClientId": "voidwell-platform",
    "ClientSecret": "dev-secret",
    "TokenServiceAddress": "http://voidwellauth:5000/connect/token"
  },
  "RedisConfiguration": "localhost:6379"
}
```

The EF design-time factory reads `ConnectionString` from `appsettings.json` and `appsettings.Development.json` in the working directory of the Data project or in `src/Voidwell.Platform.Api/`, then environment variables, so migration commands pick up the same file.

### Logging

Logging comes from `Voidwell.Common.Logging`: Serilog configured from the `Serilog` section. In Development it writes readable text to the console; otherwise it writes compact JSON.

## Running

```bash
dotnet run --project src/Voidwell.Platform.Api
```

The API listens on `http://0.0.0.0:5000`, and Swagger UI is served at `/swagger`. Pending EF migrations are applied on startup.

## Endpoints and authorization

Reads are public. Writes require a bearer token whose roles include the one listed.

| Endpoint | Access |
|---|---|
| `GET /post`, `GET /post/{id}` | Public |
| `POST /post`, `DELETE /post/{id}`, `GET /post/edit/{id}`, `PUT /post/edit/{id}` | `Administrator` |
| `GET /gameevent`, `GET /gameevent/{id}`, `GET /gameevent/game/{gameId}` | Public |
| `POST /gameevent`, `PUT /gameevent/{id}` | `Events` |
| `DELETE /gameevent/{id}` | `Administrator` |
| `GET /utils/time` | Public |

Both JWTs and reference tokens are accepted. Reference tokens are checked through introspection against the auth server.

## Database migrations

Add a migration (timestamped unless a name is given):

```bash
scripts/init-migrate.sh [name]
```

The script wraps `dotnet ef migrations add` and needs a POSIX shell (Git Bash or WSL on Windows). Migrations are applied automatically when the API starts. To apply them by hand:

```bash
dotnet ef database update --project src/Voidwell.Platform.Data --startup-project src/Voidwell.Platform.Api
```

## Tests

Tests use xunit v3 on Microsoft Testing Platform (enabled through `global.json`):

```bash
dotnet test --solution Voidwell.Platform.slnx
```

Each application project has a matching test project named `<Project>.Test` under `test/`. Shared test helpers live in `test/Shared`, and common packages are set once in `test/Directory.Build.props`. Run a single project with `dotnet test --project test/Voidwell.Platform.Api.Test`.

## Docker

```bash
docker build -t voidwell-platform .
docker run -p 5000:5000 -e ConnectionString=... -e Auth__ClientSecret=... -e Keycloak__ClientSecret=... -e Keycloak__TokenServiceAddress=... -e DaybreakGames__ClientId=... -e DaybreakGames__ClientSecret=... -e DaybreakGames__TokenServiceAddress=... voidwell-platform
```

`Dockerfile.debug` builds a development image that runs the API under `dotnet watch`.

## Project layout

| Project | Role |
|---|---|
| `Voidwell.Platform.Api` | ASP.NET Core host, controllers and services |
| `Voidwell.Platform.Clients` | HTTP clients for Keycloak and Voidwell.DaybreakGames, with their models and configuration |
| `Voidwell.Platform.Data` | EF Core context, repositories, migrations |

## License

[MIT](LICENSE)
