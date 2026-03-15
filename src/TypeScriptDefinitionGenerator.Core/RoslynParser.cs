using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace TypeScriptDefinitionGenerator.Core;

public class RoslynParser
{
    private static readonly Regex IsNumber = new Regex("^[0-9a-fx]+[ul]{0,2}$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly HashSet<string> IgnoreAttributes = new(StringComparer.OrdinalIgnoreCase)
    {
        "System.Runtime.Serialization.IgnoreDataMemberAttribute",
        "System.Text.Json.Serialization.JsonIgnoreAttribute",
        "System.Web.Script.Serialization.ScriptIgnoreAttribute"
    };

    private static readonly Dictionary<string, string[]> NameAttributes = new(StringComparer.OrdinalIgnoreCase)
    {
        { "DataMember", new[] { "Name" } },
        { "JsonPropertyName", new[] { "" } }
    };

    public static IEnumerable<IntellisenseObject> ProcessFile(string filePath, string fileContent, IGeneratorOptions options, HashSet<string>? projectFiles = null)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(fileContent, path: filePath);
        var refs = GetDefaultReferences();
        var compilation = CSharpCompilation.Create(
            "TempAssembly",
            new[] { syntaxTree },
            refs,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var semanticModel = compilation.GetSemanticModel(syntaxTree);
        var list = new List<IntellisenseObject>();
        var underProcess = new HashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default);
        var processed = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        projectFiles ??= new HashSet<string>();
        var sourceDir = Path.GetDirectoryName(filePath) ?? ".";

        var typesToProcess = new List<(INamedTypeSymbol Symbol, bool IsEnum)>();
        foreach (var typeDecl in syntaxTree.GetRoot().DescendantNodes().OfType<BaseTypeDeclarationSyntax>())
        {
            var symbol = semanticModel.GetDeclaredSymbol(typeDecl, default) as INamedTypeSymbol;
            if (symbol == null)
                continue;

            var key = GetNamespace(symbol, options) + "." + symbol.Name;
            if (processed.Contains(key))
                continue;
            processed.Add(key);

            if (typeDecl is EnumDeclarationSyntax)
                typesToProcess.Add((symbol, true));
            else if (typeDecl is ClassDeclarationSyntax && ShouldProcessClass(symbol))
                typesToProcess.Add((symbol, false));
        }

        foreach (var (symbol, isEnum) in typesToProcess)
        {
            if (isEnum)
                ProcessEnum(symbol, list, options);
            else
                ProcessClass(symbol, semanticModel, list, underProcess, options, projectFiles, sourceDir);
        }

        return new HashSet<IntellisenseObject>(list);
    }

    /// <summary>
    /// Returns a user-friendly reason why no types were generated, or null if the reason is unknown.
    /// </summary>
    public static string? GetEmptyReason(string filePath, string fileContent, IGeneratorOptions options)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(fileContent, path: filePath);
        var refs = GetDefaultReferences();
        var compilation = CSharpCompilation.Create(
            "TempAssembly",
            new[] { syntaxTree },
            refs,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var semanticModel = compilation.GetSemanticModel(syntaxTree);

        var typeDecls = syntaxTree.GetRoot().DescendantNodes().OfType<BaseTypeDeclarationSyntax>().ToList();
        if (typeDecls.Count == 0)
            return "File has no class or enum declarations.";

        var nonPublicCount = 0;
        var symbolCount = 0;
        foreach (var typeDecl in typeDecls)
        {
            var symbol = semanticModel.GetDeclaredSymbol(typeDecl, default) as INamedTypeSymbol;
            if (symbol == null)
                continue;
            symbolCount++;
            if (symbol.DeclaredAccessibility != Accessibility.Public)
                nonPublicCount++;
        }

        if (symbolCount > 0 && nonPublicCount == symbolCount)
            return "File contains only internal or private types. Only public classes and enums are generated.";

        return null;
    }

    private static bool ShouldProcessClass(INamedTypeSymbol symbol)
    {
        return symbol.DeclaredAccessibility == Accessibility.Public && !symbol.IsStatic;
    }

    private static void ProcessEnum(INamedTypeSymbol symbol, List<IntellisenseObject> list, IGeneratorOptions options)
    {
        var data = new IntellisenseObject
        {
            Name = symbol.Name,
            IsEnum = true,
            FullName = symbol.ToDisplayString(),
            Namespace = GetNamespace(symbol, options),
            Summary = GetSummary(symbol)
        };

        foreach (var member in symbol.GetMembers().OfType<IFieldSymbol>())
        {
            if (member.DeclaredAccessibility != Accessibility.Public)
                continue;

            var prop = new IntellisenseProperty
            {
                Name = member.Name,
                Summary = GetSummary(member),
                InitExpression = member.ConstantValue != null ? member.ConstantValue.ToString() : null
            };
            if (prop.InitExpression != null && !IsNumber.IsMatch(prop.InitExpression))
                prop.InitExpression = null;

            data.Properties.Add(prop);
        }

        if (data.Properties.Count > 0)
            list.Add(data);
    }

