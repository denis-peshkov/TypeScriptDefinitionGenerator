using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.IO;

namespace TypeScriptDefinitionGenerator
{
    public enum EOLType
    {
        /// <summary>Unix</summary>
        LF,

        /// <summary>Windows</summary>
        CRLF
    }

    public class OptionsDialogPage : DialogPage
    {
        internal const bool _defCamelCaseEnumerationValues = false;
        internal const bool _defCamelCasePropertyNames = true;
        internal const bool _defCamelCaseTypeNames = false;

        internal const bool _defWebEssentials2015 = false;

        internal const bool _defClassInsteadOfInterface = false;
        internal const string _defModuleName = "Server.Dtos";
        internal const bool _defUseNamespace = true;
        internal const bool _defAddAmdModuleName = false;
        internal const bool _defDeclareModule = true;
        internal const bool _defIgnoreIntellisense = true;
        internal const EOLType _defEOLType = EOLType.LF;
        internal const bool _defIndentTab = true;
        internal const byte _defIndentTabSize = 2;

        [Category("Casing")]
        [DisplayName("Camel case enum values")]
        [DefaultValue(_defCamelCaseEnumerationValues)]
        public bool CamelCaseEnumerationValues { get; set; } = _defCamelCaseEnumerationValues;

        [Category("Casing")]
        [DisplayName("Camel case property names")]
        [DefaultValue(_defCamelCasePropertyNames)]
        public bool CamelCasePropertyNames { get; set; } = _defCamelCasePropertyNames;

        [Category("Casing")]
        [DisplayName("Camel case type names")]
        [DefaultValue(_defCamelCaseTypeNames)]
        public bool CamelCaseTypeNames { get; set; } = _defCamelCaseTypeNames;

        [Category("Compatibilty")]
        [DisplayName("Web Essentials 2015 file names")]
        [Description("Web Essentials 2015 format is <filename>.cs.d.ts instead of <filename>.d.ts")]
        [DefaultValue(_defWebEssentials2015)]
        public bool WebEssentials2015 { get; set; } = _defWebEssentials2015;

        [Category("Settings")]
        [DisplayName("Default Module name")]
        [Description("Set the top-level module name for the generated .d.ts file. Default is \"Server.Dtos\"")]
        public string DefaultModuleName { get; set; } = _defModuleName;

        [Category("Settings")]
        [DisplayName("Use Namespace")]
        [Description("Use Namespace by default, otherwise \"Default Module name\" will be taken.")]
        public bool UseNamespace { get; set; } = _defUseNamespace;

        [Category("Settings")]
        [DisplayName("Add AMD module name annotation")]
        [Description("Add '/// <amd-module name='[ModuleName]' to top of generated file/>'")]
        public bool AddAmdModuleName { get; set; } = _defAddAmdModuleName;
        
        [Category("Settings")]
        [DisplayName("Class instead of Interface")]
        [Description("Controls whether to generate a class or an interface: default is an Interface")]
        [DefaultValue(_defClassInsteadOfInterface)]
        public bool ClassInsteadOfInterface { get; set; } = _defClassInsteadOfInterface;

        [Category("Settings")]
        [DisplayName("Declare module")]
        [Description("Controls whether to generate types in declared module or without one, but with export")]
        [DefaultValue(_defDeclareModule)]
        public bool DeclareModule { get; set; } = _defDeclareModule;

        [Category("Settings")]
        [DisplayName("Ignore intellisense")]
        [Description("Ignore intellisense for client side reference names")]
        [DefaultValue(_defIgnoreIntellisense)]
        public bool IgnoreIntellisense { get; set; } = _defIgnoreIntellisense;

        [Category("Settings")]
        [DisplayName("End Of Line (EOL)")]
        [Description("Choose the EOL type Unix/Windows")]
        [DefaultValue(_defEOLType)]
        public EOLType EOLType { get; set; } = _defEOLType;

        [Category("Settings")]
        [DisplayName("Indent Tab")]
        [Description("Choose indentation to use Tab/Space: default is Tab")]
        [DefaultValue(_defIndentTab)]
        public bool IndentTab { get; set; } = _defIndentTab;

        [Category("Settings")]
        [DisplayName("Indent Tab Size")]
        [Description("Set amount Spaces to replace the Tab, when Indent Tab is off")]
        [DefaultValue(_defIndentTabSize)]
        public byte IndentTabSize { get; set; } = _defIndentTabSize;
    }

    public class Options
    {
        private const string OVERRIDE_FILE_NAME = "tsdefgen.json";

        private static OptionsOverride overrides { get; set; } = null;

        public static bool CamelCaseEnumerationValues => overrides?.CamelCaseEnumerationValues ?? DtsPackage.Options.CamelCaseEnumerationValues;

        public static bool CamelCasePropertyNames => overrides?.CamelCasePropertyNames ?? DtsPackage.Options.CamelCasePropertyNames;

