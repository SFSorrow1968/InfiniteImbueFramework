# Infinite Imbue Framework Publishing Script
# Usage: ./publish.ps1
# Automates the workflow defined in _docs/PUBLISH.md

param(
    [switch]$Force
)

$ErrorActionPreference = "Stop"
Set-Location "$PSScriptRoot/.."

# 1. Safety Checks
if (Test-Path ".git") {
    Write-Host "1. Checking Git Status..." -ForegroundColor Cyan
    $status = git status --porcelain
    if ($status) {
        Write-Error "Working tree is dirty. Please commit or stash changes first."
    }
}
else {
    Write-Warning "Not a git repository. Skipping git safety check."
}

$branch = git branch --show-current
Write-Host "   Current Branch: $branch" -ForegroundColor Gray

# 2. Versioning
Write-Host "2. Reading Manifest..." -ForegroundColor Cyan
$manifestPath = "$PSScriptRoot/../manifest.json"
$manifest = Get-Content $manifestPath | ConvertFrom-Json
$version = $manifest.ModVersion
Write-Host "   Current Version: $version" -ForegroundColor Green

# Ask for confirmation
# Ask for confirmation
if (-not $Force) {
    $confirmation = Read-Host "Proceed with publishing v$version? (y/n)"
    if ($confirmation -ne 'y') {
        Write-Warning "Aborted by user."
        exit
    }
}


# 3. Build
Write-Host "3. Building..." -ForegroundColor Cyan
Set-Location "$PSScriptRoot/.."
dotnet build -c Release
dotnet build -c Nomad

# Verify Build Artifacts
if (-not (Test-Path "bin/PCVR/Infinite Imbue Framework/Infinite Imbue Framework.dll")) { Write-Error "PCVR Build Failed: DLL not found." }
if (-not (Test-Path "bin/Nomad/Infinite Imbue Framework/Infinite Imbue Framework.dll")) { Write-Error "Nomad Build Failed: DLL not found." }

# 4. Package
Write-Host "4. Packaging..." -ForegroundColor Cyan
$pcvrZip = "Infinite Imbue Framework_PCVR_v$version.zip"
$nomadZip = "Infinite Imbue Framework_Nomad_v$version.zip"

if (Test-Path $pcvrZip) { Remove-Item $pcvrZip }
if (Test-Path $nomadZip) { Remove-Item $nomadZip }

Compress-Archive -Path "bin/PCVR/Infinite Imbue Framework/*" -DestinationPath $pcvrZip
Compress-Archive -Path "bin/Nomad/Infinite Imbue Framework/*" -DestinationPath $nomadZip

Write-Host "   Created $pcvrZip" -ForegroundColor Green
Write-Host "   Created $nomadZip" -ForegroundColor Green

# 5. Git Tagging (Optional - Uncomment to enable auto-tagging)
# git add manifest.json
# git commit -m "chore: bump version to v$version"
# git tag -a "v$version" -m "Release v$version"

Write-Host "Done! Ready to upload to GitHub/Nexus." -ForegroundColor Magenta
