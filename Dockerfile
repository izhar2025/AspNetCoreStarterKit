# Dockerfile
# Multi-stage build for AspNetCoreStarterKit.API
# Build context must be the repository root (where this file lives),
# because the API project references sibling projects (Application, Domain, Infrastructure).

# ---- Stage 1: Build ----
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj files first (and restore) so Docker can cache this layer
# and skip re-downloading NuGet packages when only source files change.
COPY ["src/AspNetCoreStarterKit.API/AspNetCoreStarterKit.API.csproj", "src/AspNetCoreStarterKit.API/"]
COPY ["src/AspNetCoreStarterKit.Application/AspNetCoreStarterKit.Application.csproj", "src/AspNetCoreStarterKit.Application/"]
COPY ["src/AspNetCoreStarterKit.Domain/AspNetCoreStarterKit.Domain.csproj", "src/AspNetCoreStarterKit.Domain/"]
COPY ["src/AspNetCoreStarterKit.Infrastructure/AspNetCoreStarterKit.Infrastructure.csproj", "src/AspNetCoreStarterKit.Infrastructure/"]
RUN dotnet restore "src/AspNetCoreStarterKit.API/AspNetCoreStarterKit.API.csproj"

# Now copy everything else and build
COPY . .
WORKDIR /src/src/AspNetCoreStarterKit.API
RUN dotnet build "AspNetCoreStarterKit.API.csproj" -c Release -o /app/build --no-restore

# ---- Stage 2: Publish ----
FROM build AS publish
RUN dotnet publish "AspNetCoreStarterKit.API.csproj" -c Release -o /app/publish --no-restore /p:UseAppHost=false

# ---- Stage 3: Final runtime image ----
# Uses the ASP.NET *runtime* image (not the SDK) - much smaller, no build tools included.
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# curl is needed for the container-level HEALTHCHECK below, which calls our own /health endpoint.
RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*

COPY --from=publish /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

HEALTHCHECK --interval=15s --timeout=5s --start-period=30s --retries=5 \
    CMD curl -f http://localhost:8080/health/live || exit 1

ENTRYPOINT ["dotnet", "AspNetCoreStarterKit.API.dll"]
