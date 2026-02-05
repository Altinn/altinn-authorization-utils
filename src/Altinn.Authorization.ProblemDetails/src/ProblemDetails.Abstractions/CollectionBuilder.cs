using CommunityToolkit.Diagnostics;
using System.Collections;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Altinn.Authorization.ProblemDetails;

/// <summary>
/// Provides a builder for efficiently constructing a collection of elements of type T.
/// </summary>
/// <remarks>
/// The items order are not guaranteed to be preserved in all operations.
/// Originally copied and modified from the source of <see cref="System.Diagnostics.TagList"/>.
/// </remarks>
/// <typeparam name="T">The type of elements in the list.</typeparam>
internal struct CollectionBuilder<T>
    : ICollection<T>
    , IReadOnlyCollection<T>
    where T : IEquatable<T>
{
    private const int OverflowAdditionalCapacity = 8;

    // Up to eight items are stored in an inline array. Once there are more items than will fit in the inline array,
    // an array is allocated to store all the items and the inline array is abandoned. Even if the size shrinks down
    // to below eight items, the array continues to be used.

    private InlineItems _inlineItems;
    private T[]? _overflowItems;
    private int _itemCount;

    /// <summary>
    /// Initializes a new instance of the <see cref="CollectionBuilder{T}"/> structure using the specified <paramref name="items" />.
    /// </summary>
    /// <param name="items">A span of items to initialize the list with.</param>
    public CollectionBuilder(params ReadOnlySpan<T> items) 
        : this()
    {
        _itemCount = items.Length;

        scoped Span<T> data = _itemCount <= InlineItems.Length 
            ? _inlineItems 
            : _overflowItems = new T[_itemCount + OverflowAdditionalCapacity];

        items.CopyTo(data);
    }

    /// <summary>
    /// Gets the number of items contained in the <see cref="CollectionBuilder{T}" />.
    /// </summary>
    public readonly int Count => _itemCount;

    /// <summary>
    /// Gets a value indicating whether the <see cref="CollectionBuilder{T}" /> is read-only. This property will always return <see langword="false" />.
    /// </summary>
    readonly bool ICollection<T>.IsReadOnly => false;

    /// <summary>
    /// Gets or sets the tags at the specified index.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="index" /> is not a valid index in the <see cref="CollectionBuilder{T}" />.</exception>
    public T this[int index]
    {
        get
        {
            Guard.IsLessThan(index, _itemCount);

            return _overflowItems is null 
                ? _inlineItems[index] 
                : _overflowItems[index];
        }
        set
        {
            Guard.IsLessThan(index, _itemCount);

            if (_overflowItems is null)
            {
                _inlineItems[index] = value;
            }
            else
            {
                _overflowItems[index] = value;
            }
        }
    }

    /// <summary>
    /// Adds an item to the list.
    /// </summary>
    /// <param name="item">Item to add to the list.</param>
    public void Add(T item)
    {
        int count = _itemCount;
        if (_overflowItems is null && (uint)count < InlineItems.Length)
        {
            _inlineItems[count] = item;
            _itemCount++;
        }
        else
        {
            AddToOverflow(ref this, item);
        }

        /// <summary>
        /// Adds an item to the overflow list. Slow path outlined from Add to maximize the chance for the fast path to be inlined.
        /// </summary>
        /// <param name="item">Item to add to the list.</param>
        [MethodImpl(MethodImplOptions.NoInlining)]
        static void AddToOverflow(ref CollectionBuilder<T> self, T item)
        {
            Debug.Assert(self._overflowItems is not null || self._itemCount == InlineItems.Length);

            if (self._overflowItems is null)
            {
                self._overflowItems = new T[InlineItems.Length + OverflowAdditionalCapacity];
                ((ReadOnlySpan<T>)self._inlineItems).CopyTo(self._overflowItems);
            }
            else if (self._itemCount == self._overflowItems.Length)
            {
                Array.Resize(ref self._overflowItems, self._itemCount + OverflowAdditionalCapacity);
            }

            self._overflowItems[self._itemCount] = item;
            self._itemCount++;
        }
    }

    /// <summary>
    /// Copies the contents of this into a destination <paramref name="destination" /> span.
    /// </summary>
    /// <param name="destination">The destination <see cref="Span{T}"/>.</param>
    /// <exception cref="ArgumentException">This does not fit into <paramref name="destination"/>.</exception>
    public readonly void CopyTo(Span<T> destination)
    {
        Guard.HasSizeGreaterThanOrEqualTo(destination, _itemCount);

        Items.CopyTo(destination);
    }

    /// <summary>
    /// Copies the entire <see cref="CollectionBuilder{T}" /> to a compatible one-dimensional array, starting at the specified index of the target array.
    /// </summary>
    /// <param name="array">The one-dimensional Array that is the destination of the elements copied from <see cref="CollectionBuilder{T}" />. The Array must have zero-based indexing.</param>
    /// <param name="arrayIndex">The zero-based index in <paramref name="array" /> at which copying begins.</param>
    /// <exception cref="ArgumentNullException"><paramref name="array" /> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="arrayIndex" /> is less than 0 or greater than the <paramref name="array" /> length.</exception>
    public readonly void CopyTo(T[] array, int arrayIndex)
    {
        Guard.IsNotNull(array);
        Guard.HasSizeGreaterThanOrEqualTo(array, arrayIndex + _itemCount);

        if (_itemCount > 0)
        {
            CopyTo(array.AsSpan(arrayIndex));
        }
    }

    /// <summary>
    /// Removes all elements from the <see cref="CollectionBuilder{T}" />.
    /// </summary>
    public void Clear() 
        => _itemCount = 0;

    /// <summary>
    /// Determines whether an item is in the <see cref="CollectionBuilder{T}" />.
    /// </summary>
    /// <param name="item">The item to locate in the <see cref="CollectionBuilder{T}" />.</param>
    /// <returns><see langword="true" /> if item is found in the <see cref="CollectionBuilder{T}" />; otherwise, <see langword="false" />.</returns>
    public readonly bool Contains(T item)
        => Items.Contains(item);

    /// <summary>
    /// Removes the first occurrence of a specific object from the <see cref="CollectionBuilder{T}" />.
    /// </summary>
    /// <param name="item">The item to remove from the <see cref="CollectionBuilder{T}" />.</param>
    /// <returns><see langword="true" /> if item is successfully removed; otherwise, <see langword="false" />. This method also returns <see langword="false" /> if item was not found in the <see cref="CollectionBuilder{T}" />.</returns>
    /// <remarks>
    /// This method does not preserve the order of items in the list.
    /// </remarks>
    public bool Remove(T item)
    {
        scoped Span<T> items = _overflowItems is not null
            ? _overflowItems
            : _inlineItems;

        items = items[.._itemCount];
        
        var index = items.IndexOf(item);
        if (index < 0)
        {
            return false;
        }

        if (index < _itemCount - 1)
        {
            items[index] = items[_itemCount - 1];
        }

        _itemCount--;
        return true;
    }

    internal void SwapRemoveAt(int index)
    {
        Guard.IsLessThan(index, _itemCount);

        scoped Span<T> items = _overflowItems is not null
            ? _overflowItems
            : _inlineItems;

        if (index < _itemCount - 1)
        {
            items[index] = items[_itemCount - 1];
        }

        _itemCount--;
    }

    /// <summary>
    /// Creates an <see cref="ImmutableArray{T}"/> from this <see cref="CollectionBuilder{T}"/>.
    /// </summary>
    /// <returns>A new <see cref="ImmutableArray{T}"/>.</returns>
    public readonly ImmutableArray<T> ToImmutable()
        => ImmutableArray.Create(Items);

    /// <summary>
    /// Returns an enumerator that iterates through the <see cref="CollectionBuilder{T}" />.
    /// </summary>
    /// <returns>Returns an enumerator that iterates through the <see cref="CollectionBuilder{T}" />.</returns>
    public readonly CollectionBuilderEnumerator<T> GetEnumerator() => new CollectionBuilderEnumerator<T>(in this);

    /// <summary>
    /// Returns an enumerator that iterates through the <see cref="CollectionBuilder{T}" />.
    /// </summary>
    /// <returns>Returns an enumerator that iterates through the <see cref="CollectionBuilder{T}" />.</returns>
    readonly IEnumerator<T> IEnumerable<T>.GetEnumerator() => new CollectionBuilderEnumerator<T>(in this);

    /// <summary>
    /// Returns an enumerator that iterates through the <see cref="CollectionBuilder{T}" />.
    /// </summary>
    /// <returns>Returns an enumerator that iterates through the <see cref="CollectionBuilder{T}" />.</returns>
    readonly IEnumerator IEnumerable.GetEnumerator() => new CollectionBuilderEnumerator<T>(in this);

    /// <summary>
    /// Gets a raw <see cref="Span{T}"/> over the items in this <see cref="CollectionBuilder{T}"/>.
    /// This is unsafe, becuase if items are added to the list after this is called, the span may be dangeling.
    /// This should only be used in scenarios where the caller does add to the collection while the span is in use.
    /// </summary>
    /// <returns></returns>
    [UnscopedRef]
    internal Span<T> AsSpanUnsafe()
        => _overflowItems is not null
            ? _overflowItems.AsSpan(0, _itemCount) 
            : ((Span<T>)_inlineItems).Slice(0, _itemCount);

    [UnscopedRef]
    internal readonly ReadOnlySpan<T> Items
        => _overflowItems is not null
        ? _overflowItems.AsSpan(0, _itemCount) 
        : ((ReadOnlySpan<T>)_inlineItems).Slice(0, _itemCount);

    [InlineArray(8)]
    private struct InlineItems
    {
        public const int Length = 8;
        private T _first;
    }
}
