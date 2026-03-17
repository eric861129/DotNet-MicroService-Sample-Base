[CmdletBinding()]
param(
    [switch] $KeepEnvFile
)

. (Join-Path $PSScriptRoot 'common.ps1')

$repoRoot = Get-RepoRoot

Write-Step 'Stopping containers and removing volumes'
& (Join-Path $PSScriptRoot 'dev-down.ps1') -RemoveVolumes -RemoveOrphans

Write-Step 'Cleaning local build output'
$cleanupTargets = Get-ChildItem -Path $repoRoot -Recurse -Directory -Force |
    Where-Object { $_.Name -in @('bin', 'obj', 'TestResults', '.vs') }

foreach ($target in $cleanupTargets) {
    cmd /c rmdir /s /q "$($target.FullName)" 2>$null
}

Write-Success 'Removed bin / obj / TestResults / .vs'

if (-not $KeepEnvFile) {
    $envPath = Join-Path $repoRoot '.env'
    if (Test-Path $envPath) {
        cmd /c del /q "$envPath"
        Write-Success 'Removed .env'
    }
}
