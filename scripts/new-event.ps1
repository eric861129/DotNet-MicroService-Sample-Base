[CmdletBinding()]
param(
    [Parameter(Mandatory)] [string] $EventName,
    [Parameter(Mandatory)] [string] $ContractsProject,
    [string] $ConsumerProject,
    [switch] $DryRun
)

. (Join-Path $PSScriptRoot 'common.ps1')

$repoRoot = Get-RepoRoot
$normalizedEventName = Get-PascalCaseName -Value $EventName

if (-not $normalizedEventName.EndsWith('IntegrationEvent', [System.StringComparison]::Ordinal)) {
    $eventClassName = "$normalizedEventName" + 'IntegrationEvent'
}
else {
    $eventClassName = $normalizedEventName
}

function Resolve-ProjectPath {
    param(
        [string] $RepoRoot,
        [string] $PathValue
    )

    if ([System.IO.Path]::IsPathRooted($PathValue)) {
        return $PathValue
    }

    return (Join-Path $RepoRoot $PathValue)
}

$contractsProjectPath = Resolve-ProjectPath -RepoRoot $repoRoot -PathValue $ContractsProject
if (-not (Test-Path $contractsProjectPath)) {
    throw "Contracts project was not found: $contractsProjectPath"
}

$contractsDirectory = Split-Path -Parent $contractsProjectPath
$contractsNamespace = [System.IO.Path]::GetFileNameWithoutExtension($contractsProjectPath)
$eventPath = Join-Path $contractsDirectory "$eventClassName.cs"

$eventContent = @"
using Enterprise.Messaging;

namespace $contractsNamespace;

public sealed record $eventClassName : IntegrationEvent
{
    public Guid EntityId { get; init; }
}
"@

if ($DryRun) {
    Write-Step 'Dry run'
    Write-Note "Would create event file: $eventPath"
}
else {
    Set-Content -Path $eventPath -Value $eventContent -Encoding utf8
    Write-Success "Created event file: $eventPath"
}

if ($ConsumerProject) {
    $consumerProjectPath = Resolve-ProjectPath -RepoRoot $repoRoot -PathValue $ConsumerProject
    if (-not (Test-Path $consumerProjectPath)) {
        throw "Consumer project was not found: $consumerProjectPath"
    }

    $consumerDirectory = Split-Path -Parent $consumerProjectPath
    $consumerNamespace = [System.IO.Path]::GetFileNameWithoutExtension($consumerProjectPath)
    $consumerClassName = $eventClassName.Replace('IntegrationEvent', 'Consumer')
    $consumerPath = Join-Path $consumerDirectory "$consumerClassName.cs"

    $consumerContent = @"
using MassTransit;
using $contractsNamespace;

namespace $consumerNamespace;

public sealed class $consumerClassName : IConsumer<$eventClassName>
{
    public Task Consume(ConsumeContext<$eventClassName> context)
    {
        return Task.CompletedTask;
    }
}
"@

    if ($DryRun) {
        Write-Note "Would create consumer file: $consumerPath"
    }
    else {
        Set-Content -Path $consumerPath -Value $consumerContent -Encoding utf8
        Write-Success "Created consumer file: $consumerPath"
    }
}

Write-Step 'Next steps'
Write-Note 'Reference the Contracts project where needed.'
Write-Note 'Register the consumer in MassTransit if you created one.'
Write-Note 'Add domain-specific payload fields to the event class.'
