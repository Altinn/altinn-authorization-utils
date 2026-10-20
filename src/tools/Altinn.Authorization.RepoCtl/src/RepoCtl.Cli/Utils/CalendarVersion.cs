using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Altinn.Authorization.RepoCtl.Utils;

internal static partial class CalendarVersion
{
    [GeneratedRegex(@"^[0-9]{4}\.[0-9]{2}\.[0-9]{2}(\.(?!000)[0-9]{3})?$", RegexOptions.CultureInvariant)]
    private static partial Regex CalendarVersionPattern { get; }

    public static bool TryValidate(string version, [NotNullWhen(false)] out string? error)
    {
        var match = CalendarVersionPattern.Match(version);
        if (!match.Success || match.Length != version.Length)
        {
            error = $"Invalid calendar version: {version}";
            return false;
        }

        if (!DateOnly.TryParseExact(version.AsSpan(0, 10), "yyyy.MM.dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
        {
            error = $"Invalid calendar date in version: {version}";
            return false;
        }

        error = null;
        return true;
    }
}
