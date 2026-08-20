using System.Collections;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using Altinn.Authorization.RepoCtl.Model.Utils;
using CommunityToolkit.Diagnostics;

namespace Altinn.Authorization.RepoCtl.Model;

/// <summary>
/// Represents a (potentially filtered) set of verticals in an Altinn repository.
/// </summary>
public sealed class AltinnVerticalSet
    : IReadOnlySet<AltinnVertical>
    , IReadOnlyDictionary<AltinnVerticalId, AltinnVertical>
{
    internal static AltinnVerticalSet Create(ImmutableArray<AltinnVertical>.Builder verticals)
    {
        verticals.SortBy(static v => v.Id);

        RemoveDuplicates(verticals);
        return new(verticals.DrainToImmutable());

        static void RemoveDuplicates(ImmutableArray<AltinnVertical>.Builder verticals)
        {
            if (verticals.Count < 2)
            {
                return;
            }

            var write = 1;
            for (var read = 1; read < verticals.Count; read++)
            {
                if (verticals[write - 1].Id != verticals[read].Id)
                {
                    verticals[write++] = verticals[read];
                }
            }

            var removed = verticals.Count - write;
            for (var i = 0; i < removed; i++)
            {
                verticals.RemoveAt(verticals.Count - 1);
            }
        }
    }

    private readonly ImmutableArray<AltinnVertical> _verticals;

    private AltinnVerticalSet(ImmutableArray<AltinnVertical> verticals)
    {
        _verticals = verticals;
    }

    /// <inheritdoc/>
    public int Count
        => _verticals.Length;

    /// <inheritdoc/>
    public AltinnVertical this[AltinnVerticalId key]
        => TryGet(key, out var vertical)
        ? vertical
        : ThrowHelper.ThrowArgumentException<AltinnVertical>(nameof(key), $"The vertical with ID '{key}' was not found in this set.");

    /// <inheritdoc/>
    IEnumerable<AltinnVerticalId> IReadOnlyDictionary<AltinnVerticalId, AltinnVertical>.Keys
        => _verticals.Select(static v => v.Id);

    /// <inheritdoc/>
    IEnumerable<AltinnVertical> IReadOnlyDictionary<AltinnVerticalId, AltinnVertical>.Values
        => _verticals;

    /// <summary>
    /// Gets the verticals in this set as an enumerable.
    /// </summary>
    /// <returns>An enumerable of the verticals in this set.</returns>
    public IEnumerable<AltinnVertical> AsEnumerable()
        => this;

    /// <inheritdoc/>
    public bool Contains(AltinnVertical item)
        => TryGet(item.Id, out var vertical)
        && EqualityComparer<AltinnVertical>.Default.Equals(vertical, item);

    /// <inheritdoc/>
    bool IReadOnlyDictionary<AltinnVerticalId, AltinnVertical>.ContainsKey(AltinnVerticalId key)
        => TryGet(key, out _);

    /// <inheritdoc/>
    bool IReadOnlyDictionary<AltinnVerticalId, AltinnVertical>.TryGetValue(AltinnVerticalId key, [MaybeNullWhen(false)] out AltinnVertical value)
        => TryGet(key, out value);

    /// <inheritdoc/>
    public bool TryGet(AltinnVerticalId key, [MaybeNullWhen(false)] out AltinnVertical value)
    {
        var index = _verticals.BinarySearchBy(static v => v.Id, key);
        if (index < 0)
        {
            value = default;
            return false;
        }

        value = _verticals[index];
        return true;
    }

    /// <summary>
    /// Filters the verticals in this set to only those of the specified kinds.
    /// </summary>
    /// <param name="kinds">The kinds to include.</param>
    /// <returns>A new <see cref="AltinnVerticalSet"/> containing only the verticals of the specified kinds.</returns>
    public AltinnVerticalSet OfKind(AltinnVerticalKindSet kinds)
        => Filter(vertical => kinds.Contains(vertical.Kind));

    /// <summary>
    /// Filters the verticals in this set to only those that are descendants of the specified directory.
    /// </summary>
    /// <param name="directory">The directory to filter by.</param>
    /// <returns>A new <see cref="AltinnVerticalSet"/> containing only the verticals in the specified directory.</returns>
    public AltinnVerticalSet InDirectory(DirectoryInfo directory)
        => Filter(vertical => vertical.Directory.IsDescendantOf(directory));

    private AltinnVerticalSet Filter(Func<AltinnVertical, bool> predicate)
    {
        var newSet = _verticals.RemoveAll(v => !predicate(v));
        if (newSet.Length == _verticals.Length)
        {
            return this;
        }

        return new AltinnVerticalSet(newSet);
    }

    /// <inheritdoc/>
    public ImmutableArray<AltinnVertical>.Enumerator GetEnumerator()
        => _verticals.GetEnumerator();

    /// <inheritdoc/>
    IEnumerator<AltinnVertical> IEnumerable<AltinnVertical>.GetEnumerator()
        => ((IEnumerable<AltinnVertical>)_verticals).GetEnumerator();

    /// <inheritdoc/>
    IEnumerator<KeyValuePair<AltinnVerticalId, AltinnVertical>> IEnumerable<KeyValuePair<AltinnVerticalId, AltinnVertical>>.GetEnumerator()
        => _verticals.Select(static v => KeyValuePair.Create(v.Id, v)).GetEnumerator();

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<AltinnVertical>)_verticals).GetEnumerator();

    /// <inheritdoc/>
    bool IReadOnlySet<AltinnVertical>.IsProperSubsetOf(IEnumerable<AltinnVertical> other)
        => new HashSet<AltinnVertical>(_verticals).IsProperSubsetOf(other);

    /// <inheritdoc/>
    bool IReadOnlySet<AltinnVertical>.IsProperSupersetOf(IEnumerable<AltinnVertical> other)
        => new HashSet<AltinnVertical>(_verticals).IsProperSupersetOf(other);

    /// <inheritdoc/>
    bool IReadOnlySet<AltinnVertical>.IsSubsetOf(IEnumerable<AltinnVertical> other)
        => new HashSet<AltinnVertical>(_verticals).IsSubsetOf(other);

    /// <inheritdoc/>
    bool IReadOnlySet<AltinnVertical>.IsSupersetOf(IEnumerable<AltinnVertical> other)
        => new HashSet<AltinnVertical>(_verticals).IsSupersetOf(other);

    /// <inheritdoc/>
    bool IReadOnlySet<AltinnVertical>.Overlaps(IEnumerable<AltinnVertical> other)
        => new HashSet<AltinnVertical>(_verticals).Overlaps(other);

    /// <inheritdoc/>
    bool IReadOnlySet<AltinnVertical>.SetEquals(IEnumerable<AltinnVertical> other)
        => new HashSet<AltinnVertical>(_verticals).SetEquals(other);
}
