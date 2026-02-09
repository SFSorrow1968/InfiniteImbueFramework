param(
    [string]$Root = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path,
    [string]$CatalogPath = "Catalogs/Item",
    [switch]$Strict
)

$ErrorActionPreference = "Stop"

$moduleType = "InfiniteImbueFramework.ItemModuleInfiniteImbue, InfiniteImbueFramework"
$validAssignmentModes = @(
    "ByImbueIndex",
    "Cycle",
    "FirstOnly",
    "RandomPerSpawn",
    "RoundRobinPerSpawn",
    "ConditionalHandVelocity"
)
$validConflictPolicies = @(
    "ForceConfiguredSpell",
    "RespectExternalSpell",
    "RespectExternalSpellNoEnergyWrite"
)

$catalogRoot = Join-Path $Root $CatalogPath
if (-not (Test-Path $catalogRoot)) {
    Write-Error "[IIF-Validator] Catalog path not found: $catalogRoot"
    exit 1
}

function Has-Property {
    param(
        [object]$Object,
        [string]$Name
    )
    return $null -ne $Object -and ($Object.PSObject.Properties.Name -contains $Name)
}

function Try-GetDouble {
    param(
        [object]$Object,
        [string]$Name,
        [double]$DefaultValue = 0
    )
    if (-not (Has-Property -Object $Object -Name $Name)) {
        return @{ Ok = $true; Value = $DefaultValue; Present = $false }
    }
    try {
        $value = [double]$Object.$Name
        return @{ Ok = $true; Value = $value; Present = $true }
    }
    catch {
        return @{ Ok = $false; Value = $DefaultValue; Present = $true }
    }
}

function Try-GetInt {
    param(
        [object]$Object,
        [string]$Name,
        [int]$DefaultValue = 0
    )
    if (-not (Has-Property -Object $Object -Name $Name)) {
        return @{ Ok = $true; Value = $DefaultValue; Present = $false }
    }
    try {
        $value = [int]$Object.$Name
        return @{ Ok = $true; Value = $value; Present = $true }
    }
    catch {
        return @{ Ok = $false; Value = $DefaultValue; Present = $true }
    }
}

$errors = New-Object System.Collections.Generic.List[string]
$warnings = New-Object System.Collections.Generic.List[string]
$checkedModules = 0

function Add-Error {
    param([string]$Message)
    $errors.Add($Message)
}

function Add-Warning {
    param([string]$Message)
    $warnings.Add($Message)
}

