# TypeScript Definition Generator — Rider Kotlin Frontend

Контекстное меню Solution View (ПКМ по .cs файлу).

## Требования

- **Java 17+** — IntelliJ Platform Gradle Plugin 2.x требует Java 17
- Gradle 8.13+ (через `./gradlew`)

## Сборка

```bash
# Убедитесь, что используется Java 17+
java -version   # должно быть 17 или выше

# Если Java 8/11 — задайте JAVA_HOME:
export JAVA_HOME=/path/to/jdk-17   # macOS: $(/usr/libexec/java_home -v 17)

./gradlew buildPlugin
```

Выход: `build/distributions/TypeScriptDefinitionGenerator.Rider-1.0.0.zip`
