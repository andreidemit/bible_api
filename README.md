# Bible API (C# .NET Core Edition)

This repository contains a C# .NET Core Web API implementation of a Bible verses and passages API. It serves JSON responses for public domain Bible translations, reading data directly from XML files stored in Azure Blob Storage.

**🎯 This is a complete conversion from the original Python FastAPI implementation to C# .NET Core, maintaining full API compatibility.**

## Projects

- **BibleApi**: Web API service for serving Bible data
- **BibleImporter**: Console application for importing XML Bible data into Azure SQL Database
- **BibleApi.Tests**: Unit tests for the API
- **BibleImporter.Tests**: Unit tests for the importer

## Quick Start

### Using Docker

Build and run the application in a container:

```bash
# Build the Docker image
docker build -t bible-api:latest .

# Run with Docker
docker run -p 8000:8000 \
  -e AppSettings__AzureStorageConnectionString="your-connection-string" \
  bible-api:latest

# Or use Docker Compose for development
docker-compose -f docker-compose.dev.yml up

# For production deployment
docker-compose up -d
```

### Quick Setup Script

Use the development setup script for automated environment configuration:

```bash
# Clone and setup the development environment
git clone https://github.com/andreidemit/bible_api.git
cd bible_api
chmod +x scripts/setup-dev.sh
./scripts/setup-dev.sh
```

```bash
# Build the Docker image
docker build -f Dockerfile.dotnet -t bible-api-dotnet:latest .

# Run with environment variables
docker run -p 8000:8000 \
  -e APPSETTINGS__AZURESTORAGECONNECTIONSTRING="your-azure-connection-string" \
  -e APPSETTINGS__AZURECONTAINERNAME="bible-translations" \
  bible-api-dotnet:latest
```

### Running Locally

```bash
# Clone the repository
git clone https://github.com/andreidemit/bible_api.git
cd bible_api/BibleApi

# Install dependencies
dotnet restore

# Set environment variables (optional - will use mock data if not set)
export AppSettings__AzureStorageConnectionString="your-azure-connection-string"
export AppSettings__AzureContainerName="bible-translations"

# Run the application
dotnet run --urls=http://localhost:8000
```

The API will be available at `http://localhost:8000` with automatic Swagger documentation at `/swagger`.

## API Documentation

### Core Endpoints

All endpoints return JSON responses. Full API documentation is available at `/swagger` when running the application.

#### List Translations
```
GET /v1/data
```
Returns a list of all available Bible translations.

#### Get Books for Translation
```
GET /v1/data/{translationId}
```
Returns all books available in the specified translation.

#### Get Chapters for Book
```
GET /v1/data/{translationId}/{bookId}
```
Returns all chapters for the specified book in the translation.

#### Get Verses for Chapter
```
GET /v1/data/{translationId}/{bookId}/{chapter}
```
Returns all verses for the specified chapter.

#### Random Verse
```
GET /v1/data/{translationId}/random/{bookId}
```
Returns a random verse from the specified book(s). Use `OT` for Old Testament, `NT` for New Testament, or specific book IDs separated by commas.

#### Health Check
```
GET /healthz
```
Returns API health status.

### Example API Calls

```bash
# List all translations
curl http://localhost:8000/v1/data

# Get books in King James Version
curl http://localhost:8000/v1/data/kjv

# Get chapters in Genesis
curl http://localhost:8000/v1/data/kjv/GEN

# Get verses from John chapter 3
curl http://localhost:8000/v1/data/kjv/JHN/3

# Get random verse from New Testament
curl http://localhost:8000/v1/data/kjv/random/NT
```

### Environment Configuration

Copy the example environment file and customize it:

```bash
cp .env.example .env
# Edit .env with your Azure Storage credentials
```

### Required Settings

| Setting | Environment Variable | Description |
|---------|---------------------|-------------|
| `AzureStorageConnectionString` | `AppSettings__AzureStorageConnectionString` | Azure Storage connection string for Bible XML files |
| `AzureContainerName` | `AppSettings__AzureContainerName` | Container name (default: "bible-translations") |

### Optional Settings