$files = Get-ChildItem -Path $catalogRoot -Filter "*.json" -File -Recurse | Sort-Object FullName
foreach ($file in $files) {
    $jsonText = Get-Content -Path $file.FullName -Raw
    try {
        $json = $jsonText | ConvertFrom-Json
    }
    catch {
        Add-Error "[$($file.FullName)] Invalid JSON: $($_.Exception.Message)"
        continue
    }

    if ((Has-Property -Object $json -Name '$type') -and $json.'$type' -ne "ThunderRoad.ItemData, ThunderRoad") {
        Add-Warning "[$($file.FullName)] Root `$type is '$($json.'$type')' (expected 'ThunderRoad.ItemData, ThunderRoad')."
    }
    if (-not (Has-Property -Object $json -Name 'version')) {
        Add-Warning "[$($file.FullName)] Missing root version field."
    }

    if (-not (Has-Property -Object $json -Name 'modules')) {
        continue
    }

    $modules = @($json.modules)
    for ($moduleIndex = 0; $moduleIndex -lt $modules.Count; $moduleIndex++) {
        $module = $modules[$moduleIndex]
        if ($null -eq $module) {
            continue
        }
        if (-not (Has-Property -Object $module -Name '$type')) {
            continue
        }
        if ($module.'$type' -ne $moduleType) {
            continue
        }

        $checkedModules++
        $prefix = "[$($file.FullName)] module[$moduleIndex]"

        $schemaVersion = Try-GetInt -Object $module -Name "schemaVersion" -DefaultValue 1
        if (-not $schemaVersion.Ok) {
            Add-Error "$prefix schemaVersion must be an integer."
        }
        elseif (-not $schemaVersion.Present) {
            Add-Warning "$prefix missing schemaVersion (default is 1)."
        }
        elseif ($schemaVersion.Value -ne 1) {
            Add-Warning "$prefix schemaVersion=$($schemaVersion.Value) (supported=1)."
        }

        if (-not (Has-Property -Object $module -Name "spells")) {
            Add-Error "$prefix missing spells array."
            continue
        }
        $spells = @($module.spells)
        if ($spells.Count -eq 0) {
            Add-Error "$prefix spells array is empty."
            continue
        }

        for ($spellIndex = 0; $spellIndex -lt $spells.Count; $spellIndex++) {
            $spell = $spells[$spellIndex]
            if ($null -eq $spell) {
                Add-Error "$prefix spells[$spellIndex] is null."
                continue
            }
            if (-not (Has-Property -Object $spell -Name "spellId") -or [string]::IsNullOrWhiteSpace([string]$spell.spellId)) {
                Add-Error "$prefix spells[$spellIndex].spellId is required."
            }

            $level = Try-GetDouble -Object $spell -Name "level" -DefaultValue 1
            if (-not $level.Ok) {
                Add-Error "$prefix spells[$spellIndex].level must be numeric."
            }
            elseif ($level.Value -lt 0 -or $level.Value -gt 1) {
                Add-Warning "$prefix spells[$spellIndex].level=$($level.Value) outside expected 0..1 range."
            }

            $energy = Try-GetDouble -Object $spell -Name "energy" -DefaultValue -1
            if (-not $energy.Ok) {
                Add-Error "$prefix spells[$spellIndex].energy must be numeric."
            }
            elseif ($energy.Value -lt 0 -and $energy.Value -ne -1) {
                Add-Warning "$prefix spells[$spellIndex].energy=$($energy.Value) is negative; use -1 to fallback to level."
            }
        }

        $assignmentMode = if (Has-Property -Object $module -Name "assignmentMode") { [string]$module.assignmentMode } else { "ByImbueIndex" }
        if ($validAssignmentModes -notcontains $assignmentMode) {
            Add-Error "$prefix assignmentMode='$assignmentMode' is invalid."
        }

        $conflictPolicy = if (Has-Property -Object $module -Name "conflictPolicy") { [string]$module.conflictPolicy } else { "ForceConfiguredSpell" }
        if ($validConflictPolicies -notcontains $conflictPolicy) {
            Add-Error "$prefix conflictPolicy='$conflictPolicy' is invalid."
        }

        $maintainBelow = Try-GetDouble -Object $module -Name "maintainBelowRatio" -DefaultValue 0.98
        $refillTo = Try-GetDouble -Object $module -Name "refillToRatio" -DefaultValue 1.0
        $minSetEnergyInterval = Try-GetDouble -Object $module -Name "minSetEnergyInterval" -DefaultValue 0.5
        $velocityThreshold = Try-GetDouble -Object $module -Name "conditionalVelocityThreshold" -DefaultValue 6.0
        $velocityHysteresis = Try-GetDouble -Object $module -Name "conditionalVelocityHysteresis" -DefaultValue 1.0
        $minSwitchInterval = Try-GetDouble -Object $module -Name "conditionalMinSwitchInterval" -DefaultValue 0.25

        if (-not $maintainBelow.Ok) { Add-Error "$prefix maintainBelowRatio must be numeric." }
        if (-not $refillTo.Ok) { Add-Error "$prefix refillToRatio must be numeric." }
        if (-not $minSetEnergyInterval.Ok) { Add-Error "$prefix minSetEnergyInterval must be numeric." }
        if (-not $velocityThreshold.Ok) { Add-Error "$prefix conditionalVelocityThreshold must be numeric." }
        if (-not $velocityHysteresis.Ok) { Add-Error "$prefix conditionalVelocityHysteresis must be numeric." }
        if (-not $minSwitchInterval.Ok) { Add-Error "$prefix conditionalMinSwitchInterval must be numeric." }

        if ($maintainBelow.Ok -and ($maintainBelow.Value -lt 0 -or $maintainBelow.Value -gt 1)) {
            Add-Warning "$prefix maintainBelowRatio=$($maintainBelow.Value) outside expected 0..1 range."
        }
        if ($refillTo.Ok -and ($refillTo.Value -lt 0 -or $refillTo.Value -gt 1)) {
            Add-Warning "$prefix refillToRatio=$($refillTo.Value) outside expected 0..1 range."
        }
        if ($maintainBelow.Ok -and $refillTo.Ok -and $refillTo.Value -lt $maintainBelow.Value) {
            Add-Warning "$prefix refillToRatio=$($refillTo.Value) is below maintainBelowRatio=$($maintainBelow.Value)."
        }
        if ($minSetEnergyInterval.Ok -and $minSetEnergyInterval.Value -lt 0) {
            Add-Warning "$prefix minSetEnergyInterval=$($minSetEnergyInterval.Value) should be >= 0."
        }
        if ($velocityThreshold.Ok -and $velocityThreshold.Value -lt 0) {
            Add-Warning "$prefix conditionalVelocityThreshold=$($velocityThreshold.Value) should be >= 0."
        }
        if ($velocityHysteresis.Ok -and $velocityHysteresis.Value -lt 0) {
            Add-Warning "$prefix conditionalVelocityHysteresis=$($velocityHysteresis.Value) should be >= 0."
        }
        if ($minSwitchInterval.Ok -and $minSwitchInterval.Value -lt 0) {
            Add-Warning "$prefix conditionalMinSwitchInterval=$($minSwitchInterval.Value) should be >= 0."
        }

        if ($assignmentMode -eq "ConditionalHandVelocity" -and $spells.Count -lt 2) {
            Add-Warning "$prefix assignmentMode=ConditionalHandVelocity expects at least 2 spells."
        }
        if ($assignmentMode -eq "RandomPerSpawn" -and $spells.Count -lt 2) {
            Add-Warning "$prefix assignmentMode=RandomPerSpawn expects at least 2 spells for visible variation."
        }
        if ($assignmentMode -eq "RoundRobinPerSpawn" -and $spells.Count -lt 2) {
            Add-Warning "$prefix assignmentMode=RoundRobinPerSpawn expects at least 2 spells for visible variation."
        }
    }
}

foreach ($warn in $warnings) {
    Write-Host "[IIF-Validator][WARN] $warn"
}
foreach ($err in $errors) {
    Write-Host "[IIF-Validator][ERROR] $err"
}

Write-Host "[IIF-Validator] Files=$($files.Count) ModulesChecked=$checkedModules Warnings=$($warnings.Count) Errors=$($errors.Count) Strict=$($Strict.IsPresent)"

if ($errors.Count -gt 0) {
    exit 1
}
if ($Strict.IsPresent -and $warnings.Count -gt 0) {
    exit 1
}
exit 0
