using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Altinn.Urn;

/// <summary>
/// A raw, untyped URN.
/// </summary>
/// <remarks>
/// This type does not provide any parsing or validation of the URN.
/// To construct a <see cref="RawUrn"/>, you need to know where the prefix ends and the value starts.
/// </remarks>
[DebuggerDisplay("{Urn}")]
public readonly struct RawUrn
    : IUrn
    , IEquatable<RawUrn>
{
    public static RawUrn Create(IUrn urn)
        => new(urn.Urn, urn.PrefixSpan.Length + 1);

    public static RawUrn CreateUnchecked(string urn, int valueIndex)
        => new(urn, valueIndex);

    public static RawUrn Create(string urn, int valueIndex)
    {
        ArgumentNullException.ThrowIfNull(urn);
        ArgumentOutOfRangeException.ThrowIfNegative(valueIndex);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(valueIndex, urn.Length);

        return new(urn, valueIndex);
    }

    private readonly string _urn;
    private readonly int _valueIndex;

    private RawUrn(string urn, int valueIndex)
    {
        _urn = urn;
        _valueIndex = valueIndex;
    }

    /// <summary>
    /// Gets a value indicating whether the URN is empty.
    /// </summary>
    public bool HasValue => _urn is not null;

    /// <inheritdoc/>
    public string Urn => _urn;

    /// <inheritdoc/>
    public ReadOnlySpan<char> PrefixSpan => _urn.AsSpan(0, _valueIndex - 1);

    /// <inheritdoc/>
    public ReadOnlySpan<char> ValueSpan => _urn.AsSpan(_valueIndex);

    /// <inheritdoc/>
    [DebuggerHidden]
    public ReadOnlyMemory<char> PrefixMemory => _urn.AsMemory(0, _valueIndex - 1);

    /// <inheritdoc/>
    [DebuggerHidden]
    public ReadOnlyMemory<char> ValueMemory => _urn.AsMemory(_valueIndex);

    /// <inheritdoc/>
    public ReadOnlyMemory<char> AsMemory() => _urn.AsMemory();

    /// <inheritdoc/>
    public ReadOnlySpan<char> AsSpan() => _urn.AsSpan();

    /// <inheritdoc/>
    public bool Equals(RawUrn other)
    {
        return _urn == other._urn
            && _valueIndex == other._valueIndex;
    }

    /// <inheritdoc/>
    public string ToString(string? format, IFormatProvider? formatProvider)
    {
        return format.AsSpan() switch
        {
            ['P'] => new string(PrefixSpan),
            ['S'] or ['V', ..] => new string(ValueSpan),
            _ => _urn,
        };
    }

    /// <inheritdoc/>
    public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
    {
        var source = format switch
        {
            ['P'] => PrefixSpan,
            ['S'] or ['V', ..] => ValueSpan,
            _ => _urn.AsSpan(),
        };

        if (source.TryCopyTo(destination))
        {
            charsWritten = source.Length;
            return true;
        }

        charsWritten = 0;
        return false;
    }

    /// <inheritdoc/>
    public override string ToString()
        => _urn;

    /// <inheritdoc/>
    public override bool Equals([NotNullWhen(true)] object? obj)
        => obj is RawUrn urn && Equals(urn);

    /// <inheritdoc/>
    public override int GetHashCode()
        => HashCode.Combine(_urn, _valueIndex);

    public static bool operator ==(RawUrn left, RawUrn right)
        => left.Equals(right);

    public static bool operator !=(RawUrn left, RawUrn right)
        => !left.Equals(right);
}
