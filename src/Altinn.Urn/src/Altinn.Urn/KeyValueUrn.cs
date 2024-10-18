using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Altinn.Urn;

/// <summary>
/// A raw, untyped URN with a prefix and a value part.
/// </summary>
/// <remarks>
/// This type does not provide any parsing or validation of the URN.
/// To construct a <see cref="KeyValueUrn"/>, you need to know where the prefix ends and the value starts.
/// </remarks>
[DebuggerDisplay("{Urn}")]
[JsonConverter(typeof(JsonConverter))]
public readonly struct KeyValueUrn
    : IKeyValueUrn
    , IEquatable<KeyValueUrn>
    , IEqualityOperators<KeyValueUrn, KeyValueUrn, bool>
{
    /// <summary>
    /// Creates a new instance of the <see cref="KeyValueUrn"/> type.
    /// </summary>
    /// <param name="urn">The original <see cref="IKeyValueUrn"/>.</param>
    /// <returns>A new <see cref="KeyValueUrn"/>.</returns>
    public static KeyValueUrn Create(IKeyValueUrn urn)
        => new(urn.Urn, urn.KeySpan.Length + 1);

    /// <summary>
    /// Creates a new instance of the <see cref="KeyValueUrn"/> type without checking any of the invariants.
    /// </summary>
    /// <param name="urn">The urn value.</param>
    /// <param name="valueIndex">The index at which the value starts.</param>
    /// <returns>A new <see cref="KeyValueUrn"/>.</returns>
    public static KeyValueUrn CreateUnchecked(string urn, int valueIndex)
        => new(urn, valueIndex);

    /// <summary>
    /// Creates a new instance of the <see cref="KeyValueUrn"/> type.
    /// </summary>
    /// <param name="urn">The urn value.</param>
    /// <param name="valueIndex">The index at which the value starts.</param>
    /// <returns>A new <see cref="KeyValueUrn"/>.</returns>
    public static KeyValueUrn Create(string urn, int valueIndex)
    {
        ArgumentNullException.ThrowIfNull(urn);
        ArgumentOutOfRangeException.ThrowIfNegative(valueIndex);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(valueIndex, urn.Length);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(valueIndex, 4);
        if (!urn.AsSpan().StartsWith("urn:"))
        {
            throw new ArgumentException("Urn must start with 'urn:'.", nameof(urn));
        }

        var sep = urn[valueIndex - 1];
        if (sep != ':')
        {
            throw new ArgumentException("Urn value must be preceded by a ':' separator.", nameof(urn));
        }

        return new(urn, valueIndex);
    }

    private readonly string _urn;
    private readonly int _valueIndex;

    private KeyValueUrn(string urn, int valueIndex)
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
    public ReadOnlyMemory<char> PrefixMemory => _urn.AsMemory(0, _valueIndex - 1);

    /// <inheritdoc/>
    public ReadOnlySpan<char> KeySpan => _urn.AsSpan(4, _valueIndex - 5);

    /// <inheritdoc/>
    public ReadOnlySpan<char> ValueSpan => _urn.AsSpan(_valueIndex);

    /// <inheritdoc/>
    [DebuggerHidden]
    public ReadOnlyMemory<char> KeyMemory => _urn.AsMemory(4, _valueIndex - 5);

    /// <inheritdoc/>
    [DebuggerHidden]
    public ReadOnlyMemory<char> ValueMemory => _urn.AsMemory(_valueIndex);

    /// <inheritdoc/>
    public ReadOnlyMemory<char> AsMemory() => _urn.AsMemory();

    /// <inheritdoc/>
    public ReadOnlySpan<char> AsSpan() => _urn.AsSpan();

    /// <inheritdoc/>
    public bool Equals(KeyValueUrn other)
    {
        return _urn == other._urn
            && _valueIndex == other._valueIndex;
    }

    /// <inheritdoc/>
    public string ToString(string? format, IFormatProvider? formatProvider)
    {
        return format.AsSpan() switch
        {
            ['P'] => new string(KeySpan),
            ['S'] or ['V', ..] => new string(ValueSpan),
            _ => _urn,
        };
    }

    /// <inheritdoc/>
    public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
    {
        var source = format switch
        {
            ['P'] => KeySpan,
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
        => obj is KeyValueUrn urn && Equals(urn);

    /// <inheritdoc/>
    public override int GetHashCode()
        => HashCode.Combine(_urn, _valueIndex);

    /// <inheritdoc/>
    public static bool operator ==(KeyValueUrn left, KeyValueUrn right)
        => left.Equals(right);

    /// <inheritdoc/>
    public static bool operator !=(KeyValueUrn left, KeyValueUrn right)
        => !left.Equals(right);

    private sealed class JsonConverter
        : JsonConverter<KeyValueUrn>
    {
        public override KeyValueUrn Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotSupportedException($"Deserialization of {nameof(KeyValueUrn)} is not supported.");
        }

        public override void Write(Utf8JsonWriter writer, KeyValueUrn value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.Urn);
        }
    }
}