    private static void ProcessClass(INamedTypeSymbol symbol, SemanticModel semanticModel, List<IntellisenseObject> list,
        HashSet<INamedTypeSymbol> underProcess, IGeneratorOptions options, HashSet<string> projectFiles, string sourceDir)
    {
        if (underProcess.Contains(symbol))
            return;

        var baseType = symbol.BaseType;
        INamedTypeSymbol? baseClass = null;
        if (baseType != null && baseType.SpecialType != SpecialType.System_Object)
        {
            baseClass = baseType as INamedTypeSymbol;
        }

        var traversedTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var references = new HashSet<string>();
        var properties = GetProperties(symbol, semanticModel, traversedTypes, references, options, projectFiles, sourceDir);

        foreach (var nested in symbol.GetTypeMembers())
        {
            if (nested.TypeKind == TypeKind.Class && nested.DeclaredAccessibility == Accessibility.Public)
            {
                ProcessClass(nested, semanticModel, list, underProcess, options, projectFiles, sourceDir);
            }
            else if (nested.TypeKind == TypeKind.Enum && nested.DeclaredAccessibility == Accessibility.Public)
            {
                ProcessEnum(nested, list, options);
            }
        }

        var intellisenseObject = new IntellisenseObject(properties.ToList(), references)
        {
            Namespace = GetNamespace(symbol, options),
            Name = symbol.Name,
            BaseNamespace = baseClass != null ? GetNamespace(baseClass, options) : null,
            BaseName = baseClass?.Name,
            FullName = symbol.ToDisplayString(),
            Summary = GetSummary(symbol)
        };

        if (baseClass != null && !underProcess.Contains(baseClass))
        {
            underProcess.Add(baseClass);
            var baseLocation = baseClass.DeclaringSyntaxReferences.FirstOrDefault()?.SyntaxTree?.FilePath;
            if (!string.IsNullOrEmpty(baseLocation) && File.Exists(baseLocation))
            {
                var baseContent = File.ReadAllText(baseLocation);
                var baseList = ProcessFile(baseLocation!, baseContent, options, projectFiles).ToList();
                foreach (var r in baseList.SelectMany(x => x.References))
                    intellisenseObject.UpdateReferences(new[] { r });
                list.AddRange(baseList);
            }
            underProcess.Remove(baseClass);
        }

        list.Add(intellisenseObject);
    }

    private static IEnumerable<IntellisenseProperty> GetProperties(INamedTypeSymbol symbol, SemanticModel semanticModel,
        HashSet<string> traversedTypes, HashSet<string> references, IGeneratorOptions options, HashSet<string> projectFiles, string sourceDir)
    {
        foreach (var member in symbol.GetMembers().OfType<IPropertySymbol>())
        {
            if (member.DeclaredAccessibility != Accessibility.Public || member.GetMethod == null)
                continue;
            if (member.IsStatic)
                continue;
            if (HasIgnoreAttribute(member))
                continue;

            var type = GetType(member.Type, symbol, semanticModel, traversedTypes, references, options, projectFiles, sourceDir);
            var prop = new IntellisenseProperty
            {
                Name = GetPropertyName(member),
                Type = type,
                Summary = GetSummary(member)
            };
            yield return prop;
        }
    }

    private static bool HasIgnoreAttribute(IPropertySymbol property)
    {
        return property.GetAttributes().Any(a =>
            IgnoreAttributes.Contains(a.AttributeClass?.ToDisplayString() ?? ""));
    }

    private static string GetPropertyName(IPropertySymbol property)
    {
        foreach (var attr in property.GetAttributes())
        {
            var className = attr.AttributeClass?.Name ?? "";
            if (!NameAttributes.TryGetValue(className, out var argumentNames))
                continue;

            var arg = attr.ConstructorArguments.Concat(attr.NamedArguments.Select(a => a.Value))
                .FirstOrDefault(a => a.Value is string s && !string.IsNullOrEmpty(s));

            if (arg.Value is string value)
                return value.Trim('"', '\'');
        }
        return property.Name;
    }

