# outputs.tf
output "api_url" {
  description = "URL of the deployed API"
  value       = "http://${azurerm_container_group.main.fqdn}:8080"
}

output "swagger_url" {
  description = "Swagger documentation URL"
  value       = "http://${azurerm_container_group.main.fqdn}:8080/swagger/index.html"
}

output "container_registry_name" {
  description = "Name of the container registry"
  value       = azurerm_container_registry.main.name
}

output "container_registry_login_server" {
  description = "Login server URL of the container registry"
  value       = azurerm_container_registry.main.login_server
}

output "postgres_server_fqdn" {
  description = "FQDN of the PostgreSQL server"
  value       = azurerm_postgresql_flexible_server.main.fqdn
}

output "frontend_url" {
  description = "URL of the frontend static web app"
  value       = "https://${azurerm_static_web_app.frontend.default_host_name}"
}

output "static_web_app_token" {
  description = "API token for Static Web App deployment"
  value       = azurerm_static_web_app.frontend.api_key
  sensitive   = true
}

output "postgres_database_name" {
  description = "Name of the created database"
  value       = azurerm_postgresql_flexible_server_database.main.name
}

output "resource_group_name" {
  description = "Name of the resource group"
  value       = azurerm_resource_group.main.name
}