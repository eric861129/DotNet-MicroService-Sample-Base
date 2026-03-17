[CmdletBinding()]
param(
    [switch] $WithContainerTests,
    [switch] $AgainstRunningStack
)

. (Join-Path $PSScriptRoot 'common.ps1')

$repoRoot = Get-RepoRoot
$envPath = Ensure-EnvFile -RepoRoot $repoRoot
$envMap = Get-DotEnvMap -Path $envPath

& (Join-Path $PSScriptRoot 'check-prereqs.ps1')

Write-Step 'Running restore / build / test'
Invoke-DotNet -RepoRoot $repoRoot -Arguments @('restore', 'EnterpriseMicroservicesBoilerplate.sln')
Invoke-DotNet -RepoRoot $repoRoot -Arguments @('build', 'EnterpriseMicroservicesBoilerplate.sln', '-c', 'Debug', '--no-restore')

$previousFlag = $env:RUN_CONTAINER_TESTS
try {
    if ($WithContainerTests) {
        $env:RUN_CONTAINER_TESTS = 'true'
        Write-Note 'Enabled Testcontainers smoke test'
    }

    Invoke-DotNet -RepoRoot $repoRoot -Arguments @('test', 'EnterpriseMicroservicesBoilerplate.sln', '-c', 'Debug', '--no-build')
}
finally {
    $env:RUN_CONTAINER_TESTS = $previousFlag
}

if ($AgainstRunningStack) {
    Write-Step 'Checking running service endpoints'

    $healthChecks = @(
        @{ Name = 'Gateway'; Url = "http://localhost:$($envMap['GATEWAY_PORT'])/" },
        @{ Name = 'AuthService'; Url = "http://localhost:$($envMap['AUTH_SERVICE_PORT'])/connect/health" },
        @{ Name = 'CatalogService'; Url = "http://localhost:$($envMap['CATALOG_SERVICE_PORT'])/health" },
        @{ Name = 'InventoryService'; Url = "http://localhost:$($envMap['INVENTORY_SERVICE_PORT'])/health" },
        @{ Name = 'OrderingService'; Url = "http://localhost:$($envMap['ORDERING_SERVICE_PORT'])/health" },
        @{ Name = 'NotificationService'; Url = "http://localhost:$($envMap['NOTIFICATION_SERVICE_PORT'])/health" }
    )

    foreach ($check in $healthChecks) {
        $response = Invoke-WebRequest -Uri $check.Url -UseBasicParsing -TimeoutSec 10
        if ($response.StatusCode -lt 200 -or $response.StatusCode -ge 300) {
            throw "$($check.Name) failed with HTTP $($response.StatusCode)"
        }

        Write-Success "$($check.Name) is reachable"
    }
}