    private static IntellisenseType GetType(ITypeSymbol typeSymbol, INamedTypeSymbol rootElement, SemanticModel semanticModel,
        HashSet<string> traversedTypes, HashSet<string> references, IGeneratorOptions options, HashSet<string> projectFiles, string sourceDir)
    {
        var isArray = typeSymbol is IArrayTypeSymbol;
        var typeStr = typeSymbol.ToDisplayString();
        var isCollection = typeStr.StartsWith("System.Collections", StringComparison.Ordinal);
        var isDictionary = typeStr.Contains("Dictionary") || typeStr.Contains("IDictionary");

        ITypeSymbol effectiveType = typeSymbol;
        if (typeSymbol is IArrayTypeSymbol arrayType)
            effectiveType = arrayType.ElementType;
        else if (isCollection && typeSymbol is INamedTypeSymbol namedType && namedType.IsGenericType)
            effectiveType = namedType.TypeArguments.FirstOrDefault() ?? typeSymbol;

        var result = new IntellisenseType
        {
            IsArray = !isDictionary && (isArray || isCollection),
            IsDictionary = isDictionary,
            CodeName = effectiveType.ToDisplayString()
        };

        if (effectiveType is INamedTypeSymbol namedEffective)
        {
            var isPrimitive = IsPrimitive(effectiveType);
            var hasIntellisense = options.IgnoreIntellisense;

            if (!isPrimitive)
            {
                var loc = namedEffective.DeclaringSyntaxReferences.FirstOrDefault()?.SyntaxTree?.FilePath;
                if (!string.IsNullOrEmpty(loc))
                {
                    var dtsFile = Utility.GenerateFileName(loc!, options);
                    if (projectFiles.Contains(dtsFile) || File.Exists(dtsFile))
                    {
                        hasIntellisense = true;
                        references.Add(dtsFile);
                    }
                }
            }

            if (hasIntellisense && !isPrimitive)
            {
                var ns = GetNamespace(namedEffective, options);
                var name = namedEffective.Name;
                result.ClientSideReferenceName = (options.DeclareModule ? ns + "." : "") + Utility.CamelCaseClassName(name, options);
            }

            if (!isPrimitive && !traversedTypes.Contains(namedEffective.ToDisplayString()) && !isCollection)
            {
                traversedTypes.Add(namedEffective.ToDisplayString());
                result.Shape = GetProperties(namedEffective, semanticModel, traversedTypes, references, options, projectFiles, sourceDir).ToList();
                traversedTypes.Remove(namedEffective.ToDisplayString());
            }
        }

        return result;
    }

    private static bool IsPrimitive(ITypeSymbol type)
    {
        var str = type.ToDisplayString().ToLowerInvariant();
        if (type.SpecialType != SpecialType.None && type.SpecialType != SpecialType.None)
        {
            switch (type.SpecialType)
            {
                case SpecialType.System_String:
                case SpecialType.System_Int16:
                case SpecialType.System_Int32:
                case SpecialType.System_Int64:
                case SpecialType.System_UInt16:
                case SpecialType.System_UInt32:
                case SpecialType.System_UInt64:
                case SpecialType.System_Byte:
                case SpecialType.System_SByte:
                case SpecialType.System_Single:
                case SpecialType.System_Double:
                case SpecialType.System_Decimal:
                case SpecialType.System_Boolean:
                    return true;
            }
        }
        return str.Contains("datetime") || str.Contains("guid");
    }

    private static string GetNamespace(ISymbol symbol, IGeneratorOptions options)
    {
        if (!options.UseNamespace)
            return options.DefaultModuleName;

        var ns = symbol.ContainingNamespace?.ToDisplayString();
        return (string.IsNullOrEmpty(ns) ? options.DefaultModuleName : ns)!;
    }

    private static string? GetSummary(ISymbol symbol)
    {
        var doc = symbol.GetDocumentationCommentXml();
        if (string.IsNullOrWhiteSpace(doc))
            return null;

        try
        {
            var el = XElement.Parse(doc);
            var summary = el.Descendants("summary").Select(x => x.Value).FirstOrDefault();
            return string.IsNullOrWhiteSpace(summary) ? null : summary.Trim();
        }
        catch
        {
            return null;
        }
    }

    private static PortableExecutableReference[] GetDefaultReferences()
    {
        var refs = new List<PortableExecutableReference>();
        var runtimePath = Path.GetDirectoryName(typeof(object).Assembly.Location) ?? "";

        var trustedAssemblies = ((string?)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES"))?.Split(Path.PathSeparator) ?? Array.Empty<string>();
        foreach (var path in trustedAssemblies)
        {
            if (path.IndexOf("mscorlib", StringComparison.OrdinalIgnoreCase) >= 0 ||
                path.IndexOf("System.Runtime", StringComparison.OrdinalIgnoreCase) >= 0 ||
                path.IndexOf("System.Collections", StringComparison.OrdinalIgnoreCase) >= 0 ||
                path.IndexOf("netstandard", StringComparison.OrdinalIgnoreCase) >= 0 ||
                path.IndexOf("System.Private.CoreLib", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                if (File.Exists(path))
                    refs.Add(MetadataReference.CreateFromFile(path));
            }
        }

        if (refs.Count == 0)
        {
            var files = new[] { "mscorlib.dll", "System.Runtime.dll", "System.Collections.dll", "System.Linq.dll", "System.Private.CoreLib.dll", "netstandard.dll" };
            foreach (var file in files)
            {
                var fullPath = Path.Combine(runtimePath, file);
                if (File.Exists(fullPath))
                    refs.Add(MetadataReference.CreateFromFile(fullPath));
            }
        }

        return refs.ToArray();
    }
}