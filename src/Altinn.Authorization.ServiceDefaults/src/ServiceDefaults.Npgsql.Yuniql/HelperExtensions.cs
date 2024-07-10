using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Altinn.Authorization.ServiceDefaults.Npgsql.Yuniql;

/// <summary>
/// Helper extension methods.
/// </summary>
[ExcludeFromCodeCoverage]
internal static class HelperExtensions
{
    public static string? GetVersion(this Assembly assembly)
    {
        var informationalVersion = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
        if (informationalVersion is not null)
        {
            return informationalVersion;
        }

        var version = assembly.GetName().Version;
        if (version is not null)
        {
            return version.ToString();
        }

        return null;
    }

    public static string RemoveBuildInfo(this string version)
    {
        var plusIndex = version.IndexOf('+');
        if (plusIndex == -1)
        {
            return version;
        }

        return version.Substring(0, plusIndex);
    }

    public static string TruncateVersionLength(this string value, int maxLength)
    {
        if (value.Length <= maxLength)
        {
            return value;
        }

        var minusIndex = value.LastIndexOf('-');
        if (minusIndex == -1)
        {
            return value.Substring(0, maxLength);
        }

        return value.Substring(0, minusIndex).TruncateVersionLength(maxLength);
    }
}
