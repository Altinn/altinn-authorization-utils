using System.Collections;
using System.Collections.Immutable;
using System.Runtime.InteropServices;
using CommunityToolkit.Diagnostics;

namespace Altinn.Authorization.ModelUtils;

/// <content>
/// Contains the inner <see cref="ImmutableValueSet{T}.Builder"/> class.
/// </content>
public sealed partial class ImmutableValueSet<T>
{
    /// <summary>
    /// A sorted set backed by a <see cref="ImmutableArray{T}.Builder"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Instance members of this class are <em>not</em> thread-safe.
    /// </para>
    /// </remarks>
    public sealed class Builder
        : ISet<T>
        , IReadOnlyList<T>
    {
        private readonly ImmutableArray<T> _source;

        private IComparer<T> _comparer;
        private List<T>? _builder;

        internal Builder(ImmutableArray<T> source, IComparer<T> comparer)
        {
            _source = source;
            _comparer = comparer;
        }

        internal Builder(ImmutableValueSet<T> source)
            : this(source._inner.ToImmutableArray(), source._comparer)
        {
        }

        #region ISet<T> properties

        /// <inheritdoc/>
        public int Count
            => _builder is not null
                ? _builder.Count
                : _source.Length;

        bool ICollection<T>.IsReadOnly
            => false;

        #endregion

        /// <inheritdoc/>
        public T this[int index]
            => _builder is not null
                ? _builder[index]
                : _source[index];

        /// <summary>
        /// Gets or sets the comparer used to determine value equality and ordering.
        /// </summary>
        public IComparer<T> Comparer
        {
            get => _comparer;
            set
            {
                Guard.IsNotNull(value);

                if (value != _comparer)
                {
                    _comparer = value;

                    if (_builder is not null)
                    {
                        var old = CollectionsMarshal.AsSpan(_builder);
                        _builder = new(_builder.Capacity);
                        UnionWith(old);
                    }
                    else
                    {
                        var old = _source.AsSpan();
                        _builder = new(_source.Length);
                        UnionWith(old);
                    }
                }
            }
        }

        /// <summary>
        /// Gets a span over the current contents of this builder, in sorted order.
        /// </summary>
        /// <remarks>
        /// The returned span is only valid until the next mutation of this builder.
        /// </remarks>
        /// <returns>A read-only span over the current contents of this builder.</returns>
        private ReadOnlySpan<T> AsSpan()
            => _builder is not null
                ? CollectionsMarshal.AsSpan(_builder)
                : _source.AsSpan();

        #region Public methods

        /// <summary>
        /// Creates an <see cref="ImmutableValueSet{T}"/> from the current contents of this builder.
        /// The builder remains mutable and can be further modified after this method returns.
        /// Subsequent mutations to the builder do not affect the immutability or contents of the returned set.
        /// </summary>
        /// <returns>An <see cref="ImmutableValueSet{T}"/> containing the current contents of this builder.</returns>
        public ImmutableValueSet<T> ToImmutable()
        {
            if (_builder is not null)
            {
                return new ImmutableValueSet<T>([.. _builder], _comparer);
            }

            return new ImmutableValueSet<T>(_source, _comparer);
        }

        #endregion

        #region ISet<T> methods

        /// <inheritdoc/>
        public bool Add(T item)
        {
            var index = AsSpan().BinarySearch(item, _comparer);
            if (index >= 0)
            {
                return false;
            }

            var insertIndex = ~index;
            EnsureBuilder().Insert(insertIndex, item);

            return true;
        }

        /// <inheritdoc/>
        public void ExceptWith(IEnumerable<T> other)
        {
            Guard.IsNotNull(other);

            foreach (var item in other)
            {
                Remove(item);
            }
        }

        /// <inheritdoc/>
        public void IntersectWith(IEnumerable<T> other)
        {
            Guard.IsNotNull(other);

            List<T> builder = new();
            foreach (var item in new SortedSet<T>(other, _comparer))
            {
                if (Contains(item))
                {
                    builder.Add(item);
                }
            }

            builder.Sort(_comparer);
            _builder = builder;
        }

        /// <inheritdoc/>
        public void SymmetricExceptWith(IEnumerable<T> other)
        {
            var builder = EnsureBuilder();
            foreach (var item in new SortedSet<T>(other, _comparer))
            {
                var index = CollectionsMarshal.AsSpan(builder).BinarySearch(item, _comparer);

                if (index >= 0)
                {
                    builder.RemoveAt(index);
                }
                else
                {
                    builder.Insert(~index, item);
                }
            }
        }

        /// <inheritdoc/>
        public void UnionWith(IEnumerable<T> other)
        {
            Guard.IsNotNull(other);

            var builder = EnsureBuilder();
            foreach (var item in other)
            {
                var index = CollectionsMarshal.AsSpan(builder).BinarySearch(item, _comparer);

                if (index < 0)
                {
                    builder.Insert(~index, item);
                }
            }
        }

        /// <inheritdoc cref="UnionWith(IEnumerable{T})"/>
        internal void UnionWith(ReadOnlySpan<T> other)
        {
            if (other.IsEmpty)
            {
                return;
            }

            var builder = EnsureBuilder();
            foreach (var item in other)
            {
                var index = CollectionsMarshal.AsSpan(builder).BinarySearch(item, _comparer);

                if (index < 0)
                {
                    builder.Insert(~index, item);
                }
            }
        }

        /// <inheritdoc/>
        bool ISet<T>.IsProperSubsetOf(IEnumerable<T> other)
            => new SortedSet<T>(this, _comparer).IsProperSubsetOf(other);

        /// <inheritdoc/>
        bool ISet<T>.IsProperSupersetOf(IEnumerable<T> other)
            => new SortedSet<T>(this, _comparer).IsProperSupersetOf(other);

        /// <inheritdoc/>
        bool ISet<T>.IsSubsetOf(IEnumerable<T> other)
            => new SortedSet<T>(this, _comparer).IsSubsetOf(other);

        /// <inheritdoc/>
        bool ISet<T>.IsSupersetOf(IEnumerable<T> other)
            => new SortedSet<T>(this, _comparer).IsSupersetOf(other);

        /// <inheritdoc/>
        bool ISet<T>.Overlaps(IEnumerable<T> other)
            => new SortedSet<T>(this, _comparer).Overlaps(other);

        /// <inheritdoc/>
        bool ISet<T>.SetEquals(IEnumerable<T> other)
            => new SortedSet<T>(this, _comparer).SetEquals(other);

        /// <inheritdoc/>
        public void Clear()
        {
            EnsureBuilder().Clear();
        }

        /// <inheritdoc/>
        void ICollection<T>.CopyTo(T[] array, int arrayIndex)
            => AsSpan().CopyTo(array.AsSpan(arrayIndex));

        /// <inheritdoc/>
        void ICollection<T>.Add(T item)
            => Add(item);

        /// <inheritdoc/>
        public bool Contains(T item)
            => AsSpan().BinarySearch(item, _comparer) >= 0;

        /// <inheritdoc/>
        public bool Remove(T item)
        {
            var index = AsSpan().BinarySearch(item, _comparer);
            if (index < 0)
            {
                return false;
            }

            EnsureBuilder().RemoveAt(index);

            return true;
        }

        /// <inheritdoc/>
        public IEnumerator<T> GetEnumerator()
        {
            if (_builder is not null)
            {
                return _builder.GetEnumerator();
            }

            return EnumerateWithBuilderCheck(this, _source.GetEnumerator());

            static IEnumerator<T> EnumerateWithBuilderCheck(Builder builder, ImmutableArray<T>.Enumerator enumerator)
            {
                while (enumerator.MoveNext())
                {
                    if (builder._builder is not null)
                    {
                        ThrowHelper.ThrowInvalidOperationException("Collection was modified; enumeration operation may not execute.");
                    }

                    yield return enumerator.Current;
                }
            }
        }

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();

        /// <inheritdoc cref="ImmutableValueSet{T}.IndexOf(T)"/>
        public int IndexOf(T item)
            => AsSpan().BinarySearch(item, _comparer);

        #endregion

        #region Private helpers

        private List<T> EnsureBuilder()
        {
            if (_builder is null)
            {
                _builder = [.. _source];
            }

            return _builder;
        }

        #endregion
    }
}
