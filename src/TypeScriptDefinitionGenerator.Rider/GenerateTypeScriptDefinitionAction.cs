using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using JetBrains.Application.DataContext;
using JetBrains.Application.UI.Actions;
using JetBrains.Application.UI.ActionsRevised.Menu;
using JetBrains.ProjectModel;
using JetBrains.ProjectModel.DataContext;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.DataContext;

namespace TypeScriptDefinitionGenerator.Rider;

/// <summary>
/// Action handler for TypeScriptDefinitionGenerator.GenerateTypeScriptDefinition (bound by naming convention).
/// </summary>
public class TypeScriptDefinitionGenerator_GenerateTypeScriptDefinitionAction : IExecutableAction
{
    public bool Update(IDataContext context, ActionPresentation presentation, DelegateUpdate nextUpdate)
    {
        var filePath = GetFilePathFromContext(context);
        var enabled = !string.IsNullOrEmpty(filePath) && filePath.EndsWith(".cs", StringComparison.OrdinalIgnoreCase);
        presentation.Visible = enabled;
        return enabled;
    }

    public void Execute(IDataContext context, DelegateExecute nextExecute)
    {
        var filePath = GetFilePathFromContext(context);
        if (string.IsNullOrEmpty(filePath) || !filePath.EndsWith(".cs", StringComparison.OrdinalIgnoreCase))
        {
            ShowNotification(context, "No C# file selected", isError: true);
            return;
        }

        if (!File.Exists(filePath))
        {
            ShowNotification(context, $"File not found: {filePath}", isError: true);
            return;
        }

        var solution = context.GetData(ProjectModelDataConstants.SOLUTION);
        var solutionDir = solution?.SolutionDirectory?.FullPath;
        if (string.IsNullOrEmpty(solutionDir))
        {
            solutionDir = Path.GetDirectoryName(filePath);
            while (!string.IsNullOrEmpty(solutionDir))
            {
                var slnFiles = Directory.GetFiles(solutionDir, "*.sln");
                if (slnFiles.Length > 0)
                    break;
                solutionDir = Path.GetDirectoryName(solutionDir);
            }
        }

        if (string.IsNullOrEmpty(solutionDir))
        {
            ShowNotification(context, "Could not find solution directory", isError: true);
            return;
        }

        var cliProjectPath = Path.Combine(solutionDir, "src", "TypeScriptDefinitionGenerator.Cli", "TypeScriptDefinitionGenerator.Cli.csproj");
        if (!File.Exists(cliProjectPath))
        {
            ShowNotification(context, $"CLI project not found: {cliProjectPath}", isError: true);
            return;
        }

        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = $"run --project \"{cliProjectPath}\" -- \"{filePath}\"",
                WorkingDirectory = solutionDir,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            using var process = Process.Start(startInfo);
            if (process == null)
            {
                ShowNotification(context, "Failed to start CLI process", isError: true);
                return;
            }

            var output = process.StandardOutput.ReadToEnd();
            var error = process.StandardError.ReadToEnd();
            process.WaitForExit(30000);

            if (process.ExitCode == 0)
            {
                var message = string.IsNullOrEmpty(output) ? "TypeScript definition generated successfully" : output.Trim();
                ShowNotification(context, message, isError: false);
            }
            else
            {
                var message = string.IsNullOrEmpty(error) ? output : error;
                ShowNotification(context, $"Generation failed: {message}", isError: true);
            }
        }
        catch (Exception ex)
        {
            ShowNotification(context, $"Error: {ex.Message}", isError: true);
        }
    }

    private static string? GetFilePathFromContext(IDataContext context)
    {
        var projectFile = context.GetData(ProjectModelDataConstants.PROJECT_MODEL_ELEMENT) as IProjectFile;
        if (projectFile != null)
            return projectFile.Location?.FullPath;

        var declaredElements = context.GetData(JetBrains.ReSharper.Psi.DataContext.PsiDataConstants.DECLARED_ELEMENTS);
        var declaredElement = declaredElements?.FirstOrDefault();
        var sourceFile = declaredElement?.GetSourceFiles().FirstOrDefault();
        if (sourceFile != null)
            return sourceFile.GetLocation()?.FullPath;

        return null;
    }

    private static void ShowNotification(IDataContext context, string message, bool isError)
    {
        try
        {
            if (isError)
                JetBrains.Util.MessageBox.ShowError(message, "TypeScript Definition Generator");
            else
                JetBrains.Util.MessageBox.ShowInfo(message, "TypeScript Definition Generator");
        }
        catch
        {
            // Fallback: at least the action completed
        }
    }
}