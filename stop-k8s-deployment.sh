#!/bin/bash
# stop-k8s-deployment.sh - Stop Kubernetes pods and services

set -e

echo "=========================================="
echo "Stopping Kubernetes User Management API"
echo "=========================================="

# Check if we're connected to AKS cluster
if ! kubectl cluster-info > /dev/null 2>&1; then
    echo "Error: Not connected to Kubernetes cluster"
    echo "Run: az aks get-credentials --resource-group rg-user-management-api --name aks-user-management"
    exit 1
fi

# Scale deployment to 0 replicas (stops all pods but keeps configuration)
echo "Scaling deployment to 0 replicas..."
kubectl scale deployment user-management-api --replicas=0

# Wait for pods to terminate
echo "Waiting for pods to terminate..."
sleep 10

# Check if pods are stopped
POD_COUNT=$(kubectl get pods -l app=user-management-api --no-headers | wc -l)
if [ "$POD_COUNT" -eq 0 ]; then
    echo "✓ All API pods have been stopped"
else
    echo "⚠ Some pods may still be terminating:"
    kubectl get pods -l app=user-management-api
fi

# Stop PostgreSQL server to save costs
echo ""
echo "Stopping PostgreSQL server to save costs..."
cd terraform

POSTGRES_NAME=$(terraform output -raw postgres_server_name 2>/dev/null || echo "userapi-postgres")
RESOURCE_GROUP=$(terraform output -raw resource_group_name 2>/dev/null || echo "rg-user-management-api")

az postgres flexible-server stop \
    --resource-group "$RESOURCE_GROUP" \
    --name "$POSTGRES_NAME" \
    --no-wait

cd ..

echo ""
echo "✓ Kubernetes deployment stopped successfully!"
echo "✓ PostgreSQL server is stopping (saves costs)"
echo ""
echo "To restart: ./start-k8s-deployment.sh"
echo "To completely remove: kubectl delete -f terraform/kubernetes-manifests/"
echo ""