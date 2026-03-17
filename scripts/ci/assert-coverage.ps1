[CmdletBinding()]
param(
    [Parameter(Mandatory)] [string] $ResultsDirectory,
    [double] $MinimumLineRate = 0.75
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$coverageFiles = Get-ChildItem -Path $ResultsDirectory -Recurse -Filter "coverage.cobertura.xml" -File
if ($coverageFiles.Count -eq 0) {
    throw "No coverage.cobertura.xml files were found under $ResultsDirectory."
}

$coveredLines = 0
$validLines = 0

foreach ($file in $coverageFiles) {
    [xml]$xml = Get-Content $file.FullName
    $coveredLines += [int]$xml.coverage.'lines-covered'
    $validLines += [int]$xml.coverage.'lines-valid'
}

if ($validLines -le 0) {
    throw "Coverage report does not contain any valid lines."
}

$lineRate = $coveredLines / $validLines
$lineRateText = '{0:P2}' -f $lineRate
$minimumText = '{0:P2}' -f $MinimumLineRate

Write-Host "Computed line coverage: $lineRateText"

if ($lineRate -lt $MinimumLineRate) {
    throw "Line coverage $lineRateText is below the required threshold $minimumText."
}
