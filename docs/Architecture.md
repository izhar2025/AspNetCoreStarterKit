# Architecture

This project follows **Clean Architecture** (a.k.a. Onion/Hexagonal Architecture): dependencies point inward, and the core business logic has no knowledge of infrastructure or delivery mechanisms.

```
┌─────────────────────────────────────────────┐
│                    API                       │  Controllers, Middleware, Program.cs
│         (depends on Application + Infra)     │  — composition root
├─────────────────────────────────────────────┤
│               Infrastructure                 │  EF Core, Repositories, Email/File/
│         (implements Application interfaces)  │  Excel/JWT services
├─────────────────────────────────────────────┤
│                Application                    │  CQRS features (MediatR), DTOs,
│         (depends only on Domain)              │  validators, mapping profiles,
│                                                │  interfaces (no implementations)
├─────────────────────────────────────────────┤
│                   Domain                      │  Entities, domain interfaces
│         (no dependencies on anything)         │  — the innermost layer
└─────────────────────────────────────────────┘
```

## Layers

### Domain (`AspNetCoreStarterKit.Domain`)
The innermost layer. Contains entities (`User`, `Role`, `Permission`, `RefreshToken`, `PasswordResetToken`, etc.), the `BaseEntity` with common audit fields (`CreatedOn`, `ModifiedOn`, `IsActive`, ...), and repository/unit-of-work interfaces (`IGenericRepository<T>`, `IUnitOfWork`). It has zero dependencies on other layers or on EF Core — nothing here knows how it's persisted.

### Application (`AspNetCoreStarterKit.Application`)
Contains the actual business logic, organized by feature under `Features/` (`Auth`, `Users`, `Roles`, `Sample`) using the CQRS pattern via **MediatR**. Each feature file typically bundles the command/query record, its handler, and its **FluentValidation** validator together — e.g. `Features/Auth/Login.cs` has `LoginCommand`, `LoginCommandHandler`, and `LoginCommandValidator` side by side.

Cross-cutting concerns are implemented as MediatR pipeline behaviors:
- `ValidationBehaviour` — runs FluentValidation validators before a handler executes and short-circuits with a validation error if they fail.
- `LoggingBehaviour` — logs each request/response through the pipeline.

This layer also defines the interfaces that Infrastructure implements (`IEmailService`, `IFileStorageService`, `IJwtService`, `IPasswordHasher`, etc.) — Application depends only on abstractions, never on concrete infrastructure.

**AutoMapper** profiles (`Mappings/MappingProfile.cs`) map between entities and DTOs.

### Infrastructure (`AspNetCoreStarterKit.Infrastructure`)
Implements everything Application declares an interface for:
- `ApplicationDbContext` (EF Core) + `GenericRepository<T>` / `UnitOfWork` — persistence
- `JwtService` / `PasswordHasher` — auth primitives
- `EmailService` — SMTP email delivery, including password-reset emails
- `LocalFileStorageService` — file upload/download/delete on local disk
- `ExcelBulkUploadService` / `ExcelTemplateService` — Excel import/export for bulk operations
- `DatabaseHealthCheck` — `IHealthCheck` implementation used by `/health`
- `ApplicationDbContextSeed` — seeds default roles, permissions, and users on startup

### API (`AspNetCoreStarterKit.API`)
The composition root and delivery mechanism. `Program.cs` wires up Serilog, JWT auth, Swagger, CORS, health checks, and calls `AddApplication()` / `AddInfrastructure()` to register everything from the inner layers. Controllers are thin — they just send a MediatR command/query and translate the `ApiResponse<T>` into an HTTP response via `BaseApiController`.

Two custom middleware components sit in the pipeline:
- `ExceptionMiddleware` — catches unhandled exceptions and returns a consistent `ApiResponse` error shape instead of a raw stack trace.
- `PermissionMiddleware` — reads a `[RequirePermission("...")]` attribute off the matched endpoint and checks the current user's role has that permission before letting the request through.

## Authorization model

Rather than plain ASP.NET role checks, this starter kit uses **permission-based authorization**:

```
User ──> Role ──> RolePermission ──> Permission
```

Controllers/actions are decorated with `[RequirePermission("ViewUsers")]` etc. `PermissionMiddleware` resolves the current user's role, loads its permissions, and checks for a match — giving you fine-grained control per action rather than one blanket role check per controller.

## Request flow

```
HTTP request
  → Controller (API)
    → MediatR.Send(command/query)
      → ValidationBehaviour  (FluentValidation)
      → LoggingBehaviour
      → Handler (Application)
        → IUnitOfWork / IGenericRepository (Infrastructure, via EF Core)
        → other services (IEmailService, IJwtService, ...)
      ← ApiResponse<T>
  ← HTTP response
```

## Why this shape?

- **Testability** — Application logic depends only on interfaces, so handlers can be unit tested with mocked repositories/services (see `tests/AspNetCoreStarterKit.Application.Tests`).
- **Swappable infrastructure** — swapping `LocalFileStorageService` for an S3/Blob implementation, or SQL Server for PostgreSQL, only touches the Infrastructure layer.
- **Consistent, discoverable features** — each MediatR feature file is self-contained (command + handler + validator), so adding a new endpoint means adding one file plus a one-line controller action, rather than touching a service class shared across the whole app.
