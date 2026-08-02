# Getting Started

## Which path should I take?

There are two ways to run this project — pick whichever fits you:

| | Best for | Requires installing |
|---|---|---|
| 🐳 **[Docker](Docker.md)** (recommended if you're new) | Fastest way to get running, no local SQL Server needed | Just Docker Desktop |
| 💻 **Manual setup** (this page) | Debugging in Visual Studio, contributing to the code | .NET 8 SDK + SQL Server |

If you just want to try the project or aren't sure, **use [docs/Docker.md](Docker.md)** — it's a step-by-step guide assuming zero Docker experience and gets you running with one command. The rest of this page covers the manual/local path.

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- SQL Server (LocalDB works fine for local development) or another EF Core-compatible SQL Server instance
- Visual Studio 2022 (recommended) or the `dotnet` CLI

## 1. Clone the repository

```bash
git clone https://github.com/izhar2025/AspNetCoreStarterKit.git
cd AspNetCoreStarterKit
```

## 2. Configure `appsettings.json`

Open `src/AspNetCoreStarterKit.API/appsettings.json` (or `appsettings.Development.json` if you add one) and set the following:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=StarterKitDb;Trusted_Connection=True;"
  },
  "Jwt": {
    "Key": "a-secret-key-at-least-32-characters-long",
    "Issuer": "YourProject",
    "Audience": "YourProjectClient",
    "AccessTokenExpiryMinutes": "15",
    "RefreshTokenExpiryDays": "7"
  },
  "FileStorage": {
    "LocalPath": "C:\\YourProject\\Uploads",
    "MaxFileSize": 10485760,
    "AllowedExtensions": ".jpg,.jpeg,.png,.pdf,.doc,.docx"
  },
  "Email": {
    "SmtpHost": "smtp.gmail.com",
    "SmtpPort": 587,
    "SmtpUser": "your-email@gmail.com",
    "SmtpPass": "your-app-password",
    "FromEmail": "noreply@yourproject.com",
    "FromName": "YourProject System"
  },
  "App": {
    "PasswordResetUrl": "https://your-frontend.com/reset-password"
  }
}
```

| Key | Used for |
|---|---|
| `ConnectionStrings:DefaultConnection` | SQL Server connection string |
| `Jwt:*` | Access/refresh token signing and expiry |
| `FileStorage:*` | Where uploaded files are written to disk, size/extension limits |
| `Email:*` | SMTP credentials used by `EmailService` (password reset emails) |
| `App:PasswordResetUrl` | The frontend URL the password-reset email link points to — update this to your actual client app before going to production |
| `Cors:AllowedOrigins` | Required in Production — the list of origins allowed by CORS |

> ⚠️ Never commit real secrets. `appsettings.Production.json`/`appsettings.Example.json` in this repo use placeholder values on purpose — replace them via environment variables, user-secrets, or a secrets manager in real deployments.

## 3. Apply database migrations

From Visual Studio's Package Manager Console:

```powershell
Update-Database
```

Or with the CLI:

```bash
dotnet ef database update --project src/AspNetCoreStarterKit.Infrastructure --startup-project src/AspNetCoreStarterKit.API
```

On first run, the app also seeds the database automatically at startup (see `ApplicationDbContextSeed`) if it's empty — roles, permissions, and a set of default users.

## 4. Run the API

```bash
dotnet run --project src/AspNetCoreStarterKit.API
```

or press **F5** in Visual Studio.

Swagger UI is available at `/swagger` when running in the `Development` environment.

## 5. Log in

The database seed creates four default accounts, one per system role:

| Username | Password | Role |
|---|---|---|
| `admin` | `Admin@123` | Admin (full access) |
| `operator` | `Operator@123` | Operator |
| `viewer` | `Viewer@123` | Viewer (read-only) |
| `security` | `Security@123` | Security |

**Change or remove these before deploying anywhere near production.**

Authenticate via Swagger or:

```bash
curl -X POST https://localhost:5001/api/v1/auth/login \
  -H "Content-Type: application/json" \
  -d '{ "username": "admin", "password": "Admin@123" }'
```

Take the `data.token` from the response and use it as a Bearer token for authenticated endpoints, or click **Authorize** in Swagger and paste `Bearer <token>`.

New users can also self-register (no admin required) via:

```bash
curl -X POST https://localhost:5001/api/v1/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "username": "jdoe",
    "email": "jdoe@example.com",
    "fullName": "Jane Doe",
    "password": "P@ssw0rd!",
    "confirmPassword": "P@ssw0rd!"
  }'
```

Self-registered accounts get the least-privileged `Viewer` role by default; an `admin` (or anyone with `ManageUsers`/`ManageRoles`) can promote them afterwards.

## 6. Verify health checks

```bash
curl https://localhost:5001/health
```

Should return a `Healthy` status with a `database` check, confirming the app can reach SQL Server.

## Next steps

- Prefer Docker instead? [Docker.md](Docker.md)
- Full endpoint reference: [API.md](API.md)
- How the layers fit together: [Architecture.md](Architecture.md)
- Cutting a new version/release: [Releasing.md](Releasing.md)
- Want to add a new feature? Copy the pattern in `Features/Sample/` — it's a self-contained CRUD example (command + handler + validator per file) you can rename and adapt.
