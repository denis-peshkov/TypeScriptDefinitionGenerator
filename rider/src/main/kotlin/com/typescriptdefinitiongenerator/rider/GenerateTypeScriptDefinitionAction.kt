package com.typescriptdefinitiongenerator.rider

import com.intellij.openapi.actionSystem.ActionUpdateThread
import com.intellij.openapi.actionSystem.AnAction
import com.intellij.openapi.actionSystem.AnActionEvent
import com.intellij.openapi.actionSystem.CommonDataKeys
import com.intellij.openapi.ui.Messages
import com.intellij.openapi.vfs.LocalFileSystem
import java.io.File

class GenerateTypeScriptDefinitionAction : AnAction() {

    override fun getActionUpdateThread() = ActionUpdateThread.BGT

    override fun update(e: AnActionEvent) {
        val file = e.getData(CommonDataKeys.VIRTUAL_FILE)
        val visible = file != null && file.extension?.equals("cs", ignoreCase = true) == true
        e.presentation.isEnabledAndVisible = visible
    }

    override fun actionPerformed(e: AnActionEvent) {
        val file = e.getData(CommonDataKeys.VIRTUAL_FILE) ?: run {
            Messages.showErrorDialog(e.project, "No file selected", "TypeScript Definition Generator")
            return
        }
        val filePath = file.path
        if (!filePath.endsWith(".cs", ignoreCase = true)) {
            Messages.showErrorDialog(e.project, "Please select a C# file", "TypeScript Definition Generator")
            return
        }

        var solutionDir = e.project?.basePath ?: findSolutionDirectory(filePath)
        if (solutionDir == null) {
            Messages.showErrorDialog(e.project, "Could not find solution directory", "TypeScript Definition Generator")
            return
        }

        val cliProjectPath = File(solutionDir, "src/TypeScriptDefinitionGenerator.Cli/TypeScriptDefinitionGenerator.Cli.csproj")
        if (!cliProjectPath.exists()) {
            Messages.showErrorDialog(
                e.project,
                "CLI project not found: ${cliProjectPath.absolutePath}\n\nOpen a solution that contains TypeScriptDefinitionGenerator.",
                "TypeScript Definition Generator"
            )
            return
        }

        val dotnetPath = resolveDotnetPath()
        if (dotnetPath == null) {
            Messages.showErrorDialog(
                e.project,
                "dotnet not found. Add it to PATH or install .NET SDK.\n\nCommon paths: /usr/local/share/dotnet/dotnet, /opt/homebrew/bin/dotnet",
                "TypeScript Definition Generator"
            )
            return
        }

        try {
            val process = ProcessBuilder(
                dotnetPath, "run", "--project", cliProjectPath.absolutePath, "--", filePath
            )
                .directory(File(solutionDir))
                .redirectErrorStream(true)
                .start()

            val output = process.inputStream.bufferedReader().readText()
            process.waitFor()

            val generatedPath = output.lineSequence()
                .mapNotNull { line -> line.substringAfter("Generated: ", "").takeIf { it.isNotEmpty() }?.trim() }
                .firstOrNull()
            if (process.exitValue() == 0 && generatedPath != null) {
                refreshGeneratedFiles(filePath, output)
                updateCsproj(filePath, output, solutionDir)
            } else {
                val message = when {
                    output.trim().isNotEmpty() -> output.trim()
                    process.exitValue() != 0 -> "Generation failed (exit code ${process.exitValue()})"
                    else -> "File was not generated"
                }
                Messages.showErrorDialog(e.project, message, "TypeScript Definition Generator")
            }
        } catch (ex: Exception) {
            Messages.showErrorDialog(e.project, "Error: ${ex.message}", "TypeScript Definition Generator")
        }
    }

    private fun resolveDotnetPath(): String? {
        val dotnetRoot = System.getenv("DOTNET_ROOT")
        if (dotnetRoot != null) {
            val exe = File(dotnetRoot, if (System.getProperty("os.name").lowercase().contains("win")) "dotnet.exe" else "dotnet")
            if (exe.canExecute()) return exe.absolutePath
        }
        val pathEnv = System.getenv("PATH") ?: ""
        for (dir in pathEnv.split(File.pathSeparator)) {
            val exe = File(dir, if (System.getProperty("os.name").lowercase().contains("win")) "dotnet.exe" else "dotnet")
            if (exe.canExecute()) return exe.absolutePath
        }
        val commonPaths = listOf(
            "/usr/local/share/dotnet/dotnet",
            "/opt/homebrew/bin/dotnet",
            "/opt/homebrew/share/dotnet/dotnet"
        )
        for (p in commonPaths) {
            val f = File(p)
            if (f.canExecute()) return f.absolutePath
        }
        return null
    }

    private fun refreshGeneratedFiles(sourceFilePath: String, output: String) {
        val dirsToRefresh = mutableSetOf<File>()
        File(sourceFilePath).parentFile?.let { dirsToRefresh.add(it) }
        output.lineSequence()
            .mapNotNull { line -> line.substringAfter("Generated: ", "").takeIf { it.isNotEmpty() }?.trim() }
            .mapNotNull { path -> File(path).parentFile }
            .forEach { dirsToRefresh.add(it) }
        if (dirsToRefresh.isEmpty()) return
        Thread.sleep(100) // ensure file is flushed to disk
        for (dir in dirsToRefresh) {
            if (dir.exists()) {
                val vf = LocalFileSystem.getInstance().refreshAndFindFileByIoFile(dir)
                vf?.refresh(false, true)
            }
        }
    }

