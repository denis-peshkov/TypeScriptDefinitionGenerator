namespace TypeScriptDefinitionGenerator.Core;

public class IntellisenseProperty
{
    public IntellisenseProperty()
    {
    }

    public IntellisenseProperty(IntellisenseType type, string propertyName)
    {
        Type = type;
        Name = propertyName;
    }

    public string Name { get; set; } = string.Empty;

    public string NameWithOption => (Type != null && Type.IsOptional) ? Name + "?" : Name;

    public IntellisenseType? Type { get; set; }

    public string? Summary { get; set; }

    public string? InitExpression { get; set; }
}