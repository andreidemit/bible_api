#############################
# Bible API - .NET Core Production Image
# Build: docker build -t bible-api:latest .
#############################

# Use the official .NET SDK image for building
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src

# Copy csproj and restore dependencies (layer caching optimization)
COPY BibleApi/BibleApi.csproj BibleApi/
RUN dotnet restore BibleApi/BibleApi.csproj

# Copy everything else and build
COPY BibleApi/ BibleApi/
WORKDIR /src/BibleApi
RUN dotnet build BibleApi.csproj -c Release -o /app/build

# Publish the application with optimizations
RUN dotnet publish BibleApi.csproj -c Release -o /app/publish /p:UseAppHost=false \
    --no-restore --verbosity minimal

# Use the official .NET runtime image for running (Alpine for smaller size)
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS runtime

# Set environment variables for performance
ENV ASPNETCORE_ENVIRONMENT=Production \
    ASPNETCORE_URLS=http://+:8000 \
    DOTNET_RUNNING_IN_CONTAINER=true \
    DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=true \
    DOTNET_USE_POLLING_FILE_WATCHER=true \
    ASPNETCORE_FORWARDEDHEADERS_ENABLED=true

WORKDIR /app

# Install curl for health checks (Alpine uses apk)
RUN apk add --no-cache curl

# Create non-root user
RUN adduser --disabled-password --gecos '' --uid 1001 appuser \
    && chown -R appuser:appuser /app
USER appuser

# Copy published application
COPY --from=build --chown=appuser:appuser /app/publish .
RUN apt-get update && apt-get install -y --no-install-recommends \
    curl \
    && rm -rf /var/lib/apt/lists/*

# Create non-root user
RUN useradd --system --create-home --uid 1001 appuser \
    && chown -R appuser:appuser /app
USER appuser

# Copy published application
COPY --from=build /app/publish .

EXPOSE 8000

# Health check with optimized interval
HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
    CMD curl -fsS http://localhost:8000/healthz || exit 1

# Default command
ENTRYPOINT ["dotnet", "BibleApi.dll"]