using System.Collections;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Altinn.Authorization.ModelUtils;

/// <summary>
/// A set of initialization methods for instances of <see cref="ImmutableValueArray{T}"/>.
/// </summary>
public static partial class ImmutableValueArray
{
    /// <summary>
    /// Creates an empty <see cref="ImmutableValueArray{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type of element stored in the array.</typeparam>
    /// <returns>An empty array.</returns>
    public static ImmutableValueArray<T> Create<T>() 
        => ImmutableValueArray<T>.Create(ImmutableArray<T>.Empty);

    /// <summary>
    /// Creates an <see cref="ImmutableValueArray{T}"/> with the specified element as its only member.
    /// </summary>
    /// <typeparam name="T">The type of element stored in the array.</typeparam>
    /// <param name="item">The element to store in the array.</param>
    /// <returns>A 1-element immutable array containing the specified item.</returns>
    public static ImmutableValueArray<T> Create<T>(T item)
        => ImmutableValueArray<T>.Create(ImmutableArray.Create(item));

    /// <summary>
    /// Creates an <see cref="ImmutableValueArray{T}"/> with the specified elements.
    /// </summary>
    /// <typeparam name="T">The type of element stored in the array.</typeparam>
    /// <param name="item1">The first element to store in the array.</param>
    /// <param name="item2">The second element to store in the array.</param>
    /// <returns>A 2-element immutable array containing the specified items.</returns>
    public static ImmutableValueArray<T> Create<T>(T item1, T item2)
        => ImmutableValueArray<T>.Create(ImmutableArray.Create(item1, item2));

    /// <summary>
    /// Creates an <see cref="ImmutableValueArray{T}"/> with the specified elements.
    /// </summary>
    /// <typeparam name="T">The type of element stored in the array.</typeparam>
    /// <param name="item1">The first element to store in the array.</param>
    /// <param name="item2">The second element to store in the array.</param>
    /// <param name="item3">The third element to store in the array.</param>
    /// <returns>A 3-element immutable array containing the specified items.</returns>
    public static ImmutableValueArray<T> Create<T>(T item1, T item2, T item3)
        => ImmutableValueArray<T>.Create(ImmutableArray.Create(item1, item2, item3));

    /// <summary>
    /// Creates an <see cref="ImmutableValueArray{T}"/> with the specified elements.
    /// </summary>
    /// <typeparam name="T">The type of element stored in the array.</typeparam>
    /// <param name="item1">The first element to store in the array.</param>
    /// <param name="item2">The second element to store in the array.</param>
    /// <param name="item3">The third element to store in the array.</param>
    /// <param name="item4">The fourth element to store in the array.</param>
    /// <returns>A 4-element immutable array containing the specified items.</returns>
    public static ImmutableValueArray<T> Create<T>(T item1, T item2, T item3, T item4)
        => ImmutableValueArray<T>.Create(ImmutableArray.Create(item1, item2, item3, item4));

    /// <summary>
    /// Creates an <see cref="ImmutableValueArray{T}"/> with the specified elements.
    /// </summary>
    /// <typeparam name="T">The type of element stored in the array.</typeparam>
    /// <param name="items">The elements to store in the array.</param>
    /// <returns>An immutable array containing the specified items.</returns>
    public static ImmutableValueArray<T> Create<T>(params ReadOnlySpan<T> items)
        => ImmutableValueArray<T>.Create(ImmutableArray.Create(items));

    /// <summary>
    /// Creates an <see cref="ImmutableValueArray{T}"/> with the specified elements.
    /// </summary>
    /// <typeparam name="T">The type of element stored in the array.</typeparam>
    /// <param name="items">The elements to store in the array.</param>
    /// <returns>An immutable array containing the specified items.</returns>
    [OverloadResolutionPriority(-1)]
    public static ImmutableValueArray<T> Create<T>(Span<T> items)
        => ImmutableValueArray<T>.Create(ImmutableArray.Create(items));

    /// <summary>
    /// Creates an <see cref="ImmutableValueArray{T}"/> with the specified elements.
    /// </summary>
    /// <typeparam name="T">The type of element stored in the array.</typeparam>
    /// <param name="items">The elements to store in the array.</param>
    /// <returns>An immutable array containing the specified items.</returns>
    public static ImmutableValueArray<T> Create<T>(params T[]? items)
        => ImmutableValueArray<T>.Create(ImmutableArray.Create(items));

    /// <summary>
    /// Creates an <see cref="ImmutableValueArray{T}"/> with the specified elements.
    /// </summary>
    /// <typeparam name="T">The type of element stored in the array.</typeparam>
    /// <param name="items">The elements to store in the array.</param>
    /// <returns>An immutable array containing the specified items.</returns>
    public static ImmutableValueArray<T> Create<T>(ImmutableArray<T> items)
        => ImmutableValueArray<T>.Create(items);

    /// <summary>
    /// Creates an <see cref="ImmutableValueArray{T}"/> with the specified elements.
    /// </summary>
    /// <typeparam name="T">The type of element stored in the array.</typeparam>
    /// <param name="items">The elements to store in the array.</param>
    /// <returns>An immutable array containing the specified items.</returns>
    public static ImmutableValueArray<T> Create<T>(ImmutableValueArray<T> items)
        => items;

    /// <summary>
    /// Initializes a new instance of the <see cref="ImmutableValueArray{T}"/> struct.
    /// </summary>
    /// <param name="items">The array to initialize the array with. A defensive copy is made.</param>
    /// <param name="start">The index of the first element in the source array to include in the resulting array.</param>
    /// <param name="length">The number of elements from the source array to include in the resulting array.</param>
    /// <remarks>
    /// This overload allows helper methods or custom builder classes to efficiently avoid paying a redundant
    /// tax for copying an array when the new array is a segment of an existing array.
    /// </remarks>
    public static ImmutableValueArray<T> Create<T>(T[] items, int start, int length)
        => ImmutableValueArray<T>.Create(ImmutableArray.Create(items, start, length));

