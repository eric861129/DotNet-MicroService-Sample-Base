[CmdletBinding()]
param()

. (Join-Path $PSScriptRoot 'common.ps1')

$repoRoot = Get-RepoRoot

Write-Step 'Checking prerequisites'

if (-not (Test-CommandAvailable 'dotnet')) {
    throw 'dotnet was not found. Install .NET SDK 9.0.x first.'
}

$dotnetVersion = (& dotnet --version | Select-Object -First 1).Trim()
Write-Success "dotnet SDK: $dotnetVersion"

if (-not $dotnetVersion.StartsWith('9.')) {
    Write-Note 'This repo is validated with .NET SDK 9.0.x.'
}

if (-not (Test-CommandAvailable 'docker')) {
    throw 'docker was not found. Install Docker Desktop first.'
}

try {
    & docker info | Out-Null
}
catch {
    throw 'docker is installed, but the Docker daemon is not running.'
}

Write-Success 'docker command is available'
& docker compose version | Out-Null
Write-Success 'docker compose command is available'

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
