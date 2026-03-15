#!/bin/bash
# Build TypeScript Definition Generator Rider plugin
# Output: TypeScriptDefinitionGenerator.Rider-<version>.zip
# Requires: dotnet; Java 17+ for full build (Kotlin context menu)

set -e
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cd "$SCRIPT_DIR"

CONFIGURATION="${1:-Release}"
VERSION="${2:-1.0.0}"
DOTNET_ONLY="${3:-}"
PLUGIN_NAME="TypeScriptDefinitionGenerator.Rider"
OUTPUT_DIR="output"
ZIP_NAME="${PLUGIN_NAME}-${VERSION}.zip"

# --dotnet-only: skip Kotlin (no Solution View context menu), works without Java 17
if [ "$CONFIGURATION" = "--dotnet-only" ]; then
    DOTNET_ONLY=1
    CONFIGURATION="Release"
elif [ "$VERSION" = "--dotnet-only" ]; then
    DOTNET_ONLY=1
    VERSION="1.0.0"
fi

echo "Building $PLUGIN_NAME ($CONFIGURATION)..."

# 1. Build .NET backend
echo "  Building .NET backend..."
dotnet build "src/${PLUGIN_NAME}/${PLUGIN_NAME}.csproj" -c "$CONFIGURATION"

PLUGIN_DIR="$OUTPUT_DIR/${PLUGIN_NAME}"
rm -rf "$PLUGIN_DIR"
mkdir -p "$PLUGIN_DIR"
mkdir -p "$PLUGIN_DIR/dotnet"
mkdir -p "$PLUGIN_DIR/META-INF"

# 2. Build Kotlin frontend (Solution View context menu) — requires Java 17+
KOTLIN_BUILT=0
if [ -z "$DOTNET_ONLY" ]; then
    echo "  Building Kotlin frontend..."
    JAVA_CMD="java"
    [ -n "$JAVA_HOME" ] && [ -x "$JAVA_HOME/bin/java" ] && JAVA_CMD="$JAVA_HOME/bin/java"
    # Fallback: find Java 17 (macOS Homebrew)
    if ! $JAVA_CMD -version 2>&1 | grep -qE 'version "1[7-9]|version "2[0-9]'; then
        for j in /opt/homebrew/opt/openjdk@17/libexec/openjdk.jdk/Contents/Home /usr/local/opt/openjdk@17/libexec/openjdk.jdk/Contents/Home; do
            [ -x "$j/bin/java" ] && export JAVA_HOME="$j" && JAVA_CMD="$j/bin/java" && break
        done
    fi
    if $JAVA_CMD -version 2>&1 | grep -qE 'version "1[7-9]|version "2[0-9]'; then
        if [ -f "rider/gradlew" ]; then
            if (cd rider && export JAVA_HOME="$JAVA_HOME" && ./gradlew buildPlugin -q); then
                KOTLIN_BUILT=1
            else
                echo "  Kotlin build failed. Run: cd rider && ./gradlew buildPlugin"
            fi
        fi
    else
        echo "  Skipping Kotlin (Java 17+ required). Set JAVA_HOME or use --dotnet-only."
    fi
fi

# 3. Assemble plugin
if [ "$KOTLIN_BUILT" = "1" ]; then
    RIDER_ZIP=""
    for candidate in "rider/build/distributions/${PLUGIN_NAME}-${VERSION}.zip" "rider/build/distributions/${PLUGIN_NAME}-1.0.0.zip"; do
        if [ -f "$candidate" ]; then
            RIDER_ZIP="$candidate"
            break
        fi
    done
    [ -z "$RIDER_ZIP" ] && RIDER_ZIP=$(ls rider/build/distributions/*.zip 2>/dev/null | head -1)
    if [ -f "$RIDER_ZIP" ]; then
        TMP_EXTRACT="$OUTPUT_DIR/.tmp_extract"
        rm -rf "$TMP_EXTRACT"
        unzip -q -o "$RIDER_ZIP" -d "$TMP_EXTRACT"
        EXTRACTED=$(find "$TMP_EXTRACT" -maxdepth 1 -type d -name "${PLUGIN_NAME}*" | head -1)
        if [ -n "$EXTRACTED" ] && [ -d "$EXTRACTED" ]; then
            cp -R "$EXTRACTED"/* "$PLUGIN_DIR/"
        elif [ -d "$TMP_EXTRACT/META-INF" ]; then
            cp -R "$TMP_EXTRACT"/META-INF/* "$PLUGIN_DIR/META-INF/"
            [ -d "$TMP_EXTRACT/lib" ] && cp -R "$TMP_EXTRACT"/lib "$PLUGIN_DIR/"
        fi
        rm -rf "$TMP_EXTRACT"
    fi
fi

# Use plugin.xml: Kotlin version (with actions) or minimal .NET-only version
if [ "$KOTLIN_BUILT" = "1" ]; then
    cp "rider/src/main/resources/META-INF/plugin.xml" "$PLUGIN_DIR/META-INF/"
else
    cp "src/${PLUGIN_NAME}/META-INF/plugin.xml" "$PLUGIN_DIR/META-INF/"
    echo "  Note: Kotlin frontend not included — use Find Action (Ctrl+Shift+A) or External Tool."
fi

# Add dotnet backend
cp "src/${PLUGIN_NAME}/bin/${CONFIGURATION}/netstandard2.0/${PLUGIN_NAME}.dll" "$PLUGIN_DIR/dotnet/"
cp "src/${PLUGIN_NAME}/bin/${CONFIGURATION}/netstandard2.0/${PLUGIN_NAME}.pdb" "$PLUGIN_DIR/dotnet/" 2>/dev/null || true

# Create zip
cd "$OUTPUT_DIR"
zip -r -q "$ZIP_NAME" "$PLUGIN_NAME"
cd ..
rm -rf "$PLUGIN_DIR"

echo "Plugin built: $OUTPUT_DIR/$ZIP_NAME"
echo "Install: Settings -> Plugins -> Gear icon -> Install Plugin from Disk"
