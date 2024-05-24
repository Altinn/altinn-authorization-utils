using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Altinn.Urn.Json;

/// <summary>
/// A utility wrapper for URNs that can be serialized and deserialized as strings.
/// </summary>
/// <typeparam name="T">The URN type.</typeparam>
[JsonConverter(typeof(StringUrnJsonConverter))]
public readonly struct UrnJsonString<T>
    : IUrnJsonWrapper<UrnJsonString<T>, T>
    where T : IKeyValueUrn<T>
{
    /// <summary>
    /// Gets the URN value.
    /// </summary>
    public T? Value { get; }

    /// <summary>
    /// Gets a value indicating whether the URN has a value.
    /// </summary>
    [MemberNotNullWhen(true, nameof(Value))]
    public bool HasValue => Value is not null;

    private UrnJsonString(T? value)
    {
        Value = value;
    }

    /// <inheritdoc/>
    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        if (ReferenceEquals(Value, obj))
        {
            return true;
        }

        if (ReferenceEquals(Value, null))
        {
            return false;
        }

        return Value.Equals(obj);
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        return Value?.GetHashCode() ?? 0;
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return Value?.ToString() ?? "";
    }

    /// <summary>
    /// Implicitly converts a URN value to a <see cref="UrnJsonString{T}"/>.
    /// </summary>
    /// <param name="value">The URN value.</param>
    public static implicit operator UrnJsonString<T>(T? value)
        => new(value);
}