    /// <summary>
    /// Initializes a new instance of the <see cref="ImmutableValueArray{T}"/> struct.
    /// </summary>
    /// <param name="items">The array to initialize the array with.
    /// The selected array segment may be copied into a new array.</param>
    /// <param name="start">The index of the first element in the source array to include in the resulting array.</param>
    /// <param name="length">The number of elements from the source array to include in the resulting array.</param>
    /// <remarks>
    /// This overload allows helper methods or custom builder classes to efficiently avoid paying a redundant
    /// tax for copying an array when the new array is a segment of an existing array.
    /// </remarks>
    public static ImmutableValueArray<T> Create<T>(ImmutableArray<T> items, int start, int length)
        => ImmutableValueArray<T>.Create(ImmutableArray.Create(items, start, length));

    /// <summary>
    /// Initializes a new instance of the <see cref="ImmutableValueArray{T}"/> struct.
    /// </summary>
    /// <param name="items">The array to initialize the array with.
    /// The selected array segment may be copied into a new array.</param>
    /// <param name="start">The index of the first element in the source array to include in the resulting array.</param>
    /// <param name="length">The number of elements from the source array to include in the resulting array.</param>
    /// <remarks>
    /// This overload allows helper methods or custom builder classes to efficiently avoid paying a redundant
    /// tax for copying an array when the new array is a segment of an existing array.
    /// </remarks>
    public static ImmutableValueArray<T> Create<T>(ImmutableValueArray<T> items, int start, int length)
        => items.Slice(start, length);

    /// <summary>
    /// Produce an <see cref="ImmutableValueArray{T}"/> of contents from specified elements.
    /// </summary>
    /// <typeparam name="T">The type of element in the list.</typeparam>
    /// <param name="items">The elements to store in the array.</param>
    /// <returns>An immutable array containing the specified items.</returns>
    public static ImmutableValueArray<T> ToImmutableValueArray<T>(this ReadOnlySpan<T> items)
        => Create(items);

    /// <summary>
    /// Produce an <see cref="ImmutableValueArray{T}"/> of contents from specified elements.
    /// </summary>
    /// <typeparam name="T">The type of element in the list.</typeparam>
    /// <param name="items">The elements to store in the array.</param>
    /// <returns>An immutable array containing the specified items.</returns>
    [OverloadResolutionPriority(-1)]
    public static ImmutableValueArray<T> ToImmutableValueArray<T>(this Span<T> items)
        => Create(items);

    /// <summary>
    /// Produce an <see cref="ImmutableValueArray{T}"/> of contents from specified elements.
    /// </summary>
    /// <typeparam name="T">The type of element in the list.</typeparam>
    /// <param name="items">The elements to store in the array.</param>
    /// <returns>An immutable array containing the specified items.</returns>
    [OverloadResolutionPriority(-2)]
    public static ImmutableValueArray<T> ToImmutableValueArray<T>(this ImmutableArray<T> items)
        => Create(items);

    /// <summary>
    /// Enumerates a sequence exactly once and produces an immutable array of its contents.
    /// </summary>
    /// <typeparam name="T">The type of element in the sequence.</typeparam>
    /// <param name="items">The sequence to enumerate.</param>
    /// <returns>An immutable array containing the specified items.</returns>
    [OverloadResolutionPriority(-3)]
    public static ImmutableValueArray<T> ToImmutableValueArray<T>(this IEnumerable<T> items)
    {
        if (items is ImmutableValueArray<T> immutableValueArray)
        {
            return immutableValueArray;
        }

        return Create(items.ToImmutableArray());
    }

    /// <summary>
    /// Returns an immutable copy of the current contents of the builder's collection.
    /// </summary>
    /// <param name="builder">The builder to create the immutable array from.</param>
    /// <returns>An immutable array containing the specified items from <paramref name="builder"/>.</returns>
    public static ImmutableValueArray<T> ToImmutableValueArray<T>(this ImmutableArray<T>.Builder builder)
        => Create(builder.ToImmutableArray());

    /// <summary>
    /// Extracts the internal array as an <see cref="ImmutableValueArray{T}"/> and replaces it
    /// with a zero length array.
    /// </summary>
    /// <exception cref="InvalidOperationException">When <see cref="ImmutableArray{T}.Builder.Count"/> doesn't
    /// equal <see cref="ImmutableArray{T}.Builder.Capacity"/>.</exception>
    public static ImmutableValueArray<T> MoveToImmutableValueArray<T>(this ImmutableArray<T>.Builder builder)
        => Create(builder.MoveToImmutable());

    /// <summary>
    /// Returns the current contents as an <see cref="ImmutableValueArray{T}"/> and sets the collection to a zero length array.
    /// </summary>
    /// <remarks>
    /// If <see cref="ImmutableArray{T}.Builder.Capacity"/> equals <see cref="ImmutableArray{T}.Builder.Count"/>, 
    /// the internal array will be extracted as an <see cref="ImmutableValueArray{T}"/> without copying the contents.
    /// Otherwise, the contents will be copied into a new array. The collection will then be set to a zero length array.
    /// </remarks>
    /// <returns>An immutable array.</returns>
    public static ImmutableValueArray<T> DrainToImmutableValueArray<T>(this ImmutableArray<T>.Builder builder)
        => Create(builder.DrainToImmutable());
}

