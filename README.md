# 🚀 AspNetCoreStarterKit

[![Build Status](https://github.com/izhar2025/AspNetCoreStarterKit/actions/workflows/dotnet.yml/badge.svg)](https://github.com/izhar2025/AspNetCoreStarterKit/actions/workflows/dotnet.yml)
[![Docker Publish](https://github.com/izhar2025/AspNetCoreStarterKit/actions/workflows/docker-publish.yml/badge.svg)](https://github.com/izhar2025/AspNetCoreStarterKit/actions/workflows/docker-publish.yml)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)
[![Version](https://img.shields.io/badge/version-1.0.0-blue.svg)](CHANGELOG.md)

A production-ready ASP.NET Core 8 Web API Starter Kit built using Clean Architecture principles.

## ✨ Features

**Core**
- ✅ ASP.NET Core 8
- ✅ Entity Framework Core
- ✅ SQL Server
- ✅ Repository Pattern
- ✅ Unit of Work
- ✅ Dependency Injection
- ✅ Swagger / OpenAPI
- ✅ Global Exception Handling
- ✅ Clean Architecture
- ✅ Ready for Production

**Authentication & Security**
- ✅ JWT Authentication
- ✅ Refresh Tokens
- ✅ Permission-based Authorization (roles map to fine-grained permissions)
- ✅ User Registration
- ✅ Password Reset (with email delivery)
- ✅ Account lockout after repeated failed logins

**Production Features**
- ✅ Serilog Logging (console + rolling file sinks)
- ✅ Health Checks (`/health`, `/health/live`, `/health/ready`)
- ✅ FluentValidation
- ✅ AutoMapper
- ✅ Email Service (SMTP)
- ✅ File Upload Service (local storage, with base64 upload support)
- ✅ Excel bulk upload/template generation
- ⬜ Localization

---

## 🛠 Technology Stack

- ASP.NET Core 8
- C#
- Entity Framework Core
- SQL Server
- JWT (with refresh tokens)
- MediatR (CQRS)
- FluentValidation
- AutoMapper
- Serilog
- Swagger
- Visual Studio 2022

---

## 📂 Project Structure

```
src/
│
├── AspNetCoreStarterKit.API              # Controllers, middleware, composition root
├── AspNetCoreStarterKit.Application      # CQRS features, DTOs, interfaces, validators
├── AspNetCoreStarterKit.Domain           # Entities, domain interfaces
├── AspNetCoreStarterKit.Infrastructure   # EF Core, repositories, external services
│
tests/
│
docs/
```

See [docs/Architecture.md](docs/Architecture.md) for a deeper look at how the layers fit together, and [docs/API.md](docs/API.md) for the full endpoint reference.

---

## 🚀 Getting Started

### Option A: Docker (recommended)

```bash
git clone https://github.com/izhar2025/AspNetCoreStarterKit.git
cd AspNetCoreStarterKit
cp .env.example .env   # then edit .env with your own values
docker compose up --build
```

API available at `http://localhost:8080` (Swagger at `/swagger`). No local SQL Server or .NET SDK install required.

New to Docker? [docs/Docker.md](docs/Docker.md) is a full step-by-step guide assuming zero prior experience.

### Option B: Manual (.NET SDK + local SQL Server)

**Clone Repository**

```bash
git clone https://github.com/izhar2025/AspNetCoreStarterKit.git
```

**Open Solution**

Open the solution in **Visual Studio 2022**.

**Update Connection String**

Edit:

```
appsettings.json
```

**Apply Database**

```bash
Update-Database
```

or

```bash
dotnet ef database update
```

**Run Project**

Press

```
F5
```

or

```bash
dotnet run
```

Full walkthrough, including default seeded accounts and configuration keys, is in [docs/GettingStarted.md](docs/GettingStarted.md).

---

## 📖 Roadmap

- [x] ASP.NET Core 8
- [x] JWT Authentication
- [x] Swagger
- [x] Serilog
- [x] Health Checks
- [x] Docker Support
- [x] Docker Compose
- [x] CI/CD (build+test on every push; versioned image published to GHCR on tagged releases)
- [x] Versioning (SemVer, centralized via `Directory.Build.props`, see [CHANGELOG.md](CHANGELOG.md))
- [ ] Redis Cache
- [ ] Unit Testing
- [ ] Localization

---

## 🤝 Contributing


Pull requests are welcome.

For major changes, please open an issue first.

---

## 📜 License

This project is licensed under the MIT License.