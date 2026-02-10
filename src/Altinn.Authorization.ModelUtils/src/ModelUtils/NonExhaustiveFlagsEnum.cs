using Altinn.Authorization.ModelUtils.EnumUtils;
using CommunityToolkit.Diagnostics;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Altinn.Authorization.ModelUtils;

/// <summary>
/// Static utility class for <see cref="NonExhaustiveFlagsEnum{T}"/>.
/// </summary>
public static class NonExhaustiveFlagsEnum
{
    /// <summary>
    /// Creates a new <see cref="NonExhaustiveFlagsEnum{T}"/> from the specified value.
    /// </summary>
    /// <typeparam name="T">The enum type.</typeparam>
    /// <param name="value">The value.</param>
    /// <returns>A <see cref="NonExhaustiveFlagsEnum{T}"/> wrapper for <paramref name="value"/>.</returns>
    public static NonExhaustiveFlagsEnum<T> Create<T>(T value)
        where T : struct, Enum
        => value;

    /// <summary>
    /// Checks if a type is a <see cref="NonExhaustiveFlagsEnum{T}"/> and returns the field type.
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <param name="fieldType">The inner field type, if <paramref name="type"/> is a constructed <see cref="NonExhaustiveFlagsEnum{T}"/> type.</param>
    /// <returns><see langword="true"/> if <paramref name="type"/> is a constructed <see cref="NonExhaustiveFlagsEnum{T}"/> type, otherwise <see langword="false"/>.</returns>
    public static bool IsNonExhaustiveFlagsEnumType(Type type, [NotNullWhen(true)] out Type? fieldType)
    {
        if (type.IsConstructedGenericType && type.GetGenericTypeDefinition() == typeof(NonExhaustiveFlagsEnum<>))
        {
            fieldType = type.GetGenericArguments()[0];
            return true;
        }

        fieldType = null;
        return false;
    }

    [AttributeUsage(AttributeTargets.Struct, Inherited = false, AllowMultiple = false)]
    internal sealed class ConverterAttribute
        : JsonConverterAttribute
    {
        private static readonly ConcurrentDictionary<Type, JsonConverter> _cache = new();

        public override JsonConverter? CreateConverter(Type typeToConvert)
            => _cache.GetOrAdd(typeToConvert, CreateFactory);

        private static JsonConverter CreateFactory(Type typeToConvert)
        {
            Debug.Assert(typeToConvert.IsGenericType && typeToConvert.GetGenericTypeDefinition() == typeof(NonExhaustiveFlagsEnum<>));
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
                Debug.Assert(typeToConvert == typeof(NonExhaustiveFlagsEnum<T>));
                return true;
            }

            public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
            {
                Debug.Assert(typeToConvert == typeof(NonExhaustiveFlagsEnum<T>));
                return new NonExhaustiveFlagsEnum<T>.Converter(options, _converter);
            }
        }
    }
}

