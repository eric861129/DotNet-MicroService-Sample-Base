[CmdletBinding()]
param(
    [switch] $RemoveVolumes,
    [switch] $RemoveOrphans
)

. (Join-Path $PSScriptRoot 'common.ps1')

$repoRoot = Get-RepoRoot
Ensure-EnvFile -RepoRoot $repoRoot | Out-Null

$composeArgs = @('down')
if ($RemoveVolumes) {
    $composeArgs += '--volumes'
}

if ($RemoveOrphans) {
    $composeArgs += '--remove-orphans'
}

Write-Step 'Stopping docker compose services'
Invoke-Compose -RepoRoot $repoRoot -Arguments $composeArgs
Write-Success 'Environment stopped'
