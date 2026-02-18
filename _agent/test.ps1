param(
    [switch]$Strict
)

$ErrorActionPreference = "Stop"

$ciSmokePath = Join-Path $PSScriptRoot "ci-smoke.ps1"
if (-not (Test-Path $ciSmokePath)) {
    Write-Error "Missing required script: $ciSmokePath"
    exit 1
}

if ($Strict) {
    & $ciSmokePath -Strict
}
else {
    & $ciSmokePath
}

if ($LASTEXITCODE -ne $null -and $LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}

Write-Host "[test] Smoke checks completed."