    private fun updateCsproj(sourceFilePath: String, output: String, solutionDir: String) {
        val generatedPath = output.lineSequence()
            .mapNotNull { line -> line.substringAfter("Generated: ", "").takeIf { it.isNotEmpty() }?.trim() }
            .firstOrNull() ?: sourceFilePath.replace(Regex("\\.cs$", RegexOption.IGNORE_CASE), ".generated.d.ts")
        val sourceFile = File(sourceFilePath)
        val projectDir = findCsprojDirectory(sourceFile) ?: return
        val projectPath = projectDir.toPath()
        val csproj = projectDir.listFiles()?.firstOrNull { it.name.endsWith(".csproj") } ?: return
        val relativeSource = projectPath.relativize(sourceFile.toPath()).toString().replace("/", "\\")
        val generatedFile = File(generatedPath)
        val relativeGenerated = if (generatedFile.exists()) {
            projectPath.relativize(generatedFile.toPath()).toString().replace("/", "\\")
        } else {
            relativeSource.replace(".cs", ".generated.d.ts")
        }
        val originalContent = csproj.readText(Charsets.UTF_8)
        var content = originalContent
        val sourceItem = """<TypeScriptDefinitionSource Include="$relativeSource" />"""
        val noneItem = """<None Update="$relativeGenerated">
      <DependentUpon>$relativeSource</DependentUpon>
    </None>"""
        if (!content.contains("TypeScriptDefinitionSource Include=\"$relativeSource\"")) {
            content = addOrCreateItemGroup(content, "TypeScriptDefinitionSource", sourceItem)
        }
        if (!content.contains("None Update=\"$relativeGenerated\"")) {
            content = addOrCreateItemGroup(content, "None", noneItem)
        }
        val buildTargetsPath = File(solutionDir, "build/TypeScriptDefinitionGenerator.targets")
        val dirBuildTargets = File(projectDir, "Directory.Build.targets")
        val createdDirBuild = buildTargetsPath.exists() && !dirBuildTargets.exists()
        if (createdDirBuild) {
            val targetsRelPath = projectPath.relativize(buildTargetsPath.toPath()).toString().replace("\\", "/")
            val dirBuildContent = """<?xml version="1.0" encoding="utf-8"?>
<Project xmlns="http://schemas.microsoft.com/developer/msbuild/2003">
  <Import Project="$(MSBuildThisFileDirectory)$targetsRelPath" />
</Project>
"""
            dirBuildTargets.writeText(dirBuildContent, Charsets.UTF_8)
        }
        if (content != originalContent) {
            csproj.writeText(content, Charsets.UTF_8)
        }
        if (content != originalContent || createdDirBuild) {
            LocalFileSystem.getInstance().refreshAndFindFileByIoFile(projectDir)
        }
    }

    private fun addOrCreateItemGroup(content: String, itemName: String, newItem: String): String {
        val pathAttr = when (itemName) {
            "TypeScriptDefinitionSource" -> Regex("""Include="([^"]+)"""")
            "None" -> Regex("""Update="([^"]+)"""")
            else -> return content
        }
        val newPath = pathAttr.find(newItem)?.groupValues?.get(1) ?: return content
        val itemRegex = when (itemName) {
            "TypeScriptDefinitionSource" -> Regex("""<TypeScriptDefinitionSource\s+Include="[^"]+"\s*/>""")
            "None" -> Regex("""<None\s+Update="[^"]+"[^>]*>[\s\S]*?</None>""")
            else -> return content
        }
        val itemGroupRegex = Regex("""<ItemGroup>(\s*)(.*?)(\s*)</ItemGroup>""", RegexOption.DOT_MATCHES_ALL)
        val match = itemGroupRegex.findAll(content).firstOrNull { m ->
            itemRegex.containsMatchIn(m.groupValues[2])
        } ?: run {
            return content.replace("</Project>", """
  <ItemGroup>
    $newItem
  </ItemGroup>
</Project>""")
        }
        val (prefix, block, suffix) = match.destructured
        val existingItems = itemRegex.findAll(block).map { it.value }.toList()
        val existingPaths = existingItems.mapNotNull { pathAttr.find(it)?.groupValues?.get(1) }
        if (newPath in existingPaths) return content
        val allItems = (existingItems + newItem).sortedBy { pathAttr.find(it)?.groupValues?.get(1) ?: "" }
        val itemsContent = allItems.joinToString("\n    ")
        val newItemGroup = "<ItemGroup>$prefix$itemsContent$suffix</ItemGroup>"
        return content.replace(match.value, newItemGroup)
    }

    private fun findCsprojDirectory(file: File): File? {
        var dir = file.parentFile ?: return null
        while (dir != null) {
            if (dir.listFiles()?.any { it.name.endsWith(".csproj") } == true) return dir
            dir = dir.parentFile
        }
        return null
    }

    private fun findSolutionDirectory(filePath: String): String? {
        var dir = File(filePath).parentFile ?: return null
        while (dir != null) {
            val hasSolution = dir.listFiles()?.any { f ->
                f.name.endsWith(".sln", ignoreCase = true) || f.name.endsWith(".slnx", ignoreCase = true)
            } == true
            if (hasSolution) return dir.absolutePath
            dir = dir.parentFile
        }
        return null
    }
}
