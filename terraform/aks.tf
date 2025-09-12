# aks.tf - Add this to your terraform/ directory

# AKS Cluster
resource "azurerm_kubernetes_cluster" "main" {
  name                = "aks-user-management"
  location            = azurerm_resource_group.main.location
  resource_group_name = azurerm_resource_group.main.name
  dns_prefix          = "aks-user-mgmt"
  
  # Cost-effective node pool for learning
  default_node_pool {
    name       = "default"
    node_count = 1
    vm_size    = "Standard_B2s"  # 2 vCPU, 4GB RAM - affordable
    
    # Enable auto-scaling (optional)
    enable_auto_scaling = true
    min_count          = 1
    max_count          = 3
  }

  # Managed identity for AKS cluster
  identity {
    type = "SystemAssigned"
  }

  # Network configuration
  network_profile {
    network_plugin = "kubenet"
    load_balancer_sku = "standard"
  }

  # Enable RBAC
  role_based_access_control_enabled = true

  tags = var.tags
}

# Grant AKS permission to pull from ACR
resource "azurerm_role_assignment" "aks_acr_pull" {
  principal_id                     = azurerm_kubernetes_cluster.main.kubelet_identity[0].object_id
  role_definition_name             = "AcrPull"
  scope                           = azurerm_container_registry.main.id
  skip_service_principal_aad_check = true
}

# Output AKS cluster information
output "aks_cluster_name" {
  description = "Name of the AKS cluster"
  value       = azurerm_kubernetes_cluster.main.name
}

output "aks_cluster_fqdn" {
  description = "FQDN of the AKS cluster"
  value       = azurerm_kubernetes_cluster.main.fqdn
}

output "aks_node_resource_group" {
  description = "Auto-created resource group for AKS nodes"
  value       = azurerm_kubernetes_cluster.main.node_resource_group
}