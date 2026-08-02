# Docker Setup Guide (Beginner-Friendly)

This guide assumes you've never used Docker before. By the end, you'll have the full API + database running with one command, without installing SQL Server or the .NET SDK on your machine at all.

## What is Docker, actually?

Normally, to run this project you'd need to: install the exact .NET version, install SQL Server, configure it, install any other dependencies, and hope your setup matches everyone else's.

Docker sidesteps all of that. A **container** is a small, self-contained box that already has everything the app needs bundled inside it — the right .NET runtime, the right settings, everything. You run the container, and it behaves identically whether you're on Windows, Mac, Linux, or a cloud server.

Two terms you'll see:
- **Image** — the "recipe" / template (built once from the `Dockerfile`)
- **Container** — a running instance of that image

**Docker Compose** is a tool for running *multiple* containers together as one coordinated group — in our case, the API container and a SQL Server container, started together and able to talk to each other.

## 1. Install Docker

You only need **Docker Desktop** — it includes Docker Compose already, no separate install needed.

- **Windows / Mac:** Download and install [Docker Desktop](https://www.docker.com/products/docker-desktop/). Open it once after installing — you should see a little whale icon in your system tray/menu bar when it's running.
- **Linux:** Follow the [official install guide](https://docs.docker.com/engine/install/) for your distribution, then install the [Compose plugin](https://docs.docker.com/compose/install/linux/).

Verify it worked by opening a terminal and running:

```bash
docker --version
docker compose version
```

Both should print a version number. If you get "command not found," Docker Desktop probably isn't running yet, or the install didn't finish — restart your terminal and check the Docker Desktop app is open.

## 2. Get the project

```bash
git clone https://github.com/izhar2025/AspNetCoreStarterKit.git
cd AspNetCoreStarterKit
```

## 3. Create your `.env` file

The `docker-compose.yml` in this repo needs some values (passwords, secret keys) that shouldn't be hardcoded or committed to git. A template is provided:

```bash
cp .env.example .env
```

Now open `.env` in any text editor and fill in real values. At minimum:

```env
SA_PASSWORD=YourStr0ng!Passw0rd
JWT_KEY=change-this-to-a-random-secret-at-least-32-characters-long
```

**`SA_PASSWORD`** — the SQL Server admin password. SQL Server enforces complexity rules: it needs to be 8+ characters and include upper case, lower case, a digit, and a symbol. `YourStr0ng!Passw0rd` (from the example) satisfies this — swap it for your own if you like, keeping the same shape.

**`JWT_KEY`** — the secret used to sign login tokens. Needs to be at least 32 characters. You can generate a random one:

```bash
# Mac/Linux
openssl rand -base64 32

# Windows (PowerShell)
[Convert]::ToBase64String((1..32|%{Get-Random -Max 256}))
```

Paste the output in as `JWT_KEY`. The `Email__*` values can be left as-is for now if you don't need password-reset emails to actually send — nothing else will break.

> `.env` is already listed in `.gitignore` — it will never get committed. Don't remove it from `.gitignore`.

## 4. Start everything

```bash
docker compose up --build
```

What happens, in order:
1. Docker builds the API image from the `Dockerfile` (first run only takes a few minutes — it's downloading base images and restoring NuGet packages; subsequent runs are much faster thanks to caching)
2. A SQL Server container starts
3. Compose waits for SQL Server to report healthy (via its own healthcheck) before starting the API — this avoids the API crashing because it tried to connect before the database was ready
4. The API starts, runs its database migrations automatically, and seeds default data

You'll see interleaved logs from both containers in your terminal. Watch for a line like `Now listening on: http://[::]:8080` from the `api` container — that means it's up.

**Want it running in the background instead?** Add `-d` (detached mode):

```bash
docker compose up --build -d
```

## 5. Try it out

- **Swagger UI:** http://localhost:8080/swagger
- **Health check:** http://localhost:8080/health — should return `"status": "Healthy"`
- **Login** with a seeded default account (see [GettingStarted.md](GettingStarted.md) for the full list):

```bash
curl -X POST http://localhost:8080/api/v1/auth/login \
  -H "Content-Type: application/json" \
  -d '{ "username": "admin", "password": "Admin@123" }'
```

## 6. Everyday commands

| I want to... | Command |
|---|---|
| See logs (if running detached) | `docker compose logs -f` |
| See logs for just the API | `docker compose logs -f api` |
| Stop everything (keeps your data) | `docker compose down` |
| Stop everything **and delete the database** | `docker compose down -v` |
| Rebuild after changing code | `docker compose up --build` |
| Check what's running | `docker compose ps` |
| Open a shell inside the API container | `docker compose exec api bash` |
| Restart just one service | `docker compose restart api` |

## 7. Troubleshooting

**"Port 8080 is already in use"**
Something else on your machine is using that port. Either stop it, or change the port mapping in `docker-compose.yml` — the line `"8080:8080"` under the `api` service. Changing it to `"5000:8080"` makes the API reachable at `http://localhost:5000` instead.

**API container keeps restarting / can't connect to the database**
SQL Server can genuinely take 20-30 seconds to fully start up the first time. Compose's healthcheck should handle the wait automatically, but if you still see connection errors, give it another minute and run `docker compose logs db` to confirm SQL Server itself started cleanly (look for `SQL Server is now ready for client connections`).

**"Login failed for user 'sa'"**
Your `SA_PASSWORD` in `.env` probably doesn't meet SQL Server's complexity requirements (8+ chars, upper+lower+digit+symbol), or you changed it *after* the database volume was already created — SQL Server only applies the password on first initialization. Fix: `docker compose down -v` (this wipes the database) then `docker compose up --build` again with the corrected password.

**Changes to my C# code aren't showing up**
You need to rebuild the image after changing code: `docker compose up --build`. Plain `docker compose up` reuses the previously built image.

**I want a completely clean slate**
```bash
docker compose down -v
docker compose up --build
```
This removes the database volume entirely, so you'll lose any data and get a fresh seed on next startup.

## 8. How this fits together (for the curious)

- **`Dockerfile`** — a multi-stage build. Stage 1 compiles the code using the full .NET SDK image. Stage 2 publishes it. Stage 3 copies *only* the published output into a much smaller runtime-only image (no compiler, no SDK) — this keeps the final image lean and reduces its attack surface.
- **`docker-compose.yml`** — defines the `api` and `db` services, a shared network so they can reach each other by service name (the API's connection string points at `Server=db`, not `localhost`), and a named volume so your database survives container restarts.
- **`.dockerignore`** — keeps things like `bin/`, `obj/`, and `.git/` out of the image build context, so builds are faster and images are smaller.

## Next steps

- Manual (non-Docker) setup: [GettingStarted.md](GettingStarted.md)
- Full endpoint reference: [API.md](API.md)
- How a release gets built into a versioned image automatically: [Releasing.md](Releasing.md)
