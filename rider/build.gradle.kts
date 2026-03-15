plugins {
    id("java")
    id("org.jetbrains.kotlin.jvm") version "1.9.0"
    id("org.jetbrains.intellij") version "1.17.4"
}

group = "com.typescriptdefinitiongenerator"
version = "1.0.0"

repositories {
    mavenCentral()
}

intellij {
    type.set("RD")
    version.set("2024.1")  // 1.x supports up to 2024.1; plugin works in Rider 2025.x
}

tasks {
    patchPluginXml {
        sinceBuild.set("241")
        untilBuild.set("")  // no upper limit — compatible with Rider 2025.3
    }
    buildPlugin {
        archiveBaseName.set("TypeScriptDefinitionGenerator.Rider")
    }
    jar {
        archiveBaseName.set("TypeScriptDefinitionGenerator.Rider")
    }
}
