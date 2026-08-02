# API Documentation

Base URL: `/api/v1`

All endpoints return a consistent envelope:

```json
{
  "success": true,
  "data": { },
  "message": "Success",
  "errors": null,
  "statusCode": 200,
  "timestamp": "2026-08-02T12:00:00Z"
}
```

Authenticated endpoints expect a bearer token:

```
Authorization: Bearer <accessToken>
```

---

## Auth — `/api/v1/auth`

| Method | Route | Auth | Description |
|---|---|---|---|
| POST | `/register` | Anonymous | Self-service registration. New users are created with the default `Viewer` role and logged in immediately (returns tokens). |
| POST | `/login` | Anonymous | Authenticates with `Username` (username or email) + `Password`. Returns access token, refresh token, and user profile. Locks the account for 15 minutes after 5 failed attempts. |
| POST | `/refresh-token` | Anonymous | Exchanges a valid refresh token for a new access/refresh token pair. |
| POST | `/logout` | Bearer | Revokes the current refresh token. |
| POST | `/change-password` | Bearer | Changes the password for the currently authenticated user. |
| POST | `/forgot-password` | Anonymous | Generates a password reset token and emails a reset link. Always returns a generic success message, whether or not the email exists, to avoid leaking account existence. |
| POST | `/reset-password` | Anonymous | Completes a password reset using the token emailed by `/forgot-password`. |
| GET | `/me` | Bearer | Returns the current authenticated user's profile. |

### Register

```http
POST /api/v1/auth/register
Content-Type: application/json

{
  "username": "jdoe",
  "email": "jdoe@example.com",
  "fullName": "Jane Doe",
  "phoneNumber": "+1234567890",
  "password": "P@ssw0rd!",
  "confirmPassword": "P@ssw0rd!"
}
```

Returns the same shape as `/login` — an access token, refresh token, and `expiresAt`, so the client can go straight into an authenticated session.

### Forgot / Reset password flow

1. `POST /forgot-password` with `{ "email": "..." }` — a reset token is generated (valid 24 hours) and emailed via the configured SMTP settings (`Email:*` in `appsettings.json`). The link base comes from `App:PasswordResetUrl`.
2. `POST /reset-password` with `{ "token", "email", "newPassword", "confirmPassword" }` to set the new password.

---

## Users — `/api/v1/users`

Requires `[Authorize]` + the `ManageUsers` permission on the controller, with individual endpoints further scoped by permission (e.g. `ViewUsers` for reads).

| Method | Route | Permission | Description |
|---|---|---|---|
| GET | `/` | `ViewUsers` | Paged list of users. Supports `page`, `pageSize`, `roleId`, `isActive`, `isLockedOut`, `search`, `sortBy`, `sortDesc`. |
| GET | `/{id}` | `ViewUsers` | Get a single user. |
| POST | `/` | `ManageUsers` | Admin-creates a user with an explicit role (unlike `/auth/register`, which self-assigns `Viewer`). |
| PUT | `/{id}` | `ManageUsers` | Update a user. |
| DELETE | `/{id}` | `ManageUsers` | Delete a user. |
| PUT | `/{id}/lock` | `ManageUsers` | Lock a user account. |
| PUT | `/{id}/unlock` | `ManageUsers` | Unlock a user account. |
| POST | `/{id}/reset-password` | `ManageUsers` | Admin-driven password reset (no email/token flow). |
| POST | `/bulk-upload` | `ManageUsers` | Bulk-create users from an uploaded Excel file. |
| GET | `/bulk-upload/template` | `ManageUsers` | Downloads the Excel template for bulk user upload. |

---

## Roles — `/api/v1/roles`

Requires `[Authorize]` + the `ManageRoles` permission, with reads scoped by `ViewRoles` / `ViewPermissions`.

| Method | Route | Permission | Description |
|---|---|---|---|
| GET | `/` | `ViewRoles` | Paged list of roles. Supports `page`, `pageSize`, `isSystemRole`, `includePermissions`, `search`, `sortBy`, `sortDesc`. |
| GET | `/{id}` | `ViewRoles` | Get a single role, including its permissions. |
| POST | `/` | `ManageRoles` | Create a role. |
| PUT | `/{id}` | `ManageRoles` | Update a role. |
| DELETE | `/{id}` | `ManageRoles` | Delete a role. |
| PUT | `/{id}/permissions` | `ManageRoles` | Replace the permission set assigned to a role. |
| GET | `/{id}/permissions` | `ManageRoles` | Get the permissions assigned to a role. |
| GET | `/permissions` | `ViewPermissions` | Paged list of all permissions in the system, filterable by `category`, `module`, `search`. |

---

## Sample — `/api/v1/sample`

A CRUD reference implementation showing the CQRS + AutoMapper + FluentValidation pattern the rest of the API follows — a good starting point to copy when adding your own entities. Includes bulk upload support, mirroring the pattern used by `/users/bulk-upload`.

---

## Health — `/health`

No authentication — these are meant for load balancers, uptime monitors, and container orchestrators.

| Route | Description |
|---|---|
| `/health` | Full JSON report: overall status, and per-check name/status/description/duration. |
| `/health/live` | Liveness only — always returns healthy if the process is running, no dependency checks. Safe to poll frequently. |
| `/health/ready` | Readiness — includes the database connectivity check. |

Example `/health` response:

```json
{
  "status": "Healthy",
  "totalDuration": "00:00:00.0123456",
  "checks": [
    {
      "name": "database",
      "status": "Healthy",
      "description": "Database connection is healthy.",
      "duration": "00:00:00.0100000"
    }
  ]
}
```

---

## Error responses

Validation and business-rule failures return `success: false` with a `400` (or `401`/`404` where relevant) status and a human-readable `message`. Unhandled exceptions are caught by `ExceptionMiddleware` and returned in the same envelope shape rather than a raw stack trace.
