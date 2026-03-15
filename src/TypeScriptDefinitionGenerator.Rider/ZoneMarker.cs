using JetBrains.Application.BuildScript.Application.Zones;

namespace TypeScriptDefinitionGenerator.Rider;

/// <summary>
/// Zone marker required for ReSharper to load this plugin assembly.
/// Empty zone marker: components are always loaded (no zone dependencies).
/// </summary>
[ZoneMarker]
public class ZoneMarker
{
}