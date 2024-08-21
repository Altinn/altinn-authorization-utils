using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Text.Json.Serialization;

namespace Altinn.Urn.Json;

/// <summary>
/// A utility wrapper for URNs that can be serialized and deserialized as JSON objects with a type and a value property.
/// </summary>
[JsonConverter(typeof(TypeValueObjectKeyValueUrnJsonConverter))]
public readonly struct UrnJsonTypeValue
    : IEqualityOperators<UrnJsonTypeValue, UrnJsonTypeValue, bool>
{
    /// <summary>
    /// Gets the URN value.
    /// </summary>
    public KeyValueUrn Value { get; }

    /// <summary>
    /// Gets a value indicating whether the URN has a value.
    /// </summary>
    public bool HasValue => Value.HasValue;

    private UrnJsonTypeValue(KeyValueUrn value)
    {
        Value = value;
    }

    /// <inheritdoc/>
    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        return Value.Equals(obj);
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return Value.ToString();
    }

    /// <summary>
    /// Implicitly converts a URN value to a <see cref="UrnJsonTypeValue{T}"/>.
    /// </summary>
    /// <param name="value"></param>
    public static implicit operator UrnJsonTypeValue(KeyValueUrn value)
        => new(value);

    /// <inheritdoc/>
    public static bool operator ==(UrnJsonTypeValue left, UrnJsonTypeValue right)
        => left.Equals(right);

    /// <inheritdoc/>
    public static bool operator !=(UrnJsonTypeValue left, UrnJsonTypeValue right)
        => !(left == right);
}

/// <summary>
/// A utility wrapper for URNs that can be serialized and deserialized as JSON objects with a type and a value property.
/// </summary>
/// <typeparam name="T"></typeparam>
[JsonConverter(typeof(TypeValueObjectUrnJsonConverter))]
public readonly struct UrnJsonTypeValue<T>
    : IUrnJsonWrapper<UrnJsonTypeValue<T>, T>
    , IEqualityOperators<UrnJsonTypeValue<T>, UrnJsonTypeValue<T>, bool>
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

    private UrnJsonTypeValue(T? value)
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
    /// Implicitly converts a URN value to a <see cref="UrnJsonTypeValue{T}"/>.
    /// </summary>
    /// <param name="value"></param>
    public static implicit operator UrnJsonTypeValue<T>(T? value)
        => new(value);

    /// <inheritdoc/>
    public static bool operator ==(UrnJsonTypeValue<T> left, UrnJsonTypeValue<T> right) 
        => left.Equals(right);

    /// <inheritdoc/>
    public static bool operator !=(UrnJsonTypeValue<T> left, UrnJsonTypeValue<T> right) 
        => !(left == right);
}
