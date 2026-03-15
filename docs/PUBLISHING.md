# Публикация в маркетплейсы

## GitHub Actions (Repository → Settings → Secrets and variables → Actions)

| Secret | Описание |
|--------|----------|
| `VS_MARKETPLACE_PAT` | Personal Access Token из Azure DevOps (Marketplace → Manage) для публикации VSIX в [Visual Studio Marketplace](https://marketplace.visualstudio.com/) |
| `JETBRAINS_MARKETPLACE_TOKEN` | Permanent Token из [JetBrains Marketplace → My Tokens](https://plugins.jetbrains.com/author/me/tokens) для публикации Rider-плагина |
| `NUGET_API_KEY` | API Key из [nuget.org → Account → API Keys](https://www.nuget.org/account/apikeys) для публикации пакетов TypeScriptDefinitionGenerator.Cli и TypeScriptDefinitionGenerator.MSBuild |

## VS Marketplace (PAT)

1. [Azure DevOps](https://dev.azure.com) → User settings → Personal access tokens
2. New Token, Scope: **Marketplace** → **Manage**
3. Добавить в GitHub как secret `VS_MARKETPLACE_PAT`

## JetBrains Marketplace (Token)

1. [plugins.jetbrains.com](https://plugins.jetbrains.com) → Author → My Tokens
2. Create new token
3. Добавить в GitHub как secret `JETBRAINS_MARKETPLACE_TOKEN`

## NuGet.org (API Key)

1. [nuget.org](https://www.nuget.org) → Account → API Keys
2. Create → Generate new key (Scope: Push new packages)
3. Добавить в GitHub как secret `NUGET_API_KEY`

Публикуются пакеты: `TypeScriptDefinitionGenerator.Cli` (dotnet tool), `TypeScriptDefinitionGenerator.MSBuild` (MSBuild targets).

## Локальная публикация

```powershell
# После сборки (msbuild + build-rider-plugin.sh)
$env:VS_MARKETPLACE_PAT = "your-pat"
$env:JETBRAINS_MARKETPLACE_TOKEN = "your-token"
.\publish-marketplace.ps1 -VsixPath "src\TypeScriptDefinitionGenerator\bin\Release\TypeScriptDefinitionGenerator.vsix"
```

```powershell
# Публикация NuGet-пакетов
$env:NUGET_API_KEY = "your-api-key"
dotnet nuget push artifacts/nuget/*.nupkg --source https://api.nuget.org/v3/index.json --api-key $env:NUGET_API_KEY --skip-duplicate
```
