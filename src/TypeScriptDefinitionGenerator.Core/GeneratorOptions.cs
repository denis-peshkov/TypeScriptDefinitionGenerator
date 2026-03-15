namespace TypeScriptDefinitionGenerator.Core;

public enum EOLType
{
    /// <summary>Unix</summary>
    LF,

    /// <summary>Windows</summary>
    CRLF
}

public interface IGeneratorOptions
{
    bool CamelCaseEnumerationValues { get; }
    bool CamelCasePropertyNames { get; }
    bool CamelCaseTypeNames { get; }
    string DefaultModuleName { get; }
    bool UseNamespace { get; }
    bool ClassInsteadOfInterface { get; }
    bool DeclareModule { get; }
    bool IgnoreIntellisense { get; }
    EOLType EOLType { get; }
    bool IndentTab { get; }
    byte IndentTabSize { get; }
    bool WebEssentials2015 { get; }
}

public class GeneratorOptions : IGeneratorOptions
{
    public bool CamelCaseEnumerationValues { get; set; }
    public bool CamelCasePropertyNames { get; set; } = true;
    public bool CamelCaseTypeNames { get; set; }
    public string DefaultModuleName { get; set; } = "Server.Dtos";
    public bool UseNamespace { get; set; } = true;
    public bool ClassInsteadOfInterface { get; set; }
    public bool DeclareModule { get; set; } = true;
    public bool IgnoreIntellisense { get; set; } = true;
    public EOLType EOLType { get; set; } = EOLType.LF;
    public bool IndentTab { get; set; } = true;
    public byte IndentTabSize { get; set; } = 2;
    public bool WebEssentials2015 { get; set; }
}
