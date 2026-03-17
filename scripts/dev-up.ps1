[CmdletBinding()]
param(
    [switch] $InfrastructureOnly,
    [switch] $SkipPrereqs,
    [switch] $NoBuild
)

. (Join-Path $PSScriptRoot 'common.ps1')

$repoRoot = Get-RepoRoot
$envPath = Ensure-EnvFile -RepoRoot $repoRoot
$envMap = Get-DotEnvMap -Path $envPath

if (-not $SkipPrereqs) {
    & (Join-Path $PSScriptRoot 'check-prereqs.ps1')
}

$infraServices = @(
    'catalog-db',
    'ordering-db',
    'inventory-db',
    'notification-db',
    'auth-db',
    'rabbitmq',
    'otel-collector',
    'prometheus',
    'loki',
    'tempo',
    'grafana'
)

$applicationServices = @(
    'auth-service-api',
    'catalog-service-api',
    'inventory-service-api',
    'ordering-service-api',
    'notification-service-api',
    'gateway-api'
)

if ($InfrastructureOnly) {
    Write-Step 'Starting infrastructure containers only'
    Invoke-Compose -RepoRoot $repoRoot -Arguments (@('up', '-d') + $infraServices)
}
else {
    Write-Step 'Starting the full development stack'
    $composeArgs = @('up')
    if (-not $NoBuild) {
        $composeArgs += '--build'
    }

    $composeArgs += '-d'
    Invoke-Compose -RepoRoot $repoRoot -Arguments ($composeArgs + $infraServices + $applicationServices)
}

Write-Step 'Useful endpoints'
Write-Note "Gateway: http://localhost:$($envMap['GATEWAY_PORT'])"
Write-Note "Auth Service: http://localhost:$($envMap['AUTH_SERVICE_PORT'])"
Write-Note "Catalog Service: http://localhost:$($envMap['CATALOG_SERVICE_PORT'])"
Write-Note "Inventory Service: http://localhost:$($envMap['INVENTORY_SERVICE_PORT'])"
Write-Note "Ordering Service: http://localhost:$($envMap['ORDERING_SERVICE_PORT'])"
Write-Note "Notification Service: http://localhost:$($envMap['NOTIFICATION_SERVICE_PORT'])"
Write-Note "RabbitMQ: http://localhost:$($envMap['RABBITMQ_MANAGEMENT_PORT'])"
Write-Note "Grafana: http://localhost:$($envMap['GRAFANA_PORT'])"
