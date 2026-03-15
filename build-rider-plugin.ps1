# Build TypeScript Definition Generator Rider plugin
# Output: TypeScriptDefinitionGenerator.Rider-<version>.zip
# Requires: dotnet; Java 17+ for full build (Kotlin context menu)
# Usage: .\build-rider-plugin.ps1 [Configuration] [Version] [--dotnet-only]
# Example: .\build-rider-plugin.ps1 Release 1.0.0

$ErrorActionPreference = "Stop"
Set-Location $PSScriptRoot

$configArg = $args[0]
$versionArg = $args[1]
$thirdArg = $args[2]

$CONFIGURATION = "Release"
$VERSION = "1.0.0"
$DOTNET_ONLY = $false

if ($configArg -eq "--dotnet-only") {
    $DOTNET_ONLY = $true
} elseif ($versionArg -eq "--dotnet-only") {
    $DOTNET_ONLY = $true
} else {
    if ($null -ne $configArg -and $configArg -ne "") { $CONFIGURATION = $configArg }
    if ($null -ne $versionArg -and $versionArg -ne "") { $VERSION = $versionArg }
}
if ($thirdArg -eq "--dotnet-only") { $DOTNET_ONLY = $true }

$PLUGIN_NAME = "TypeScriptDefinitionGenerator.Rider"
$OUTPUT_DIR = "output"
$ZIP_NAME = "${PLUGIN_NAME}-${VERSION}.zip"

Write-Host "Building $PLUGIN_NAME ($CONFIGURATION)..."

# 1. Build .NET backend
Write-Host "  Building .NET backend..."
dotnet build "src\$PLUGIN_NAME\$PLUGIN_NAME.csproj" -c $CONFIGURATION

$PLUGIN_DIR = "$OUTPUT_DIR\$PLUGIN_NAME"
if (Test-Path $PLUGIN_DIR) { Remove-Item -Path $PLUGIN_DIR -Recurse -Force }
New-Item -ItemType Directory -Path $PLUGIN_DIR -Force | Out-Null
New-Item -ItemType Directory -Path "$PLUGIN_DIR\dotnet" -Force | Out-Null
New-Item -ItemType Directory -Path "$PLUGIN_DIR\META-INF" -Force | Out-Null

# 2. Build Kotlin frontend (Solution View context menu) — requires Java 17+
$KOTLIN_BUILT = $false
if (-not $DOTNET_ONLY) {
    Write-Host "  Building Kotlin frontend..."
    $javaCmd = "java"
    if ($env:JAVA_HOME -and (Test-Path "$env:JAVA_HOME\bin\java.exe")) {
        $javaCmd = "$env:JAVA_HOME\bin\java.exe"
    }
    try {
        $javaVersionOutput = & $javaCmd -version 2>&1 | Out-String
        $hasJava17 = $javaVersionOutput -match 'version "1[7-9]|version "2[0-9]'
    } catch {
        $hasJava17 = $false
    }

    if (-not $hasJava17) {
        # Try common Java 17 locations on Windows
        $jdkPaths = @(
            "$env:JAVA_HOME",
            "C:\Program Files\Eclipse Adoptium\jdk-17*",
            "C:\Program Files\Microsoft\jdk-17*",
            "C:\Program Files\Java\jdk-17*",
            "C:\Program Files\OpenJDK\jdk-17*"
        )
        foreach ($j in $jdkPaths) {
            if (-not $j) { continue }
            $resolved = $ExecutionContext.InvokeCommand.ExpandString($j)
            $javaExe = Get-ChildItem -Path $resolved -Filter "java.exe" -Recurse -ErrorAction SilentlyContinue | Select-Object -First 1
            if ($javaExe) {
                $env:JAVA_HOME = $javaExe.Directory.Parent.FullName
                $javaCmd = $javaExe.FullName
                try {
                    $javaVersionOutput = & $javaCmd -version 2>&1 | Out-String
                    $hasJava17 = $javaVersionOutput -match 'version "1[7-9]|version "2[0-9]'
                } catch { $hasJava17 = $false }
                if ($hasJava17) { break }
            }
        }
    }

    if ($hasJava17 -and (Test-Path "rider\gradlew.bat")) {
        Push-Location rider
        try {
            & .\gradlew.bat buildPlugin "-Pversion=$VERSION" -q
            if ($LASTEXITCODE -eq 0) { $KOTLIN_BUILT = $true } else { Write-Host "  Kotlin build failed. Run: cd rider; .\gradlew.bat buildPlugin" }
        } finally {
            Pop-Location
        }
    } elseif ($hasJava17 -and (Test-Path "rider\gradlew")) {
        # Unix gradlew — try running via Git Bash or WSL if available
        Push-Location rider
        try {
            if (Get-Command "bash" -ErrorAction SilentlyContinue) {
                $env:JAVA_HOME = $env:JAVA_HOME
                bash -c "./gradlew buildPlugin -Pversion=$VERSION -q"
                if ($LASTEXITCODE -eq 0) { $KOTLIN_BUILT = $true }
            }
            if (-not $KOTLIN_BUILT) { Write-Host "  Skipping Kotlin (use gradlew.bat or run build-rider-plugin.sh in Git Bash/WSL)." -ForegroundColor Red }
        } finally {
            Pop-Location
        }
    } else {
        if (-not $hasJava17) { Write-Host "  Skipping Kotlin (Java 17+ required). Set JAVA_HOME or use --dotnet-only." -ForegroundColor Red }
        elseif (-not (Test-Path "rider\gradlew.bat")) { Write-Host "  Skipping Kotlin (rider\gradlew.bat not found; add it or use build-rider-plugin.sh on Unix)." }
    }
}

