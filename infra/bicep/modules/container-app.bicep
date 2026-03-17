param environmentName string
param appName string
param image string
param targetPort int = 8080
param ingressExternal bool = false
param registryServer string
param registryUsername string
@secure()
param registryPassword string

resource containerApp 'Microsoft.App/containerApps@2024-03-01' = {
  name: appName
  location: resourceGroup().location
  properties: {
    managedEnvironmentId: resourceId('Microsoft.App/managedEnvironments', environmentName)
    configuration: {
      registries: [
        {
          server: registryServer
          username: registryUsername
          passwordSecretRef: 'acr-password'
        }
      ]
      secrets: [
        {
          name: 'acr-password'
          value: registryPassword
        }
      ]
      ingress: {
        external: ingressExternal
        targetPort: targetPort
        transport: 'auto'
      }
    }
    template: {
      containers: [
        {
          name: appName
          image: image
          resources: {
            cpu: json('0.5')
            memory: '1Gi'
          }
        }
      ]
      scale: {
        minReplicas: 1
        maxReplicas: 3
      }
    }
  }
}
