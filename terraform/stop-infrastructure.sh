#!/bin/bash
echo "Starting PostgreSQL server..."
az postgres flexible-server start --resource-group $(terraform output -raw resource_group_name) --name userapi-postgres

echo "Starting container instance..."
az container start --resource-group $(terraform output -raw resource_group_name) --name user-management-api

echo "Waiting for services to start..."
sleep 30

echo "API URL: $(terraform output -raw api_url)"
echo "Swagger: $(terraform output -raw api_url)/swagger"