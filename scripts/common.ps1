Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function Get-RepoRoot {
    return (Split-Path -Parent $PSScriptRoot)
}

function Write-Step {
    param([Parameter(Mandatory)] [string] $Message)

    Write-Host ''
    Write-Host "==> $Message" -ForegroundColor Cyan
}

function Write-Note {
    param([Parameter(Mandatory)] [string] $Message)

    Write-Host "  - $Message" -ForegroundColor DarkGray
}

function Write-Success {
    param([Parameter(Mandatory)] [string] $Message)

    Write-Host "  [OK] $Message" -ForegroundColor Green
}

function Ensure-EnvFile {
    param([Parameter(Mandatory)] [string] $RepoRoot)

    $envPath = Join-Path $RepoRoot '.env'
    $examplePath = Join-Path $RepoRoot '.env.example'

    if (-not (Test-Path $examplePath)) {
        throw 'Cannot find .env.example.'
    }

    if (-not (Test-Path $envPath)) {
        Copy-Item $examplePath $envPath
        Write-Success 'Created .env from .env.example.'
    }

    return $envPath
}

function Get-DotEnvMap {
    param([Parameter(Mandatory)] [string] $Path)

    $values = @{}
    foreach ($line in Get-Content $Path) {
        $trimmed = $line.Trim()
        if ([string]::IsNullOrWhiteSpace($trimmed) -or $trimmed.StartsWith('#')) {
            continue
        }

        $separatorIndex = $trimmed.IndexOf('=')
        if ($separatorIndex -lt 1) {
            continue
        }

        $key = $trimmed.Substring(0, $separatorIndex).Trim()
        $value = $trimmed.Substring($separatorIndex + 1).Trim()
        $values[$key] = $value
    }

    return $values
}

function Test-CommandAvailable {
    param([Parameter(Mandatory)] [string] $CommandName)

    return [bool](Get-Command $CommandName -ErrorAction SilentlyContinue)
}

function Test-PortAvailable {
    param([Parameter(Mandatory)] [int] $Port)

    $listeners = [System.Net.NetworkInformation.IPGlobalProperties]::GetIPGlobalProperties().GetActiveTcpListeners()
    return -not ($listeners | Where-Object Port -eq $Port)
}

function Get-ConfiguredPorts {
    param([Parameter(Mandatory)] [hashtable] $EnvironmentMap)

    $keys = @(
        'GATEWAY_PORT',
        'AUTH_SERVICE_PORT',
        'CATALOG_SERVICE_PORT',
        'INVENTORY_SERVICE_PORT',
        'ORDERING_SERVICE_PORT',
        'NOTIFICATION_SERVICE_PORT',
        'CATALOG_DB_PORT',
        'ORDERING_DB_PORT',
        'INVENTORY_DB_PORT',
        'NOTIFICATION_DB_PORT',
        'AUTH_DB_PORT',
        'RABBITMQ_AMQP_PORT',
        'RABBITMQ_MANAGEMENT_PORT',
        'OTEL_GRPC_PORT',
        'OTEL_HTTP_PORT',
        'PROMETHEUS_PORT',
        'LOKI_PORT',
        'TEMPO_PORT',
        'GRAFANA_PORT'
    )

    foreach ($key in $keys) {
        if ($EnvironmentMap.ContainsKey($key) -and $EnvironmentMap[$key]) {
            [pscustomobject]@{
                Name = $key
                Port = [int]$EnvironmentMap[$key]
            }
        }
    }
}

function Invoke-Compose {
    param(
        [Parameter(Mandatory)] [string] $RepoRoot,
        [Parameter(Mandatory)] [string[]] $Arguments
    )

    Push-Location $RepoRoot
    try {
        & docker compose --env-file .env @Arguments
    }
    finally {
        Pop-Location
    }
}

function Invoke-DotNet {
    param(
        [Parameter(Mandatory)] [string] $RepoRoot,
        [Parameter(Mandatory)] [string[]] $Arguments
    )

    Push-Location $RepoRoot
    try {
        & dotnet @Arguments
    }
    finally {
        Pop-Location
    }
}

function Get-PascalCaseName {
    param([Parameter(Mandatory)] [string] $Value)

    $parts = @([regex]::Matches($Value, '[A-Za-z0-9]+') | ForEach-Object { $_.Value })
    if ($parts.Count -eq 0) {
        throw 'Name must contain at least one alphanumeric segment.'
    }

    return ($parts | ForEach-Object {
        if ($_.Length -eq 1) {
            $_.ToUpperInvariant()
        }
        else {
            $_.Substring(0, 1).ToUpperInvariant() + $_.Substring(1).ToLowerInvariant()
        }
    }) -join ''
}

function Get-KebabCaseName {
    param([Parameter(Mandatory)] [string] $Value)

    return ([regex]::Replace($Value, '(?<!^)([A-Z])', '-$1')).ToLowerInvariant()
}
