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
        var jsonPath = Path.Combine(projectDirectory, OverrideFileName);
        if (!File.Exists(jsonPath))
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
}