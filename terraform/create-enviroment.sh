#!/bin/bash
set -e

echo "=========================================="
echo "Creating User Management API Environment"
echo "=========================================="

# Check required environment variables
if [[ -z "$DB_ADMIN_PASSWORD" || -z "$JWT_SECRET_KEY" ]]; then
    echo "Error: Required environment variables not set"
    echo "Set them with:"
    echo "  export DB_ADMIN_PASSWORD='your-password'"
    echo "  export JWT_SECRET_KEY='your-jwt-secret'"
    exit 1
fi

echo "Deploying infrastructure with Terraform..."
cd terraform

terraform init
terraform plan -var="db_admin_password=$DB_ADMIN_PASSWORD" -var="jwt_secret_key=$JWT_SECRET_KEY"
terraform apply -var="db_admin_password=$DB_ADMIN_PASSWORD" -var="jwt_secret_key=$JWT_SECRET_KEY"

echo "✓ Infrastructure deployment completed!"
echo "API URL: $(terraform output -raw api_url)"
echo "Swagger: $(terraform output -raw swagger_url)"

cd ..