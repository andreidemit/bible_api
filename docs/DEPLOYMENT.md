# Deployment Guide

This guide provides comprehensive instructions for deploying the Bible API in various environments.

## Table of Contents

- [Prerequisites](#prerequisites)
- [Environment Configuration](#environment-configuration)
- [Deployment Methods](#deployment-methods)
  - [Local Development](#local-development)
  - [Docker](#docker)
  - [Azure Container Instances](#azure-container-instances)
  - [Kubernetes](#kubernetes)
- [CI/CD Pipeline](#cicd-pipeline)
- [Monitoring and Maintenance](#monitoring-and-maintenance)
- [Troubleshooting](#troubleshooting)

## Prerequisites

### Required Tools

- **.NET 8.0 SDK** (for local development)
- **Docker** (for containerized deployments)
- **Azure CLI** (for Azure deployments)
- **kubectl** (for Kubernetes deployments)

### Required Configuration

1. **Azure Storage Account** with Bible XML files
2. **Environment Variables** configured
3. **SSL Certificates** (for production HTTPS)

## Environment Configuration

### 1. Copy Environment Template

```bash
cp .env.example .env
```

### 2. Configure Required Variables

```bash
# Azure Storage Configuration
AZURE_STORAGE_CONNECTION_STRING=DefaultEndpointsProtocol=https;AccountName=youraccount;AccountKey=yourkey;EndpointSuffix=core.windows.net
AZURE_CONTAINER_NAME=bible-translations

# API Configuration
BASE_URL=https://api.yourdomain.com
ALLOWED_ORIGINS=https://yourdomain.com,https://app.yourdomain.com

# Optional: SSL Configuration
SSL_CERT_PATH=./certs/cert.pem
SSL_KEY_PATH=./certs/key.pem
```

## Deployment Methods

### Local Development

#### Automated Setup

```bash
./scripts/setup-dev.sh
```

#### Manual Setup

```bash
# Install dependencies
dotnet restore

# Set environment variables
export AppSettings__AzureStorageConnectionString="your-connection-string"

# Run the application
cd BibleApi
dotnet run --urls=http://localhost:8000
```

#### Development with Hot Reload

```bash
cd BibleApi
dotnet watch run --urls=http://localhost:8000
```

### Docker

#### Development Environment

```bash
# Build and run development container
docker-compose -f docker-compose.dev.yml up

# With custom configuration
cp .env.example .env
# Edit .env file
docker-compose -f docker-compose.dev.yml up
```

#### Production Environment

```bash
# Build production image
docker build -t bible-api:latest .

# Run production container
docker run -d \
  --name bible-api \
  -p 8000:8000 \
  -e AppSettings__AzureStorageConnectionString="$CONNECTION_STRING" \
  -e AppSettings__BaseUrl="https://api.yourdomain.com" \
  --restart unless-stopped \
  bible-api:latest

# Or use Docker Compose
docker-compose up -d
```

### Azure Container Instances

#### Using Deployment Script (Recommended)

```bash
# Set required environment variables
export AZURE_STORAGE_CONNECTION_STRING="your-connection-string"
export AZURE_CONTAINER_NAME="bible-translations"
export AZURE_LOCATION="eastus"

# Deploy
./scripts/deploy-azure.sh production bible-api-rg
```

### Kubernetes

#### Using Deployment Script (Recommended)

```bash
# Set required environment variables
export AZURE_STORAGE_CONNECTION_STRING="your-connection-string"
export DOMAIN="yourdomain.com"

# Deploy
./scripts/deploy-k8s.sh production bible-api latest
```

## CI/CD Pipeline

### GitHub Actions Workflow

The repository includes an enhanced CI/CD pipeline that:

1. **Builds and tests** the application
2. **Performs security scanning** with CodeQL
3. **Builds multi-architecture Docker images**
4. **Scans containers** for vulnerabilities
5. **Publishes to GitHub Container Registry**

### Triggering Deployments

- **Pull Requests**: Build and test only
- **Push to main/master**: Full pipeline with image publishing
- **Tagged releases**: Full pipeline with versioned images

## Monitoring and Maintenance

### Health Checks

All deployment methods include health check endpoints:

- **Health Check URL**: `/healthz`
- **API Documentation**: `/swagger`

### Logging

The application uses structured logging with .NET ILogger.

### Security Considerations

- Use HTTPS in production environments
- Never commit secrets to source control
- Use environment variables or secret management services
- Enable container image scanning
- Run containers as non-root users

For detailed troubleshooting and advanced configuration, see the full documentation in the repository.