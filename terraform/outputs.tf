# outputs.tf
output "api_url" {
  description = "URL of the deployed API"
  value       = "http://${azurerm_container_group.main.fqdn}:8080"
}

output "swagger_url" {
  description = "Swagger documentation URL"
  value       = "http://${azurerm_container_group.main.fqdn}:8080/swagger"
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

output "postgres_database_name" {
  description = "Name of the created database"
  value       = azurerm_postgresql_flexible_server_database.main.name
}

output "resource_group_name" {
  description = "Name of the resource group"
  value       = azurerm_resource_group.main.name
}