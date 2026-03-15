using System;
using System.IO;
using TypeScriptDefinitionGenerator.Core;

namespace TypeScriptDefinitionGenerator.Cli;

internal static class Program
{
    private static int Main(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("TypeScript Definition Generator");
            Console.WriteLine("Usage: tsdefgen <path-to-cs-file> [path-to-cs-file2 ...]");
            Console.WriteLine("Example: tsdefgen Models/CustomerDto.cs");
            return 1;
        }

        var success = true;
        foreach (var path in args)
        {
            var fullPath = Path.GetFullPath(path);
            if (!File.Exists(fullPath))
            {
                Console.Error.WriteLine($"File not found: {fullPath}");
                success = false;
                continue;
            }

            if (!fullPath.EndsWith(".cs", StringComparison.OrdinalIgnoreCase))
            {
                Console.Error.WriteLine($"Skipping non-C# file: {fullPath}");
                continue;
            }

            try
            {
                GeneratorService.CreateDtsFile(fullPath, null, Console.WriteLine);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error processing {fullPath}: {ex.Message}");
                success = false;
            }
        }

        return success ? 0 : 1;
    }
}