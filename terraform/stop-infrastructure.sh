#!/bin/bash
echo "Stopping container instance..."
az container stop --resource-group $(terraform output -raw resource_group_name) --name user-management-api

echo "Stopping PostgreSQL server..."
az postgres flexible-server stop --resource-group $(terraform output -raw resource_group_name) --name userapi-postgres

echo "Infrastructure stopped. This will reduce costs significantly."