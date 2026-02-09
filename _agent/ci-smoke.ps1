param(
    [switch]$Strict
)

$ErrorActionPreference = "Stop"

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot "..")
$repoRootPath = $repoRoot.Path
$projectPath = Join-Path $repoRootPath "InfiniteImbueFramework.csproj"
$testProjectPath = Join-Path $repoRootPath "InfiniteImbueFramework.Tests\InfiniteImbueFramework.Tests.csproj"
$validatorPath = Join-Path $repoRootPath "_tools\Validate-IIFCatalogs.ps1"

Write-Host "[IIF-CI] Running catalog validator..."
& $validatorPath -Root $repoRootPath -CatalogPath "Catalogs/Item" -Strict:$Strict
if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}

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
    $msg = "[IIF-CI] Missing game libraries in ${libsPath}: $($missingDlls -join ', ')"
    if ($Strict) {
        Write-Error $msg
        exit 1
    }

    Write-Warning $msg
    Write-Warning "[IIF-CI] Skipping Release/Nomad build in non-strict mode."
}
else {
    Write-Host "[IIF-CI] Building Release..."
    dotnet build $projectPath -c Release | Out-Host
    if ($LASTEXITCODE -ne 0) {
        exit $LASTEXITCODE
    }

    Write-Host "[IIF-CI] Building Nomad..."
    dotnet build $projectPath -c Nomad | Out-Host
    if ($LASTEXITCODE -ne 0) {
        exit $LASTEXITCODE
    }
}

if (Test-Path $testProjectPath) {
    Write-Host "[IIF-CI] Running tests..."
    dotnet test $testProjectPath -c Release --nologo -v minimal | Out-Host
    if ($LASTEXITCODE -ne 0) {
        exit $LASTEXITCODE
    }
}

Write-Host "[IIF-CI] Smoke checks complete."
exit 0
