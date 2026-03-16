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
<Import Project="path/to/src/TypeScriptDefinitionGenerator.MSBuild/TypeScriptDefinitionGenerator.MSBuild.targets" />

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

Плагин добавляет пункт «Generate TypeScript Definition» в контекстное меню для .cs файлов (в меню Generate и в Project View). Плагин запускает `TypeScriptDefinitionGenerator.Cli` как subprocess. Плагин совместим с Rider 2024.1+ (включая Rider 2025.3).

### Сборка плагина

**Общее требование:** .NET SDK на любой ОС.

Два варианта сборки:
- **Вариант 1 (полная):** .NET backend + Kotlin frontend — контекстное меню по ПКМ по .cs в Solution View. **Kotlin устанавливать не нужно** — он подтягивается сборкой (Gradle). Нужны только **Java 17+** (JDK) и на Windows — скрипт `gradlew.bat` (см. ниже).
- **Вариант 2 (только .NET):** только .NET backend — пункта в контекстном меню нет; генерация через **Find Action** (Ctrl+Shift+A / Cmd+Shift+A) → «Generate TypeScript Definition». Java и Kotlin не требуются.

---

#### Windows

**Как установить предварительные требования (только для Варианта 1):**

1. **Разрешить выполнение PowerShell-скриптов** (если ещё не разрешено):  
   Откройте PowerShell и выполните:  
   `Set-ExecutionPolicy -Scope CurrentUser -ExecutionPolicy RemoteSigned`

