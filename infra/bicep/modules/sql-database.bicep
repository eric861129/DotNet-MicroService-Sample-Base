param sqlServerName string
param databases array

resource sqlServer 'Microsoft.Sql/servers@2023-08-01-preview' existing = {
  name: sqlServerName
}

resource sqlDatabases 'Microsoft.Sql/servers/databases@2023-08-01-preview' = [for dbName in databases: {
  name: dbName
  parent: sqlServer
  location: resourceGroup().location
  sku: {
    name: 'Basic'
    tier: 'Basic'
  }
}]
