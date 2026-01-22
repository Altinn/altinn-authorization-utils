using CommunityToolkit.Diagnostics;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Altinn.Authorization.ModelUtils;

/// <summary>
/// Static utility class for <see cref="NonExhaustive{T}"/>.
/// </summary>
public static class NonExhaustive
{
    /// <summary>
    /// Creates a new <see cref="NonExhaustive{T}"/> from the specified value.
    /// </summary>
    /// <remarks>
    /// Should not be used for enums. Use <see cref="NonExhaustiveEnum.Create{T}(T)"/> instead.
    /// </remarks>
    /// <typeparam name="T">The inner type.</typeparam>
    /// <param name="value">The value.</param>
    /// <returns>A <see cref="NonExhaustive{T}"/> wrapper for <paramref name="value"/>.</returns>
    public static NonExhaustive<T> Create<T>(T value)
        where T : notnull
        => value;

    /// <summary>
    /// Checks if a type is a <see cref="NonExhaustive{T}"/> and returns the field type.
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <param name="fieldType">The inner field type, if <paramref name="type"/> is a constructed <see cref="NonExhaustive{T}"/> type.</param>
    /// <returns><see langword="true"/> if <paramref name="type"/> is a constructed <see cref="NonExhaustive{T}"/> type, otherwise <see langword="false"/>.</returns>
    public static bool IsNonExhaustiveType(Type type, [NotNullWhen(true)] out Type? fieldType)
    {
        if (type.IsConstructedGenericType && type.GetGenericTypeDefinition() == typeof(NonExhaustive<>))
        {
            fieldType = type.GetGenericArguments()[0];
            return true;
        }

        fieldType = null;
        return false;
    }

    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    internal sealed class ConverterAttribute
        : JsonConverterAttribute
    {
        private static readonly ConcurrentDictionary<Type, JsonConverter> _cache = new();

        public override JsonConverter? CreateConverter(Type typeToConvert)
            => _cache.GetOrAdd(typeToConvert, CreateFactory);

        private static JsonConverter CreateFactory(Type typeToConvert)
        {
            Debug.Assert(typeToConvert.IsGenericType && typeToConvert.GetGenericTypeDefinition() == typeof(NonExhaustive<>));
            var inner = typeToConvert.GetGenericArguments()[0];

            var factoryType = typeof(Factory<>).MakeGenericType(inner);
            var factory = Activator.CreateInstance(factoryType);
            return (JsonConverter)factory!;
        }

        private sealed class Factory<T>
            : JsonConverterFactory
            where T : notnull
        {
            // This factory is only created for types we can convert
            public override bool CanConvert(Type typeToConvert)
            {
                Debug.Assert(typeToConvert == typeof(NonExhaustive<T>));
                return true;
            }

            public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
            {
                Debug.Assert(typeToConvert == typeof(NonExhaustive<T>));
                return new NonExhaustive<T>.Converter(options);
            }
        }
    }
}

