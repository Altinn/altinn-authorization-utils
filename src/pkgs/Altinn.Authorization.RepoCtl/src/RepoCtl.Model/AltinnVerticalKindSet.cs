using System.Collections;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using Altinn.Authorization.RepoCtl.Model.Utils;
using CommunityToolkit.Diagnostics;

namespace Altinn.Authorization.RepoCtl.Model;

/// <summary>
/// A set of <see cref="AltinnVerticalKind"/>s.
/// </summary>
[DebuggerDisplay("{ToString(),nq}")]
[CollectionBuilder(typeof(AltinnVerticalKindSet), nameof(Create))]
public readonly record struct AltinnVerticalKindSet
    : IImmutableSet<AltinnVerticalKind>
    , IReadOnlySet<AltinnVerticalKind>
    , IEquatable<AltinnVerticalKindSet>
    , IFormattable
    , ISpanFormattable
{
    private const Kinds MinValue = Kinds.Application;
    private const Kinds MaxValue = Kinds.Library;

    private const int MAX_ITEMS = 4;

    private const string STRING_SEPARATOR = ", ";
    private const int STRING_MAX_ITEM_LENGTH = 4;
    private const int STRING_SEPARATOR_LENGTH = 2;
    private const int STRING_BUFFER_LENGTH = MAX_ITEMS * STRING_MAX_ITEM_LENGTH + (MAX_ITEMS - 1) * STRING_SEPARATOR_LENGTH;

    /// <summary>
    /// Represents a deployable application.
    /// </summary>
    internal static readonly AltinnVerticalKindSet Application = new(Kinds.Application);

    /// <summary>
    /// Represents an internal library that can be used by other libraries or applications.
    /// </summary>

    internal static readonly AltinnVerticalKindSet Library = new(Kinds.Library);

    /// <summary>
    /// Represents a package that is published to a package registry. Can be used by all other verticals.
    /// </summary>
    internal static readonly AltinnVerticalKindSet Package = new(Kinds.Package);

    /// <summary>
    /// Represents a tool (a special kind of package) that is published to a package registry. Cannot be used as a dependency.
    /// </summary>
    internal static readonly AltinnVerticalKindSet Tool = new(Kinds.Tool);

    /// <summary>
    /// Represents an empty set of vertical-kinds.
    /// </summary>
    public static AltinnVerticalKindSet Empty
        => default;

    /// <summary>
    /// Represents a set containing all vertical-kinds.
    /// </summary>
    public static AltinnVerticalKindSet All
        => new(Kinds.Application | Kinds.Library | Kinds.Package | Kinds.Tool);

    /// <summary>
    /// Creates a new <see cref="AltinnVerticalKindSet"/> from the specified kinds.
    /// </summary>
    /// <param name="kinds">The vertical kinds to add to the set.</param>
    /// <returns>A new <see cref="AltinnVerticalKindSet"/> containing the specified kinds.</returns>
    public static AltinnVerticalKindSet Create(params scoped ReadOnlySpan<AltinnVerticalKind> kinds)
    {
        var value = Kinds.None;
        foreach (var kind in kinds)
        {
            value |= kind._value._value;
        }

        return new AltinnVerticalKindSet(value);
    }

    private readonly Kinds _value;

    private AltinnVerticalKindSet(Kinds kinds)
    {
        _value = kinds;
    }

    /// <inheritdoc/>
    public int Count
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => BitOperations.PopCount((uint)_value);
    }

    /// <inheritdoc/>
    public override int GetHashCode()
        => _value.GetHashCode();

    /// <inheritdoc/>
    public override string ToString()
        => Count switch
        {
            0 or 1 => ToStringSingle(_value),
            _ => ToStringMultiple(_value),
        };

    /// <inheritdoc/>
    public string ToString(string? format, IFormatProvider? formatProvider)
        => ToString();

    /// <inheritdoc/>
    public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
        => Count switch
        {
            0 or 1 => ToStringSingle(_value).TryCopyTo(destination, out charsWritten),
            _ => TryFormatMultiple(_value, destination, out charsWritten),
        };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string ToStringSingle(Kinds kinds)
        => kinds switch
        {
            Kinds.None => "none",
            Kinds.Application => "app",
            Kinds.Library => "lib",
            Kinds.Package => "pkg",
            Kinds.Tool => "tool",
            _ => ThrowHelper.ThrowArgumentOutOfRangeException<string>(nameof(kinds), $"The value '{kinds}' is not a valid AltinnVerticalKind."),
        };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string ToStringMultiple(Kinds kinds)
    {
        Span<char> buffer = stackalloc char[STRING_BUFFER_LENGTH];
        var success = TryFormatMultiple(kinds, buffer, out var charsWritten);
        Debug.Assert(success);

        return new string(buffer.Slice(0, charsWritten));
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static bool TryFormatMultiple(Kinds kinds, scoped Span<char> destination, out int charsWritten)
    {
        Debug.Assert(BitOperations.PopCount((uint)kinds) > 1);

        charsWritten = 0;
        var first = true;

        var enumerator = new Enumerator(kinds);
        while (enumerator.MoveNext())
        {
            if (!first)
            {
                if (!Append(destination, STRING_SEPARATOR, ref charsWritten))
                {
                    return false;
                }
            }

            first = false;
            if (!Append(destination, ToStringSingle(enumerator.Current._value._value), ref charsWritten))
            {
                return false;
            }
        }

        return true;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static bool Append(scoped Span<char> destination, scoped ReadOnlySpan<char> value, ref int charsWritten)
        {
            if (!value.TryCopyTo(destination[charsWritten..]))
            {
                return false;
            }

            charsWritten += value.Length;
            return true;
        }
    }

    /// <inheritdoc/>
    public bool Contains(AltinnVerticalKind item)
    {
        return item.HasValue && _value.HasFlag(item._value._value);
    }

    /// <inheritdoc cref="IEnumerable{T}.GetEnumerator()"/>
    public Enumerator GetEnumerator()
        => new(_value);

    /// <inheritdoc/>
    IEnumerator<AltinnVerticalKind> IEnumerable<AltinnVerticalKind>.GetEnumerator()
        => GetEnumerator();

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator()
        => GetEnumerator();

    /// <inheritdoc cref="IReadOnlySet{T}.IsProperSubsetOf(IEnumerable{T})"/>
    public bool IsProperSubsetOf(AltinnVerticalKindSet other)
        => _value != other._value && (_value & other._value) == _value;

    /// <inheritdoc cref="IReadOnlySet{T}.IsProperSupersetOf(IEnumerable{T})"/>
    public bool IsProperSupersetOf(AltinnVerticalKindSet other)
        => _value != other._value && (_value & other._value) == other._value;

    /// <inheritdoc cref="IReadOnlySet{T}.IsSubsetOf(IEnumerable{T})"/>
    public bool IsSubsetOf(AltinnVerticalKindSet other)
        => (_value & other._value) == _value;

    /// <inheritdoc cref="IReadOnlySet{T}.IsSupersetOf(IEnumerable{T})"/>
    public bool IsSupersetOf(AltinnVerticalKindSet other)
        => (_value & other._value) == other._value;

    /// <inheritdoc cref="IReadOnlySet{T}.Overlaps(IEnumerable{T})"/>
    public bool Overlaps(AltinnVerticalKindSet other)
        => (_value & other._value) != Kinds.None;

    /// <inheritdoc cref="IReadOnlySet{T}.SetEquals(IEnumerable{T})"/>
    public bool SetEquals(AltinnVerticalKindSet other)
        => _value == other._value;

    /// <inheritdoc cref="IImmutableSet{T}.Add(T)"/>
    public AltinnVerticalKindSet Add(AltinnVerticalKind item)
        => new(_value | item._value._value);

    /// <inheritdoc cref="IImmutableSet{T}.Remove(T)"/>
    public AltinnVerticalKindSet Remove(AltinnVerticalKind item)
        => new(_value & ~item._value._value);

    /// <inheritdoc cref="IImmutableSet{T}.Intersect(IEnumerable{T})"/>
    public AltinnVerticalKindSet Intersect(AltinnVerticalKindSet other)
        => new(_value & other._value);

    /// <inheritdoc cref="IImmutableSet{T}.Except(IEnumerable{T})"/>
    public AltinnVerticalKindSet Except(AltinnVerticalKindSet other)
        => new(_value & ~other._value);

    /// <inheritdoc cref="IImmutableSet{T}.SymmetricExcept(IEnumerable{T})"/>
    public AltinnVerticalKindSet SymmetricExcept(AltinnVerticalKindSet other)
        => new(_value ^ other._value);

    /// <inheritdoc cref="IImmutableSet{T}.Union(IEnumerable{T})"/>
    public AltinnVerticalKindSet Union(AltinnVerticalKindSet other)
        => new(_value | other._value);

    /// <inheritdoc/>
    bool IReadOnlySet<AltinnVerticalKind>.IsProperSubsetOf(IEnumerable<AltinnVerticalKind> other)
        => IsProperSubsetOf(ToSet(other));

    /// <inheritdoc/>
    bool IReadOnlySet<AltinnVerticalKind>.IsProperSupersetOf(IEnumerable<AltinnVerticalKind> other)
        => IsProperSupersetOf(ToSet(other));

    /// <inheritdoc/>
    bool IReadOnlySet<AltinnVerticalKind>.IsSubsetOf(IEnumerable<AltinnVerticalKind> other)
        => IsSubsetOf(ToSet(other));

    /// <inheritdoc/>
    bool IReadOnlySet<AltinnVerticalKind>.IsSupersetOf(IEnumerable<AltinnVerticalKind> other)
        => IsSupersetOf(ToSet(other));

    /// <inheritdoc/>
    bool IReadOnlySet<AltinnVerticalKind>.Overlaps(IEnumerable<AltinnVerticalKind> other)
        => Overlaps(ToSet(other));

    /// <inheritdoc/>
    bool IReadOnlySet<AltinnVerticalKind>.SetEquals(IEnumerable<AltinnVerticalKind> other)
        => SetEquals(ToSet(other));

    /// <inheritdoc/>
    bool IImmutableSet<AltinnVerticalKind>.IsProperSubsetOf(IEnumerable<AltinnVerticalKind> other)
        => IsProperSubsetOf(ToSet(other));

    /// <inheritdoc/>
    bool IImmutableSet<AltinnVerticalKind>.IsProperSupersetOf(IEnumerable<AltinnVerticalKind> other)
        => IsProperSupersetOf(ToSet(other));

    /// <inheritdoc/>
    bool IImmutableSet<AltinnVerticalKind>.IsSubsetOf(IEnumerable<AltinnVerticalKind> other)
        => IsSubsetOf(ToSet(other));

    /// <inheritdoc/>
    bool IImmutableSet<AltinnVerticalKind>.IsSupersetOf(IEnumerable<AltinnVerticalKind> other)
        => IsSupersetOf(ToSet(other));

    /// <inheritdoc/>
    bool IImmutableSet<AltinnVerticalKind>.Overlaps(IEnumerable<AltinnVerticalKind> other)
        => Overlaps(ToSet(other));

    /// <inheritdoc/>
    bool IImmutableSet<AltinnVerticalKind>.SetEquals(IEnumerable<AltinnVerticalKind> other)
        => SetEquals(ToSet(other));

    /// <inheritdoc/>
    IImmutableSet<AltinnVerticalKind> IImmutableSet<AltinnVerticalKind>.Clear()
        => default(AltinnVerticalKindSet);

    /// <inheritdoc/>
    IImmutableSet<AltinnVerticalKind> IImmutableSet<AltinnVerticalKind>.Add(AltinnVerticalKind value)
        => Add(value);

    /// <inheritdoc/>
    IImmutableSet<AltinnVerticalKind> IImmutableSet<AltinnVerticalKind>.Remove(AltinnVerticalKind value)
        => Remove(value);

    /// <inheritdoc/>
    IImmutableSet<AltinnVerticalKind> IImmutableSet<AltinnVerticalKind>.Intersect(IEnumerable<AltinnVerticalKind> other)
        => Intersect(ToSet(other));

    /// <inheritdoc/>
    IImmutableSet<AltinnVerticalKind> IImmutableSet<AltinnVerticalKind>.Except(IEnumerable<AltinnVerticalKind> other)
        => Except(ToSet(other));

    /// <inheritdoc/>
    IImmutableSet<AltinnVerticalKind> IImmutableSet<AltinnVerticalKind>.SymmetricExcept(IEnumerable<AltinnVerticalKind> other)
        => SymmetricExcept(ToSet(other));

    /// <inheritdoc/>
    IImmutableSet<AltinnVerticalKind> IImmutableSet<AltinnVerticalKind>.Union(IEnumerable<AltinnVerticalKind> other)
        => Union(ToSet(other));

    /// <inheritdoc/>
    bool IImmutableSet<AltinnVerticalKind>.TryGetValue(AltinnVerticalKind equalValue, out AltinnVerticalKind actualValue)
    {
        actualValue = equalValue;
        return Contains(equalValue);
    }

    private static AltinnVerticalKindSet ToSet(IEnumerable<AltinnVerticalKind> other)
    {
        if (other is null)
        {
            ThrowHelper.ThrowArgumentNullException(nameof(other));
        }

        if (other is AltinnVerticalKindSet set)
        {
            return set;
        }

        var kinds = Kinds.None;
        foreach (var item in other)
        {
            kinds |= item._value._value;
        }

        return new AltinnVerticalKindSet(kinds);
    }

    /// <summary>
    /// Defines an implicit conversion from <see cref="AltinnVerticalKind"/> to <see cref="AltinnVerticalKindSet"/>.
    /// </summary>
    /// <param name="kind">The <see cref="AltinnVerticalKind"/> to convert to an <see cref="AltinnVerticalKindSet"/>.</param>
    public static implicit operator AltinnVerticalKindSet(AltinnVerticalKind kind)
        => new(kind._value._value);

    internal static int Compare(AltinnVerticalKind x, AltinnVerticalKind y)
    {
        Debug.Assert(x._value.Count == 1);
        Debug.Assert(y._value.Count == 1);
        return x._value._value.CompareTo(y._value._value);
    }

    /// <summary>
    /// The kind of verticals.
    /// </summary>
    /// <remarks>
    /// The order of the values is important, as it is used for sorting verticals.
    /// </remarks>
    [Flags]
    internal enum Kinds
        : byte
    {
        /// <summary>
        /// Represents no vertical-kinds.
        /// </summary>
        None = 0,

        /// <summary>
        /// Represents a deployable application.
        /// </summary>
        Application = 1 << 0,

        /// <summary>
        /// Represents a package that is published to a package registry. Can be used by all other verticals.
        /// </summary>
        Package = 1 << 1,

        /// <summary>
        /// Represents a tool (a special kind of package) that is published to a package registry. Cannot be used as a dependency.
        /// </summary>
        Tool = 1 << 2,

        /// <summary>
        /// Represents an internal library that can be used by other libraries or applications.
        /// </summary>
        Library = 1 << 3,
    }

    /// <summary>
    /// An enumerator for <see cref="AltinnVerticalKindSet"/>.
    /// </summary>
    public struct Enumerator
        : IEnumerator<AltinnVerticalKind>
    {
        private Kinds _remaining;
        private AltinnVerticalKind _current;

        /// <summary>
        /// Initializes a new instance of the <see cref="Enumerator"/> struct.
        /// </summary>
        internal Enumerator(Kinds kinds)
        {
            _remaining = kinds;
            _current = default;
        }

        /// <inheritdoc/>
        public AltinnVerticalKind Current
            => _current;

        /// <inheritdoc/>
        object IEnumerator.Current
            => Current;

        /// <inheritdoc/>
        public void Dispose()
        {
        }

        /// <inheritdoc/>
        public bool MoveNext()
        {
            if (_remaining == Kinds.None)
            {
                return false;
            }

            var next = (Kinds)(1 << BitOperations.TrailingZeroCount((uint)_remaining));
            if (next < MinValue || next > MaxValue)
            {
                ThrowHelper.ThrowInvalidOperationException($"The value '{next}' is not a valid AltinnVerticalKind.");
            }

            _current = new(new(next));

            _remaining &= ~next;
            return true;
        }

        /// <inheritdoc/>
        public void Reset()
            => ThrowHelper.ThrowNotSupportedException();
    }
}
