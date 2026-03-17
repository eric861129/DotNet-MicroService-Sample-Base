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
    'ordering-db'
)

$applicationServices = @(
    'auth-service-api',
    'catalog-service-api',
    'ordering-service-api',
    'gateway-api'
)

if ($InfrastructureOnly) {
    Write-Step 'Starting database containers only'
    Invoke-Compose -RepoRoot $repoRoot -Arguments (@('up', '-d') + $infraServices)
}
else {
    Write-Step 'Starting the Base Lite stack'
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
Write-Note "Ordering Service: http://localhost:$($envMap['ORDERING_SERVICE_PORT'])"
