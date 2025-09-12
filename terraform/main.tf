# main.tf
terraform {
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~>3.0"
    }
    random = {
      source  = "hashicorp/random"
      version = "~>3.1"
    }
  }
  
  # ADD THIS BACKEND CONFIGURATION
  backend "azurerm" {
    resource_group_name  = "rg-user-management-api"
    storage_account_name = "tfstate112715"
    container_name       = "tfstate"
    key                  = "terraform.tfstate"
  }
}

provider "azurerm" {
  features {}
}

# Resource Group
resource "azurerm_resource_group" "main" {
  name     = var.resource_group_name
  location = var.location
}

# Storage Account for Terraform State
resource "azurerm_storage_account" "tfstate" {
  name                     = "tfstate112715"
  resource_group_name      = azurerm_resource_group.main.name
  location                 = azurerm_resource_group.main.location
  account_tier             = "Standard"
  account_replication_type = "LRS"
  
  tags = var.tags
}

resource "azurerm_storage_container" "tfstate" {
  name                  = "tfstate"
  storage_account_name  = azurerm_storage_account.tfstate.name
  container_access_type = "private"
}

# Container Registry
resource "azurerm_container_registry" "main" {
  name                = "userapiregistry"
  resource_group_name = azurerm_resource_group.main.name
  location            = azurerm_resource_group.main.location
  sku                 = "Basic"
  admin_enabled       = true
  
  tags = var.tags
}

# PostgreSQL Flexible Server
resource "azurerm_postgresql_flexible_server" "main" {
  name                   = var.postgres_server_name
  resource_group_name    = azurerm_resource_group.main.name
  location               = azurerm_resource_group.main.location
  version                = "15"
  administrator_login    = var.db_admin_username
  administrator_password = var.db_admin_password
  
  sku_name   = var.postgres_sku
  storage_mb = var.postgres_storage_mb
  
  backup_retention_days        = 7
  geo_redundant_backup_enabled = false
  
  tags = var.tags
  
  lifecycle {
    ignore_changes = [zone]
  }
}

# Database
resource "azurerm_postgresql_flexible_server_database" "main" {
  name      = var.database_name
  server_id = azurerm_postgresql_flexible_server.main.id
  collation = "en_US.utf8"
  charset   = "utf8"
}

# PostgreSQL Firewall Rule - Allow all IPs (for development)
resource "azurerm_postgresql_flexible_server_firewall_rule" "allow_all" {
  name             = "AllowAll"
  server_id        = azurerm_postgresql_flexible_server.main.id
  start_ip_address = "0.0.0.0"
  end_ip_address   = "255.255.255.255"
}

# Container Instance
resource "azurerm_container_group" "main" {
  name                = "user-management-api"
  location            = azurerm_resource_group.main.location
  resource_group_name = azurerm_resource_group.main.name
  ip_address_type     = "Public"
  dns_name_label      = "userapi09041259"
  os_type             = "Linux"
  
  container {
    name   = "user-management-api"
    image  = "${azurerm_container_registry.main.login_server}/user-management-api:latest"
    cpu    = var.container_cpu
    memory = var.container_memory
    
    ports {
      port     = 8080
      protocol = "TCP"
    }
    
    environment_variables = {
      "ConnectionStrings__DefaultConnection" = "Server=${azurerm_postgresql_flexible_server.main.fqdn};Database=${var.database_name};Port=5432;User Id=${var.db_admin_username};Password=${var.db_admin_password};Ssl Mode=Require;"
      "Jwt__Key"                            = var.jwt_secret_key
      "Jwt__Issuer"                         = var.jwt_issuer
      "Jwt__Audience"                       = var.jwt_audience
      "ASPNETCORE_URLS"                     = "http://0.0.0.0:8080"
      "AllowedHosts"                        = "*"
    }
  }
  
  image_registry_credential {
    server   = azurerm_container_registry.main.login_server
    username = azurerm_container_registry.main.admin_username
    password = azurerm_container_registry.main.admin_password
  }
  
  tags = var.tags
}