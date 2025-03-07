using CommunityToolkit.Diagnostics;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Altinn.Authorization.ModelUtils;

/// <summary>
/// Static utility class for <see cref="NonExhaustiveEnum{T}"/>.
/// </summary>
public static class NonExhaustiveEnum
{
    internal sealed class ConverterAttribute
        : JsonConverterAttribute
    {
        private static readonly ConcurrentDictionary<Type, JsonConverter> _cache = new();

        public override JsonConverter? CreateConverter(Type typeToConvert)
            => _cache.GetOrAdd(typeToConvert, CreateFactory);

        private static JsonConverter CreateFactory(Type typeToConvert)
        {
            Debug.Assert(typeToConvert.IsGenericType && typeToConvert.GetGenericTypeDefinition() == typeof(NonExhaustiveEnum<>));
            var inner = typeToConvert.GetGenericArguments()[0];

            var factoryType = typeof(Factory<>).MakeGenericType(inner);
            var factory = Activator.CreateInstance(factoryType);
            return (JsonConverter)factory!;
        }

        private sealed class Factory<T>
            : JsonConverterFactory
            where T : struct, Enum
        {
            private readonly JsonStringEnumConverter _converter;

            public Factory()
            {
                var type = typeof(T);
                
                if (type.GetCustomAttribute<StringEnumConverterAttribute>() is { } attribute)
                {
                    _converter = attribute.Converter;
                }
                else
                {
                    _converter = new JsonStringEnumConverter(namingPolicy: null, allowIntegerValues: false);
                }
            }

            // This factory is only created for types we can convert
            public override bool CanConvert(Type typeToConvert)
            {
                Debug.Assert(typeToConvert == typeof(NonExhaustiveEnum<T>));
                return true;
            }

            public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
            {
                Debug.Assert(typeToConvert == typeof(NonExhaustiveEnum<T>));
                return new NonExhaustiveEnum<T>.Converter(options, _converter);
            }
        }
    }
}

