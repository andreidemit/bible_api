#!/bin/bash

# Bible API Deployment Script for Azure Container Instances
# Usage: ./deploy-azure.sh [environment] [resource-group]

set -e

# Configuration
ENVIRONMENT=${1:-production}
RESOURCE_GROUP=${2:-bible-api-rg}
CONTAINER_NAME="bible-api-${ENVIRONMENT}"
IMAGE_NAME="bible-api:latest"
LOCATION=${AZURE_LOCATION:-eastus}

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

log() {
    echo -e "${GREEN}[$(date +'%Y-%m-%d %H:%M:%S')] $1${NC}"
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
    
    if ! command -v az &> /dev/null; then
        error "Azure CLI is not installed. Please install it first."
    fi
    
    if ! az account show &> /dev/null; then
        error "Not logged in to Azure. Please run 'az login' first."
    fi
    
    if [[ -z "${AZURE_STORAGE_CONNECTION_STRING}" ]]; then
        error "AZURE_STORAGE_CONNECTION_STRING environment variable is required"
    fi
    
    log "Prerequisites check passed"
}

# Create resource group if it doesn't exist
create_resource_group() {
    log "Creating resource group '${RESOURCE_GROUP}' if it doesn't exist..."
    az group create \
        --name "${RESOURCE_GROUP}" \
        --location "${LOCATION}" \
        --output table
}

# Deploy container instance
deploy_container() {
    log "Deploying container instance '${CONTAINER_NAME}'..."
    
    # Check if container instance already exists
    if az container show --resource-group "${RESOURCE_GROUP}" --name "${CONTAINER_NAME}" &> /dev/null; then
        warn "Container instance '${CONTAINER_NAME}' already exists. Deleting..."
        az container delete \
            --resource-group "${RESOURCE_GROUP}" \
            --name "${CONTAINER_NAME}" \
            --yes
        
        # Wait for deletion to complete
        log "Waiting for container deletion to complete..."
        sleep 30
    fi
    
    # Create new container instance
    az container create \
        --resource-group "${RESOURCE_GROUP}" \
        --name "${CONTAINER_NAME}" \
        --image "${IMAGE_NAME}" \
        --cpu 1 \
        --memory 1.5 \
        --restart-policy Always \
        --ports 8000 \
        --ip-address Public \
        --dns-name-label "${CONTAINER_NAME}" \
        --environment-variables \
            ASPNETCORE_ENVIRONMENT="${ENVIRONMENT}" \
            AppSettings__AzureStorageConnectionString="${AZURE_STORAGE_CONNECTION_STRING}" \
            AppSettings__AzureContainerName="${AZURE_CONTAINER_NAME:-bible-translations}" \
            AppSettings__BaseUrl="https://${CONTAINER_NAME}.${LOCATION}.azurecontainer.io" \
        --output table
}

# Get deployment information
get_deployment_info() {
    log "Getting deployment information..."
    
    CONTAINER_INFO=$(az container show \
        --resource-group "${RESOURCE_GROUP}" \
        --name "${CONTAINER_NAME}" \
        --query "{fqdn:ipAddress.fqdn,ip:ipAddress.ip,state:containers[0].instanceView.currentState.state}" \
        --output table)
    
    echo "${CONTAINER_INFO}"
    
    FQDN=$(az container show \
        --resource-group "${RESOURCE_GROUP}" \
        --name "${CONTAINER_NAME}" \
        --query "ipAddress.fqdn" \
        --output tsv)
    
    if [[ -n "${FQDN}" ]]; then
        log "Bible API deployed successfully!"
        log "Access URL: https://${FQDN}:8000"
        log "Health check: https://${FQDN}:8000/healthz"
        log "API documentation: https://${FQDN}:8000/swagger"
    fi
}

# Main deployment process
main() {
    log "Starting Bible API deployment to Azure Container Instances"
    log "Environment: ${ENVIRONMENT}"
    log "Resource Group: ${RESOURCE_GROUP}"
    log "Container Name: ${CONTAINER_NAME}"
    
    check_prerequisites
    create_resource_group
    deploy_container
    get_deployment_info
    
    log "Deployment completed successfully!"
}

# Run main function
main "$@"