/// <summary>
/// A readonly array with O(1) indexable lookup time and value semantics.
/// </summary>
/// <typeparam name="T">The type of element stored by the array.</typeparam>
/// <devremarks>
/// IMPORTANT NOTICE FOR MAINTAINERS AND REVIEWERS:
/// This type should be thread-safe. As a struct, it cannot protect its own fields
/// from being changed from one thread while its members are executing on other threads
/// because structs can change *in place* simply by reassigning the field containing
/// this struct. Therefore it is extremely important that
/// ** Every member should only dereference <c>this</c> ONCE. **
/// If a member needs to reference the array field, that counts as a dereference of <c>this</c>.
/// Calling other instance members (properties or methods) also counts as dereferencing <c>this</c>.
/// Any member that needs to use <c>this</c> more than once must instead
/// assign <c>this</c> to a local variable and use that for the rest of the code instead.
/// This effectively copies the one field in the struct to a local variable so that
/// it is insulated from other threads.
/// </devremarks>
[DebuggerDisplay("{_inner,nq}")]
[CollectionBuilder(typeof(ImmutableValueArray), nameof(ImmutableValueArray.Create))]
public readonly partial struct ImmutableValueArray<T>
    : IReadOnlyList<T>
    , IList<T>
    , IEquatable<ImmutableValueArray<T>>
    , IList
    , IImmutableList<T>
    , IEqualityOperators<ImmutableValueArray<T>, ImmutableValueArray<T>, bool>
{
    /// <summary>
    /// An empty (initialized) instance of <see cref="ImmutableValueArray{T}"/>.
    /// </summary>
    public static readonly ImmutableValueArray<T> Empty = new(ImmutableArray<T>.Empty);

    [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
    private readonly ImmutableArray<T> _inner;

    private ImmutableValueArray(ImmutableArray<T> inner)
    {
        _inner = inner;
    }

    internal static ImmutableValueArray<T> Create(ImmutableArray<T> inner) => new(inner);

    /// <summary>
    /// Gets a <see cref="ImmutableArray{T}"/> from this instance.
    /// </summary>
    /// <returns>A <see cref="ImmutableArray{T}"/>.</returns>
    public ImmutableArray<T> ToImmutableArray()
        => _inner;

    /// <summary>
    /// Gets the element at the specified index in the read-only list.
    /// </summary>
    /// <param name="index">The zero-based index of the element to get.</param>
    /// <returns>The element at the specified index in the read-only list.</returns>
    public T this[int index]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _inner[index];
    }

    /// <summary>
    /// Gets a read-only reference to the element at the specified index in the read-only list.
    /// </summary>
    /// <param name="index">The zero-based index of the element to get a reference to.</param>
    /// <returns>A read-only reference to the element at the specified index in the read-only list.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ref readonly T ItemRef(int index) => ref _inner.ItemRef(index);

    /// <summary>
    /// Gets a value indicating whether this collection is empty.
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public bool IsEmpty => _inner.IsEmpty;

    /// <summary>
    /// Gets the number of elements in the array.
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public int Length => _inner.Length;

    /// <summary>
    /// Gets a value indicating whether this struct was initialized without an actual array instance.
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public bool IsDefault => _inner.IsDefault;

    /// <summary>
    /// Gets a value indicating whether this struct is empty or uninitialized.
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public bool IsDefaultOrEmpty => _inner.IsDefaultOrEmpty;

    /// <summary>
    /// Copies the contents of this array to the specified array.
    /// </summary>
    /// <param name="destination">The array to copy to.</param>
    public void CopyTo(T[] destination) => _inner.CopyTo(destination);

    /// <summary>
    /// Copies the contents of this array to the specified array.
    /// </summary>
    /// <param name="destination">The array to copy to.</param>
    /// <param name="destinationIndex">The index into the destination array to which the first copied element is written.</param>
    public void CopyTo(T[] destination, int destinationIndex) => _inner.CopyTo(destination, destinationIndex);

    /// <summary>
    /// Copies the contents of this array to the specified array.
    /// </summary>
    /// <param name="sourceIndex">The index into this collection of the first element to copy.</param>
    /// <param name="destination">The array to copy to.</param>
    /// <param name="destinationIndex">The index into the destination array to which the first copied element is written.</param>
    /// <param name="length">The number of elements to copy.</param>
    public void CopyTo(int sourceIndex, T[] destination, int destinationIndex, int length) => _inner.CopyTo(sourceIndex, destination, destinationIndex, length);

    /// <summary>
    /// Returns an enumerator for the contents of the array.
    /// </summary>
    /// <returns>An enumerator.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ImmutableArray<T>.Enumerator GetEnumerator() => _inner.GetEnumerator();

    /// <summary>
    /// Returns a hash code for this instance.
    /// </summary>
    /// <returns>
    /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
    /// </returns>
    public override int GetHashCode()
        => GetHashCode(comparer: null);

    /// <summary>
    /// Returns a hash code for this instance.
    /// </summary>
    /// <param name="comparer">The equality comparer to use to compare items.</param>
    /// <returns>
    /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
    /// </returns>
    public int GetHashCode(IEqualityComparer<T>? comparer)
    {
        ImmutableArray<T> inner = _inner;
        if (inner.IsDefault)
        {
            return 0;
        }

        HashCode hash = default;
        hash.Add(inner.Length);

        comparer ??= EqualityComparer<T>.Default;
        foreach (var item in inner)
        {
            hash.Add(item, comparer);
        }

        return hash.ToHashCode();
    }

    /// <summary>
    /// Determines whether the specified <see cref="object"/> is equal to this instance.
    /// </summary>
    /// <param name="obj">The <see cref="object"/> to compare with this instance.</param>
    /// <returns>
    ///   <c>true</c> if the specified <see cref="object"/> is equal to this instance; otherwise, <c>false</c>.
    /// </returns>
    public override bool Equals([NotNullWhen(true)] object? obj)
        => obj is ImmutableValueArray<T> other && Equals(other);

    /// <summary>
    /// Indicates whether the current object is equal to another object of the same type.
    /// </summary>
    /// <param name="other">An object to compare with this object.</param>
    /// <returns>
    /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
    /// </returns>
    public bool Equals(ImmutableValueArray<T> other)
        => Equals(other, comparer: null);

    /// <summary>
    /// Indicates whether the current object is equal to another object of the same type.
    /// </summary>
    /// <param name="other">An object to compare with this object.</param>
    /// <param name="comparer">The <see cref="IEqualityComparer{T}"/> implementation to use when comparing elements, or <see langword="null"/> to use the default <see cref="IEqualityComparer{T}"/> for the type of an element.</param>
    /// <returns>
    /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
    /// </returns>
    public bool Equals(ImmutableValueArray<T> other, IEqualityComparer<T>? comparer)
    {
        ImmutableArray<T> left = _inner;
        ImmutableArray<T> right = other._inner;

        // reference equality check
        if (left.Equals(right))
        {
            return true;
        }

        // value equality check
        return left.AsSpan().SequenceEqual(right.AsSpan(), comparer);
    }

    /// <summary>
    /// Returns an enumerator for the contents of the array.
    /// </summary>
    /// <returns>An enumerator.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the <see cref="IsDefault"/> property returns true.</exception>
    IEnumerator<T> IEnumerable<T>.GetEnumerator() => ((IEnumerable<T>)_inner).GetEnumerator();

    /// <summary>
    /// Returns an enumerator for the contents of the array.
    /// </summary>
    /// <returns>An enumerator.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the <see cref="IsDefault"/> property returns true.</exception>
    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_inner).GetEnumerator();

    /// <summary>
    /// Gets or sets the element at the specified index in the read-only list.
    /// </summary>
    /// <param name="index">The zero-based index of the element to get.</param>
    /// <returns>The element at the specified index in the read-only list.</returns>
    /// <exception cref="NotSupportedException">Always thrown from the setter.</exception>
    /// <exception cref="InvalidOperationException">Thrown if the <see cref="IsDefault"/> property returns true.</exception>
    T IList<T>.this[int index]
    {
        get => ((IList<T>)_inner)[index];
        set => ((IList<T>)_inner)[index] = value;
    }

    /// <summary>
    /// Gets a value indicating whether this instance is read only.
    /// </summary>
    /// <value>
    /// <c>true</c> if this instance is read only; otherwise, <c>false</c>.
    /// </value>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    bool ICollection<T>.IsReadOnly => true;

    /// <summary>
    /// Gets the number of array in the collection.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the <see cref="IsDefault"/> property returns true.</exception>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    int ICollection<T>.Count => ((ICollection<T>)_inner).Count;

    /// <summary>
    /// Gets the number of array in the collection.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the <see cref="IsDefault"/> property returns true.</exception>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    int IReadOnlyCollection<T>.Count => ((IReadOnlyCollection<T>)_inner).Count;

    /// <summary>
    /// Gets the element at the specified index.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <returns>
    /// The element.
    /// </returns>
    /// <exception cref="InvalidOperationException">Thrown if the <see cref="IsDefault"/> property returns true.</exception>
    T IReadOnlyList<T>.this[int index] => ((IReadOnlyList<T>)_inner)[index];

    /// <summary>
    /// Creates a new read-only span over this immutable array.
    /// </summary>
    /// <returns>The read-only span representation of this immutable array.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ReadOnlySpan<T> AsSpan() => _inner.AsSpan();

    /// <summary>
    /// Creates a new read-only memory region over this immutable array.
    /// </summary>
    /// <returns>The read-only memory representation of this immutable array.</returns>
    public ReadOnlyMemory<T> AsMemory() => _inner.AsMemory();

    /// <summary>
    /// Searches the array for the specified item.
    /// </summary>
    /// <param name="item">The item to search for.</param>
    /// <returns>The 0-based index into the array where the item was found; or -1 if it could not be found.</returns>
    public int IndexOf(T item) => _inner.IndexOf(item);

    /// <summary>
    /// Searches the array for the specified item.
    /// </summary>
    /// <param name="item">The item to search for.</param>
    /// <param name="startIndex">The index at which to begin the search.</param>
    /// <param name="equalityComparer">The equality comparer to use in the search.</param>
    /// <returns>The 0-based index into the array where the item was found; or -1 if it could not be found.</returns>
    public int IndexOf(T item, int startIndex, IEqualityComparer<T>? equalityComparer) => _inner.IndexOf(item, startIndex, equalityComparer);

    /// <summary>
    /// Searches the array for the specified item.
    /// </summary>
    /// <param name="item">The item to search for.</param>
    /// <param name="startIndex">The index at which to begin the search.</param>
    /// <returns>The 0-based index into the array where the item was found; or -1 if it could not be found.</returns>
    public int IndexOf(T item, int startIndex) => _inner.IndexOf(item, startIndex);

    /// <summary>
    /// Searches the array for the specified item.
    /// </summary>
    /// <param name="item">The item to search for.</param>
    /// <param name="startIndex">The index at which to begin the search.</param>
    /// <param name="count">The number of elements to search.</param>
    /// <returns>The 0-based index into the array where the item was found; or -1 if it could not be found.</returns>
    public int IndexOf(T item, int startIndex, int count) => _inner.IndexOf(item, startIndex, count);

    /// <summary>
    /// Searches the array for the specified item.
    /// </summary>
    /// <param name="item">The item to search for.</param>
    /// <param name="startIndex">The index at which to begin the search.</param>
    /// <param name="count">The number of elements to search.</param>
    /// <param name="equalityComparer">
    /// The equality comparer to use in the search.
    /// If <c>null</c>, <see cref="EqualityComparer{T}.Default"/> is used.
    /// </param>
    /// <returns>The 0-based index into the array where the item was found; or -1 if it could not be found.</returns>
    public int IndexOf(T item, int startIndex, int count, IEqualityComparer<T>? equalityComparer) => _inner.IndexOf(item, startIndex, count, equalityComparer);

    /// <summary>
    /// Searches the array for the specified item in reverse.
    /// </summary>
    /// <param name="item">The item to search for.</param>
    /// <returns>The 0-based index into the array where the item was found; or -1 if it could not be found.</returns>
    public int LastIndexOf(T item) => _inner.LastIndexOf(item);

    /// <summary>
    /// Searches the array for the specified item in reverse.
    /// </summary>
    /// <param name="item">The item to search for.</param>
    /// <param name="startIndex">The index at which to begin the search.</param>
    /// <returns>The 0-based index into the array where the item was found; or -1 if it could not be found.</returns>
    public int LastIndexOf(T item, int startIndex) => _inner.LastIndexOf(item, startIndex);

    /// <summary>
    /// Searches the array for the specified item in reverse.
    /// </summary>
    /// <param name="item">The item to search for.</param>
    /// <param name="startIndex">The index at which to begin the search.</param>
    /// <param name="count">The number of elements to search.</param>
    /// <returns>The 0-based index into the array where the item was found; or -1 if it could not be found.</returns>
    public int LastIndexOf(T item, int startIndex, int count) => _inner.LastIndexOf(item, startIndex, count);

    /// <summary>
    /// Searches the array for the specified item in reverse.
    /// </summary>
    /// <param name="item">The item to search for.</param>
    /// <param name="startIndex">The index at which to begin the search.</param>
    /// <param name="count">The number of elements to search.</param>
    /// <param name="equalityComparer">The equality comparer to use in the search.</param>
    /// <returns>The 0-based index into the array where the item was found; or -1 if it could not be found.</returns>
    public int LastIndexOf(T item, int startIndex, int count, IEqualityComparer<T>? equalityComparer) => _inner.LastIndexOf(item, startIndex, count, equalityComparer);

    /// <summary>
    /// Determines whether the specified item exists in the array.
    /// </summary>
    /// <param name="item">The item to search for.</param>
    /// <returns><c>true</c> if an equal value was found in the array; <c>false</c> otherwise.</returns>
    public bool Contains(T item) => _inner.Contains(item);

    /// <summary>
    /// Determines whether the specified item exists in the array.
    /// </summary>
    /// <param name="item">The item to search for.</param>
    /// <param name="equalityComparer">
    /// The equality comparer to use in the search.
    /// If <c>null</c>, <see cref="EqualityComparer{T}.Default"/> is used.
    /// </param>
    /// <returns><c>true</c> if an equal value was found in the array; <c>false</c> otherwise.</returns>
    public bool Contains(T item, IEqualityComparer<T>? equalityComparer) => _inner.Contains(item, equalityComparer);

    /// <summary>
    /// Returns a new array with the specified value inserted at the specified position.
    /// </summary>
    /// <param name="index">The 0-based index into the array at which the new item should be added.</param>
    /// <param name="item">The item to insert at the start of the array.</param>
    /// <returns>A new array.</returns>
    public ImmutableValueArray<T> Insert(int index, T item) => new(_inner.Insert(index, item));

    /// <summary>
    /// Inserts the specified values at the specified index.
    /// </summary>
    /// <param name="index">The index at which to insert the value.</param>
    /// <param name="items">The elements to insert.</param>
    /// <returns>The new immutable collection.</returns>
    public ImmutableValueArray<T> InsertRange(int index, IEnumerable<T> items) => new(_inner.InsertRange(index, items));

    /// <summary>
    /// Inserts the specified values at the specified index.
    /// </summary>
    /// <param name="index">The index at which to insert the value.</param>
    /// <param name="items">The elements to insert.</param>
    /// <returns>The new immutable collection.</returns>
    public ImmutableValueArray<T> InsertRange(int index, ImmutableArray<T> items) => new(_inner.InsertRange(index, items));

    /// <summary>
    /// Inserts the specified values at the specified index.
    /// </summary>
    /// <param name="index">The index at which to insert the value.</param>
    /// <param name="items">The elements to insert.</param>
    /// <returns>The new immutable collection.</returns>
    public ImmutableValueArray<T> InsertRange(int index, ImmutableValueArray<T> items) => new(_inner.InsertRange(index, items._inner));

    /// <summary>
    /// Returns a new array with the specified value inserted at the end.
    /// </summary>
    /// <param name="item">The item to insert at the end of the array.</param>
    /// <returns>A new array.</returns>
    public ImmutableValueArray<T> Add(T item) => new(_inner.Add(item));

    /// <summary>
    /// Adds the specified values to this list.
    /// </summary>
    /// <param name="items">The values to add.</param>
    /// <returns>A new list with the elements added.</returns>
    public ImmutableValueArray<T> AddRange(IEnumerable<T> items) => new(_inner.AddRange(items));

    /// <summary>
    /// Adds the specified items to the end of the array.
    /// </summary>
    /// <param name="items">The values to add.</param>
    /// <param name="length">The number of elements from the source array to add.</param>
    /// <returns>A new list with the elements added.</returns>
    public ImmutableValueArray<T> AddRange(T[] items, int length) => new(_inner.AddRange(items, length));

    /// <summary>
    /// Adds the specified items to the end of the array.
    /// </summary>
    /// <typeparam name="TDerived">The type that derives from the type of item already in the array.</typeparam>
    /// <param name="items">The values to add.</param>
    /// <returns>A new list with the elements added.</returns>
    public ImmutableValueArray<T> AddRange<TDerived>(TDerived[] items) where TDerived : T => new(_inner.AddRange(items));

    /// <summary>
    /// Adds the specified items to the end of the array.
    /// </summary>
    /// <param name="items">The values to add.</param>
    /// <param name="length">The number of elements from the source array to add.</param>
    /// <returns>A new list with the elements added.</returns>
    public ImmutableValueArray<T> AddRange(ImmutableArray<T> items, int length) => new(_inner.AddRange(items, length));

    /// <summary>
    /// Adds the specified items to the end of the array.
    /// </summary>
    /// <param name="items">The values to add.</param>
    /// <param name="length">The number of elements from the source array to add.</param>
    /// <returns>A new list with the elements added.</returns>
    public ImmutableValueArray<T> AddRange(ImmutableValueArray<T> items, int length) => new(_inner.AddRange(items._inner, length));

    /// <summary>
    /// Adds the specified items to the end of the array.
    /// </summary>
    /// <typeparam name="TDerived">The type that derives from the type of item already in the array.</typeparam>
    /// <param name="items">The values to add.</param>
    /// <returns>A new list with the elements added.</returns>
    public ImmutableValueArray<T> AddRange<TDerived>(ImmutableArray<TDerived> items) where TDerived : T => new(_inner.AddRange(items));

    /// <summary>
    /// Adds the specified items to the end of the array.
    /// </summary>
    /// <typeparam name="TDerived">The type that derives from the type of item already in the array.</typeparam>
    /// <param name="items">The values to add.</param>
    /// <returns>A new list with the elements added.</returns>
    public ImmutableValueArray<T> AddRange<TDerived>(ImmutableValueArray<TDerived> items) where TDerived : T => new(_inner.AddRange(items._inner));

    /// <summary>
    /// Adds the specified values to this list.
    /// </summary>
    /// <param name="items">The values to add.</param>
    /// <returns>A new list with the elements added.</returns>
    public ImmutableValueArray<T> AddRange(ImmutableArray<T> items) => new(_inner.AddRange(items));

    /// <summary>
    /// Adds the specified values to this list.
    /// </summary>
    /// <param name="items">The values to add.</param>
    /// <returns>A new list with the elements added.</returns>
    public ImmutableValueArray<T> AddRange(ImmutableValueArray<T> items) => new(_inner.AddRange(items._inner));

    /// <summary>
    /// Returns an array with the item at the specified position replaced.
    /// </summary>
    /// <param name="index">The index of the item to replace.</param>
    /// <param name="item">The new item.</param>
    /// <returns>The new array.</returns>
    public ImmutableValueArray<T> SetItem(int index, T item) => new(_inner.SetItem(index, item));

    /// <summary>
    /// Replaces the first equal element in the list with the specified element.
    /// </summary>
    /// <param name="oldValue">The element to replace.</param>
    /// <param name="newValue">The element to replace the old element with.</param>
    /// <returns>The new list -- even if the value being replaced is equal to the new value for that position.</returns>
    /// <exception cref="ArgumentException">Thrown when the old value does not exist in the list.</exception>
    public ImmutableValueArray<T> Replace(T oldValue, T newValue) => new(_inner.Replace(oldValue, newValue));

    /// <summary>
    /// Replaces the first equal element in the list with the specified element.
    /// </summary>
    /// <param name="oldValue">The element to replace.</param>
    /// <param name="newValue">The element to replace the old element with.</param>
    /// <param name="equalityComparer">
    /// The equality comparer to use in the search.
    /// If <c>null</c>, <see cref="EqualityComparer{T}.Default"/> is used.
    /// </param>
    /// <returns>The new list -- even if the value being replaced is equal to the new value for that position.</returns>
    /// <exception cref="ArgumentException">Thrown when the old value does not exist in the list.</exception>
    public ImmutableValueArray<T> Replace(T oldValue, T newValue, IEqualityComparer<T>? equalityComparer) => new(_inner.Replace(oldValue, newValue, equalityComparer));

    /// <summary>
    /// Returns an array with the first occurrence of the specified element removed from the array.
    /// If no match is found, the current array is returned.
    /// </summary>
    /// <param name="item">The item to remove.</param>
    /// <returns>The new array.</returns>
    public ImmutableValueArray<T> Remove(T item) => new(_inner.Remove(item));

    /// <summary>
    /// Returns an array with the first occurrence of the specified element removed from the array.
    /// If no match is found, the current array is returned.
    /// </summary>
    /// <param name="item">The item to remove.</param>
    /// <param name="equalityComparer">
    /// The equality comparer to use in the search.
    /// If <c>null</c>, <see cref="EqualityComparer{T}.Default"/> is used.
    /// </param>
    /// <returns>The new array.</returns>
    public ImmutableValueArray<T> Remove(T item, IEqualityComparer<T>? equalityComparer) => new(_inner.Remove(item, equalityComparer));

    /// <summary>
    /// Returns an array with the element at the specified position removed.
    /// </summary>
    /// <param name="index">The 0-based index into the array for the element to omit from the returned array.</param>
    /// <returns>The new array.</returns>
    public ImmutableValueArray<T> RemoveAt(int index) => new(_inner.RemoveAt(index));

    /// <summary>
    /// Returns an array with the elements at the specified position removed.
    /// </summary>
    /// <param name="index">The 0-based index into the array for the element to omit from the returned array.</param>
    /// <param name="length">The number of elements to remove.</param>
    /// <returns>The new array.</returns>
    public ImmutableValueArray<T> RemoveRange(int index, int length) => new(_inner.RemoveRange(index, length));

    /// <summary>
    /// Removes the specified values from this list.
    /// </summary>
    /// <param name="items">The items to remove if matches are found in this list.</param>
    /// <returns>
    /// A new list with the elements removed.
    /// </returns>
    public ImmutableValueArray<T> RemoveRange(IEnumerable<T> items) => new(_inner.RemoveRange(items));

    /// <summary>
    /// Removes the specified values from this list.
    /// </summary>
    /// <param name="items">The items to remove if matches are found in this list.</param>
    /// <param name="equalityComparer">
    /// The equality comparer to use in the search.
    /// If <c>null</c>, <see cref="EqualityComparer{T}.Default"/> is used.
    /// </param>
    /// <returns>
    /// A new list with the elements removed.
    /// </returns>
    public ImmutableValueArray<T> RemoveRange(IEnumerable<T> items, IEqualityComparer<T>? equalityComparer) => new(_inner.RemoveRange(items, equalityComparer));

    /// <summary>
    /// Removes the specified values from this list.
    /// </summary>
    /// <param name="items">The items to remove if matches are found in this list.</param>
    /// <returns>
    /// A new list with the elements removed.
    /// </returns>
    public ImmutableValueArray<T> RemoveRange(ImmutableArray<T> items) => new(_inner.RemoveRange(items));

    /// <summary>
    /// Removes the specified values from this list.
    /// </summary>
    /// <param name="items">The items to remove if matches are found in this list.</param>
    /// <returns>
    /// A new list with the elements removed.
    /// </returns>
    public ImmutableValueArray<T> RemoveRange(ImmutableValueArray<T> items) => new(_inner.RemoveRange(items._inner));

    /// <summary>
    /// Removes the specified values from this list.
    /// </summary>
    /// <param name="items">The items to remove if matches are found in this list.</param>
    /// <param name="equalityComparer">
    /// The equality comparer to use in the search.
    /// </param>
    /// <returns>
    /// A new list with the elements removed.
    /// </returns>
    public ImmutableValueArray<T> RemoveRange(ImmutableArray<T> items, IEqualityComparer<T>? equalityComparer) => new(_inner.RemoveRange(items, equalityComparer));

    /// <summary>
    /// Removes the specified values from this list.
    /// </summary>
    /// <param name="items">The items to remove if matches are found in this list.</param>
    /// <param name="equalityComparer">
    /// The equality comparer to use in the search.
    /// </param>
    /// <returns>
    /// A new list with the elements removed.
    /// </returns>
    public ImmutableValueArray<T> RemoveRange(ImmutableValueArray<T> items, IEqualityComparer<T>? equalityComparer) => new(_inner.RemoveRange(items._inner, equalityComparer));

    /// <summary>
    /// Removes all the elements that match the conditions defined by the specified
    /// predicate.
    /// </summary>
    /// <param name="match">
    /// The <see cref="Predicate{T}"/> delegate that defines the conditions of the elements
    /// to remove.
    /// </param>
    /// <returns>
    /// The new list.
    /// </returns>
    public ImmutableValueArray<T> RemoveAll(Predicate<T> match) => new(_inner.RemoveAll(match));

    /// <summary>
    /// Returns an empty array.
    /// </summary>
    public ImmutableValueArray<T> Clear() => Empty;

    /// <summary>
    /// Returns a sorted instance of this array.
    /// </summary>
    public ImmutableValueArray<T> Sort() => new(_inner.Sort());

    /// <summary>
    /// Sorts the elements in the entire <see cref="ImmutableArray{T}"/> using
    /// the specified <see cref="Comparison{T}"/>.
    /// </summary>
    /// <param name="comparison">
    /// The <see cref="Comparison{T}"/> to use when comparing elements.
    /// </param>
    /// <returns>The sorted list.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="comparison"/> is null.</exception>
    public ImmutableValueArray<T> Sort(Comparison<T> comparison) => new(_inner.Sort(comparison));

    /// <summary>
    /// Returns a sorted instance of this array.
    /// </summary>
    /// <param name="comparer">The comparer to use in sorting. If <c>null</c>, the default comparer is used.</param>
    public ImmutableValueArray<T> Sort(IComparer<T>? comparer) => new(_inner.Sort(comparer));

    /// <summary>
    /// Returns a sorted instance of this array.
    /// </summary>
    /// <param name="index">The index of the first element to consider in the sort.</param>
    /// <param name="count">The number of elements to include in the sort.</param>
    /// <param name="comparer">The comparer to use in sorting. If <c>null</c>, the default comparer is used.</param>
    public ImmutableValueArray<T> Sort(int index, int count, IComparer<T>? comparer) => new(_inner.Sort(index, count, comparer));

    /// <summary>
    /// Filters the elements of this array to those assignable to the specified type.
    /// </summary>
    /// <typeparam name="TResult">The type to filter the elements of the sequence on.</typeparam>
    /// <returns>
    /// An <see cref="IEnumerable{T}"/> that contains elements from
    /// the input sequence of type <typeparamref name="TResult"/>.
    /// </returns>
    public IEnumerable<TResult> OfType<TResult>() => _inner.OfType<TResult>();

    /// <summary>
    /// Adds the specified values to this list.
    /// </summary>
    /// <param name="items">The values to add.</param>
    /// <returns>A new list with the elements added.</returns>
    public ImmutableValueArray<T> AddRange(params ReadOnlySpan<T> items) => new(_inner.AddRange(items));

    /// <summary>
    /// Adds the specified values to this list.
    /// </summary>
    /// <param name="items">The values to add.</param>
    /// <returns>A new list with the elements added.</returns>
    public ImmutableValueArray<T> AddRange(params T[] items) => new(_inner.AddRange(items));

    /// <summary>
    /// Creates a <see cref="ReadOnlySpan{T}"/> over the portion of current <see cref="ImmutableArray{T}"/> beginning at a specified position for a specified length.
    /// </summary>
    /// <param name="start">The index at which to begin the span.</param>
    /// <param name="length">The number of items in the span.</param>
    /// <returns>The <see cref="ReadOnlySpan{T}"/> representation of the <see cref="ImmutableArray{T}"/></returns>
    public ReadOnlySpan<T> AsSpan(int start, int length) => _inner.AsSpan(start, length);

    /// <summary>
    /// Copies the elements of current <see cref="ImmutableArray{T}"/> to an <see cref="Span{T}"/>.
    /// </summary>
    /// <param name="destination">The <see cref="Span{T}"/> that is the destination of the elements copied from current <see cref="ImmutableArray{T}"/>.</param>
    public void CopyTo(Span<T> destination) => _inner.CopyTo(destination);

    /// <summary>
    /// Inserts the specified values at the specified index.
    /// </summary>
    /// <param name="index">The index at which to insert the value.</param>
    /// <param name="items">The elements to insert.</param>
    /// <returns>The new immutable collection.</returns>
    public ImmutableValueArray<T> InsertRange(int index, T[] items) => new(_inner.InsertRange(index, items));

    /// <summary>
    /// Inserts the specified values at the specified index.
    /// </summary>
    /// <param name="index">The index at which to insert the value.</param>
    /// <param name="items">The elements to insert.</param>
    /// <returns>The new immutable collection.</returns>
    public ImmutableValueArray<T> InsertRange(int index, params ReadOnlySpan<T> items) => new(_inner.InsertRange(index, items));

    /// <summary>
    /// Removes the specified values from this list.
    /// </summary>
    /// <param name="items">The items to remove if matches are found in this list.</param>
    /// <param name="equalityComparer">
    /// The equality comparer to use in the search.
    /// </param>
    /// <returns>
    /// A new list with the elements removed.
    /// </returns>
    public ImmutableValueArray<T> RemoveRange(ReadOnlySpan<T> items, IEqualityComparer<T>? equalityComparer = null) => new(_inner.RemoveRange(items, equalityComparer));

    /// <summary>
    /// Removes the specified values from this list.
    /// </summary>
    /// <param name="items">The items to remove if matches are found in this list.</param>
    /// <param name="equalityComparer">
    /// The equality comparer to use in the search.
    /// </param>
    /// <returns>
    /// A new list with the elements removed.
    /// </returns>
    public ImmutableValueArray<T> RemoveRange(T[] items, IEqualityComparer<T>? equalityComparer = null) => new(_inner.RemoveRange(items, equalityComparer));

    /// <summary>
    /// Forms a slice out of the current <see cref="ImmutableArray{T}"/> starting at a specified index for a specified length.
    /// </summary>
    /// <param name="start">The index at which to begin this slice.</param>
    /// <param name="length">The desired length for the slice.</param>
    /// <returns>A <see cref="ImmutableArray{T}"/> that consists of length elements from the current <see cref="ImmutableArray{T}"/> starting at start.</returns>
    public ImmutableValueArray<T> Slice(int start, int length) => new(_inner.Slice(start, length));

    #region Operators

    /// <summary>
    /// Checks equality between two instances.
    /// </summary>
    /// <param name="left">The instance to the left of the operator.</param>
    /// <param name="right">The instance to the right of the operator.</param>
    /// <returns><c>true</c> if the values' underlying arrays are reference equal; <c>false</c> otherwise.</returns>
    public static bool operator ==(ImmutableValueArray<T> left, ImmutableValueArray<T> right)
    {
        return left.Equals(right);
    }

    /// <summary>
    /// Checks inequality between two instances.
    /// </summary>
    /// <param name="left">The instance to the left of the operator.</param>
    /// <param name="right">The instance to the right of the operator.</param>
    /// <returns><c>true</c> if the values' underlying arrays are reference not equal; <c>false</c> otherwise.</returns>
    public static bool operator !=(ImmutableValueArray<T> left, ImmutableValueArray<T> right)
    {
        return !left.Equals(right);
    }

    #endregion

    #region Explicit interface methods

    /// <inheritdoc/>
    void IList<T>.Insert(int index, T item) => ((IList<T>)_inner).Insert(index, item);

    /// <inheritdoc/>
    void IList<T>.RemoveAt(int index) => ((IList<T>)_inner).RemoveAt(index);

    /// <inheritdoc/>
    void ICollection<T>.Add(T item) => ((ICollection<T>)_inner).Add(item);

    /// <inheritdoc/>
    void ICollection<T>.Clear() => ((ICollection<T>)_inner).Clear();

    /// <inheritdoc/>
    bool ICollection<T>.Remove(T item) => ((ICollection<T>)_inner).Remove(item);

    /// <inheritdoc/>
    int IList.Add(object? value) => ((IList)_inner).Add(value);

    /// <inheritdoc/>
    void IList.Clear() => ((IList)_inner).Clear();

    /// <inheritdoc/>
    void IList.Insert(int index, object? value) => ((IList)_inner).Insert(index, value);

    /// <inheritdoc/>
    void IList.Remove(object? value) => ((IList)_inner).Remove(value);

    /// <inheritdoc/>
    void IList.RemoveAt(int index) => ((IList)_inner).RemoveAt(index);

    /// <inheritdoc/>
    void ICollection.CopyTo(Array array, int index) => ((ICollection)_inner).CopyTo(array, index);

    /// <inheritdoc/>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    bool IList.IsFixedSize => true;

    /// <inheritdoc/>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    bool IList.IsReadOnly => true;

    /// <inheritdoc/>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    int ICollection.Count => ((ICollection)_inner).Count;

    /// <inheritdoc/>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    // This is immutable, so it is always thread-safe.
    bool ICollection.IsSynchronized => true;

    /// <inheritdoc/>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    object ICollection.SyncRoot => ((ICollection)_inner).SyncRoot;

    /// <inheritdoc/>
    object? IList.this[int index]
    {
        get => ((IList)_inner)[index];
        set => ((IList)_inner)[index] = value;
    }

    /// <inheritdoc/>
    IImmutableList<T> IImmutableList<T>.Clear()
    {
        ImmutableValueArray<T> self = this;
        ThrowInvalidOperationIfNotInitialized(self);
        return self.Clear();
    }

    /// <inheritdoc/>
    IImmutableList<T> IImmutableList<T>.Add(T value)
    {
        ImmutableValueArray<T> self = this;
        ThrowInvalidOperationIfNotInitialized(self);
        return self.Add(value);
    }

    /// <inheritdoc/>
    IImmutableList<T> IImmutableList<T>.AddRange(IEnumerable<T> items)
    {
        ImmutableValueArray<T> self = this;
        ThrowInvalidOperationIfNotInitialized(self);
        return self.AddRange(items);
    }

    /// <inheritdoc/>
    IImmutableList<T> IImmutableList<T>.Insert(int index, T element)
    {
        ImmutableValueArray<T> self = this;
        ThrowInvalidOperationIfNotInitialized(self);
        return self.Insert(index, element);
    }

    /// <inheritdoc/>
    IImmutableList<T> IImmutableList<T>.InsertRange(int index, IEnumerable<T> items)
    {
        ImmutableValueArray<T> self = this;
        ThrowInvalidOperationIfNotInitialized(self);
        return self.InsertRange(index, items);
    }

    /// <inheritdoc/>
    IImmutableList<T> IImmutableList<T>.Remove(T value, IEqualityComparer<T>? equalityComparer)
    {
        ImmutableValueArray<T> self = this;
        ThrowInvalidOperationIfNotInitialized(self);
        return self.Remove(value, equalityComparer);
    }

    /// <inheritdoc/>
    IImmutableList<T> IImmutableList<T>.RemoveAll(Predicate<T> match)
    {
        ImmutableValueArray<T> self = this;
        ThrowInvalidOperationIfNotInitialized(self);
        return self.RemoveAll(match);
    }

    /// <inheritdoc/>
    IImmutableList<T> IImmutableList<T>.RemoveRange(IEnumerable<T> items, IEqualityComparer<T>? equalityComparer)
    {
        ImmutableValueArray<T> self = this;
        ThrowInvalidOperationIfNotInitialized(self);
        return self.RemoveRange(items, equalityComparer);
    }

    /// <inheritdoc/>
    IImmutableList<T> IImmutableList<T>.RemoveRange(int index, int count)
    {
        ImmutableValueArray<T> self = this;
        ThrowInvalidOperationIfNotInitialized(self);
        return self.RemoveRange(index, count);
    }

    /// <inheritdoc/>
    IImmutableList<T> IImmutableList<T>.RemoveAt(int index)
    {
        ImmutableValueArray<T> self = this;
        ThrowInvalidOperationIfNotInitialized(self);
        return self.RemoveAt(index);
    }

    /// <inheritdoc/>
    IImmutableList<T> IImmutableList<T>.SetItem(int index, T value)
    {
        ImmutableValueArray<T> self = this;
        ThrowInvalidOperationIfNotInitialized(self);
        return self.SetItem(index, value);
    }

    /// <inheritdoc/>
    IImmutableList<T> IImmutableList<T>.Replace(T oldValue, T newValue, IEqualityComparer<T>? equalityComparer)
    {
        ImmutableValueArray<T> self = this;
        ThrowInvalidOperationIfNotInitialized(self);
        return self.Replace(oldValue, newValue, equalityComparer);
    }

    /// <inheritdoc/>
    bool IList.Contains(object? value)
    {
        ImmutableValueArray<T> self = this;
        ThrowInvalidOperationIfNotInitialized(self);
        return IsCompatibleObject(value, out var typed)
            && self.Contains(typed!);
    }

    /// <inheritdoc/>
    int IList.IndexOf(object? value)
    {
        ImmutableValueArray<T> self = this;
        ThrowInvalidOperationIfNotInitialized(self);
        return IsCompatibleObject(value, out var typed)
            ? self.IndexOf(typed!)
            : -1;
    }

    #endregion

    private static bool IsCompatibleObject(object? value, out T? result)
    {
        result = default;

        if (value is T t)
        {
            result = t;
            return true;
        }

        return default(T) is null && value is null;
    }

    /// <summary>
    /// Throws an <see cref="InvalidOperationException"/> if the <see cref="_inner"/> field is uninitialized, i.e. the
    /// <see cref="IsDefault"/> property returns true.  The <see cref="InvalidOperationException"/> message specifies
    /// that the operation cannot be performed on a default instance of <see cref="ImmutableArray{T}"/>.
    ///
    /// This is intended for explicitly implemented interface method and property implementations.
    /// </summary>
    private static void ThrowInvalidOperationIfNotInitialized(ImmutableValueArray<T> self)
    {
        ((IImmutableList<T>)self._inner).Clear(); // throws if uninitialized
    }
}
