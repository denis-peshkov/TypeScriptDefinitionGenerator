using System;
using System.IO;
using System.Text.Json;

namespace TypeScriptDefinitionGenerator.Core;

public static class OptionsLoader
{
    private const string OverrideFileName = "tsdefgen.json";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true
    };

    public static IGeneratorOptions LoadFromProjectDirectory(string projectDirectory)
    {
        var jsonPath = FindOverrideFile(projectDirectory);
        if (jsonPath == null)
            return new GeneratorOptions();

        try
        {
            var json = File.ReadAllText(jsonPath);
            var overrides = JsonSerializer.Deserialize<GeneratorOptions>(json, JsonOptions);
            return overrides ?? new GeneratorOptions();
        }
        catch
        {
            return new GeneratorOptions();
        }
    }

    private static string? FindOverrideFile(string startDirectory)
    {
        var dir = startDirectory;
        while (!string.IsNullOrEmpty(dir))
        {
            var path = Path.Combine(dir, OverrideFileName);
            if (File.Exists(path))
                return path;
            dir = Path.GetDirectoryName(dir);
        }
        return null;
    }
}