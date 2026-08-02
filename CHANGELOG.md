# Changelog

All notable changes to this project are documented here.

The format follows [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project uses [Semantic Versioning](https://semver.org/) (`MAJOR.MINOR.PATCH`).

See [docs/Releasing.md](docs/Releasing.md) for how releases are cut.

## [Unreleased]

## [1.0.0] - 2026-08-02

### Added
- ASP.NET Core 8 Web API with Clean Architecture (`Domain` / `Application` / `Infrastructure` / `API`)
- JWT authentication with refresh tokens
- Permission-based authorization (`Role` → `RolePermission` → `Permission`)
- Public self-registration (`POST /api/v1/auth/register`), separate from admin-created users
- Password reset flow with email delivery (`forgot-password` / `reset-password`)
- Health check endpoints: `/health`, `/health/live`, `/health/ready`
- Serilog structured logging (console + rolling file sinks)
- FluentValidation + AutoMapper wired through the MediatR pipeline
- Local file storage service (upload/download/delete, including base64 upload)
- Excel bulk upload/template generation for Users
- Docker support: multi-stage `Dockerfile` and `docker-compose.yml` (API + SQL Server)
- CI (GitHub Actions): build + test on every push/PR to `main`
- CD (GitHub Actions): build and publish a versioned Docker image to GHCR on tagged releases
- Centralized project versioning via `Directory.Build.props`

[Unreleased]: https://github.com/izhar2025/AspNetCoreStarterKit/compare/v1.0.0...HEAD
[1.0.0]: https://github.com/izhar2025/AspNetCoreStarterKit/releases/tag/v1.0.0
