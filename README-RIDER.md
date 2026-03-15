# TypeScript Definition Generator — использование в JetBrains Rider

Расширение для Visual Studio генерирует `.d.ts` файлы из C# DTO моделей. В Rider можно достичь того же результата следующими способами:

## Способ 1: External Tool (рекомендуется)

1. **Установите dotnet tool:**
   ```bash
   dotnet tool install -g TypeScriptDefinitionGenerator.Cli
   ```
   Или из исходников:
   ```bash
   cd src/TypeScriptDefinitionGenerator.Cli
   dotnet pack -c Release
   dotnet tool install -g --add-source ./bin/Release TypeScriptDefinitionGenerator.Cli
   ```
   Или запускайте напрямую (без установки):
   ```bash
   dotnet run --project src/TypeScriptDefinitionGenerator.Cli -- $FilePath$
   ```

2. **Настройте External Tool в Rider:**
   - **Settings** → **Tools** → **External Tools** → **Add**
   - **Name:** Generate TypeScript Definition
   - **Program:** `dotnet`
   - **Arguments:** `tsdefgen $FilePath$` (при установленном tool) или `run --project src/TypeScriptDefinitionGenerator.Cli -- $FilePath$` (при работе из исходников)
   - **Working directory:** `$ContentRoot$`
   - Включите **Synchronize files after execution**

3. **Добавьте в контекстное меню:**
   - **Settings** → **Menus and Toolbars** → **Project View** → **Context Menu**
   - Добавьте созданный External Tool

## Способ 2: Запуск из терминала

```bash
# Через dotnet tool (после: dotnet tool install -g TypeScriptDefinitionGenerator.Cli)
dotnet tsdefgen path/to/YourDto.cs
dotnet tsdefgen path/to/Dto1.cs path/to/Dto2.cs

# Или напрямую из исходников
dotnet run --project src/TypeScriptDefinitionGenerator.Cli -- path/to/YourDto.cs
```

## Конфигурация (tsdefgen.json)

Поместите файл `tsdefgen.json` в корень проекта для переопределения настроек:

```json
{
  "camelCasePropertyNames": true,
  "camelCaseTypeNames": false,
  "defaultModuleName": "Server.Dtos",
  "useNamespace": true,
  "declareModule": true,
  "classInsteadOfInterface": false,
  "eolType": "LF",
  "indentTab": true,
  "indentTabSize": 2
}
```

## Автогенерация при сборке (MSBuild)

При изменении .cs файла можно автоматически перегенерировать .d.ts при сборке.

### Вариант 1: NuGet-пакет (рекомендуется)

```bash
dotnet add package TypeScriptDefinitionGenerator.MSBuild
```

Пакет добавляет `DotNetToolReference` на `TypeScriptDefinitionGenerator.Cli` и MSBuild targets. Добавьте в .csproj:

```xml
<ItemGroup>
  <PackageReference Include="TypeScriptDefinitionGenerator.MSBuild" Version="2.2.*" />
</ItemGroup>

<ItemGroup>
  <TypeScriptDefinitionSource Include="Models\Dto.cs" />
</ItemGroup>

<ItemGroup>
  <None Update="Models\Dto.generated.d.ts">
    <DependentUpon>Dto.cs</DependentUpon>
  </None>
</ItemGroup>
```

### Вариант 2: Локальный targets (для разработки в репозитории TypeScriptDefinitionGenerator)

```xml
<Import Project="path/to/build/TypeScriptDefinitionGenerator.targets" />

<ItemGroup>
  <TypeScriptDefinitionSource Include="base\ThirdClass.cs" />
</ItemGroup>

<ItemGroup>
  <None Update="base\ThirdClass.generated.d.ts">
    <DependentUpon>ThirdClass.cs</DependentUpon>
  </None>
</ItemGroup>
```

Target использует Inputs/Outputs для инкрементальной сборки — генерация запускается только когда .cs новее .d.ts.

## Rider Plugin

Плагин добавляет пункт «Generate TypeScript Definition» в контекстное меню для .cs файлов (в меню Generate и в Project View). Плагин запускает `TypeScriptDefinitionGenerator.Cli` как subprocess — без конфликтов с ReSharper SDK.

### Сборка плагина

**Требования:** .NET SDK; для контекстного меню — Java 17+.

```bash
# Полная сборка (с контекстным меню Solution View)
./build-rider-plugin.sh [Debug|Release] [версия]
# Пример: ./build-rider-plugin.sh Release 1.0.0
```

**Без Java 17:** скрипт автоматически соберёт только .NET backend (без пункта в контекстном меню). Используйте **Find Action** (Ctrl+Shift+A / Cmd+Shift+A) → «Generate TypeScript Definition». Или явно: `./build-rider-plugin.sh --dotnet-only`.

**С Java 17:** скрипт автоматически ищет Java 17 (Homebrew). Или задайте явно:
```bash
export JAVA_HOME=$(/usr/libexec/java_home -v 17)   # macOS
./build-rider-plugin.sh Release 1.0.0
```

Скрипт собирает .NET backend и при наличии Java 17 — Kotlin frontend (контекстное меню Solution View). Плагин совместим с Rider 2024.1+ (включая Rider 2025.3).

### Установка

1. Соберите плагин (см. выше)
2. В Rider: **Settings** → **Plugins** → **Gear icon** → **Install Plugin from Disk**
3. Выберите `output/TypeScriptDefinitionGenerator.Rider-<version>.zip`
4. Перезапустите Rider

### Настройки

**Settings** (Ctrl+Alt+S / Cmd+,) → **Tools** → **TypeScript Definition Generator**. Или в поле поиска настроек введите «TypeScript» — откроется страница плагина.

Глобальные настройки генерации (camelCase, module, EOL и т.д.). Сохраняются в конфигурации IDE. При каждой генерации из меню плагин записывает эти настройки в `tsdefgen.json` в корне проекта — CLI использует их.

### Использование

1. Откройте решение TypeScriptDefinitionGenerator (или любое с CLI в `src/TypeScriptDefinitionGenerator.Cli`)
2. ПКМ по .cs файлу → **Generate TypeScript Definition**
3. Или в редакторе: Alt+Enter → **Generate** → **Generate TypeScript Definition**

Плагин запускает `dotnet run --project src/TypeScriptDefinitionGenerator.Cli -- <путь-к-файлу>` в каталоге решения.

При успешной генерации плагин автоматически добавляет в .csproj:
- `PackageReference` на `TypeScriptDefinitionGenerator.MSBuild` — MSBuild targets и dotnet tool для автогенерации при сборке
- `TypeScriptDefinitionSource` — список .cs файлов для генерации
- `None` с `DependentUpon` — для отображения .d.ts в Solution Explorer
