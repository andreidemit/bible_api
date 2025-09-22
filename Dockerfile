#############################
# Bible API - .NET Core Production Image
# Build: docker build -t bible-api:latest .
#############################

# Use the official .NET SDK image for building
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG CONFIGURATION=Release

WORKDIR /src

# Copy project files and restore dependencies (better layer caching)
COPY BibleApi/BibleApi.csproj BibleApi/

# Restore dependencies in a separate layer
RUN dotnet restore BibleApi/BibleApi.csproj

# Copy source code
COPY BibleApi/ BibleApi/

# Build and publish application
WORKDIR /src/BibleApi
RUN dotnet publish BibleApi.csproj \
    -c $CONFIGURATION \
    -o /app/publish \
    --no-restore \
    --verbosity minimal \
    /p:UseAppHost=false \
    /p:PublishTrimmed=false

# Use the official .NET runtime image for running
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime

# Set environment variables
ENV ASPNETCORE_ENVIRONMENT=Production \
    ASPNETCORE_URLS=http://+:8000 \
    DOTNET_RUNNING_IN_CONTAINER=true \
    DOTNET_PRINT_TELEMETRY_MESSAGE=false \
    DOTNET_USE_POLLING_FILE_WATCHER=true

WORKDIR /app

# Install curl for health checks and clean up in same layer
RUN apt-get update && apt-get install -y --no-install-recommends \
    curl \
    && rm -rf /var/lib/apt/lists/* \
    && apt-get clean

# Create non-root user with specific UID/GID for better security
RUN groupadd --gid 1001 appgroup \
    && useradd --system --create-home --uid 1001 --gid appgroup appuser \
    && chown -R appuser:appgroup /app

# Copy published application with correct ownership
COPY --from=build --chown=appuser:appgroup /app/publish .

# Switch to non-root user
USER appuser

EXPOSE 8000

# Health check with improved configuration
HEALTHCHECK --interval=30s --timeout=10s --start-period=15s --retries=3 \
    CMD curl -fsS http://localhost:8000/healthz || exit 1

# Use array form for better signal handling
ENTRYPOINT ["dotnet", "BibleApi.dll"]