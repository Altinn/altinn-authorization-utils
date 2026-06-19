using System.Collections;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Altinn.Authorization.ModelUtils;

/// <summary>
/// A set of initialization methods for instances of <see cref="ImmutableValueSet{T}"/>.
/// </summary>
public static partial class ImmutableValueSet
{
    /// <summary>
    /// Creates an empty <see cref="ImmutableValueSet{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type of element stored in the set.</typeparam>
    /// <returns>An empty set.</returns>
    public static ImmutableValueSet<T> Create<T>()
        => ImmutableValueSet<T>.Empty;

    /// <summary>
    /// Creates an empty <see cref="ImmutableValueSet{T}"/> with the specified comparer.
    /// </summary>
    /// <typeparam name="T">The type of element stored in the set.</typeparam>
    /// <param name="comparer">The comparer to use for the set.</param>
    /// <returns>An empty set with the specified comparer.</returns>
    public static ImmutableValueSet<T> Create<T>(IComparer<T>? comparer)
        => ImmutableValueSet<T>.Empty.WithComparer(comparer);

    /// <summary>
    /// Creates an <see cref="ImmutableValueSet{T}"/> containing the specified items.
    /// </summary>
    /// <typeparam name="T">The type of element stored in the set.</typeparam>
    /// <param name="items">The items to include in the set.</param>
    /// <returns>A set containing the specified items.</returns>
    public static ImmutableValueSet<T> Create<T>(params ReadOnlySpan<T> items)
        => ImmutableValueSet<T>.Empty.Union(items);

    /// <summary>
    /// Creates an <see cref="ImmutableValueSet{T}"/> containing the specified items and using the specified comparer.
    /// </summary>
    /// <typeparam name="T">The type of element stored in the set.</typeparam>
    /// <param name="comparer">The comparer to use for the set.</param>
    /// <param name="items">The items to include in the set.</param>
    /// <returns>A set containing the specified items and using the specified comparer.</returns>
    public static ImmutableValueSet<T> Create<T>(IComparer<T>? comparer, params ReadOnlySpan<T> items)
        => ImmutableValueSet<T>.Empty.WithComparer(comparer).Union(items);

    /// <summary>
    /// Creates a builder for constructing an <see cref="ImmutableValueSet{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type of element stored in the set.</typeparam>
    /// <returns>A builder for constructing an <see cref="ImmutableValueSet{T}"/>.</returns>
    public static ImmutableValueSet<T>.Builder CreateBuilder<T>()
        => Create<T>().ToBuilder();

    /// <summary>
    /// Creates a builder for constructing an <see cref="ImmutableValueSet{T}"/> with the specified comparer.
    /// </summary>
    /// <typeparam name="T">The type of element stored in the set.</typeparam>
    /// <param name="comparer">The comparer to use for the set.</param>
    /// <returns>A builder for constructing an <see cref="ImmutableValueSet{T}"/> with the specified comparer.</returns>
    public static ImmutableValueSet<T>.Builder CreateBuilder<T>(IComparer<T>? comparer)
        => Create<T>(comparer).ToBuilder();

    extension<T>(IEnumerable<T> items)
    {
        /// <summary>
        /// Converts the specified items to an <see cref="ImmutableValueSet{T}"/> using the provided comparer.
        /// </summary>
        /// <param name="comparer">The comparer to use.</param>
        /// <returns>An <see cref="ImmutableValueSet{T}"/> containing the specified items.</returns>
        public ImmutableValueSet<T> ToImmutableValueSet(IComparer<T>? comparer)
        {
            if (items is ImmutableValueSet<T> existingSet)
            {
                return existingSet.WithComparer(comparer);
            }

            return ImmutableValueSet<T>.Empty.WithComparer(comparer).Union(items);
        }

        /// <summary>
        /// Converts the specified items to an <see cref="ImmutableValueSet{T}"/> using the default comparer.
        /// </summary>
        /// <returns>An <see cref="ImmutableValueSet{T}"/> containing the specified items.</returns>
        public ImmutableValueSet<T> ToImmutableValueSet()
            => items.ToImmutableValueSet(comparer: null);
    }

    extension<T>(ImmutableValueSet<T>.Builder builder)
    {
        /// <summary>
        /// Converts the builder to an <see cref="ImmutableValueSet{T}"/>.
        /// </summary>
        /// <returns>An <see cref="ImmutableValueSet{T}"/> containing the items in the builder.</returns>
        public ImmutableValueSet<T> ToImmutableValueSet()
            => builder.ToImmutable();
    }
}

