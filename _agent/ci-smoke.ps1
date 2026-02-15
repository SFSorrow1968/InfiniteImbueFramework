param(
    [switch]$Strict
)

$ErrorActionPreference = "Stop"

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot "..")
$repoRootPath = $repoRoot.Path

Write-Host "[CI] Script Root: $PSScriptRoot"
Write-Host "[CI] Repo Root: $repoRootPath"

# Dynamically find the main project file
# Get all csproj files first, then filter in memory to be safer
$allProjects = Get-ChildItem -Path $repoRootPath -Filter "*.csproj"
$projectFile = $allProjects | Where-Object { $_.Name -notlike "*.Tests.csproj" } | Select-Object -First 1

if (-not $projectFile) {
    Write-Error "Could not find main .csproj file in $repoRootPath"
    Write-Host "Contents of $repoRootPath :"
    Get-ChildItem -Path $repoRootPath | Select-Object Name, Length, Mode | Format-Table | Out-Host
    exit 1
}
$projectPath = $projectFile.FullName
Write-Host "[CI] Found Project: $projectPath"

# Dynamically find the test project file
$testProjectFile = Get-ChildItem -Path $repoRootPath -Recurse -Filter "*.Tests.csproj" | Select-Object -First 1
$testProjectPath = if ($testProjectFile) { $testProjectFile.FullName } else { $null }

$libsPath = Join-Path (Split-Path $repoRootPath -Parent) "libs"
$requiredDlls = @(
    "ThunderRoad.dll",
    "Assembly-CSharp.dll",
    "Assembly-CSharp-firstpass.dll",
    "UnityEngine.dll",
    "UnityEngine.CoreModule.dll",
    "UnityEngine.IMGUIModule.dll",
    "UnityEngine.TextRenderingModule.dll"
)

$missingDlls = @()
foreach ($dll in $requiredDlls) {
    $dllPath = Join-Path $libsPath $dll
    if (-not (Test-Path $dllPath)) {
        $missingDlls += $dll
    }
}

if ($missingDlls.Count -gt 0) {
    $msg = "[CI] Missing game libraries in ${libsPath}: $($missingDlls -join ', ')"
    if ($Strict) {
        Write-Error $msg
        exit 1
    }

    # graceful exit for CI
    Write-Warning $msg
    Write-Warning "[CI] Skipping Release/Nomad build in non-strict mode (CI environment detected)."
    
    # Still run tests if possible, or skip them too if they depend on the DLLs
    if ($testProjectPath) {
        Write-Warning "[CI] skipping tests as they likely depend on missing DLLs."
    }
    
    exit 0
}
else {
    Write-Host "[CI] Building Release..."
    dotnet build $projectPath -c Release
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

    Write-Host "[CI] Building Nomad..."
    dotnet build $projectPath -c Nomad
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
}

if ($testProjectPath) {
    Write-Host "[CI] Running tests..."
    dotnet test $testProjectPath -c Release --nologo -v minimal
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
}

Write-Host "[CI] Smoke checks complete."
exit 0