/// <summary>
/// Represents an enum that can have unknown values.
/// </summary>
/// <typeparam name="T">The inner enum type.</typeparam>
[DebuggerDisplay("{DebuggerDisplay,nq}")]
[NonExhaustiveEnum.Converter]
public readonly struct NonExhaustiveEnum<T>
    : IEquatable<NonExhaustiveEnum<T>>
    , IEquatable<T>
    where T : struct, Enum
{
    private readonly T _value;
    private readonly string? _string;

    private NonExhaustiveEnum(T value)
    {
        _value = value;
        _string = null;
    }

    private NonExhaustiveEnum(string value)
    {
        _value = default;
        _string = value;
    }

    /// <summary>
    /// Gets a value indicating whether the enum is an unknown value.
    /// </summary>
    [MemberNotNullWhen(true, nameof(_string))]
    public bool IsUnknown
        => _string is not null;

    /// <summary>
    /// Gets a value indicating whether the enum is a well-known value.
    /// </summary>
    [MemberNotNullWhen(false, nameof(_string))]
    public bool IsWellKnown
        => _string is null;

    /// <summary>
    /// Gets the well-known value.
    /// </summary>
    /// <exception cref="InvalidCastException">
    /// If the value is unknown.
    /// </exception>
    public T Value => (T)this;

    /// <summary>
    /// Gets the well-known value.
    /// </summary>
    /// <exception cref="InvalidCastException">
    /// If the value is well-known.
    /// </exception>
    public string UnknownValue => (string)this;

    /// <summary>
    /// Attempts to get the well-known value.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns><see langword="true"/> if the current value is well-known, otherwise <see langword="false"/>.</returns>
    public bool TryGetValue(out T value)
    {
        value = _value;
        return IsWellKnown;
    }

    /// <inheritdoc/>
    public override int GetHashCode()
        => _string is not null
        ? _string.GetHashCode()
        : _value.GetHashCode();

    /// <inheritdoc/>
    public override string ToString()
        => _string is not null
        ? _string
        : _value.ToString();

    /// <inheritdoc/>
    public override bool Equals([NotNullWhen(true)] object? obj)
        => obj switch
        {
            NonExhaustiveEnum<T> other => Equals(other),
            T other => Equals(other),
            string other => Equals(other),
            _ => false,
        };

    /// <inheritdoc/>
    public bool Equals(NonExhaustiveEnum<T> other)
        => string.Equals(other._string, _string, StringComparison.Ordinal)
        && _value.Equals(other._value);

    /// <inheritdoc/>
    public bool Equals(T other)
        => _string is null
        && _value.Equals(other);

    /// <inheritdoc cref="Equals(T)"/>
    public bool Equals(string? other)
        => _string is not null
        && string.Equals(other, _string, StringComparison.Ordinal);

    private string DebuggerDisplay
        => _string is not null
        ? $"\"{_string}\""
        : _value.ToString();

    /// <summary>
    /// Implicitly converts a <typeparamref name="T"/> to a <see cref="NonExhaustiveEnum{T}"/>.
    /// </summary>
    /// <param name="value">The <typeparamref name="T"/>.</param>
    public static implicit operator NonExhaustiveEnum<T>(T value)
        => new(value);

    /// <summary>
    /// Explicitly converts a <see cref="NonExhaustiveEnum{T}"/> to a <typeparamref name="T"/>.
    /// </summary>
    /// <param name="value">The <see cref="NonExhaustiveEnum{T}"/>.</param>
    /// <exception cref="InvalidCastException">
    /// If the value is unknown.
    /// </exception>
    public static explicit operator T(NonExhaustiveEnum<T> value)
    {
        if (value.IsUnknown)
        {
            InvalidCast();
        }

        return value._value;
    }

    /// <summary>
    /// Explicitly converts a <see cref="NonExhaustiveEnum{T}"/> to a <see langword="string"/>.
    /// </summary>
    /// <param name="value">The <see cref="NonExhaustiveEnum{T}"/>.</param>
    /// <exception cref="InvalidCastException">
    /// If the value is well-known.
    /// </exception>
    public static explicit operator string(NonExhaustiveEnum<T> value)
    {
        if (value.IsWellKnown)
        {
            InvalidCast();
        }

        return value._string;
    }

    /// <inheritdoc cref="System.Numerics.IEqualityOperators{TSelf, TOther, TResult}.op_Equality(TSelf,TOther)"/>"/>
    public static bool operator ==(NonExhaustiveEnum<T> left, T right)
        => left.Equals(right);

    /// <inheritdoc cref="System.Numerics.IEqualityOperators{TSelf, TOther, TResult}.op_Inequality(TSelf,TOther)"/>"/>
    public static bool operator !=(NonExhaustiveEnum<T> left, T right)
        => !left.Equals(right);

    /// <inheritdoc cref="System.Numerics.IEqualityOperators{TSelf, TOther, TResult}.op_Equality(TSelf,TOther)"/>"/>
    public static bool operator ==(T left, NonExhaustiveEnum<T> right)
        => right.Equals(left);

    /// <inheritdoc cref="System.Numerics.IEqualityOperators{TSelf, TOther, TResult}.op_Inequality(TSelf,TOther)"/>"/>
    public static bool operator !=(T left, NonExhaustiveEnum<T> right)
        => !right.Equals(left);

    /// <inheritdoc cref="System.Numerics.IEqualityOperators{TSelf, TOther, TResult}.op_Equality(TSelf,TOther)"/>"/>
    public static bool operator ==(string? left, NonExhaustiveEnum<T> right)
        => right.Equals(left);

    /// <inheritdoc cref="System.Numerics.IEqualityOperators{TSelf, TOther, TResult}.op_Inequality(TSelf,TOther)"/>"/>
    public static bool operator !=(string? left, NonExhaustiveEnum<T> right)
        => !right.Equals(left);

    /// <inheritdoc cref="System.Numerics.IEqualityOperators{TSelf, TOther, TResult}.op_Equality(TSelf,TOther)"/>"/>
    public static bool operator ==(NonExhaustiveEnum<T> left, string? right)
        => left.Equals(right);

    /// <inheritdoc cref="System.Numerics.IEqualityOperators{TSelf, TOther, TResult}.op_Inequality(TSelf,TOther)"/>"/>
    public static bool operator !=(NonExhaustiveEnum<T> left, string? right)
        => !left.Equals(right);

    /// <inheritdoc cref="System.Numerics.IEqualityOperators{TSelf, TOther, TResult}.op_Equality(TSelf,TOther)"/>"/>
    public static bool operator ==(NonExhaustiveEnum<T> left, NonExhaustiveEnum<T> right)
        => left.Equals(right);

    /// <inheritdoc cref="System.Numerics.IEqualityOperators{TSelf, TOther, TResult}.op_Inequality(TSelf,TOther)"/>"/>
    public static bool operator !=(NonExhaustiveEnum<T> left, NonExhaustiveEnum<T> right)
        => !left.Equals(right);

    [DoesNotReturn]
    private static void InvalidCast()
    {
        throw new InvalidCastException("Cannot cast unknown value to well-known value");
    }

    internal sealed class Converter
        : JsonConverter<NonExhaustiveEnum<T>>
    {
        private readonly JsonConverter<T> _inner;
        private readonly JsonConverter<string> _string;

        public Converter(JsonSerializerOptions options, JsonStringEnumConverter inner)
        {
            _inner = (JsonConverter<T>)inner.CreateConverter(typeof(T), options);
            _string = (JsonConverter<string>)options.GetConverter(typeof(string));
        }

        public override NonExhaustiveEnum<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            try
            {
                return _inner.Read(ref reader, typeof(T), options);
            }
            catch (JsonException ex) when (reader.TokenType == JsonTokenType.String || reader.TokenType == JsonTokenType.PropertyName)
            {
                var value = _string.Read(ref reader, typeof(string), options)
                    ?? throw new JsonException("Unexpected null value", ex);

                return new NonExhaustiveEnum<T>(value);
            }
        }

        public override NonExhaustiveEnum<T> ReadAsPropertyName(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            try
            {
                return _inner.ReadAsPropertyName(ref reader, typeof(T), options);
            }
            catch (JsonException ex) when (reader.TokenType == JsonTokenType.String || reader.TokenType == JsonTokenType.PropertyName)
            {
                var value = _string.ReadAsPropertyName(ref reader, typeof(string), options)
                    ?? throw new JsonException("Unexpected null value", ex);
                return new NonExhaustiveEnum<T>(value);
            }
        }

        public override void Write(Utf8JsonWriter writer, NonExhaustiveEnum<T> value, JsonSerializerOptions options)
        {
            if (value.IsWellKnown)
            {
                _inner.Write(writer, value._value, options);
            }
            else
            {
                _string.Write(writer, value._string, options);
            }
        }

        public override void WriteAsPropertyName(Utf8JsonWriter writer, [DisallowNull] NonExhaustiveEnum<T> value, JsonSerializerOptions options)
        {
            if (value.IsWellKnown)
            {
                _inner.WriteAsPropertyName(writer, value._value, options);
            }
            else
            {
                _string.WriteAsPropertyName(writer, value._string, options);
            }
        }
    }
}
