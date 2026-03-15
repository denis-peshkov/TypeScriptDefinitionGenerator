using System;
using System.Collections.Generic;

namespace TypeScriptDefinitionGenerator.Core;

public class IntellisenseObject : IEquatable<IntellisenseObject>
{
    public string Namespace { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? BaseNamespace { get; set; }
    public string? BaseName { get; set; }
    public string FullName { get; set; } = string.Empty;
    public bool IsEnum { get; set; }
    public string? Summary { get; set; }
    public IList<IntellisenseProperty> Properties { get; }
    public HashSet<string> References { get; }

    public IntellisenseObject()
    {
        Properties = new List<IntellisenseProperty>();
        References = new HashSet<string>();
    }

    public IntellisenseObject(IList<IntellisenseProperty> properties)
    {
        Properties = properties;
        References = new HashSet<string>();
    }

    public IntellisenseObject(IList<IntellisenseProperty> properties, HashSet<string> references)
    {
        Properties = properties;
        References = references;
    }

    public void UpdateReferences(IEnumerable<string> moreReferences)
    {
        References.UnionWith(moreReferences);
    }

    public bool Equals(IntellisenseObject? other)
    {
        return other != null &&
               other.Name == Name &&
               other.Namespace == Namespace &&
               other.BaseName == BaseName &&
               other.BaseNamespace == BaseNamespace &&
               other.FullName == FullName;
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as IntellisenseObject);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            var hash = 17;
            hash = (hash * 397) ^ (Name?.GetHashCode() ?? 0);
            hash = (hash * 397) ^ (Namespace?.GetHashCode() ?? 0);
            hash = (hash * 397) ^ (FullName?.GetHashCode() ?? 0);
            return hash;
        }
    }
}