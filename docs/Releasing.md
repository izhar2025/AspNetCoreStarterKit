# Releasing & Versioning

This project follows [Semantic Versioning](https://semver.org/): `MAJOR.MINOR.PATCH`.

- **MAJOR** — breaking changes (e.g. changing the shape of `ApiResponse<T>`, removing an endpoint)
- **MINOR** — new backward-compatible features (e.g. adding the `/auth/register` endpoint)
- **PATCH** — bug fixes, no new features or breaking changes

## Where the version lives

The version number is defined **once**, in [`Directory.Build.props`](../Directory.Build.props) at the repo root. MSBuild automatically applies it to every project under this folder — you don't need to update version numbers in each `.csproj` individually.

```xml
<Version>1.0.0</Version>
```

## How to cut a release

1. **Update the version**

   Edit `Directory.Build.props` and bump `<Version>` (and `<AssemblyVersion>` / `<FileVersion>`) according to what changed.

2. **Update the changelog**

   Move the relevant entries from the `[Unreleased]` section of [`CHANGELOG.md`](../CHANGELOG.md) into a new dated section for the version you're releasing.

3. **Commit**

   ```bash
   git add Directory.Build.props CHANGELOG.md
   git commit -m "chore: release v1.1.0"
   git push origin main
   ```

4. **Tag the release**

   The tag is what actually triggers the Docker publish workflow — it must start with `v` and match `MAJOR.MINOR.PATCH`:

   ```bash
   git tag v1.1.0
   git push origin v1.1.0
   ```

5. **What happens automatically**

   Pushing the tag triggers [`.github/workflows/docker-publish.yml`](../.github/workflows/docker-publish.yml), which:
   - Builds the Docker image from the `Dockerfile`
   - Pushes it to GitHub Container Registry (GHCR) tagged as:
     - `ghcr.io/izhar2025/aspnetcorestarterkit:1.1.0` (exact version)
     - `ghcr.io/izhar2025/aspnetcorestarterkit:1.1` (major.minor, moves with patch releases)
     - `ghcr.io/izhar2025/aspnetcorestarterkit:latest`

6. **(Optional) Create a GitHub Release**

   On GitHub, go to **Releases → Draft a new release**, pick the tag you just pushed, and paste in the relevant `CHANGELOG.md` section as the release notes. This gives people a readable summary instead of just a git tag.

## Pulling a released image

Once published, anyone can run a specific version without building anything themselves:

```bash
docker pull ghcr.io/izhar2025/aspnetcorestarterkit:1.1.0
docker run -p 8080:8080 \
  -e ConnectionStrings__DefaultConnection="..." \
  -e Jwt__Key="..." \
  ghcr.io/izhar2025/aspnetcorestarterkit:1.1.0
```

## Relationship to CI

This is separate from [`.github/workflows/dotnet.yml`](../.github/workflows/dotnet.yml), which runs `dotnet build` + `dotnet test` on every push/PR to `main` — that's your safety net *before* a release. Only tag a release once `main` is green.

```
push to main → dotnet.yml runs build + test
                         │
                         ▼ (once you're happy with main)
              bump version, update CHANGELOG, tag vX.Y.Z
                         │
                         ▼
        docker-publish.yml builds & pushes image to GHCR
```
