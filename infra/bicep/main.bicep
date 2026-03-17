@description('部署環境代碼，例如 dev、qa、prod。')
param environmentName string = 'dev'

@description('Azure 部署區域。')
param location string = resourceGroup().location

@description('Container Image Tag。')
param imageTag string = 'latest'

@description('Container Registry Login Server，例如 myacr.azurecr.io。')
param containerRegistryServer string

@description('Container Registry Username。')
param containerRegistryUsername string

@secure()
@description('Container Registry Password。')
param containerRegistryPassword string

var prefix = 'ems-${environmentName}'

resource logAnalytics 'Microsoft.OperationalInsights/workspaces@2023-09-01' = {
  name: '${prefix}-law'
  location: location
  properties: {
    sku: {
      name: 'PerGB2018'
    }
    retentionInDays: 30
  }
}

resource appConfig 'Microsoft.AppConfiguration/configurationStores@2024-05-01' = {
  name: '${prefix}-appcfg'
  location: location
  sku: {
    name: 'standard'
  }
}

resource keyVault 'Microsoft.KeyVault/vaults@2023-02-01' = {
  name: '${prefix}-kv'
  location: location
  properties: {
    tenantId: tenant().tenantId
    sku: {
      family: 'A'
      name: 'standard'
    }
    accessPolicies: []
    enableRbacAuthorization: true
    publicNetworkAccess: 'Enabled'
  }
}

resource serviceBus 'Microsoft.ServiceBus/namespaces@2024-01-01' = {
  name: '${prefix}-sb'
  location: location
  sku: {
    name: 'Standard'
    tier: 'Standard'
  }
}

resource sqlServer 'Microsoft.Sql/servers@2023-08-01-preview' = {
  name: '${prefix}-sql'
  location: location
  properties: {
    administratorLogin: 'sqladminuser'
    administratorLoginPassword: 'ChangeM3InVault!'
    publicNetworkAccess: 'Enabled'
    version: '12.0'
  }
}

module sqlDatabases './modules/sql-database.bicep' = {
  name: 'sql-databases'
  params: {
    sqlServerName: sqlServer.name
    databases: [
      'CatalogDb'
      'OrderingDb'
      'InventoryDb'
      'NotificationDb'
      'AuthDb'
    ]
  }
}

resource managedEnvironment 'Microsoft.App/managedEnvironments@2024-03-01' = {
  name: '${prefix}-aca-env'
  location: location
  properties: {
    appLogsConfiguration: {
      destination: 'log-analytics'
      logAnalyticsConfiguration: {
        customerId: logAnalytics.properties.customerId
        sharedKey: listKeys(logAnalytics.id, logAnalytics.apiVersion).primarySharedKey
      }
    }
  }
}

module gatewayApp './modules/container-app.bicep' = {
  name: 'gateway-app'
  params: {
    environmentName: managedEnvironment.name
    appName: '${prefix}-gateway'
    image: '${containerRegistryServer}/gateway-api:${imageTag}'
    targetPort: 8080
    ingressExternal: true
    registryServer: containerRegistryServer
    registryUsername: containerRegistryUsername
    registryPassword: containerRegistryPassword
  }
}

module authApp './modules/container-app.bicep' = {
  name: 'auth-app'
  params: {
    environmentName: managedEnvironment.name
    appName: '${prefix}-auth'
    image: '${containerRegistryServer}/auth-service-api:${imageTag}'
    targetPort: 8080
    ingressExternal: false
    registryServer: containerRegistryServer
    registryUsername: containerRegistryUsername
    registryPassword: containerRegistryPassword
  }
}

module catalogApp './modules/container-app.bicep' = {
  name: 'catalog-app'
  params: {
    environmentName: managedEnvironment.name
    appName: '${prefix}-catalog'
    image: '${containerRegistryServer}/catalog-service-api:${imageTag}'
    targetPort: 8080
    ingressExternal: false
    registryServer: containerRegistryServer
    registryUsername: containerRegistryUsername
    registryPassword: containerRegistryPassword
  }
}

module inventoryApp './modules/container-app.bicep' = {
  name: 'inventory-app'
  params: {
    environmentName: managedEnvironment.name
    appName: '${prefix}-inventory'
    image: '${containerRegistryServer}/inventory-service-api:${imageTag}'
    targetPort: 8080
    ingressExternal: false
    registryServer: containerRegistryServer
    registryUsername: containerRegistryUsername
    registryPassword: containerRegistryPassword
  }
}

module orderingApp './modules/container-app.bicep' = {
  name: 'ordering-app'
  params: {
    environmentName: managedEnvironment.name
    appName: '${prefix}-ordering'
    image: '${containerRegistryServer}/ordering-service-api:${imageTag}'
    targetPort: 8080
    ingressExternal: false
    registryServer: containerRegistryServer
    registryUsername: containerRegistryUsername
    registryPassword: containerRegistryPassword
  }
}

module notificationApp './modules/container-app.bicep' = {
  name: 'notification-app'
  params: {
    environmentName: managedEnvironment.name
    appName: '${prefix}-notification'
    image: '${containerRegistryServer}/notification-service-api:${imageTag}'
    targetPort: 8080
    ingressExternal: false
    registryServer: containerRegistryServer
    registryUsername: containerRegistryUsername
    registryPassword: containerRegistryPassword
  }
}
