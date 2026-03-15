# Build the VSIX using Visual Studio MSBuild (avoids System.Security.Permissions issue with dotnet build).
# Requires Visual Studio 2022 with .NET desktop workload.

$ErrorActionPreference = "Stop"
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location $scriptDir

$vswhere = "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe"
if (-not (Test-Path $vswhere)) {
    Write-Error "vswhere.exe not found. Install Visual Studio 2022."
    exit 1
}

$msbuild = & $vswhere -latest -requires Microsoft.Component.MSBuild -find "MSBuild\**\Bin\MSBuild.exe" | Select-Object -First 1
if (-not $msbuild) {
    Write-Error "MSBuild not found. Install Visual Studio 2022 with .NET desktop build tools."
    exit 1
}

$sln = "TypeScriptDefinitionGenerator.slnx"
if (-not (Test-Path $sln)) {
    Write-Error "Solution not found: $sln"
    exit 1
}

$config = if ($args -contains "Release") { "Release" } else { "Debug" }
# Restore first so NuGet packages (Microsoft.VisualStudio.SDK, etc.) are available
& $msbuild $sln /t:restore /p:Configuration=$config /v:m
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
& $msbuild $sln /t:TypeScriptDefinitionGenerator /p:Configuration=$config /p:DeployExtension=false /v:m
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

$outDir = "src\TypeScriptDefinitionGenerator\bin\$config\net472"
$vsix = Get-ChildItem -Path $outDir -Filter "*.vsix" -ErrorAction SilentlyContinue | Select-Object -First 1
if ($vsix) {
    Write-Host "`nVSIX: $($vsix.FullName)" -ForegroundColor Green
}
