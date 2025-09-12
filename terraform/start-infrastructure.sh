#!/bin/bash
set -e

cd terraform

echo "Starting PostgreSQL server..."
POSTGRES_NAME=$(terraform output -raw postgres_server_name 2>/dev/null || echo "userapi-postgres")
RESOURCE_GROUP=$(terraform output -raw resource_group_name 2>/dev/null || echo "rg-user-management-api")

az postgres flexible-server start \
    --resource-group "$RESOURCE_GROUP" \
    --name "$POSTGRES_NAME"

echo "Restarting container instance..."
az container restart \
    --resource-group "$RESOURCE_GROUP" \
    --name user-management-api

echo "Waiting for services to start..."
sleep 30

if terraform output api_url >/dev/null 2>&1; then
    echo "API URL: $(terraform output -raw api_url)"
    echo "Swagger: $(terraform output -raw swagger_url)"
else
    echo "Services started, but no Terraform outputs available"
fi

cd ..