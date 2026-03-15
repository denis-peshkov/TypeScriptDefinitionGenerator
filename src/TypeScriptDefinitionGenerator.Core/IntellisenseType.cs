using System.Collections.Generic;
using System.Globalization;

namespace TypeScriptDefinitionGenerator.Core;

public class IntellisenseType
{
    public string CodeName { get; set; } = string.Empty;
    public bool IsArray { get; set; }
    public bool IsDictionary { get; set; }
    public bool IsOptional => CodeName.EndsWith("?");
    public string? ClientSideReferenceName { get; set; }
    public IEnumerable<IntellisenseProperty>? Shape { get; set; }
    public bool IsKnownType => TypeScriptName != "any";

    public string TypeScriptName => IsDictionary ? GetKVPTypes() : GetTargetName(CodeName, false, ClientSideReferenceName);

    private static string GetTargetName(string codeName, bool js, string? clientSideRef)
    {
        var t = codeName.ToLowerInvariant().TrimEnd('?');
        switch (t)
        {
            case "int16":
            case "int32":
            case "int64":
            case "byte":
            case "short":
            case "int":
            case "long":
            case "biginteger":
            case "float":
            case "double":
            case "decimal":
                return js ? "Number" : "number";

            case "datetime":
            case "datetimeoffset":
            case "system.datetime":
            case "system.datetimeoffset":
                return "Date";

            case "guid":
            case "system.guid":
            case "string":
                return js ? "String" : "string";

            case "bool":
            case "boolean":
                return js ? "Boolean" : "boolean";
        }
        return js ? "Object" : (clientSideRef ?? "any");
    }

    private string GetKVPTypes()
    {
        var type = CodeName.ToLowerInvariant().TrimEnd('?');
        var parts = type.Split('<', '>');
        if (parts.Length < 2)
            return "{ [index: string]: any }";

        var types = parts[1].Split(',');
        var keyType = types.Length > 0 ? GetTargetName(types[0].Trim(), false, null) : "string";
        var valueType = types.Length > 1 ? GetTargetName(types[1].Trim(), false, null) : "any";

        if (keyType != "string" && keyType != "number")
            keyType = "string";

        return string.Format(CultureInfo.InvariantCulture, "{{ [index: {0}]: {1} }}", keyType, valueType);
    }
}