| Setting | Default | Description |
|---------|---------|-------------|
| `Environment` | "development" | Application environment |
| `BaseUrl` | "http://localhost:8000" | Base URL for API responses |
| `AllowedOrigins` | ["*"] | CORS allowed origins |
| `AllowedMethods` | ["GET", "OPTIONS"] | CORS allowed methods |
| `AllowedHeaders` | ["Content-Type"] | CORS allowed headers |

### Configuration Sources

The application uses the .NET configuration system with the following precedence:

1. Command line arguments (highest priority)
2. Environment variables (prefixed with `AppSettings__`)
3. `.env` file (for development)
4. `appsettings.{Environment}.json`
5. `appsettings.json` (lowest priority)
6. Azure Key Vault (in production environments)

## Development

### Project Structure

```
BibleApi/
├── Controllers/         # API controllers
│   └── BibleController.cs
├── Models/             # Data models and DTOs
│   └── BibleModels.cs
├── Services/           # Business logic services
│   ├── IAzureXmlBibleService.cs
│   ├── AzureXmlBibleService.cs
│   └── MockAzureXmlBibleService.cs
├── Configuration/      # Configuration classes
│   └── AppSettings.cs
├── Core/              # Core utilities and constants
│   └── BibleConstants.cs
└── Program.cs         # Application entry point
```

### Building and Testing

```bash
# Build the project
dotnet build

# Run tests (when implemented)
dotnet test

# Run with hot reload for development
dotnet watch run --urls=http://localhost:8000
```

### Mock Mode

For development and testing without Azure Storage, the application automatically uses a mock service when no Azure connection string is configured. The mock service provides sample data for all endpoints.

## Features

- ✅ RESTful API with versioned endpoints (`/v1/`)
- ✅ CORS support for cross-origin requests
- ✅ Automatic OpenAPI/Swagger documentation
- ✅ Health check endpoint for monitoring
- ✅ Azure Blob Storage integration
- ✅ In-memory caching for performance
- ✅ Structured logging with .NET ILogger
- ✅ Docker containerization
- ✅ Configuration management with .NET Options pattern
- ✅ Dependency injection
- ✅ Error handling with proper HTTP status codes

## Deployment

The Bible API supports multiple deployment methods with comprehensive automation scripts and configurations.

### Quick Deployment Options

#### 1. Docker (Recommended for Development)

```bash
# Build and run locally
docker build -t bible-api:latest .
docker run -p 8000:8000 -e AppSettings__AzureStorageConnectionString="$CONNECTION_STRING" bible-api:latest

# Or use Docker Compose
cp .env.example .env  # Configure your environment
docker-compose up -d
```

#### 2. Azure Container Instances (Automated Script)

```bash
# Set required environment variables
export AZURE_STORAGE_CONNECTION_STRING="your-connection-string"
export AZURE_CONTAINER_NAME="bible-translations"

# Deploy using the automated script
./scripts/deploy-azure.sh production bible-api-rg
```

#### 3. Kubernetes (Automated Script)

```bash
# Set required environment variables
export AZURE_STORAGE_CONNECTION_STRING="your-connection-string"
export DOMAIN="yourdomain.com"

# Deploy using the automated script
./scripts/deploy-k8s.sh production bible-api latest
```

### Deployment Scripts

The repository includes comprehensive deployment scripts with proper error handling and logging:

- **`scripts/setup-dev.sh`**: Automated development environment setup
- **`scripts/deploy-azure.sh`**: Azure Container Instances deployment
- **`scripts/deploy-k8s.sh`**: Kubernetes deployment with ingress and secrets

All scripts include:
- Prerequisites checking
- Environment validation
- Health check verification
- Deployment status reporting
- Error handling and rollback capabilities

### Docker Configuration

#### Multi-Stage Dockerfile Features

- **Optimized layer caching** for faster builds
- **Security hardening** with non-root user
- **Multi-architecture support** (linux/amd64, linux/arm64)
- **Health checks** for container orchestration
- **Minimal attack surface** using ASP.NET Core runtime image

#### Docker Compose Configurations

