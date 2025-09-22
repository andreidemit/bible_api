#!/bin/bash

# Bible API Local Development Setup Script
# Usage: ./setup-dev.sh

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

log() {
    echo -e "${GREEN}[$(date +'%Y-%m-%d %H:%M:%S')] $1${NC}"
}

info() {
    echo -e "${BLUE}[$(date +'%Y-%m-%d %H:%M:%S')] $1${NC}"
}

warn() {
    echo -e "${YELLOW}[$(date +'%Y-%m-%d %H:%M:%S')] WARNING: $1${NC}"
}

error() {
    echo -e "${RED}[$(date +'%Y-%m-%d %H:%M:%S')] ERROR: $1${NC}"
    exit 1
}

# Check prerequisites
check_prerequisites() {
    log "Checking prerequisites..."
    
    if ! command -v dotnet &> /dev/null; then
        error ".NET SDK is not installed. Please install .NET 8 SDK first."
    fi
    
    DOTNET_VERSION=$(dotnet --version)
    info ".NET SDK version: ${DOTNET_VERSION}"
    
    if ! command -v docker &> /dev/null; then
        warn "Docker is not installed. Some features may not work."
    else
        info "Docker version: $(docker --version)"
    fi
    
    log "Prerequisites check completed"
}

# Setup environment file
setup_environment() {
    log "Setting up environment configuration..."
    
    if [[ ! -f .env ]]; then
        if [[ -f .env.example ]]; then
            cp .env.example .env
            log "Created .env file from .env.example"
            warn "Please edit .env file with your actual configuration values"
        else
            cat > .env <<EOF
# Bible API Environment Configuration
AZURE_STORAGE_CONNECTION_STRING=
AZURE_CONTAINER_NAME=bible-translations
BASE_URL=http://localhost:8000
ALLOWED_ORIGINS=*
EOF
            log "Created basic .env file"
        fi
    else
        info ".env file already exists"
    fi
}

# Restore dependencies
restore_dependencies() {
    log "Restoring .NET dependencies..."
    dotnet restore
    log "Dependencies restored successfully"
}

# Build application
build_application() {
    log "Building application..."
    dotnet build --configuration Debug
    log "Application built successfully"
}

# Run tests
run_tests() {
    log "Running tests..."
    dotnet test --configuration Debug --verbosity normal
    log "Tests completed successfully"
}

# Setup development certificates
setup_certificates() {
    log "Setting up HTTPS development certificates..."
    dotnet dev-certs https --trust
    log "Development certificates configured"
}

# Build Docker image
build_docker_image() {
    if command -v docker &> /dev/null; then
        log "Building Docker image for development..."
        docker build -t bible-api:dev .
        log "Docker image built successfully"
    else
        warn "Docker not available, skipping image build"
    fi
}

# Display usage instructions
show_usage() {
    log "Development environment setup completed!"
    echo
    info "To run the application locally:"
    echo "  cd BibleApi"
    echo "  dotnet run"
    echo
    info "To run with hot reload:"
    echo "  cd BibleApi"
    echo "  dotnet watch run"
    echo
    info "To run with Docker Compose:"
    echo "  docker-compose -f docker-compose.dev.yml up"
    echo
    info "Access points:"
    echo "  API: http://localhost:8000"
    echo "  Swagger: http://localhost:8000/swagger"
    echo "  Health: http://localhost:8000/healthz"
    echo
    warn "Remember to configure your .env file with proper Azure Storage credentials"
    warn "Or the application will run in mock mode with sample data"
}

# Main setup process
main() {
    log "Starting Bible API development environment setup"
    
    check_prerequisites
    setup_environment
    restore_dependencies
    build_application
    run_tests
    setup_certificates
    build_docker_image
    show_usage
    
    log "Setup completed successfully!"
}

# Run main function
main "$@"