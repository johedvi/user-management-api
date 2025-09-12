#!/bin/bash
# start-k8s-deployment.sh - Start Kubernetes pods and services

set -e

echo "=========================================="
echo "Starting Kubernetes User Management API"
echo "=========================================="

# Check if we're connected to AKS cluster
if ! kubectl cluster-info > /dev/null 2>&1; then
    echo "Connecting to AKS cluster..."
    az aks get-credentials --resource-group rg-user-management-api --name aks-user-management
fi

# Start PostgreSQL server
echo "Starting PostgreSQL server..."
cd terraform

POSTGRES_NAME=$(terraform output -raw postgres_server_name 2>/dev/null || echo "userapi-postgres")
RESOURCE_GROUP=$(terraform output -raw resource_group_name 2>/dev/null || echo "rg-user-management-api")

az postgres flexible-server start \
    --resource-group "$RESOURCE_GROUP" \
    --name "$POSTGRES_NAME" \
    --no-wait

cd ..

# Scale deployment to 2 replicas (starts pods)
echo "Scaling deployment to 2 replicas..."
kubectl scale deployment user-management-api --replicas=2

# Wait for pods to start
echo "Waiting for pods to start..."
kubectl wait --for=condition=ready pod -l app=user-management-api --timeout=300s

# Check deployment status
echo ""
echo "Deployment status:"
kubectl get pods -l app=user-management-api
kubectl get service user-management-api-service

# Get service access information
SERVICE_TYPE=$(kubectl get service user-management-api-service -o jsonpath='{.spec.type}')
echo ""
echo "Service type: $SERVICE_TYPE"

if [ "$SERVICE_TYPE" = "LoadBalancer" ]; then
    EXTERNAL_IP=$(kubectl get service user-management-api-service -o jsonpath='{.status.loadBalancer.ingress[0].ip}')
    if [ -n "$EXTERNAL_IP" ]; then
        echo "External access: http://$EXTERNAL_IP/swagger/index.html"
    else
        echo "External IP is being assigned... check with: kubectl get service user-management-api-service"
    fi
elif [ "$SERVICE_TYPE" = "ClusterIP" ]; then
    echo "Internal access only. Use port forwarding:"
    echo "  kubectl port-forward service/user-management-api-service 8080:80"
    echo "  Then access: http://localhost:8080/swagger/index.html"
fi

echo ""
echo "✓ Kubernetes deployment started successfully!"
echo ""
echo "Useful commands:"
echo "  kubectl get pods -l app=user-management-api    # Check pod status"
echo "  kubectl logs -l app=user-management-api        # View logs"
echo "  kubectl port-forward service/user-management-api-service 8080:80  # Port forward"
echo "  k9s                                           # Visual dashboard"
echo ""