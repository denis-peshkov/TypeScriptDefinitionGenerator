using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TypeScriptDefinitionGenerator.Core;

public static class GeneratorService
{
    public const string Version = "2.2.0.0";

    public static string? ConvertToTypeScript(string sourceFilePath, IGeneratorOptions? options = null, HashSet<string>? projectFiles = null)
    {
        try
        {
            options ??= OptionsLoader.LoadFromProjectDirectory(Path.GetDirectoryName(sourceFilePath) ?? ".");
            var content = File.ReadAllText(sourceFilePath);
            var list = RoslynParser.ProcessFile(sourceFilePath, content, options, projectFiles).ToList();
            if (list.Count == 0)
                return null;

            return IntellisenseWriter.WriteTypeScript(list, sourceFilePath, options, Version);
        }
        catch (Exception)
        {
            return null;
        }
    }

    public static void CreateDtsFile(string sourceFilePath, IGeneratorOptions? options = null, Action<string>? log = null)
    {
        options ??= OptionsLoader.LoadFromProjectDirectory(Path.GetDirectoryName(sourceFilePath) ?? ".");
        var dtsFile = Utility.GenerateFileName(sourceFilePath, options);
        var dts = ConvertToTypeScript(sourceFilePath, options);

        if (string.IsNullOrEmpty(dts))
        {
            log?.Invoke($"No types to generate in {sourceFilePath}");
            return;
        }

        File.WriteAllText(dtsFile, dts);
        log?.Invoke($"Generated: {dtsFile}");
    }
}