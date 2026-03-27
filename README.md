[![License](https://img.shields.io/github/license/denis-peshkov/TypeScriptDefinitionGenerator)](LICENSE)
[![GitHub Release Date](https://img.shields.io/github/release-date/denis-peshkov/TypeScriptDefinitionGenerator?label=released)](https://github.com/denis-peshkov/TypeScriptDefinitionGenerator/releases)
[![NuGetVersion](https://img.shields.io/nuget/v/TypeScriptDefinitionGenerator.svg)](https://nuget.org/packages/TypeScriptDefinitionGenerator/)
[![NugetDownloads](https://img.shields.io/nuget/dt/TypeScriptDefinitionGenerator.svg)](https://nuget.org/packages/TypeScriptDefinitionGenerator/)
[![Coverage](https://sonarcloud.io/api/project_badges/measure?project=TypeScriptDefinitionGenerator&metric=coverage)](https://sonarcloud.io/summary/new_code?id=TypeScriptDefinitionGenerator)
[![issues](https://img.shields.io/github/issues/denis-peshkov/TypeScriptDefinitionGenerator)](https://github.com/denis-peshkov/TypeScriptDefinitionGenerator/issues)
[![.NET PR](https://github.com/denis-peshkov/TypeScriptDefinitionGenerator/actions/workflows/dotnet.yml/badge.svg?event=pull_request)](https://github.com/denis-peshkov/TypeScriptDefinitionGenerator/actions/workflows/dotnet.yml)

![Size](https://img.shields.io/github/repo-size/denis-peshkov/TypeScriptDefinitionGenerator)
[![GitHub contributors](https://img.shields.io/github/contributors/denis-peshkov/TypeScriptDefinitionGenerator)](https://github.com/denis-peshkov/TypeScriptDefinitionGenerator/contributors)
[![GitHub commits since latest release (by date)](https://img.shields.io/github/commits-since/denis-peshkov/TypeScriptDefinitionGenerator/latest?label=new+commits)](https://github.com/denis-peshkov/TypeScriptDefinitionGenerator/commits/master)
![Activity](https://img.shields.io/github/commit-activity/w/denis-peshkov/TypeScriptDefinitionGenerator)
![Activity](https://img.shields.io/github/commit-activity/m/denis-peshkov/TypeScriptDefinitionGenerator)
![Activity](https://img.shields.io/github/commit-activity/y/denis-peshkov/TypeScriptDefinitionGenerator)

# TypeScript Definition Generator

## NOTE: This is a customed redistribute
The orginal repo wrote by @madskristensen, **best regard for him**!

Download this extension from the [Marketplace](https://marketplace.visualstudio.com/items?itemName=peshkov.5cb4e919-c9ff-4026-bd39-fd323a14fac7#overview)
or get the [CI build](http://vsixgallery.com/extension/5cb4e919-c9ff-4026-bd39-fd323a14fac7/).

---------------------------------------

Creates and synchronizes TypeScript Definition files (d.ts) from C# model classes(DTO) to build strongly typed web application where the server and client-side models are in sync. Works on all .NET project types

See the [change log](CHANGELOG.md) for changes and road map.

For Visual Studio 2015 support, install [Web Essentials 2015](https://marketplace.visualstudio.com/items?itemName=MadsKristensen.WebEssentials20153)

## From C# to .d.ts file
This extension will automatically generate .d.ts files from any C# file you specify. It will turn the following C# class into a TypeScript interface.

For *.cs file like this
```csharp
namespace foo
{
    public class ReuqestBase
    {
        public string RequestName { get; set; }
    }

    public class CustomerReuqest : ReuqestBase
    {
        public CustomerDto CustomerDto { get; set; }
    }

    /// <summary>
    /// dto for single customer
    /// </summary>
    public class CustomerDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public IEnumerable<string> Tags { get; set; }
        //predefined type for customers
        public CustomerType CusomerType { get; set; }
    }

    public enum CustomerType
    {
        Normal = 0,
        /// <summary>
        /// Charged over $100
        /// </summary>
        Vip = 1
    }
}
```

Becomes this .d.ts file:

```typescript
declare module Server.Dtos {
	interface ReuqestBase {
		requestName: string;
	}
	interface CustomerReuqest extends ReuqestBase {
		customerDto: Server.Dtos.CustomerDto;
	}
	/** dto for single customer */
	interface CustomerDto {
		id: number;
		name: string;
		tags: string[];
		/** predefined type for customers */
		cusomerType: Server.Dtos.CustomerType;
	}
	const enum CustomerType {
		Normal = 0,
		/** Charged over $100 */
		Vip = 1,
	}
}

```

The generated .d.ts file can then be consumed from your own TypeScript files by referencing `Server.Dtos.<typeName>` interfaces.

## Generate d.ts file from C#
To generate a .d.ts file, right-click any .cs file and select **Generate TypeScript Definition**.

Then a .generated.d.ts file is created and nested under the parent C# file.

![Context menu](art/context-menu.png)



Every time the C# file is modified and saved, the content of the .d.ts file is updated to reflect the changes.

## Settings
Configure this extension from the **Tools -> Options -> Text Editor -> JavaScript/TypeScript -> Generate d.ts** dialog.

![Settings](art/settings.png)

## NuGet (MSBuild + dotnet tool)

Для автогенерации .d.ts при сборке добавьте пакет:

```bash
dotnet add package TypeScriptDefinitionGenerator.MSBuild
```

Пакет подключает MSBuild targets и dotnet tool `tsdefgen`. В .csproj добавьте:

```xml
<ItemGroup>
  <TypeScriptDefinitionSource Include="Models\**\*.cs" />
</ItemGroup>
```

См. [README-RIDER.md](README-RIDER.md) для полной конфигурации.

## JetBrains Rider

Для использования в Rider см. [README-RIDER.md](README-RIDER.md) — плагин с контекстным меню, External Tool или dotnet tool для генерации .d.ts файлов.

## Сборка расширения (VSIX)

Сборка возможна **только на Windows** с установленной Visual Studio 2022 (VSSDK требует vsct.exe и т.д.).

**Рекомендуемый способ:** запустите скрипт в PowerShell (использует MSBuild из VS и избегает проблем с `dotnet build`):

```powershell
.\build-vsix.ps1        # Debug
.\build-vsix.ps1 Release
```

Готовый VSIX будет в `src\TypeScriptDefinitionGenerator\bin\<Configuration>\net472\*.vsix`.

**Установка VSIX на Windows:** дважды щёлкните по файлу `.vsix` или в Visual Studio: **Extensions** → **Manage Extensions** → **Install from VSIX** и укажите собранный файл.

**Альтернатива:** откройте решение в Visual Studio и выполните **Build Solution**. При сборке через `dotnet build` нужен **.NET 8 SDK**: в корне репозитория задан `global.json` для SDK 8.0.x. Установите [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) при необходимости.

## Contribute
Check out the [contribution guidelines](.github/CONTRIBUTING.md)
if you want to contribute to this project.

For cloning and building this project yourself, make sure
to install the
[Extensibility Tools 2015](https://visualstudiogallery.msdn.microsoft.com/ab39a092-1343-46e2-b0f1-6a3f91155aa6)
extension for Visual Studio which enables some features
used by this project.
