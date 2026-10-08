using System.Collections.Immutable;

namespace Altinn.Authorization.RepoCtl.Commands.Ci;

internal sealed class ChangedPaths(IEnumerable<string> filtersWithChanges)
{
    public static ChangedPaths Empty { get; } = new([]);

    private readonly ImmutableHashSet<string> _filtersWithChanges
        = filtersWithChanges.ToImmutableHashSet(StringComparer.Ordinal);

    public bool HasChanges(string filter, bool includeShared = true)
        => (includeShared && _filtersWithChanges.Contains("shared")) || _filtersWithChanges.Contains(filter);
}
