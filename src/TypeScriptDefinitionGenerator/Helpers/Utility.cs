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

        public static string RemoveDefaultExtension(string tsFile)
        {
            var defaultExt = GetDefaultExtension(string.Empty);
            if (!tsFile.EndsWith(defaultExt))
            {
                throw new System.ArgumentException("File must end with default extension");
            }

            return tsFile.Substring(0, tsFile.Length - defaultExt.Length);
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

        /// <summary>
        /// Get relative path of <paramref name="toPath"/> relative to <paramref name="fromPath"/>.
        /// </summary>
        /// <param name="fromPath">Base path</param>
        /// <param name="toPath">File to be reference relatively</param>
        /// <returns></returns>
        /// <remarks>Source: https://stackoverflow.com/questions/275689/how-to-get-relative-path-from-absolute-path#answer-485516</remarks>
        public static string GetRelativePath(string fromPath, string toPath)
        {
            int fromAttr = Directory.Exists(fromPath) ? FILE_ATTRIBUTE_DIRECTORY : FILE_ATTRIBUTE_NORMAL;
            int toAttr = Directory.Exists(toPath) ? FILE_ATTRIBUTE_DIRECTORY : FILE_ATTRIBUTE_NORMAL;

            var path = new System.Text.StringBuilder(260); // MAX_PATH
            if (PathRelativePathTo(
                path,
                fromPath,
                fromAttr,
                toPath,
                toAttr) == 0)
            {
                throw new System.ArgumentException("Paths must have a common prefix");
            }
            return path.ToString();
        }

        private const int FILE_ATTRIBUTE_DIRECTORY = 0x10;
        private const int FILE_ATTRIBUTE_NORMAL = 0x80;

        [System.Runtime.InteropServices.DllImport("shlwapi.dll", SetLastError = true)]
        private static extern int PathRelativePathTo(System.Text.StringBuilder pszPath,
            string pszFrom, int dwAttrFrom, string pszTo, int dwAttrTo);
    }
}
