# TypeScript Definition Generator — Rider Kotlin Frontend

Контекстное меню Solution View (ПКМ по .cs файлу).

## Требования

- **Java 17+** — IntelliJ Platform Gradle Plugin 2.x требует Java 17
- Gradle 8.13+ (через `./gradlew` или на Windows — `gradlew.bat`)

## Сборка

**Из корня репозитория** (рекомендуется): используйте `./build-rider-plugin.sh` (macOS/Linux) или `.\build-rider-plugin.ps1` (Windows) — они соберут и .NET, и Kotlin, и упакуют плагин в `output/TypeScriptDefinitionGenerator.Rider-<version>.zip`.

**Только Kotlin (из этой папки):**

```bash
# Убедитесь, что используется Java 17+
java -version   # должно быть 17 или выше

# Если Java 8/11 — задайте JAVA_HOME:
export JAVA_HOME=/path/to/jdk-17   # macOS: $(/usr/libexec/java_home -v 17)
# Windows: choco install openjdk17

./gradlew buildPlugin   # macOS/Linux
# или на Windows, если есть gradlew.bat:
.\gradlew.bat buildPlugin
```

Выход: `build/distributions/TypeScriptDefinitionGenerator.Rider-1.0.0.zip`

**Ошибка SSL (unable to find valid certification path):** часто из‑за корпоративного прокси. Решения: добавить CA-сертификат в Java `cacerts` через `keytool` или скачать [gradle-8.13-bin.zip](https://services.gradle.org/distributions/gradle-8.13-bin.zip) вручную и в `gradle/wrapper/gradle-wrapper.properties` задать `distributionUrl=file\:///C:/path/to/gradle-8.13-bin.zip`. Подробнее — в [README-RIDER.md](../README-RIDER.md) (раздел «Ошибка SSL при первой загрузке Gradle»).
