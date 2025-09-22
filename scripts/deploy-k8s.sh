#!/bin/bash

# Bible API Kubernetes Deployment Script
# Usage: ./deploy-k8s.sh [environment] [namespace]

set -e

# Configuration
ENVIRONMENT=${1:-production}
NAMESPACE=${2:-bible-api}
APP_NAME="bible-api"
IMAGE_TAG=${3:-latest}

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
    
    if ! command -v kubectl &> /dev/null; then
        error "kubectl is not installed. Please install it first."
    fi
    
    if ! kubectl cluster-info &> /dev/null; then
        error "Not connected to a Kubernetes cluster. Please configure kubectl first."
    fi
    
    if [[ -z "${AZURE_STORAGE_CONNECTION_STRING}" ]]; then
        error "AZURE_STORAGE_CONNECTION_STRING environment variable is required"
    fi
    
    log "Prerequisites check passed"
}

# Create namespace if it doesn't exist
create_namespace() {
    log "Creating namespace '${NAMESPACE}' if it doesn't exist..."
    kubectl create namespace "${NAMESPACE}" --dry-run=client -o yaml | kubectl apply -f -
}

# Create secrets
create_secrets() {
    log "Creating Kubernetes secrets..."
    
    # Delete existing secret if it exists
    kubectl delete secret "${APP_NAME}-secrets" -n "${NAMESPACE}" --ignore-not-found=true
    
    # Create new secret
    kubectl create secret generic "${APP_NAME}-secrets" \
        --from-literal=azure-storage-connection-string="${AZURE_STORAGE_CONNECTION_STRING}" \
        --namespace="${NAMESPACE}"
}

# Deploy application
deploy_app() {
    log "Deploying application..."
    
    # Create deployment manifest
    cat <<EOF | kubectl apply -f -
apiVersion: apps/v1
kind: Deployment
metadata:
  name: ${APP_NAME}
  namespace: ${NAMESPACE}
  labels:
    app: ${APP_NAME}
    environment: ${ENVIRONMENT}
spec:
  replicas: 3
  selector:
    matchLabels:
      app: ${APP_NAME}
  template:
    metadata:
      labels:
        app: ${APP_NAME}
        environment: ${ENVIRONMENT}
    spec:
      containers:
      - name: ${APP_NAME}
        image: bible-api:${IMAGE_TAG}
        ports:
        - containerPort: 8000
          name: http
        env:
        - name: ASPNETCORE_ENVIRONMENT
          value: "${ENVIRONMENT}"
        - name: AppSettings__AzureStorageConnectionString
          valueFrom:
            secretKeyRef:
              name: ${APP_NAME}-secrets
              key: azure-storage-connection-string
        - name: AppSettings__AzureContainerName
          value: "${AZURE_CONTAINER_NAME:-bible-translations}"
        - name: AppSettings__BaseUrl
          value: "https://api.${DOMAIN:-localhost}"
        resources:
          requests:
            memory: "256Mi"
            cpu: "100m"
          limits:
            memory: "512Mi"
            cpu: "500m"
        livenessProbe:
          httpGet:
            path: /healthz
            port: http
          initialDelaySeconds: 30
          periodSeconds: 10
        readinessProbe:
          httpGet:
            path: /healthz
            port: http
          initialDelaySeconds: 5
          periodSeconds: 5
        securityContext:
          runAsNonRoot: true
          runAsUser: 1001
          allowPrivilegeEscalation: false
          capabilities:
            drop:
            - ALL
---
apiVersion: v1
kind: Service
metadata:
  name: ${APP_NAME}-service
  namespace: ${NAMESPACE}
  labels:
    app: ${APP_NAME}
spec:
  selector:
    app: ${APP_NAME}
  ports:
  - name: http
    port: 80
    targetPort: 8000
  type: ClusterIP
---
apiVersion: networking.k8s.io/v1
kind: Ingress
metadata:
  name: ${APP_NAME}-ingress
  namespace: ${NAMESPACE}
  annotations:
    nginx.ingress.kubernetes.io/rewrite-target: /
    cert-manager.io/cluster-issuer: letsencrypt-prod
spec:
  tls:
  - hosts:
    - api.${DOMAIN:-localhost}
    secretName: ${APP_NAME}-tls
  rules:
  - host: api.${DOMAIN:-localhost}
    http:
      paths:
      - path: /
        pathType: Prefix
        backend:
          service:
            name: ${APP_NAME}-service
            port:
              number: 80
EOF
}

# Wait for deployment
wait_for_deployment() {
    log "Waiting for deployment to be ready..."
    kubectl rollout status deployment/${APP_NAME} -n "${NAMESPACE}" --timeout=300s
    
    log "Checking pod status..."
    kubectl get pods -n "${NAMESPACE}" -l app="${APP_NAME}"
}

# Get deployment info
get_deployment_info() {
    log "Getting deployment information..."
    
    # Get service info
    kubectl get service "${APP_NAME}-service" -n "${NAMESPACE}"
    
    # Get ingress info
    kubectl get ingress "${APP_NAME}-ingress" -n "${NAMESPACE}"
    
    # Get external IP/URL
    EXTERNAL_IP=$(kubectl get ingress "${APP_NAME}-ingress" -n "${NAMESPACE}" -o jsonpath='{.status.loadBalancer.ingress[0].ip}' 2>/dev/null || echo "")
    EXTERNAL_HOST=$(kubectl get ingress "${APP_NAME}-ingress" -n "${NAMESPACE}" -o jsonpath='{.spec.rules[0].host}' 2>/dev/null || echo "")
    
    if [[ -n "${EXTERNAL_HOST}" ]]; then
        log "Bible API deployed successfully!"
        log "Access URL: https://${EXTERNAL_HOST}"
        log "Health check: https://${EXTERNAL_HOST}/healthz"
        log "API documentation: https://${EXTERNAL_HOST}/swagger"
    elif [[ -n "${EXTERNAL_IP}" ]]; then
        log "Bible API deployed successfully!"
        log "External IP: ${EXTERNAL_IP}"
    fi
}

# Main deployment process
main() {
    log "Starting Bible API deployment to Kubernetes"
    log "Environment: ${ENVIRONMENT}"
    log "Namespace: ${NAMESPACE}"
    log "Image Tag: ${IMAGE_TAG}"
    
    check_prerequisites
    create_namespace
    create_secrets
    deploy_app
    wait_for_deployment
    get_deployment_info
    
    log "Deployment completed successfully!"
}

# Run main function
main "$@"