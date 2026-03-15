# TypeScript Definition Generator

Creates and synchronizes TypeScript Definition files (`.d.ts`) from C# model classes (DTO) to build strongly typed web applications where the server and client-side models are in sync. Works on all .NET project types.

## Features

- Right-click any `.cs` file and select **Generate TypeScript Definition** to create a `.generated.d.ts` file
- Automatically updates `.d.ts` when the C# file is modified and saved
- Supports classes, interfaces, enums, inheritance, and XML documentation
- Configurable: camelCase, module format, EOL, indentation

## NuGet

For MSBuild integration and CI/CD: `dotnet add package TypeScriptDefinitionGenerator.MSBuild` — adds targets for build-time generation.

## JetBrains Rider

For Rider, see [README-RIDER.md](https://github.com/denis-peshkov/TypeScriptDefinitionGenerator/blob/master/README-RIDER.md) — plugin with context menu, External Tool, or dotnet tool.

## Links

- [GitHub](https://github.com/denis-peshkov/TypeScriptDefinitionGenerator)
- [Changelog](https://github.com/denis-peshkov/TypeScriptDefinitionGenerator/blob/master/CHANGELOG.md)
