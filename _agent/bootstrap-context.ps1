param(
    [string]$Feature
)

$ErrorActionPreference = "Stop"

$modRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
$templateRoot = (Resolve-Path (Join-Path $modRoot "..\..\Docs\templates")).Path

$visionsDir = Join-Path $modRoot "_visions"
$quirksDir = Join-Path $modRoot "_quirks"
$resourcesDir = Join-Path $modRoot "_resources"

foreach ($dir in @($visionsDir, $quirksDir, $resourcesDir)) {
    if (-not (Test-Path $dir)) {
        New-Item -ItemType Directory -Path $dir | Out-Null
        Write-Host "[bootstrap] Created $dir"
    }
}

$scratchpadPath = Join-Path $modRoot "SCRATCHPAD.md"
if (-not (Test-Path $scratchpadPath)) {
    $scratchpadContent = @"
# Agent Scratchpad

## Current Focus
- Goal:
- Status:

## Context Stack
- Key files:
- Notes:

## Next Steps / Handoff
1. [ ]
2. [ ]

### Last Updated
- Agent:
- Date:
"@
    Set-Content -Path $scratchpadPath -Value $scratchpadContent -Encoding UTF8
    Write-Host "[bootstrap] Created $scratchpadPath"
}

if ($Feature -and $Feature.Trim().Length -gt 0) {
    $featureName = [System.IO.Path]::GetFileNameWithoutExtension($Feature).Trim()
    if ($featureName.Length -eq 0) {
        Write-Error "Feature name is empty after normalization."
        exit 1
    }

    $targets = @(
        @{ Template = "VISION_TEMPLATE.md"; Dir = $visionsDir; Suffix = "VISION" },
        @{ Template = "QUIRKS_TEMPLATE.md"; Dir = $quirksDir; Suffix = "QUIRKS" },
        @{ Template = "RESOURCES_TEMPLATE.md"; Dir = $resourcesDir; Suffix = "RESOURCES" }
    )

    foreach ($target in $targets) {
        $templatePath = Join-Path $templateRoot $target.Template
        $outPath = Join-Path $target.Dir ("{0}_{1}.md" -f $featureName, $target.Suffix)

        if (-not (Test-Path $outPath)) {
            $content = Get-Content -Path $templatePath -Raw
            $content = $content.Replace("[Feature Name]", $featureName).Replace("[Feature]", $featureName)
            Set-Content -Path $outPath -Value $content -Encoding UTF8
            Write-Host "[bootstrap] Created $outPath"
        }
    }
}

Write-Host "[bootstrap] Context scaffold complete."
