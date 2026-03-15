using System;
using System.Globalization;
using System.IO;

namespace TypeScriptDefinitionGenerator.Core;

public static class Utility
{
    public static string GenerateFileName(string sourceFile, IGeneratorOptions options)
    {
        return Path.ChangeExtension(sourceFile, GetDefaultExtension(Path.GetExtension(sourceFile), options));
    }

    public static string RemoveDefaultExtension(string tsFile, IGeneratorOptions options)
    {
        var defaultExt = GetDefaultExtension(string.Empty, options);
        if (!tsFile.EndsWith(defaultExt, StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("File must end with default extension");

        return tsFile.Substring(0, tsFile.Length - defaultExt.Length);
    }

    public static string GetDefaultExtension(string originalExt, IGeneratorOptions options)
    {
        var declaredExt = options.DeclareModule ? ".d" : string.Empty;
        var ext = $".generated{declaredExt}.ts";

        if (options.WebEssentials2015)
            return originalExt + ext;

        return ext;
    }

    public static string CamelCaseClassName(string name, IGeneratorOptions options)
    {
        if (options.CamelCaseTypeNames)
            name = CamelCase(name);
        return name;
    }

    public static string CamelCaseEnumValue(string name, IGeneratorOptions options)
    {
        if (options.CamelCaseEnumerationValues)
            name = CamelCase(name);
        return name;
    }

    public static string CamelCasePropertyName(string name, IGeneratorOptions options)
    {
        if (options.CamelCasePropertyNames)
            name = CamelCase(name);
        return name;
    }

    private static string CamelCase(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return name;
        return name[0].ToString(CultureInfo.InvariantCulture).ToLower(CultureInfo.InvariantCulture) + name.Substring(1);
    }

    public static string GetRelativePath(string fromPath, string toPath)
    {
        var fromFull = Path.GetFullPath(fromPath);
        var toFull = Path.GetFullPath(toPath);
        if (Directory.Exists(fromFull))
        {
            var fromUri = new Uri(EnsureTrailingSlash(fromFull));
            var toUri = new Uri(toFull);
            var relativeUri = fromUri.MakeRelativeUri(toUri);
            return Uri.UnescapeDataString(relativeUri.ToString()).Replace('/', Path.DirectorySeparatorChar);
        }
        var fromDir = Path.GetDirectoryName(fromFull) ?? fromFull;
        var fromDirUri = new Uri(EnsureTrailingSlash(fromDir));
        var toFileUri = new Uri(toFull);
        var relUri = fromDirUri.MakeRelativeUri(toFileUri);
        return Uri.UnescapeDataString(relUri.ToString()).Replace('/', Path.DirectorySeparatorChar);
    }

    private static string EnsureTrailingSlash(string path)
    {
        if (!path.EndsWith(Path.DirectorySeparatorChar.ToString()))
            return path + Path.DirectorySeparatorChar;
        return path;
    }
}