terraform {
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "3.44.1"
    }
  }

  backend "azurerm" {
    resource_group_name  = "tfstateresources"
    storage_account_name = "ngtfstate21823"
    container_name       = "tfstate"
    key                  = "terraform.tfstate"
  }
}

provider "azurerm" {
  features {

  }
}

resource "azurerm_resource_group" "mandelbrot" {
  name     = "mandelbrot-resources"
  location = "West US"
}

resource "azurerm_storage_account" "function_storage" {
  name                     = "functionsamandelbrot"
  resource_group_name      = azurerm_resource_group.mandelbrot.name
  location                 = azurerm_resource_group.mandelbrot.location
  account_tier             = "Standard"
  account_replication_type = "LRS"
}

resource "azurerm_service_plan" "mandelbrot_functions" {
  name                = "mb-app-service-plan"
  resource_group_name = azurerm_resource_group.mandelbrot.name
  location            = azurerm_resource_group.mandelbrot.location
  os_type             = "Linux"
  sku_name            = "Y1"
}

resource "azurerm_linux_function_app" "mandlebrot_function_app" {
  name                = "mandelbrot-function-app"
  resource_group_name = azurerm_resource_group.mandelbrot.name
  location            = azurerm_resource_group.mandelbrot.location

  storage_account_name       = azurerm_storage_account.function_storage.name
  storage_account_access_key = azurerm_storage_account.function_storage.primary_access_key
  service_plan_id            = azurerm_service_plan.mandelbrot_functions.id

  site_config {

  }
}

resource "azurerm_signalr_service" "signalr" {
  name                = "mandelbrot-ng-signalr"
  location            = azurerm_resource_group.mandelbrot.location
  resource_group_name = azurerm_resource_group.mandelbrot.name

  sku {
    name     = "Free_F1"
    capacity = 1
  }

  connectivity_logs_enabled = true
  messaging_logs_enabled    = true
  service_mode              = "Default"
}
