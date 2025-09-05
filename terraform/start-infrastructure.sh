# Start PostgreSQL
Write-Host "Starting PostgreSQL server..."
az postgres flexible-server start --resource-group $(terraform output -raw resource_group_name) --name $(terraform output -raw postgres_server_fqdn | Split-Path -Leaf)

# Start container instance
Write-Host "Starting container instance..."
az container start --resource-group $(terraform output -raw resource_group_name) --name user-management-api

# Wait and show status
Start-Sleep -Seconds 30
Write-Host "API URL: $(terraform output -raw api_url)"
Write-Host "Swagger: $(terraform output -raw swagger_url)"