# 3. Assemble plugin
if ($KOTLIN_BUILT) {
    $riderZip = $null
    foreach ($candidate in @(
        "rider\build\distributions\${PLUGIN_NAME}-${VERSION}.zip",
        "rider\build\distributions\${PLUGIN_NAME}-1.0.0.zip"
    )) {
        if (Test-Path $candidate) { $riderZip = $candidate; break }
    }
    if (-not $riderZip -and (Test-Path "rider\build\distributions")) {
        $firstZip = Get-ChildItem "rider\build\distributions\*.zip" -ErrorAction SilentlyContinue | Select-Object -First 1
        if ($firstZip) { $riderZip = $firstZip.FullName }
    }
    if ($riderZip) {
        $tmpExtract = "$OUTPUT_DIR\.tmp_extract"
        if (Test-Path $tmpExtract) { Remove-Item -Path $tmpExtract -Recurse -Force }
        Expand-Archive -Path $riderZip -DestinationPath $tmpExtract -Force
        $extracted = Get-ChildItem -Path $tmpExtract -Directory -Filter "${PLUGIN_NAME}*" | Select-Object -First 1
        if ($extracted) {
            Copy-Item -Path "$($extracted.FullName)\*" -Destination $PLUGIN_DIR -Recurse -Force
        } elseif (Test-Path "$tmpExtract\META-INF") {
            Copy-Item -Path "$tmpExtract\META-INF\*" -Destination "$PLUGIN_DIR\META-INF" -Recurse -Force
            if (Test-Path "$tmpExtract\lib") { Copy-Item -Path "$tmpExtract\lib" -Destination $PLUGIN_DIR -Recurse -Force }
        }
        Remove-Item -Path $tmpExtract -Recurse -Force
    }
}

# Use plugin.xml: Kotlin version (with actions) or minimal .NET-only version
$pluginMetaInf = Join-Path $PLUGIN_DIR 'META-INF'
if ($KOTLIN_BUILT) {
    Copy-Item "rider\src\main\resources\META-INF\plugin.xml" $pluginMetaInf -Force
} else {
    Copy-Item "src\$PLUGIN_NAME\META-INF\plugin.xml" $pluginMetaInf -Force
    Write-Host '  Note: Kotlin frontend not included - use Find Action (Ctrl+Shift+A) or External Tool.'
}

# Add dotnet backend
$binPath = Join-Path (Join-Path (Join-Path (Join-Path 'src' $PLUGIN_NAME) 'bin') $CONFIGURATION) 'netstandard2.0'
$pluginDotnet = Join-Path $PLUGIN_DIR 'dotnet'
Copy-Item (Join-Path $binPath ($PLUGIN_NAME + '.dll')) $pluginDotnet -Force
$pdbPath = Join-Path $binPath ($PLUGIN_NAME + '.pdb')
if (Test-Path $pdbPath) { Copy-Item $pdbPath $pluginDotnet -Force }

# Create zip
$zipPath = Join-Path $OUTPUT_DIR $ZIP_NAME
if (Test-Path $zipPath) { Remove-Item $zipPath -Force }
Push-Location $OUTPUT_DIR
try {
    Compress-Archive -Path $PLUGIN_NAME -DestinationPath $ZIP_NAME -Force
} finally {
    Pop-Location
}
Remove-Item -Path $PLUGIN_DIR -Recurse -Force

Write-Host ('Plugin built: ' + $OUTPUT_DIR + '\' + $ZIP_NAME)
Write-Host 'Install: Settings -> Plugins -> Gear icon -> Install Plugin from Disk'
