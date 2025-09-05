# variables.tf
variable "resource_group_name" {
  description = "Name of the resource group"
  type        = string
  default     = "rg-user-management-api"
}

variable "location" {
  description = "Azure region"
  type        = string
  default     = "Sweden Central"
}

variable "registry_name" {
  description = "Base name for container registry (suffix will be added)"
  type        = string
  default     = "userapiregistry"
}

variable "postgres_server_name" {
  description = "PostgreSQL server name"
  type        = string
  default     = "userapi-postgres"
}

variable "postgres_sku" {
  description = "PostgreSQL SKU"
  type        = string
  default     = "B_Standard_B1ms"
}

variable "postgres_storage_mb" {
  description = "PostgreSQL storage in MB"
  type        = number
  default     = 32768
}

variable "database_name" {
  description = "Database name"
  type        = string
  default     = "usermanagementdb"
}

variable "container_name" {
  description = "Container instance name"
  type        = string
  default     = "user-management-api"
}

variable "dns_name_label" {
  description = "DNS name label for container (suffix will be added)"
  type        = string
  default     = "userapi09041259"
}

variable "container_cpu" {
  description = "Container CPU allocation"
  type        = string
  default     = "1.0"
}

variable "container_memory" {
  description = "Container memory allocation"
  type        = string
  default     = "2"
}

variable "db_admin_username" {
  description = "Database administrator username"
  type        = string
  default     = "dbadmin"
}

variable "db_admin_password" {
  description = "Database administrator password"
  type        = string
  sensitive   = true
}

variable "jwt_secret_key" {
  description = "JWT secret key"
  type        = string
  sensitive   = true
}

variable "jwt_issuer" {
  description = "JWT issuer"
  type        = string
  default     = "myapp"
}

variable "jwt_audience" {
  description = "JWT audience"
  type        = string
  default     = "myapp"
}

variable "tags" {
  description = "Tags to apply to resources"
  type        = map(string)
  default = {
    Environment = "Development"
    Project     = "UserManagementAPI"
    ManagedBy   = "Terraform"
  }
}