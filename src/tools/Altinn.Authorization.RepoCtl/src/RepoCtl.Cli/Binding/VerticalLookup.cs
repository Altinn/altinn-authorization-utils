using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;
using Altinn.Authorization.RepoCtl.Model;

namespace Altinn.Authorization.RepoCtl.Binding;

internal static class VerticalLookup
{
    internal enum LookupKind
    {
        Id,
        Path,
        Tag,
        IdOrPath,
    }

    public static bool TryLookup(AltinnVerticalSet verticals, LookupKind kind, string lookup, DirectoryInfo workingDirectory, [NotNullWhen(true)] out AltinnVertical? vertical, [NotNullWhen(false)] out string? error)
        => kind switch
        {
            LookupKind.Id => TryLookupById(verticals, lookup, out vertical, out error),
            LookupKind.Path => TryLookupByPath(verticals, lookup, workingDirectory, out vertical, out error),
            LookupKind.Tag => TryLookupByTag(verticals, lookup, out vertical, out error),
            LookupKind.IdOrPath => TryLookupByIdOrPath(verticals, lookup, workingDirectory, out vertical, out error),
            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "Invalid lookup kind."),
        };

    public static bool TryLookupById(AltinnVerticalSet verticals, string lookup, [NotNullWhen(true)] out AltinnVertical? vertical, [NotNullWhen(false)] out string? error)
    {
        vertical = null;
        if (!AltinnVerticalId.TryParse(lookup, CultureInfo.InvariantCulture, out var id))
        {
            error = $"Invalid vertical ID: '{lookup}'.";
            return false;
        }

        if (verticals.TryGet(id, out vertical))
        {
            error = null;
            return true;
        }

        error = $"No vertical found with ID '{lookup}'.";
        return false;
    }

    public static bool TryLookupByIdOrPath(AltinnVerticalSet verticals, string lookup, DirectoryInfo workingDirectory, [NotNullWhen(true)] out AltinnVertical? vertical, [NotNullWhen(false)] out string? error)
    {
        if (TryLookupById(verticals, lookup, out vertical, out error)
            || TryLookupByPath(verticals, lookup, workingDirectory, out vertical, out error))
        {
            return true;
        }

        error = "Could not resolve the vertical selector as a unique vertical ID or directory path.";
        return false;
    }

    public static bool TryLookupByPath(AltinnVerticalSet verticals, string lookup, DirectoryInfo workingDirectory, [NotNullWhen(true)] out AltinnVertical? vertical, [NotNullWhen(false)] out string? error)
    {
        string directory;
        try
        {
            directory = Path.GetFullPath(lookup, workingDirectory.FullName);
        }
        catch (Exception ex) when (ex is ArgumentException or NotSupportedException or PathTooLongException)
        {
            vertical = null;
            error = $"Invalid directory path '{lookup}': {ex.Message}";
            return false;
        }

        return TrySingle(verticals.InDirectory(new(directory)), $"in directory '{directory}'", out vertical, out error);
    }

    public static bool TryLookupByTag(AltinnVerticalSet verticals, string tag, [NotNullWhen(true)] out AltinnVertical? vertical, [NotNullWhen(false)] out string? error)
    {
        return TrySingle(verticals.AsEnumerable().Where(candidate => MatchesTag(candidate, tag)), $"for release tag '{tag}'", out vertical, out error);

        static bool MatchesTag(AltinnVertical vertical, string tag)
        {
            var prefix = $"{vertical.Id.ToString("tag-prefix", CultureInfo.InvariantCulture)}";
            if (!(tag.Length > prefix.Length && tag.StartsWith(prefix, StringComparison.Ordinal)))
            {
                return false;
            }

            var after = tag.AsSpan(prefix.Length);
            return after.StartsWith("-v", StringComparison.Ordinal) && after.Length > 2;
        }
    }

    private static bool TrySingle(IEnumerable<AltinnVertical> candidates, string description, [NotNullWhen(true)] out AltinnVertical? vertical, [NotNullWhen(false)] out string? error)
    {
        using var enumerator = candidates.GetEnumerator();
        if (!enumerator.MoveNext())
        {
            vertical = null;
            error = $"No vertical found {description}.";
            return false;
        }

        vertical = enumerator.Current;

        if (enumerator.MoveNext())
        {
            var builder = new StringBuilder("Multiple verticals found ").Append(description).Append(":\n");
            builder.Append("  - ").Append(vertical.Id).Append('\n');
            do
            {
                builder.Append("  - ").Append(enumerator.Current.Id).Append('\n');
            }
            while (enumerator.MoveNext());

            vertical = null;
            error = builder.ToString();
            return false;
        }

        error = null;
        return true;
    }
}
