using System.Collections;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using Altinn.Authorization.RepoCtl.Model.Utils;
using CommunityToolkit.Diagnostics;

namespace Altinn.Authorization.RepoCtl.Model;

public sealed partial class AltinnVerticalSet
{
    /// <summary>
    /// Builder class for constructing and modifying instances of <see cref="AltinnVerticalSet"/>.
    /// </summary>
    public sealed class Builder
        : ISet<AltinnVertical>
        , IDictionary<AltinnVerticalId, AltinnVertical>
    {
        private readonly ImmutableArray<AltinnVertical>.Builder _builder;
        private bool _dirty;

        internal Builder(ImmutableArray<AltinnVertical>.Builder builder, bool knownValid)
        {
            _builder = builder;
            _dirty = !knownValid;
        }

        /// <summary>
        /// Converts the builder into an immutable <see cref="AltinnVerticalSet"/> instance.
        /// </summary>
        /// <returns>An immutable <see cref="AltinnVerticalSet"/> instance containing the verticals added to the builder.</returns>
        public AltinnVerticalSet ToImmutable()
            => new(EnsureNormalized().ToImmutable());

        /// <summary>
        /// Converts the builder into an immutable <see cref="AltinnVerticalSet"/> instance and drains the builder.
        /// </summary>
        /// <returns>An immutable <see cref="AltinnVerticalSet"/> instance containing the verticals added to the builder, and the builder is drained.</returns>
        public AltinnVerticalSet DrainToImmutable()
            => new(EnsureNormalized().DrainToImmutable());

        /// <inheritdoc/>
        public AltinnVertical this[AltinnVerticalId key]
        {
            get => TryGet(key, out var vertical)
                ? vertical
                : ThrowHelper.ThrowArgumentException<AltinnVertical>(nameof(key), $"The vertical with ID '{key}' was not found in this set.");

            set
            {
                EnsureMatchingKey(key, value);
                var index = GetIndex(key);

                if (index < 0)
                {
                    Add(value);
                }
                else
                {
                    _builder[index] = value;
                }
            }
        }

        /// <inheritdoc/>
        public int Count
            => EnsureNormalized().Count;

        /// <inheritdoc/>
        ICollection<AltinnVerticalId> IDictionary<AltinnVerticalId, AltinnVertical>.Keys
            => new List<AltinnVerticalId>(EnsureNormalized().Select(static v => v.Id)).AsReadOnly();

        ICollection<AltinnVertical> IDictionary<AltinnVerticalId, AltinnVertical>.Values
            => new List<AltinnVertical>(EnsureNormalized()).AsReadOnly();

        /// <inheritdoc/>
        bool ICollection<KeyValuePair<AltinnVerticalId, AltinnVertical>>.IsReadOnly
            => false;

        /// <inheritdoc/>
        bool ICollection<AltinnVertical>.IsReadOnly
            => false;

        private static void EnsureMatchingKey(AltinnVerticalId key, AltinnVertical value)
        {
            if (value.Id != key)
            {
                ThrowHelper.ThrowArgumentException(nameof(key), $"The vertical ID '{value.Id}' does not match the key '{key}'.");
            }
        }

        private ImmutableArray<AltinnVertical>.Builder EnsureNormalized()
        {
            if (!_dirty)
            {
                return _builder;
            }

            _builder.SortBy(static v => v.Id);
            RemoveDuplicates(_builder);
            _dirty = false;
            return _builder;

            // note: method assumes items are sorted
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

        /// <inheritdoc/>
        public bool Add(AltinnVertical item)
        {
            if (TryGet(item.Id, out _))
            {
                return false;
            }

            _builder.Add(item);
            _dirty = true;
            return true;
        }

        /// <summary>
        /// Adds a range of verticals to the set.
        /// </summary>
        /// <param name="items">The collection of verticals to add.</param>
        public void AddRange(IEnumerable<AltinnVertical> items)
        {
            foreach (var item in items)
            {
                Add(item);
            }
        }

        /// <inheritdoc/>
        void ICollection<AltinnVertical>.Add(AltinnVertical item)
        {
            if (!Add(item))
            {
                ThrowHelper.ThrowArgumentException(nameof(item), $"An item with the id '{item.Id}' already exists.");
            }
        }

        /// <inheritdoc/>
        void IDictionary<AltinnVerticalId, AltinnVertical>.Add(AltinnVerticalId key, AltinnVertical value)
        {
            EnsureMatchingKey(key, value);
            if (!Add(value))
            {
                ThrowHelper.ThrowArgumentException(nameof(key), $"An item with the key '{key}' already exists.");
            }
        }

        /// <inheritdoc/>
        void ICollection<KeyValuePair<AltinnVerticalId, AltinnVertical>>.Add(KeyValuePair<AltinnVerticalId, AltinnVertical> item)
            => ((IDictionary<AltinnVerticalId, AltinnVertical>)this).Add(item.Key, item.Value);

        /// <inheritdoc/>
        public void Clear()
        {
            _builder.Clear();
            _dirty = false;
        }

        // this is the ISet<AltinnVertical> tree, which uses the Id to indicate uniqueness.
        /// <inheritdoc/>
        bool ICollection<AltinnVertical>.Contains(AltinnVertical item)
            => TryGet(item.Id, out _);

        /// <inheritdoc/>
        bool ICollection<KeyValuePair<AltinnVerticalId, AltinnVertical>>.Contains(KeyValuePair<AltinnVerticalId, AltinnVertical> item)
        {
            EnsureMatchingKey(item.Key, item.Value);
            return TryGet(item.Key, out var value)
                && EqualityComparer<AltinnVertical>.Default.Equals(value, item.Value);
        }

        /// <inheritdoc/>
        bool IDictionary<AltinnVerticalId, AltinnVertical>.ContainsKey(AltinnVerticalId key)
            => TryGet(key, out _);

        /// <inheritdoc/>
        void ICollection<AltinnVertical>.CopyTo(AltinnVertical[] array, int arrayIndex)
            => EnsureNormalized().CopyTo(array, arrayIndex);

        /// <inheritdoc/>
        void ICollection<KeyValuePair<AltinnVerticalId, AltinnVertical>>.CopyTo(KeyValuePair<AltinnVerticalId, AltinnVertical>[] array, int arrayIndex)
            => EnsureNormalized().Select(static v => KeyValuePair.Create(v.Id, v)).ToList().CopyTo(array, arrayIndex);

        /// <inheritdoc/>
        void ISet<AltinnVertical>.ExceptWith(IEnumerable<AltinnVertical> other)
        {
            var ids = GetIds(other);
            EnsureNormalized().RemoveAll(vertical => ids.Contains(vertical.Id));
        }

        /// <summary>
        /// Gets the verticals in this set as an enumerable.
        /// </summary>
        /// <returns>An enumerable of the verticals in this set.</returns>
        public IEnumerable<AltinnVertical> AsEnumerable()
            => this;

        /// <inheritdoc/>
        public IEnumerator<AltinnVertical> GetEnumerator()
            => EnsureNormalized().GetEnumerator();

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();

        /// <inheritdoc/>
        IEnumerator<KeyValuePair<AltinnVerticalId, AltinnVertical>> IEnumerable<KeyValuePair<AltinnVerticalId, AltinnVertical>>.GetEnumerator()
            => EnsureNormalized().Select(static v => KeyValuePair.Create(v.Id, v)).GetEnumerator();

        /// <inheritdoc/>
        void ISet<AltinnVertical>.IntersectWith(IEnumerable<AltinnVertical> other)
        {
            var ids = GetIds(other);
            EnsureNormalized().RemoveAll(vertical => !ids.Contains(vertical.Id));
        }

        /// <inheritdoc/>
        bool ISet<AltinnVertical>.IsProperSubsetOf(IEnumerable<AltinnVertical> other)
            => GetIds(this).IsProperSubsetOf(GetIds(other));

        /// <inheritdoc/>
        bool ISet<AltinnVertical>.IsProperSupersetOf(IEnumerable<AltinnVertical> other)
            => GetIds(this).IsProperSupersetOf(GetIds(other));

        /// <inheritdoc/>
        bool ISet<AltinnVertical>.IsSubsetOf(IEnumerable<AltinnVertical> other)
            => GetIds(this).IsSubsetOf(GetIds(other));

        /// <inheritdoc/>
        bool ISet<AltinnVertical>.IsSupersetOf(IEnumerable<AltinnVertical> other)
            => GetIds(this).IsSupersetOf(GetIds(other));

        /// <inheritdoc/>
        bool ISet<AltinnVertical>.Overlaps(IEnumerable<AltinnVertical> other)
            => GetIds(this).Overlaps(GetIds(other));

        // this is the ISet<AltinnVertical> tree, which uses the Id to indicate uniqueness.
        /// <inheritdoc/>
        bool ICollection<AltinnVertical>.Remove(AltinnVertical item)
        {
            var index = GetIndex(item.Id);
            if (index < 0)
            {
                return false;
            }

            SwapRemoveAt(index);
            return true;
        }

        /// <inheritdoc/>
        public bool Remove(AltinnVerticalId key)
        {
            var index = GetIndex(key);
            if (index < 0)
            {
                return false;
            }

            SwapRemoveAt(index);
            return true;
        }

        /// <inheritdoc/>
        bool ICollection<KeyValuePair<AltinnVerticalId, AltinnVertical>>.Remove(KeyValuePair<AltinnVerticalId, AltinnVertical> item)
        {
            EnsureMatchingKey(item.Key, item.Value);
            var index = GetIndex(item.Key);
            if (index < 0)
            {
                return false;
            }

            if (EqualityComparer<AltinnVertical>.Default.Equals(_builder[index], item.Value))
            {
                SwapRemoveAt(index);
                return true;
            }

            return false;
        }

        /// <inheritdoc/>
        bool ISet<AltinnVertical>.SetEquals(IEnumerable<AltinnVertical> other)
            => GetIds(this).SetEquals(GetIds(other));

        /// <inheritdoc/>
        void ISet<AltinnVertical>.SymmetricExceptWith(IEnumerable<AltinnVertical> other)
        {
            ArgumentNullException.ThrowIfNull(other);

            // Snapshot before modifying the builder, including sequences that enumerate it.
            var verticals = other.DistinctBy(static vertical => vertical.Id).ToArray();
            foreach (var vertical in verticals)
            {
                if (!Remove(vertical.Id))
                {
                    Add(vertical);
                }
            }
        }

        /// <inheritdoc/>
        bool IDictionary<AltinnVerticalId, AltinnVertical>.TryGetValue(AltinnVerticalId key, [MaybeNullWhen(false)] out AltinnVertical value)
            => TryGet(key, out value);

        /// <inheritdoc/>
        void ISet<AltinnVertical>.UnionWith(IEnumerable<AltinnVertical> other)
        {
            ArgumentNullException.ThrowIfNull(other);

            // Snapshot before modifying the builder, including sequences that enumerate it.
            var verticals = other.ToArray();
            foreach (var vertical in verticals)
            {
                Add(vertical);
            }
        }

        private static HashSet<AltinnVerticalId> GetIds(IEnumerable<AltinnVertical> verticals)
        {
            ArgumentNullException.ThrowIfNull(verticals);

            return verticals.Select(static vertical => vertical.Id).ToHashSet();
        }

        /// <inheritdoc cref="AltinnVerticalSet.TryGet(AltinnVerticalId, out AltinnVertical)"/>
        public bool TryGet(AltinnVerticalId key, [MaybeNullWhen(false)] out AltinnVertical value)
        {
            var index = GetIndex(key);
            if (index < 0)
            {
                value = default;
                return false;
            }

            value = _builder[index];
            return true;
        }

        private int GetIndex(AltinnVerticalId key)
            => ImmutableCollectionsMarshal.AsMemory(EnsureNormalized()).Span
                .BinarySearchBy(static v => v.Id, key);

        private void SwapRemoveAt(int index)
        {
            Debug.Assert(!_dirty);

            var lastIndex = _builder.Count - 1;
            if (index != lastIndex)
            {
                (_builder[index], _builder[lastIndex]) = (_builder[lastIndex], _builder[index]);
                _dirty = true;
            }

            // note; if we remove the last item and the collection is already unique and sorted,
            // it's still unique ans sorted. Hence we don't mark dirty here.
            _builder.RemoveAt(lastIndex);
        }
    }
}