/// <summary>
/// Represents a flags-enum that can have unknown values.
/// </summary>
/// <typeparam name="T">The inner enum type.</typeparam>
[DebuggerDisplay("{DebuggerDisplay,nq}")]
[NonExhaustiveFlagsEnum.Converter]
public readonly struct NonExhaustiveFlagsEnum<T>
    : IEquatable<NonExhaustiveFlagsEnum<T>>
    , IEquatable<T>
    where T : struct, Enum
{
    private static readonly FlagsEnumModel<T> _model = FlagsEnumModel.Create<T>();

    private readonly T _value;
    private readonly ImmutableArray<string> _unknowns;

    private NonExhaustiveFlagsEnum(T value)
    {
        _value = value;
        _unknowns = default;
    }

    private NonExhaustiveFlagsEnum(T value, ImmutableArray<string> unknowns)
    {
        if (unknowns.IsDefaultOrEmpty)
        {
            ThrowHelper.ThrowArgumentException(nameof(unknowns), "Unknown values cannot be empty.");
        }

        _value = value;
        _unknowns = unknowns;
    }

    /// <summary>
    /// Gets a value indicating whether this non-exhaustive contains any values that are not recognized.
    /// </summary>
    public bool HasUnknownValues
        => !IsWellKnown;

    /// <summary>
    /// Gets a value indicating whether this non-exhaustive contains only well-known values.
    /// </summary>
    public bool IsWellKnown
        => _unknowns.IsDefault;

    /// <summary>
    /// Gets a value indicating whether this non-exhaustive contains no values (neither well-known nor unknown).
    /// </summary>
    public bool IsNone
        => IsWellKnown && _value.IsDefault();

    /// <summary>
    /// Gets the well-known value.
    /// </summary>
    /// <exception cref="InvalidCastException">
    /// If the value contains any unknown values, it cannot be cast to a well-known value.
    /// </exception>
    public T Value => (T)this;

    /// <summary>
    /// Gets only the well-known value components. If there are unknown values, this may not represent the full value of the enum and should be used with caution.
    /// </summary>
    public T PartialValue => _value;

    /// <summary>
    /// Gets the collection of values that were not recognized or mapped during parsing.
    /// </summary>
    public ImmutableArray<string> UnknownValues
        => _unknowns.IsDefault
        ? []
        : _unknowns;

    /// <summary>
    /// Attempts to retrieve the well-known value.
    /// </summary>
    /// <param name="value">When this method returns, contains the value of type <typeparamref name="T"/> if the value is well known.</param>
    /// <returns><see langword="true"/> if the current value is well-known, otherwise <see langword="false"/>.</returns>
    public bool TryGetValue(out T value) 
    {
        value = _value;
        return IsWellKnown;
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        if (IsWellKnown)
        {
            return _value.GetHashCode();
        }

        // Note: _unknowns is order-insensitive, so we cannot use it's items without (an expensive) sorting. Instead, for cheap hash code, we just use the count of unknowns.
        return HashCode.Combine(_value, _unknowns.Length);
    }

    /// <inheritdoc/>
    public override bool Equals([NotNullWhen(true)] object? obj)
        => obj switch
        {
            NonExhaustiveFlagsEnum<T> other => Equals(other),
            T other => Equals(other),
            IEnumerable<string> other => Equals(other),
            _ => false,
        };

    /// <inheritdoc/>
    public bool Equals(NonExhaustiveFlagsEnum<T> other)
    {
        if (!_value.Equals(other._value))
        {
            return false;
        }

        if (IsWellKnown != other.IsWellKnown)
        {
            return false;
        }

        if (IsWellKnown)
        {
            return true;
        }

        // Both have unknown values, compare them as sets (order-insensitive)
        return UnknownEquals(other.UnknownValues);
    }

    /// <inheritdoc/>
    public bool Equals(T other)
        => _value.Equals(other) && IsWellKnown;

    private bool Equals(IEnumerable<string> other)
    {
        if (IsWellKnown) 
        {
            return false;
        }

        return UnknownEquals(other);
    }

    private bool UnknownEquals(IEnumerable<string> other)
    {
        Debug.Assert(!_unknowns.IsDefault);

        // TODO: this could be optimized quire a bit by not constructing hashsets for every comparison, but it should be good enough for now as we don't expect many unknown values or comparisons.
        var selfValues = new HashSet<string>(_unknowns);
        var otherValues = new HashSet<string>(other);

        return selfValues.SetEquals(otherValues);
    }

    private string DebuggerDisplay
    {
        get
        {
            if (IsNone)
            {
                return "none";
            }

            if (IsWellKnown)
            {
                return _model.Format(_value);
            }

            Debug.Assert(!_unknowns.IsDefaultOrEmpty);
            var sb = new StringBuilder();
            if (!_value.IsDefault())
            {
                sb.Append(_model.Format(_value)).Append(',');
            }

            foreach (var unknown in _unknowns)
            {
                sb.Append(unknown).Append(',');
            }

            sb.Length--; // Remove trailing comma
            return sb.ToString();
        }
    }

    /// <summary>
    /// Implicitly converts a <typeparamref name="T"/> to a <see cref="NonExhaustiveEnum{T}"/>.
    /// </summary>
    /// <param name="value">The <typeparamref name="T"/>.</param>
    public static implicit operator NonExhaustiveFlagsEnum<T>(T value)
        => new(value);

    /// <summary>
    /// Explicitly converts a <see cref="NonExhaustiveEnum{T}"/> to a <typeparamref name="T"/>.
    /// </summary>
    /// <param name="value">The <see cref="NonExhaustiveEnum{T}"/>.</param>
    /// <exception cref="InvalidCastException">
    /// If the value is unknown.
    /// </exception>
    public static explicit operator T(NonExhaustiveFlagsEnum<T> value)
    {
        if (value.HasUnknownValues)
        {
            InvalidCast();
        }

        return value._value;
    }

    /// <inheritdoc cref="System.Numerics.IEqualityOperators{TSelf, TOther, TResult}.op_Equality(TSelf,TOther)"/>"/>
    public static bool operator ==(NonExhaustiveFlagsEnum<T> left, T right)
        => left.Equals(right);

    /// <inheritdoc cref="System.Numerics.IEqualityOperators{TSelf, TOther, TResult}.op_Inequality(TSelf,TOther)"/>"/>
    public static bool operator !=(NonExhaustiveFlagsEnum<T> left, T right)
        => !left.Equals(right);

    /// <inheritdoc cref="System.Numerics.IEqualityOperators{TSelf, TOther, TResult}.op_Equality(TSelf,TOther)"/>"/>
    public static bool operator ==(T left, NonExhaustiveFlagsEnum<T> right)
        => right.Equals(left);

    /// <inheritdoc cref="System.Numerics.IEqualityOperators{TSelf, TOther, TResult}.op_Inequality(TSelf,TOther)"/>"/>
    public static bool operator !=(T left, NonExhaustiveFlagsEnum<T> right)
        => !right.Equals(left);

    /// <inheritdoc cref="System.Numerics.IEqualityOperators{TSelf, TOther, TResult}.op_Equality(TSelf,TOther)"/>"/>
    public static bool operator ==(NonExhaustiveFlagsEnum<T> left, NonExhaustiveFlagsEnum<T> right)
        => left.Equals(right);

    /// <inheritdoc cref="System.Numerics.IEqualityOperators{TSelf, TOther, TResult}.op_Inequality(TSelf,TOther)"/>"/>
    public static bool operator !=(NonExhaustiveFlagsEnum<T> left, NonExhaustiveFlagsEnum<T> right)
        => !left.Equals(right);

    [DoesNotReturn]
    private static void InvalidCast()
    {
        throw new InvalidCastException("Cannot cast unknown value to well-known value");
    }

    internal sealed class Converter
        : JsonConverter<NonExhaustiveFlagsEnum<T>>
    {
        private readonly JsonConverter<T> _inner;
        private readonly JsonConverter<string> _string;

        public Converter(JsonSerializerOptions options, JsonStringEnumConverter inner)
        {
            _inner = (JsonConverter<T>)inner.CreateConverter(typeof(T), options);
            _string = (JsonConverter<string>)options.GetConverter(typeof(string));

#if DEBUG
            var enumValues = Enum.GetValues<T>();
            foreach (var value in enumValues)
            {
                using var jsonSerialized = JsonSerializer.SerializeToDocument(value, options);
                if (jsonSerialized.RootElement.ValueKind != JsonValueKind.String)
                {
                    ThrowHelper.ThrowInvalidOperationException($"Enum value '{value}' of '{typeof(T)}' is not serialized as a string, did you forget to decorate it with {nameof(StringEnumConverterAttribute)}?");
                }
            }
#endif
        }

        public override NonExhaustiveFlagsEnum<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartArray)
            {
                throw new JsonException($"Expected start of array, got {reader.TokenType}");
            }

            T known = default;
            ImmutableArray<string>.Builder? unknowns = null;

            if (!reader.Read())
            {
                throw new JsonException("Unexpected end of JSON");
            }

            while (reader.TokenType != JsonTokenType.EndArray)
            {
                try
                {
                    var value = _inner.Read(ref reader, typeof(T), options);
                    known = known.BitwiseOr(value);
                }
                catch (JsonException ex) when (reader.TokenType == JsonTokenType.String)
                {
                    var value = _string.Read(ref reader, typeof(string), options)
                        ?? throw new JsonException("Unexpected null value", ex);

                    unknowns ??= ImmutableArray.CreateBuilder<string>();
                    if (!unknowns.Contains(value))
                    {
                        unknowns.Add(value);
                    }
                }

                if (!reader.Read())
                {
                    throw new JsonException("Unexpected end of JSON");
                }
            }

            if (unknowns is null)
            {
                return new(known);
            }

            return new(known, unknowns.DrainToImmutable());
        }

        public override void Write(Utf8JsonWriter writer, NonExhaustiveFlagsEnum<T> value, JsonSerializerOptions options)
        {
            writer.WriteStartArray();
            
            foreach (var known in _model.GetComponents(value._value))
            {
                _inner.Write(writer, known, options);
            }

            if (value.HasUnknownValues)
            {
                foreach (var unknown in value.UnknownValues)
                {
                    _string.Write(writer, unknown, options);
                }
            }

            writer.WriteEndArray();
        }
    }
}
