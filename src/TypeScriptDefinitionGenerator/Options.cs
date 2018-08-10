using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.IO;

namespace TypeScriptDefinitionGenerator
{
    public class OptionsDialogPage : DialogPage
    {
        internal const bool _defCamelCaseEnumerationValues = false;
        internal const bool _defCamelCasePropertyNames = true;
        internal const bool _defCamelCaseTypeNames = false;

        internal const bool _defWebEssentials2015 = false;

        internal const bool _defClassInsteadOfInterface = false;
        internal const string _defModuleName = "Server.Dtos";
        internal const bool _defDeclareModule = true;
        internal const bool _defIgnoreIntellisense = true;

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
    }

    public class Options
    {
        const string OVERRIDE_FILE_NAME = "tsdefgen.json";
        static OptionsOverride overrides { get; set; } = null;
        public static bool CamelCaseEnumerationValues
        {
            get
            {
                return overrides != null ? overrides.CamelCaseEnumerationValues : DtsPackage.Options.CamelCaseEnumerationValues;
            }
        }

        public static bool CamelCasePropertyNames
        {
            get
            {
                return overrides != null ? overrides.CamelCasePropertyNames : DtsPackage.Options.CamelCasePropertyNames;
            }
        }

        public static bool CamelCaseTypeNames
        {
            get
            {
                return overrides != null ? overrides.CamelCaseTypeNames : DtsPackage.Options.CamelCaseTypeNames;
            }
        }
        //todo: set to server namespace
        public static string DefaultModuleName
        {
            get
            {
                return overrides != null ? overrides.DefaultModuleName : DtsPackage.Options.DefaultModuleName;
            }
        }

        public static bool ClassInsteadOfInterface
        {
            get
            {
                return overrides != null ? overrides.ClassInsteadOfInterface : DtsPackage.Options.ClassInsteadOfInterface;
            }
        }

        public static bool DeclareModule
        {
            get
            {
                return overrides != null ? overrides.DeclareModule : DtsPackage.Options.DeclareModule;
            }
        }

        public static bool IgnoreIntellisense
        {
            get
            {
                return overrides != null ? overrides.IgnoreIntellisense : DtsPackage.Options.IgnoreIntellisense;
            }
        }

        public static bool WebEssentials2015
        {
            get
            {
                return overrides != null ? overrides.WebEssentials2015 : DtsPackage.Options.WebEssentials2015;
            }
        }

        public static void ReadOptionOverrides(ProjectItem sourceItem, bool display = true)
        {
            Project proj = sourceItem.ContainingProject;

            string jsonName = "";

            foreach (ProjectItem item in proj.ProjectItems)
            {
                if (item.Name.ToLower() == OVERRIDE_FILE_NAME.ToLower())
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
        public bool ClassInsteadOfInterface { get; set; } = OptionsDialogPage._defClassInsteadOfInterface;

        //        [JsonRequired]
        public bool DeclareModule { get; set; } = OptionsDialogPage._defDeclareModule;

        //        [JsonRequired]
        public bool IgnoreIntellisense { get; set; } = OptionsDialogPage._defIgnoreIntellisense;

        //        [JsonRequired]
        public bool WebEssentials2015 { get; set; } = OptionsDialogPage._defWebEssentials2015;

    }

}
