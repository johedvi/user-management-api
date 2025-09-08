#!/bin/bash

# Start PostgreSQL
echo "Starting PostgreSQL server..."
POSTGRES_NAME=$(terraform output -raw postgres_server_fqdn | cut -d'.' -f1)
az postgres flexible-server start --resource-group $(terraform output -raw resource_group_name) --name $POSTGRES_NAME

# Start container instance
echo "Starting container instance..."
az container start --resource-group $(terraform output -raw resource_group_name) --name user-management-api

# Wait and show status
echo "Waiting for services to start..."
sleep 30

echo "API URL: $(terraform output -raw api_url)"
echo "Swagger: $(terraform output -raw swagger_url)"