/// <summary>
/// Represents a serialize-to-string value that can have unknown values.
/// </summary>
/// <remarks>
/// Should not be used for enums. Use <see cref="NonExhaustiveEnum{T}"/> instead.
/// </remarks>
/// <typeparam name="T">The inner type.</typeparam>
[DebuggerDisplay("{DebuggerDisplay,nq}")]
[NonExhaustive.Converter]
public sealed class NonExhaustive<T>
    : IEquatable<NonExhaustive<T>>
    , IEquatable<T>
    where T : notnull
{
    /// <summary>
    /// Creates a new <see cref="NonExhaustive{T}"/> representing an unknown value.
    /// </summary>
    /// <param name="value">The unknown value.</param>
    /// <returns>A <see cref="NonExhaustive{T}"/> in the unknown state.</returns>
    /// <remarks>No validation is done to make sure the value is actually unknown.</remarks>
    internal static NonExhaustive<T> Unknown(string value)
        => new(value);

    private readonly T? _value;
    private readonly string? _string;

    private NonExhaustive(T value)
    {
        Guard.IsNotNull(value);

        _value = value;
        _string = null;
    }

    private NonExhaustive(string value)
    {
        _value = default;
        _string = value;
    }

    /// <summary>
    /// Gets a value indicating whether the value is an unknown value.
    /// </summary>
    [MemberNotNullWhen(true, nameof(_string))]
    [MemberNotNullWhen(false, nameof(_value))]
    public bool IsUnknown
        => _string is not null;

    /// <summary>
    /// Gets a value indicating whether the value is a well-known value.
    /// </summary>
    [MemberNotNullWhen(false, nameof(_string))]
    [MemberNotNullWhen(true, nameof(_value))]
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
    public bool TryGetValue([MaybeNullWhen(false)] out T value)
    {
        value = _value;
        return IsWellKnown;
    }

    /// <inheritdoc/>
    public override int GetHashCode()
        => _string is not null
        ? _string.GetHashCode()
        : _value!.GetHashCode();

    /// <inheritdoc/>
    public override string? ToString()
        => _string is not null
        ? _string
        : _value!.ToString();

    /// <inheritdoc/>
    public override bool Equals([NotNullWhen(true)] object? obj)
        => obj switch
        {
            NonExhaustive<T> other => Equals(other),
            T other => Equals(other),
            string other => Equals(other),
            _ => false,
        };

    /// <inheritdoc/>
    public bool Equals([NotNullWhen(true)] NonExhaustive<T>? other)
        => other is not null
        && string.Equals(other._string, _string, StringComparison.Ordinal)
        && ValueEquals(_value, other._value);

    /// <inheritdoc/>
    public bool Equals([NotNullWhen(true)] T? other)
        => _string is null
        && ValueEquals(_value, other);

    /// <inheritdoc cref="Equals(T)"/>
    public bool Equals([NotNullWhen(true)] string? other)
        => _string is not null
        && string.Equals(other, _string, StringComparison.Ordinal);

    private static bool ValueEquals(T? left, T? right)
        => EqualityComparer<T>.Default.Equals(left, right);

    private string? DebuggerDisplay
        => _string is not null
        ? $"\"{_string}\""
        : _value!.ToString();

    /// <summary>
    /// Implicitly converts a <typeparamref name="T"/> to a <see cref="NonExhaustive{T}"/>.
    /// </summary>
    /// <param name="value">The <typeparamref name="T"/>.</param>
    public static implicit operator NonExhaustive<T>(T value)
        => new(value);

    /// <summary>
    /// Explicitly converts a <see cref="NonExhaustive{T}"/> to a <typeparamref name="T"/>.
    /// </summary>
    /// <param name="value">The <see cref="NonExhaustive{T}"/>.</param>
    /// <exception cref="InvalidCastException">
    /// If the value is unknown.
    /// </exception>
    public static explicit operator T(NonExhaustive<T> value)
    {
        if (value.IsUnknown)
        {
            InvalidCast();
        }

        return value._value;
    }

    /// <summary>
    /// Explicitly converts a <see cref="NonExhaustive{T}"/> to a <see langword="string"/>.
    /// </summary>
    /// <param name="value">The <see cref="NonExhaustive{T}"/>.</param>
    /// <exception cref="InvalidCastException">
    /// If the value is well-known.
    /// </exception>
    public static explicit operator string(NonExhaustive<T> value)
    {
        if (value.IsWellKnown)
        {
            InvalidCast();
        }

        return value._string;
    }

    /// <inheritdoc cref="System.Numerics.IEqualityOperators{TSelf, TOther, TResult}.op_Equality(TSelf,TOther)"/>"/>
    public static bool operator ==(NonExhaustive<T> left, T right)
        => left.Equals(right);

    /// <inheritdoc cref="System.Numerics.IEqualityOperators{TSelf, TOther, TResult}.op_Inequality(TSelf,TOther)"/>"/>
    public static bool operator !=(NonExhaustive<T> left, T right)
        => !left.Equals(right);

    /// <inheritdoc cref="System.Numerics.IEqualityOperators{TSelf, TOther, TResult}.op_Equality(TSelf,TOther)"/>"/>
    public static bool operator ==(T left, NonExhaustive<T> right)
        => right.Equals(left);

    /// <inheritdoc cref="System.Numerics.IEqualityOperators{TSelf, TOther, TResult}.op_Inequality(TSelf,TOther)"/>"/>
    public static bool operator !=(T left, NonExhaustive<T> right)
        => !right.Equals(left);

    /// <inheritdoc cref="System.Numerics.IEqualityOperators{TSelf, TOther, TResult}.op_Equality(TSelf,TOther)"/>"/>
    public static bool operator ==(string? left, NonExhaustive<T> right)
        => right.Equals(left);

    /// <inheritdoc cref="System.Numerics.IEqualityOperators{TSelf, TOther, TResult}.op_Inequality(TSelf,TOther)"/>"/>
    public static bool operator !=(string? left, NonExhaustive<T> right)
        => !right.Equals(left);

    /// <inheritdoc cref="System.Numerics.IEqualityOperators{TSelf, TOther, TResult}.op_Equality(TSelf,TOther)"/>"/>
    public static bool operator ==(NonExhaustive<T> left, string? right)
        => left.Equals(right);

    /// <inheritdoc cref="System.Numerics.IEqualityOperators{TSelf, TOther, TResult}.op_Inequality(TSelf,TOther)"/>"/>
    public static bool operator !=(NonExhaustive<T> left, string? right)
        => !left.Equals(right);

    /// <inheritdoc cref="System.Numerics.IEqualityOperators{TSelf, TOther, TResult}.op_Equality(TSelf,TOther)"/>"/>
    public static bool operator ==(NonExhaustive<T> left, NonExhaustive<T> right)
        => left.Equals(right);

    /// <inheritdoc cref="System.Numerics.IEqualityOperators{TSelf, TOther, TResult}.op_Inequality(TSelf,TOther)"/>"/>
    public static bool operator !=(NonExhaustive<T> left, NonExhaustive<T> right)
        => !left.Equals(right);

    [DoesNotReturn]
    private static void InvalidCast()
    {
        throw new InvalidCastException("Cannot cast unknown value to well-known value");
    }

    internal sealed class Converter
        : JsonConverter<NonExhaustive<T>>
    {
        private readonly JsonConverter<T> _inner;
        private readonly JsonConverter<string> _string;

        public Converter(JsonSerializerOptions options)
        {
            _inner = (JsonConverter<T>)options.GetConverter(typeof(T));
            _string = (JsonConverter<string>)options.GetConverter(typeof(string));
        }

        public override NonExhaustive<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType is not JsonTokenType.String and not JsonTokenType.PropertyName)
            {
                reader.GetString(); // throws
            }

            try
            {
                var value = _inner.Read(ref reader, typeof(T), options);

                // Null is already handled by the outer json machinery, as this converter does not override HandleNull
                Debug.Assert(value is not null);

                return value;
            }
            catch (JsonException ex) when (reader.TokenType == JsonTokenType.String || reader.TokenType == JsonTokenType.PropertyName)
            {
                var value = _string.Read(ref reader, typeof(string), options)
                    ?? throw new JsonException("Unexpected null value", ex);

                return new NonExhaustive<T>(value);
            }
        }

        public override NonExhaustive<T> ReadAsPropertyName(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType is not JsonTokenType.String and not JsonTokenType.PropertyName)
            {
                reader.GetString(); // throws
            }

            try
            {
                return _inner.ReadAsPropertyName(ref reader, typeof(T), options);
            }
            catch (JsonException ex) when (reader.TokenType == JsonTokenType.String || reader.TokenType == JsonTokenType.PropertyName)
            {
                var value = _string.ReadAsPropertyName(ref reader, typeof(string), options)
                    ?? throw new JsonException("Unexpected null value", ex);
                return new NonExhaustive<T>(value);
            }
        }

        public override void Write(Utf8JsonWriter writer, NonExhaustive<T> value, JsonSerializerOptions options)
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

        public override void WriteAsPropertyName(Utf8JsonWriter writer, [DisallowNull] NonExhaustive<T> value, JsonSerializerOptions options)
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