2. **Установить Java 17+:**  
   - Установите [Chocolatey](https://chocolatey.org/install) (если ещё нет).  
   - В PowerShell от имени администратора: `choco install openjdk17`  
   - Закройте и снова откройте терминал.

3. **Получить `gradlew.bat`** (для полной сборки):  
   В репозитории есть только `rider\gradlew` (Unix). Нужен `rider\gradlew.bat`:  
   - Установите [Gradle](https://gradle.org/install/) (или через Chocolatey: `choco install gradle`).  
   - В терминале: `cd rider`, затем `gradle wrapper`.  
   - Должен появиться `gradlew.bat`. Если не получается — собирайте **Вариант 2** (`--dotnet-only`).

| Вариант | Команда | Результат |
|--------|---------|-----------|
| **1 (полная)** | `.\build-rider-plugin.ps1 Release 1.0.0` | Контекстное меню + Find Action |
| **2 (только .NET)** | `.\build-rider-plugin.ps1 --dotnet-only` | Только Find Action |

Архив плагина: `output\TypeScriptDefinitionGenerator.Rider-<version>.zip`

---

#### macOS

**Как установить предварительные требования (только для Варианта 1):**

1. **Установить Java 17+:**  
   - Через Homebrew: `brew install openjdk@17`  
   - Или скачайте [Eclipse Temurin 17](https://adoptium.net/) / [Oracle JDK 17](https://www.oracle.com/java/technologies/downloads/#java17) и установите вручную.

2. **Указать Java 17 для сборки** (если установлено несколько версий):  
   `export JAVA_HOME=$(/usr/libexec/java_home -v 17)`  
   Проверка: `java -version` — должна быть 17 или выше.

| Вариант | Команда | Результат |
|--------|---------|-----------|
| **1 (полная)** | `./build-rider-plugin.sh Release 1.0.0` | Контекстное меню + Find Action |
| **2 (только .NET)** | `./build-rider-plugin.sh --dotnet-only` | Только Find Action |

Архив плагина: `output/TypeScriptDefinitionGenerator.Rider-<version>.zip`

---

#### Linux

Скрипт `./build-rider-plugin.sh`, те же два варианта (таблица как для macOS).

**Как установить предварительные требования (только для Варианта 1):**  
Установите Java 17+ из пакетного менеджера (например `sudo apt install openjdk-17-jdk` или аналог). При нескольких версиях Java задайте `export JAVA_HOME=/path/to/jdk-17`.

---

### Ошибка SSL при загрузке Gradle и зависимостей (Windows / корпоративный прокси)

Если при запуске `gradlew.bat` или сборке плагина появляется **SSLHandshakeException** / **PKIX path building failed: unable to find valid certification path to requested target**, значит JVM не доверяет сертификату (часто из‑за корпоративного прокси или своей CA). Та же ошибка возникает при загрузке **дистрибутива Gradle** (services.gradle.org) и при разрешении **зависимостей** (plugins.gradle.org, mavenCentral()). Сообщение «What went wrong: 25» без деталей — это та же SSL-ошибка: запустите `.\gradlew.bat buildPlugin --stacktrace`, чтобы увидеть полный текст.

**Вариант 1 — добавить сертификат в хранилище Java (рекомендуется):**  
Без этого не будут работать ни загрузка Gradle, ни разрешение зависимостей (IntelliJ plugin, Kotlin и т.д.).

1. Скачайте корневой сертификат вашей сети (или экспортируйте из браузера с https://services.gradle.org или https://plugins.gradle.org) в файл, например `gradle-ca.cer`.

2. **Windows** — откройте **cmd от имени администратора** (ПКМ по cmd → «Запуск от имени администратора»), перейдите в папку, где лежит `gradle-org.cer` (или укажите полный путь к файлу), и выполните (подставьте свой путь к JDK и к файлу сертификата):
   ```bat
   set JAVA_HOME=C:\Program Files\Eclipse Adoptium\jdk-17
   set CER_FILE=C:\full\path\to\gradle-org.cer

   rem Если alias "gradle" уже есть — удалите: keytool -delete -alias gradle -keystore "%JAVA_HOME%\lib\security\cacerts" -storepass changeit

   "%JAVA_HOME%\bin\keytool" -importcert -alias gradle -file "%CER_FILE%" -keystore "%JAVA_HOME%\lib\security\cacerts" -storepass changeit
   ```
   **Скрипт-помощник:** в папке `rider` лежит `import-ssl-cert.bat`. Запустите **cmd от имени администратора**, задайте `JAVA_HOME` и выполните:  
   `rider\import-ssl-cert.bat C:\full\path\to\gradle-org.cer`  
   В **PowerShell** вызывайте так (через cmd):  
   `cmd /c "`"rider\import-ssl-cert.bat`" `"C:\path\to\gradle-org.cer`""`  
   Скрипт проверит пути и выведет ошибку keytool, если что-то не так.

   **Если не срабатывает:** (1) Убедитесь, что `JAVA_HOME` указывает на тот же JDK, которым пользуется Gradle. (2) Пароль хранилища по умолчанию — `changeit`; если keytool пишет «Password verification failed», укажите правильный `-storepass` или пустой `-storepass ""`. (3) Файл сертификата — PEM или DER (.cer/.pem). (4) Ошибка «access denied» — запустите cmd от имени администратора.

   **Сертификат импортирован, но Gradle всё равно PKIX:** Gradle может запускаться под другим JDK. Используйте **локальный truststore** в проекте:
   1. В cmd: `cmd /c "rider\create-truststore.bat C:\path\to\gradle-org.cer"` (подставьте путь к .cer). Создадутся `rider\gradle-truststore.jks` и `rider\gradle-local.properties`.
   2. В папке rider: `gradlew.bat --stop`, затем `gradlew.bat buildPlugin`.

3. **macOS / Linux:**
   ```bash
   sudo "$JAVA_HOME/bin/keytool" -importcert -alias gradle -file gradle-ca.cer -keystore "$JAVA_HOME/lib/security/cacerts" -storepass changeit
   ```

4. Перезапустите терминал и снова запустите `.\gradlew.bat buildPlugin` (или `./gradlew buildPlugin`).

**Вариант 2 — только обойти загрузку дистрибутива Gradle (локальный zip):**  
Это устраняет SSL только для скачивания Gradle. Разрешение зависимостей (plugins.gradle.org, Maven) по-прежнему идёт по HTTPS, поэтому при корпоративном прокси **всё равно нужен Вариант 1** (добавить CA в Java).

1. На машине с рабочим HTTPS скачайте [gradle-8.13-bin.zip](https://services.gradle.org/distributions/gradle-8.13-bin.zip) и сохраните локально.

2. В файле `rider\gradle\wrapper\gradle-wrapper.properties` замените `distributionUrl` на локальный путь, например:
   ```properties
   distributionUrl=file\:///C:/Users/YourName/gradle-8.13-bin.zip
   ```

3. Запустите снова `.\gradlew.bat buildPlugin`. Если снова появится PKIX/SSL при загрузке плагинов — добавьте CA в Java (Вариант 1).

**Сборка падает с «What went wrong: 25» или другой неочевидной ошибкой:** выполните из папки `rider` команду с полным выводом стека:
```bat
.\gradlew.bat buildPlugin --stacktrace
```
По выводу будет видно, какой именно task упал (часто это загрузка IDE SDK по сети или задача `patchPluginXml`). Убедитесь, что есть доступ в интернет и что корпоративный прокси не блокирует Maven/Central/Google (для загрузки зависимостей и IntelliJ runtime). Предупреждения про `System::load` и `--enable-native-access` устраняются через `rider/gradle.properties` (уже добавлены нужные JVM-аргументы).

---

### Установка плагина в Rider

1. **Соберите плагин** (см. раздел «Сборка плагина» выше). После сборки в папке `output` появится архив:
   - **Windows:** `output\TypeScriptDefinitionGenerator.Rider-<version>.zip`
   - **macOS / Linux:** `output/TypeScriptDefinitionGenerator.Rider-<version>.zip`

2. **Установите плагин в Rider:**  
   - Откройте **Settings** (Ctrl+Alt+S / Cmd+,).  
   - **Plugins** → нажмите **шестерёнку** (Gear icon) → **Install Plugin from Disk…**  
   - Выберите файл `TypeScriptDefinitionGenerator.Rider-<version>.zip` из папки `output`.  
   - Нажмите **OK**, затем **Restart IDE**.

3. После перезапуска плагин готов к работе (контекстное меню по .cs или **Find Action** → «Generate TypeScript Definition»).

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