- **`docker-compose.yml`**: Production deployment with Traefik proxy
- **`docker-compose.dev.yml`**: Development with hot reload and debugging
- **Environment file support** with `.env` configuration
- **Network isolation** and service discovery
- **Volume management** for persistent data

### Azure Container Instances

The automated Azure deployment provides:

```bash
# Example deployment with full configuration
az container create \
  --resource-group bible-api-rg \
  --name bible-api-production \
  --image bible-api:latest \
  --cpu 1 --memory 1.5 \
  --restart-policy Always \
  --ports 8000 \
  --ip-address Public \
  --dns-name-label bible-api-production \
  --environment-variables \
    ASPNETCORE_ENVIRONMENT=Production \
    AppSettings__AzureStorageConnectionString="$CONNECTION_STRING"
```

### Kubernetes Deployment

Complete Kubernetes configuration with:

```yaml
# Deployment with security context and resource limits
apiVersion: apps/v1
kind: Deployment
metadata:
  name: bible-api
  namespace: bible-api
spec:
  replicas: 3
  selector:
    matchLabels:
      app: bible-api
  template:
    spec:
      containers:
      - name: bible-api
        image: bible-api:latest
        ports:
        - containerPort: 8000
        resources:
          requests:
            memory: "256Mi"
            cpu: "100m"
          limits:
            memory: "512Mi"
            cpu: "500m"
        securityContext:
          runAsNonRoot: true
          runAsUser: 1001
          allowPrivilegeEscalation: false
        livenessProbe:
          httpGet:
            path: /healthz
            port: 8000
          initialDelaySeconds: 30
        readinessProbe:
          httpGet:
            path: /healthz
            port: 8000
          initialDelaySeconds: 5
```

### CI/CD Pipeline

Enhanced GitHub Actions workflow includes:

- **Multi-stage pipeline** with proper job dependencies
- **Security scanning** with CodeQL and Trivy
- **Multi-architecture builds** for broader compatibility
- **Container registry integration** with GHCR
- **Coverage reporting** with artifact uploads
- **Automated deployment** on successful builds

### Environment-Specific Configurations

#### Development
- Mock services for offline development
- Hot reload with `dotnet watch`
- Development certificates and debugging support
- Local database integration (optional)

#### Staging
- Azure Storage integration
- Performance monitoring
- Load testing capabilities
- Blue-green deployment support

#### Production
- High availability with load balancing
- Security hardening and monitoring
- Automated backups and disaster recovery
- Performance optimization and caching

## Technology Stack

- **Framework**: ASP.NET Core 8.0
- **Language**: C# 12
- **Cloud Storage**: Azure Blob Storage
- **Documentation**: Swagger/OpenAPI
- **Containerization**: Docker
- **XML Processing**: System.Xml.Linq
- **Dependency Injection**: Built-in .NET DI container
- **Configuration**: .NET Configuration API
- **Logging**: .NET ILogger

## Migration from Python

This C# implementation maintains 100% API compatibility with the original Python FastAPI version. All endpoints, response formats, and behaviors are preserved. Key improvements include:

- **Better Performance**: Compiled C# with optimized runtime
- **Type Safety**: Strong typing throughout the application
- **Better Tooling**: Rich IDE support and debugging
- **Enterprise Ready**: Built on proven .NET platform
- **Memory Efficiency**: Better memory management than Python
- **Async/Await**: Native async support throughout

## Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## License

- **Source Code**: MIT License (see `LICENSE`)
- **Bible Translations**: All Bible translations are public domain
- **Original Python Implementation**: © 2014 Tim Morgan (retained per MIT requirements)
- **C# Port**: © 2025 Andrei Demit

## Support

- 📖 **Documentation**: Available at `/swagger` endpoint
- 🐛 **Issues**: [GitHub Issues](https://github.com/andreidemit/bible_api/issues)
- 💬 **Discussions**: [GitHub Discussions](https://github.com/andreidemit/bible_api/discussions)

---

**Note**: This is a complete rewrite in C# .NET Core while maintaining full backward compatibility with the original Python FastAPI version. Both implementations can be used interchangeably.

# Deprecated

Content merged into root README.md. Use `README.md` for all documentation.