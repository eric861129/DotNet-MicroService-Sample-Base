[CmdletBinding()]
param(
    [switch] $RequireAzureCli
)

. (Join-Path $PSScriptRoot 'common.ps1')

$repoRoot = Get-RepoRoot

Write-Step 'Checking prerequisites'

if (-not (Test-CommandAvailable 'dotnet')) {
    throw 'dotnet was not found. Install .NET SDK 9 or newer.'
}

$dotnetVersion = (& dotnet --version | Select-Object -First 1).Trim()
Write-Success "dotnet SDK: $dotnetVersion"

if (-not ($dotnetVersion.StartsWith('9.') -or $dotnetVersion.StartsWith('10.'))) {
    Write-Note 'This repo is validated with .NET 9/10 style SDKs.'
}

if (-not (Test-CommandAvailable 'docker')) {
    throw 'docker was not found. Install Docker Desktop first.'
}

try {
    & docker info | Out-Null
    if ($LASTEXITCODE -ne 0) {
        throw 'docker info returned a non-zero exit code.'
    }
}
catch {
    throw 'docker is installed, but the Docker daemon is not running.'
}

Write-Success 'docker command is available'

& docker compose version | Out-Null
Write-Success 'docker compose command is available'

if ($RequireAzureCli) {
    if (-not (Test-CommandAvailable 'az')) {
        throw 'Azure CLI was not found.'
    }

    & az bicep version | Out-Null
    Write-Success 'Azure CLI / Bicep commands are available'
}

$envPath = Ensure-EnvFile -RepoRoot $repoRoot
$envMap = Get-DotEnvMap -Path $envPath

Write-Step 'Checking configured ports'

$busyPorts = @()
foreach ($portInfo in Get-ConfiguredPorts -EnvironmentMap $envMap) {
    if (Test-PortAvailable -Port $portInfo.Port) {
        Write-Success "$($portInfo.Name) => $($portInfo.Port) is available"
    }
    else {
        $busyPorts += $portInfo
        Write-Host "  [WARN] $($portInfo.Name) => $($portInfo.Port) is already in use" -ForegroundColor Yellow
    }
}

if ($busyPorts.Count -gt 0) {
    throw 'One or more configured ports are already in use. Update .env or stop the conflicting process.'
}

Write-Step 'Preflight check completed'
Write-Success 'You can now run scripts/dev-up.ps1'
