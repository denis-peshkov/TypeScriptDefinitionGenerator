# Publish TypeScript Definition Generator to marketplaces
# Requires: $env:VS_MARKETPLACE_PAT (Azure DevOps PAT), $env:JETBRAINS_MARKETPLACE_TOKEN
# Run from repo root after build

param(
    [string]$VsixPath = "",
    [string]$RiderZipPath = "output\TypeScriptDefinitionGenerator.Rider-1.0.0.zip"
)

$ErrorActionPreference = "Stop"

# --- Visual Studio Marketplace ---
if ($env:VS_MARKETPLACE_PAT) {
    if (-not $VsixPath) {
        $vsixFiles = Get-ChildItem -Path . -Filter "*.vsix" -Recurse -ErrorAction SilentlyContinue |
            Where-Object { $_.FullName -like "*TypeScriptDefinitionGenerator*" -and $_.FullName -notlike "*obj*" }
        $VsixPath = $vsixFiles | Sort-Object LastWriteTime -Descending | Select-Object -First 1 -ExpandProperty FullName
    }
    if ($VsixPath -and (Test-Path $VsixPath)) {
        Write-Host "Publishing to Visual Studio Marketplace..." -ForegroundColor Cyan
        $vsPath = & "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe" -latest -property installationPath 2>$null
        $vsixPublisher = Join-Path $vsPath "VSSDK\VisualStudioIntegration\Tools\Bin\VsixPublisher.exe"
        if (-not (Test-Path $vsixPublisher)) {
            $vsixPublisher = Join-Path $vsPath "Common7\IDE\Extensions\Microsoft\Vsix\VsixPublisher.exe"
        }
        if (Test-Path $vsixPublisher) {
            $publishManifest = Join-Path $PSScriptRoot "vs-publish.json"
            & $vsixPublisher publish -payload $VsixPath -publishManifest $publishManifest -personalAccessToken $env:VS_MARKETPLACE_PAT
            Write-Host "VS Marketplace: OK" -ForegroundColor Green
        } else {
            Write-Warning "VsixPublisher.exe not found. Install Visual Studio SDK workload."
        }
    } else {
        Write-Warning "VSIX file not found. Build the project first."
    }
} else {
    Write-Host "VS_MARKETPLACE_PAT not set, skipping VS Marketplace" -ForegroundColor Yellow
}

# --- JetBrains Marketplace ---
if ($env:JETBRAINS_MARKETPLACE_TOKEN) {
    if (Test-Path $RiderZipPath) {
        Write-Host "Publishing to JetBrains Marketplace..." -ForegroundColor Cyan
        $xmlId = "com.typescriptdefinitiongenerator.rider"
        $riderPath = (Resolve-Path $RiderZipPath).Path
        $curlOut = & curl.exe -s -w "`n%{http_code}" -X POST `
            -H "Authorization: Bearer $env:JETBRAINS_MARKETPLACE_TOKEN" `
            -F "xmlId=$xmlId" `
            -F "file=@$riderPath" `
            "https://plugins.jetbrains.com/api/updates/upload" 2>&1
        $result = ($curlOut -split "`n")[-1]
        if ($result -eq "200" -or $result -eq "201") {
            Write-Host "JetBrains Marketplace: OK" -ForegroundColor Green
        } else {
            Write-Error "JetBrains upload failed (HTTP $result)"
        }
    } else {
        Write-Warning "Rider plugin zip not found: $RiderZipPath"
    }
} else {
    Write-Host "JETBRAINS_MARKETPLACE_TOKEN not set, skipping JetBrains Marketplace" -ForegroundColor Yellow
}
