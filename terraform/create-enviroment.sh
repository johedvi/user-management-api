#!/bin/bash

# create-environment.sh
# Recreates the entire Azure environment for User Management API

set -e  # Exit on any error

echo "=========================================="
echo "Creating User Management API Environment"
echo "=========================================="

# Check if required environment variables are set
if [[ -z "$DB_ADMIN_PASSWORD" ]]; then
    echo "Error: DB_ADMIN_PASSWORD environment variable is not set"
    echo "Set it with: export DB_ADMIN_PASSWORD='your-password'"
    exit 1
fi

if [[ -z "$JWT_SECRET_KEY" ]]; then
    echo "Error: JWT_SECRET_KEY environment variable is not set"
    echo "Set it with: export JWT_SECRET_KEY='your-jwt-secret'"
    exit 1
fi

# Configuration
SUBSCRIPTION_ID="56ed82a0-88f7-4dd4-9402-240515b36bfa"
RESOURCE_GROUP="rg-user-management-api"
LOCATION="Sweden Central"
STORAGE_ACCOUNT="tfstate112715"

echo "Configuration:"
echo "  Subscription: $SUBSCRIPTION_ID"
echo "  Resource Group: $RESOURCE_GROUP"
echo "  Location: $LOCATION"
echo "  Storage Account: $STORAGE_ACCOUNT"
echo ""

# Step 1: Login and set subscription
echo "Step 1: Verifying Azure login..."
az account show > /dev/null 2>&1 || {
    echo "Not logged in to Azure. Please run 'az login' first."
    exit 1
}

az account set --subscription "$SUBSCRIPTION_ID"
echo "✓ Using subscription: $(az account show --query name -o tsv)"

# Step 2: Create resource group if it doesn't exist
echo ""
echo "Step 2: Creating resource group..."
if az group show --name "$RESOURCE_GROUP" > /dev/null 2>&1; then
    echo "✓ Resource group $RESOURCE_GROUP already exists"
else
    az group create --name "$RESOURCE_GROUP" --location "$LOCATION"
    echo "✓ Created resource group: $RESOURCE_GROUP"
fi

# Step 3: Create storage account for Terraform state if it doesn't exist
echo ""
echo "Step 3: Setting up Terraform state storage..."
if az storage account show --name "$STORAGE_ACCOUNT" --resource-group "$RESOURCE_GROUP" > /dev/null 2>&1; then
    echo "✓ Storage account $STORAGE_ACCOUNT already exists"
else
    echo "Creating storage account for Terraform state..."
    az storage account create \
        --name "$STORAGE_ACCOUNT" \
        --resource-group "$RESOURCE_GROUP" \
        --location "$LOCATION" \
        --sku Standard_LRS
    echo "✓ Created storage account: $STORAGE_ACCOUNT"
fi

# Create storage container for Terraform state
if az storage container show --name tfstate --account-name "$STORAGE_ACCOUNT" > /dev/null 2>&1; then
    echo "✓ Storage container 'tfstate' already exists"
else
    az storage container create \
        --name tfstate \
        --account-name "$STORAGE_ACCOUNT"
    echo "✓ Created storage container: tfstate"
fi

# Step 4: Initialize and apply Terraform
echo ""
echo "Step 4: Deploying infrastructure with Terraform..."
cd terraform

# Check if terraform directory exists
if [[ ! -f "main.tf" ]]; then
    echo "Error: main.tf not found in terraform directory"
    echo "Make sure you're running this script from the project root"
    exit 1
fi

# Initialize Terraform
echo "Initializing Terraform..."
terraform init

# Validate Terraform configuration
echo "Validating Terraform configuration..."
terraform validate

# Plan Terraform deployment
echo "Planning Terraform deployment..."
terraform plan \
    -var="db_admin_password=$DB_ADMIN_PASSWORD" \
    -var="jwt_secret_key=$JWT_SECRET_KEY" \
    -out=tfplan

# Apply Terraform deployment
echo "Applying Terraform deployment..."
terraform apply tfplan

# Clean up plan file
rm -f tfplan

echo ""
echo "✓ Infrastructure deployment completed!"

# Step 5: Display outputs
echo ""
echo "Step 5: Environment information:"
echo "================================"

# Check if outputs exist and display them
if terraform output resource_group_name > /dev/null 2>&1; then
    echo "Resource Group: $(terraform output -raw resource_group_name)"
fi

if terraform output api_url > /dev/null 2>&1; then
    echo "API URL: $(terraform output -raw api_url)"
fi

if terraform output swagger_url > /dev/null 2>&1; then
    echo "Swagger Documentation: $(terraform output -raw swagger_url)"
fi

if terraform output postgres_server_fqdn > /dev/null 2>&1; then
    echo "PostgreSQL Server: $(terraform output -raw postgres_server_fqdn)"
fi

if terraform output container_registry_login_server > /dev/null 2>&1; then
    echo "Container Registry: $(terraform output -raw container_registry_login_server)"
fi

# Return to original directory
cd ..

echo ""
echo "=========================================="
echo "Environment creation completed successfully!"
echo "=========================================="
echo ""
echo "Next steps:"
echo "1. Wait for containers to start (may take a few minutes)"
echo "2. Test the API at the URL shown above"
echo "3. Use ./start-infrastructure.sh to start services"
echo "4. Use ./stop-infrastructure.sh to stop services"
echo ""
echo "To check service status:"
echo "  az container show --resource-group $RESOURCE_GROUP --name user-management-api --query instanceView.state"
echo ""