        public static bool CamelCaseTypeNames => overrides?.CamelCaseTypeNames ?? DtsPackage.Options.CamelCaseTypeNames;

        public static string DefaultModuleName => overrides?.DefaultModuleName ?? DtsPackage.Options.DefaultModuleName;

        public static bool UseNamespace => overrides?.UseNamespace ?? DtsPackage.Options.UseNamespace;

        public static bool AddAmdModuleName => overrides?.AddAmdModuleName ?? DtsPackage.Options.AddAmdModuleName;

        public static bool ClassInsteadOfInterface => overrides?.ClassInsteadOfInterface ?? DtsPackage.Options.ClassInsteadOfInterface;

        public static bool DeclareModule => overrides?.DeclareModule ?? DtsPackage.Options.DeclareModule;

        public static bool IgnoreIntellisense => overrides?.IgnoreIntellisense ?? DtsPackage.Options.IgnoreIntellisense;

        public static EOLType EOLType => overrides?.EOLType ?? DtsPackage.Options.EOLType;

        public static bool IndentTab => overrides?.IndentTab ?? DtsPackage.Options.IndentTab;

        public static byte IndentTabSize => overrides?.IndentTabSize ?? DtsPackage.Options.IndentTabSize;

        public static bool WebEssentials2015 => overrides?.WebEssentials2015 ?? DtsPackage.Options.WebEssentials2015;

        public static void ReadOptionOverrides(ProjectItem sourceItem, bool display = true)
        {
            Project proj = sourceItem.ContainingProject;

            string jsonName = "";

            foreach (ProjectItem item in proj.ProjectItems)
            {
                if (string.Equals(item.Name, OVERRIDE_FILE_NAME, StringComparison.InvariantCultureIgnoreCase))
                {
                    jsonName = item.FileNames[0];
                    break;
                }
            }

            if (!string.IsNullOrEmpty(jsonName))
            {
                // it has been modified since last read - so read again
                try
                {
                    overrides = JsonConvert.DeserializeObject<OptionsOverride>(File.ReadAllText(jsonName));
                    if (display)
                    {
                        VSHelpers.WriteOnOutputWindow(string.Format("Override file processed: {0}", jsonName));
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine(string.Format("Override file processed: {0}", jsonName));
                    }
                }
                catch (Exception e) when (e is Newtonsoft.Json.JsonReaderException || e is Newtonsoft.Json.JsonSerializationException)
                {
                    overrides = null; // incase the read fails
                    VSHelpers.WriteOnOutputWindow(string.Format("Error in Override file: {0}", jsonName));
                    VSHelpers.WriteOnOutputWindow(e.Message);
                    throw;
                }
            }
            else
            {
                if (display)
                {
                    VSHelpers.WriteOnOutputWindow("Using Global Settings");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Using Global Settings");
                }
                overrides = null;
            }
        }

        internal static void SetOptionsOverrides(OptionsOverride optionsOverride)
        {
            overrides = optionsOverride;
        }
    }

    internal class OptionsOverride
    {
        //        [JsonRequired]
        public bool CamelCaseEnumerationValues { get; set; } = OptionsDialogPage._defCamelCaseEnumerationValues;

        //        [JsonRequired]
        public bool CamelCasePropertyNames { get; set; } = OptionsDialogPage._defCamelCasePropertyNames;

        //        [JsonRequired]
        public bool CamelCaseTypeNames { get; set; } = OptionsDialogPage._defCamelCaseTypeNames;

        //        [JsonRequired]
        public string DefaultModuleName { get; set; } = OptionsDialogPage._defModuleName;

        //        [JsonRequired]
        public bool UseNamespace { get; set; } = OptionsDialogPage._defUseNamespace;
        
        //        [JsonRequired]
        public bool AddAmdModuleName { get; set; } = OptionsDialogPage._defAddAmdModuleName;

        //        [JsonRequired]
        public bool ClassInsteadOfInterface { get; set; } = OptionsDialogPage._defClassInsteadOfInterface;

        //        [JsonRequired]
        public bool DeclareModule { get; set; } = OptionsDialogPage._defDeclareModule;

        //        [JsonRequired]
        public bool IgnoreIntellisense { get; set; } = OptionsDialogPage._defIgnoreIntellisense;

        //        [JsonRequired]
        public EOLType EOLType { get; set; } = OptionsDialogPage._defEOLType;

        //        [JsonRequired]
        public bool IndentTab { get; set; } = OptionsDialogPage._defIndentTab;

        //        [JsonRequired]
        public byte IndentTabSize { get; set; } = OptionsDialogPage._defIndentTabSize;

        //        [JsonRequired]
        public bool WebEssentials2015 { get; set; } = OptionsDialogPage._defWebEssentials2015;
    }
}