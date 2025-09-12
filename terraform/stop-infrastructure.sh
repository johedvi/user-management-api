#!/bin/bash
set -e

cd terraform

echo "Stopping PostgreSQL server..."
POSTGRES_NAME=$(terraform output -raw postgres_server_name 2>/dev/null || echo "userapi-postgres")
RESOURCE_GROUP=$(terraform output -raw resource_group_name 2>/dev/null || echo "rg-user-management-api")

az postgres flexible-server stop \
    --resource-group "$RESOURCE_GROUP" \
    --name "$POSTGRES_NAME"

echo "Note: Container instances cannot be stopped individually."
echo "The container will continue running but PostgreSQL is stopped to save costs."
echo ""
echo "To fully stop everything, use: terraform destroy"

cd ..