using System.Globalization;
using System.IO;

namespace TypeScriptDefinitionGenerator.Helpers
{
    internal static class Utility
    {
        public static string GenerateFileName(string sourceFile)
        {
            return Path.ChangeExtension(sourceFile, GetDefaultExtension(Path.GetExtension(sourceFile)));
        }

        public static string GetDefaultExtension(string originalExt)
        {
            string declaredExt = Options.DeclareModule ? ".d" : string.Empty;
            string ext = $".generated{declaredExt}.ts";

            if (Options.WebEssentials2015)
            {
                return originalExt + ext;
            }
            return ext;
        }

        public static string CamelCaseClassName(string name)
        {
            if (Options.CamelCaseTypeNames)
            {
                name = CamelCase(name);
            }
            return name;
        }

        public static string CamelCaseEnumValue(string name)
        {
            if (Options.CamelCaseEnumerationValues)
            {
                name = CamelCase(name);
            }
            return name;
        }

        public static string CamelCasePropertyName(string name)
        {
            if (Options.CamelCasePropertyNames)
            {
                name = CamelCase(name);
            }
            return name;
        }

        private static string CamelCase(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return name;
            }
            return name[0].ToString(CultureInfo.CurrentCulture).ToLower(CultureInfo.CurrentCulture) + name.Substring(1);
        }
    }
}