/// <summary>
/// A readonly set value semantics.
/// </summary>
/// <typeparam name="T">The type of element stored by the set.</typeparam>
[DebuggerDisplay("{_inner,nq}")]
[CollectionBuilder(typeof(ImmutableValueSet), nameof(ImmutableValueSet.Create))]
public sealed partial class ImmutableValueSet<T>
    : IReadOnlySet<T>
    , IReadOnlyList<T>
    , IEquatable<ImmutableValueSet<T>>
    , IImmutableSet<T>
    , IEqualityOperators<ImmutableValueSet<T>, ImmutableValueSet<T>, bool>
{
    /// <summary>
    /// An empty instance of <see cref="ImmutableValueSet{T}"/> with the default comparer.
    /// </summary>
    public static readonly ImmutableValueSet<T> Empty
        = new(ImmutableArray<T>.Empty, Comparer<T>.Default);

    [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
    private readonly ImmutableArray<T> _inner;

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly IComparer<T> _comparer;

    private ImmutableValueSet(ImmutableArray<T> inner, IComparer<T> comparer)
    {
        _inner = inner;
        _comparer = comparer;
    }

    /// <inheritdoc cref="IImmutableSet{T}.Clear()"/>
    public ImmutableValueSet<T> Clear()
        => IsEmpty ? this : Empty.WithComparer(_comparer);

    #region IImmutableSet<T> Properties

    /// <inheritdoc/>
    public bool IsEmpty
        => _inner.IsEmpty;

    /// <inheritdoc/>
    public int Count
        => _inner.Length;

    #endregion

    /// <summary>
    /// Gets the comparer used by this set.
    /// </summary>
    public IComparer<T> Comparer
        => _comparer;

    #region Indexers

    /// <inheritdoc cref="ImmutableArray{T}.this[int]"/>
    public T this[int index]
        => _inner[index];

    /// <inheritdoc cref="ImmutableArray{T}.ItemRef(int)"/>
    public ref readonly T ItemRef(int index)
        => ref _inner.ItemRef(index);

    #endregion

    #region Public methods


    /// <summary>
    /// Creates a collection with the same contents as this collection that
    /// can be efficiently mutated across multiple operations using standard
    /// mutable interfaces.
    /// </summary>
    /// <returns>A builder for constructing an <see cref="ImmutableValueSet{T}"/>.</returns>
    public Builder ToBuilder()
        => new(this);

    /// <summary>
    /// Searches the set for a given value and returns the equal value it finds, if any.
    /// </summary>
    /// <param name="equalValue">The value to search for.</param>
    /// <param name="actualValue">The value from the set that the search found, or the original value if the search yielded no match.</param>
    /// <returns>A value indicating whether the search was successful.</returns>
    /// <remarks>
    /// This can be useful when you want to reuse a previously stored reference instead of
    /// a newly constructed one (so that more sharing of references can occur) or to look up
    /// a value that has more complete data than the value you currently have, although their
    /// comparer functions indicate they are equal.
    /// </remarks>
    public bool TryGetValue(T equalValue, out T actualValue)
    {
        int index = _inner.AsSpan().BinarySearch(equalValue, _comparer);
        if (index >= 0)
        {
            actualValue = _inner[index];
            return true;
        }

        actualValue = default!;
        return false;
    }

    /// <summary>
    /// Creates a new set that contains all the elements of the current set and the specified items.
    /// </summary>
    /// <param name="items">The items to union with.</param>
    /// <returns>A new set that contains all the elements of the current set and the specified items.</returns>
    internal ImmutableValueSet<T> Union(ReadOnlySpan<T> items)
        => Modify(static (builder, items) => builder.UnionWith(items), items);

    /// <summary>
    /// Creates a new set that contains all the elements of the current set and the specified items.
    /// </summary>
    /// <param name="items">The items to union with.</param>
    /// <returns>A new set that contains all the elements of the current set and the specified items.</returns>
    internal ImmutableValueSet<T> Union(IEnumerable<T> items)
        => Modify(static (builder, items) => builder.UnionWith(items), items);

    internal ImmutableValueSet<T> WithComparer(IComparer<T>? comparer)
    {
        comparer ??= Comparer<T>.Default;

        if (comparer == _comparer)
        {
            return this;
        }

        var builder = new Builder([], comparer);
        builder.UnionWith(this);

        return builder.ToImmutable();
    }

    /// <summary>
    /// Checks whether a given sequence of items entirely describe the contents of this set.
    /// </summary>
    /// <param name="other">The sequence of items to check against this set.</param>
    /// <returns>A value indicating whether the sets are equal.</returns>
    public bool SetEquals([NotNullWhen(true)] ImmutableValueSet<T>? other)
        => SetEquals(other, strict: false);

    /// <summary>
    /// Checks whether a given sequence of items entirely describe the contents of this set.
    /// </summary>
    /// <param name="other">The sequence of items to check against this set.</param>
    /// <returns>A value indicating whether the sets are equal.</returns>
    public bool SetEquals([NotNullWhen(true)] IEnumerable<T>? other)
    {
        if (other is null)
        {
            return false;
        }

        if (other is ImmutableValueSet<T> otherSet)
        {
            return SetEquals(otherSet, strict: false);
        }

        var builder = new Builder([], _comparer);
        builder.UnionWith(other);

        return SetEquals(builder.ToImmutable(), strict: false);
    }

    private bool SetEquals([NotNullWhen(true)] ImmutableValueSet<T>? other, bool strict)
    {
        if (ReferenceEquals(this, other))
        {
            return true;
        }

        if (other is null || Count != other.Count)
        {
            return false;
        }

        if (_comparer != other._comparer)
        {
            if (strict)
            {
                // The comparers are different, so we consider the sets not equal even if they contain the same elements.
                return false;
            }

            // The comparer is different, so we can't guarantee that the sets are in the same order
            // even if they contain the same elements.
            return SetEqualsSlow(this, other);
        }

        // The comparer is the same, so we can compare the elements in order.
        return _inner.AsSpan().SequenceEqual(other._inner.AsSpan(), ComparerEqualityComparer.Create(_comparer));

        static bool SetEqualsSlow(ImmutableValueSet<T> self, ImmutableValueSet<T> other)
        {
            // The comparer is different, so we have to check each element of this set against the other set.
            foreach (T item in self)
            {
                if (!other.Contains(item))
                {
                    return false;
                }
            }

            return true;
        }
    }

    /// <summary>
    /// Gets the position within this set that the specified value does or would appear.
    /// </summary>
    /// <param name="item">The value whose position is being sought.</param>
    /// <returns>
    /// The zero-based index of <paramref name="item"/>, if <paramref name="item"/> is found;
    /// otherwise, a negative number that is the bitwise complement of the index of the next
    /// element that is larger than <paramref name="item"/> or, if there is no larger element,
    /// the bitwise complement of <see cref="Count"/>.
    /// </returns>
    public int IndexOf(T item)
        => _inner.AsSpan().BinarySearch(item, _comparer);

    /// <inheritdoc cref="IEnumerable{T}.GetEnumerator()"/>
    public ImmutableArray<T>.Enumerator GetEnumerator()
        => _inner.GetEnumerator();

    #endregion

    #region IImmutableSet<T> Members

    /// <inheritdoc/>
    public bool Contains(T item)
        => IndexOf(item) >= 0;

    /// <inheritdoc/>
    IImmutableSet<T> IImmutableSet<T>.Clear()
        => Clear();

    /// <inheritdoc/>
    IImmutableSet<T> IImmutableSet<T>.Add(T value)
        => Modify(static (builder, value) => builder.Add(value), value);

    /// <inheritdoc/>
    IImmutableSet<T> IImmutableSet<T>.Remove(T value)
        => Modify(static (builder, value) => builder.Remove(value), value);

    /// <inheritdoc/>
    IImmutableSet<T> IImmutableSet<T>.Intersect(IEnumerable<T> other)
        => Modify(static (builder, other) => builder.IntersectWith(other), other);

    /// <inheritdoc/>
    IImmutableSet<T> IImmutableSet<T>.Except(IEnumerable<T> other)
        => Modify(static (builder, other) => builder.ExceptWith(other), other);

    /// <inheritdoc/>
    IImmutableSet<T> IImmutableSet<T>.SymmetricExcept(IEnumerable<T> other)
        => Modify(static (builder, other) => builder.SymmetricExceptWith(other), other);

    /// <inheritdoc/>
    IImmutableSet<T> IImmutableSet<T>.Union(IEnumerable<T> other)
        => Modify(static (builder, other) => builder.UnionWith(other), other);

    bool IImmutableSet<T>.IsProperSubsetOf(IEnumerable<T> other)
        => new SortedSet<T>(_inner, _comparer).IsProperSubsetOf(other);

    /// <inheritdoc/>
    bool IImmutableSet<T>.IsProperSupersetOf(IEnumerable<T> other)
        => new SortedSet<T>(_inner, _comparer).IsProperSupersetOf(other);

    /// <inheritdoc/>
    bool IImmutableSet<T>.IsSubsetOf(IEnumerable<T> other)
        => new SortedSet<T>(_inner, _comparer).IsSubsetOf(other);

    /// <inheritdoc/>
    bool IImmutableSet<T>.IsSupersetOf(IEnumerable<T> other)
        => new SortedSet<T>(_inner, _comparer).IsSupersetOf(other);

    bool IImmutableSet<T>.Overlaps(IEnumerable<T> other)
        => new SortedSet<T>(_inner, _comparer).Overlaps(other);

    bool IImmutableSet<T>.SetEquals(IEnumerable<T> other)
        => new SortedSet<T>(_inner, _comparer).SetEquals(other);

    #endregion

    #region IReadOnlySet<T> Members

    /// <inheritdoc/>
    bool IReadOnlySet<T>.IsProperSubsetOf(IEnumerable<T> other)
        => ((IImmutableSet<T>)this).IsProperSubsetOf(other);

    /// <inheritdoc/>
    bool IReadOnlySet<T>.IsProperSupersetOf(IEnumerable<T> other)
        => ((IImmutableSet<T>)this).IsProperSupersetOf(other);

    /// <inheritdoc/>
    bool IReadOnlySet<T>.IsSubsetOf(IEnumerable<T> other)
        => ((IImmutableSet<T>)this).IsSubsetOf(other);

    /// <inheritdoc/>
    bool IReadOnlySet<T>.IsSupersetOf(IEnumerable<T> other)
        => ((IImmutableSet<T>)this).IsSupersetOf(other);

    bool IReadOnlySet<T>.Overlaps(IEnumerable<T> other)
        => ((IImmutableSet<T>)this).Overlaps(other);

    bool IReadOnlySet<T>.SetEquals(IEnumerable<T> other)
        => ((IImmutableSet<T>)this).SetEquals(other);

    #endregion

    #region IEnumerable Members

    /// <inheritdoc/>
    IEnumerator<T> IEnumerable<T>.GetEnumerator()
        => ((IEnumerable<T>)_inner).GetEnumerator();

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable)_inner).GetEnumerator();

    #endregion

    #region Equality members

    /// <inheritdoc/>
    public bool Equals([NotNullWhen(true)] ImmutableValueSet<T>? other)
        => SetEquals(other!, strict: true);

    /// <inheritdoc/>
    public override bool Equals([NotNullWhen(true)] object? obj)
        => obj is ImmutableValueSet<T> other
        && SetEquals(other, strict: true);

    /// <inheritdoc/>
    public override int GetHashCode()
        => HashCode.Combine(Comparer, _inner.Length);

    /// <summary>
    /// Checks equality between two instances.
    /// </summary>
    /// <param name="left">The instance to the left of the operator.</param>
    /// <param name="right">The instance to the right of the operator.</param>
    /// <returns><c>true</c> if the values' underlying arrays are reference equal; <c>false</c> otherwise.</returns>
    public static bool operator ==(ImmutableValueSet<T>? left, ImmutableValueSet<T>? right)
    {
        if (ReferenceEquals(left, right))
        {
            return true;
        }

        if (left is null || right is null)
        {
            return false;
        }

        return left.SetEquals(right, strict: true);
    }

    /// <summary>
    /// Checks inequality between two instances.
    /// </summary>
    /// <param name="left">The instance to the left of the operator.</param>
    /// <param name="right">The instance to the right of the operator.</param>
    /// <returns><c>true</c> if the values' underlying arrays are reference not equal; <c>false</c> otherwise.</returns>
    public static bool operator !=(ImmutableValueSet<T>? left, ImmutableValueSet<T>? right)
    {
        return !(left == right);
    }

    #endregion

    private ImmutableValueSet<T> Modify<TArg>(Action<Builder, TArg> mutation, TArg arg)
        where TArg : allows ref struct
    {
        var builder = new Builder(this);
        mutation(builder, arg);

        return builder.ToImmutable();
    }
}
