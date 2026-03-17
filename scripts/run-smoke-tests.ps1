[CmdletBinding()]
param(
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
Invoke-DotNet -RepoRoot $repoRoot -Arguments @('test', 'EnterpriseMicroservicesBoilerplate.sln', '-c', 'Debug', '--no-build')

if ($AgainstRunningStack) {
    Write-Step 'Checking running service endpoints'

    $healthChecks = @(
        @{ Name = 'Gateway'; Url = "http://localhost:$($envMap['GATEWAY_PORT'])/" },
        @{ Name = 'AuthService'; Url = "http://localhost:$($envMap['AUTH_SERVICE_PORT'])/" },
        @{ Name = 'CatalogService'; Url = "http://localhost:$($envMap['CATALOG_SERVICE_PORT'])/health" },
        @{ Name = 'OrderingService'; Url = "http://localhost:$($envMap['ORDERING_SERVICE_PORT'])/health" }
    )

    foreach ($check in $healthChecks) {
        $response = Invoke-WebRequest -Uri $check.Url -UseBasicParsing -TimeoutSec 10
        if ($response.StatusCode -lt 200 -or $response.StatusCode -ge 300) {
            throw "$($check.Name) failed with HTTP $($response.StatusCode)"
        }

        Write-Success "$($check.Name) is reachable"
    }

    Write-Step 'Running Gateway smoke flow'
    $tokenResponse = Invoke-RestMethod -Method Post `
        -Uri "http://localhost:$($envMap['AUTH_SERVICE_PORT'])/connect/token" `
        -ContentType 'application/x-www-form-urlencoded' `
        -Body "grant_type=client_credentials&client_id=$($envMap['GATEWAY_CLIENT_ID'])&client_secret=$($envMap['GATEWAY_CLIENT_SECRET'])&scope=catalog.read ordering.write"

    $headers = @{
        Authorization = "Bearer $($tokenResponse.access_token)"
    }

    $sku = "SMOKE-$([Guid]::NewGuid().ToString('N').Substring(0, 8).ToUpperInvariant())"
    $product = Invoke-RestMethod -Method Post `
        -Uri "http://localhost:$($envMap['GATEWAY_PORT'])/catalog/products" `
        -Headers $headers `
        -ContentType 'application/json' `
        -Body (@{
            sku = $sku
            name = 'Smoke Test Product'
            price = 99.50
        } | ConvertTo-Json)

    $order = Invoke-RestMethod -Method Post `
        -Uri "http://localhost:$($envMap['GATEWAY_PORT'])/ordering/orders" `
        -Headers $headers `
        -ContentType 'application/json' `
        -Body (@{
            customerEmail = 'smoke@example.com'
            items = @(
                @{
                    productId = $product.productId
                    quantity = 1
                }
            )
        } | ConvertTo-Json -Depth 4)

    Write-Success "Created smoke product: $($product.productId)"
    Write-Success "Created smoke order: $($order.orderId)"
}
