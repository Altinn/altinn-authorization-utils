using System.Buffers;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Altinn.Urn.Sample.Api.Json;

[ExcludeFromCodeCoverage]
public sealed class StringParsableJsonConverter
    : JsonConverterFactory
{
    private static ImmutableDictionary<Type, JsonConverter> _converters
        = ImmutableDictionary<Type, JsonConverter>.Empty;

    private static bool IsGenericConstructedTypeOf(Type iface, Type genericDef)
    {
        if (!iface.IsConstructedGenericType)
        {
            return false;
        }

        var def = iface.GetGenericTypeDefinition();
        return def == genericDef;
    }

    private static bool IsStringCompatible(Type typeToConvert, Type[] interfaces)
    {
        if (!typeToConvert.IsAssignableTo(typeof(ISpanFormattable)))
        {
            return false;
        }

        return interfaces.Any(static i => IsGenericConstructedTypeOf(i, typeof(IParsable<>)));
    }

    private static bool IsCharSpanCompatible(Type typeToConvert, Type[] interfaces)
    {
        if (!typeToConvert.IsAssignableTo(typeof(ISpanFormattable)))
        {
            return false;
        }

        return interfaces.Any(static i => IsGenericConstructedTypeOf(i, typeof(ISpanParsable<>)));
    }

    private static bool IsUtf8SpanCompatible(Type typeToConvert, Type[] interfaces)
    {
        if (!typeToConvert.IsAssignableTo(typeof(ISpanFormattable)))
        {
            return false;
        }

        return interfaces.Any(static i => IsGenericConstructedTypeOf(i, typeof(IUtf8SpanParsable<>)));
    }

    public override bool CanConvert(Type typeToConvert)
        => IsStringCompatible(typeToConvert, typeToConvert.GetInterfaces());

    public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        => GetOrCreateConverter(typeToConvert);

    private static JsonConverter GetOrCreateConverter(Type typeToConvert)
    {
        return ImmutableInterlocked.GetOrAdd(ref _converters, typeToConvert, CreateConverter);

        static JsonConverter CreateConverter(Type typeToConvert)
        {
            var ifaces = typeToConvert.GetInterfaces();
            var isUtf8Compatible = IsUtf8SpanCompatible(typeToConvert, ifaces);
            var isCharCompatible = IsCharSpanCompatible(typeToConvert, ifaces);

            Type converterType;
            if (isUtf8Compatible && isCharCompatible)
            {
                converterType = typeof(Utf8SpanConverter<>).MakeGenericType(typeToConvert);
                return (JsonConverter)Activator.CreateInstance(converterType)!;
            }

            if (isCharCompatible)
            {
                converterType = typeof(CharSpanConverter<>).MakeGenericType(typeToConvert);
                return (JsonConverter)Activator.CreateInstance(converterType)!;
            }

            converterType = typeof(StringConverter<>).MakeGenericType(typeToConvert);
            return (JsonConverter)Activator.CreateInstance(converterType)!;
        }
    }

    private sealed class Utf8SpanConverter<T>
        : CharSpanConverter<T>
        where T : IUtf8SpanParsable<T>, IUtf8SpanFormattable, ISpanParsable<T>, ISpanFormattable
    {
        public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
            {
                return default;
            }

            if (reader.TokenType != JsonTokenType.String && reader.TokenType != JsonTokenType.PropertyName)
            {
                throw new JsonException($"Expected string, but found {reader.TokenType}");
            }

            if (reader.ValueIsEscaped)
            {
                // value is escaped, and the APIs to unescape are not public
                // so we drop down to string parsing instead
                return ReadAsString(ref reader);
            }

            byte[]? rented = null;
            ReadOnlySpan<byte> bytes;

            try
            {
                if (reader.HasValueSequence)
                {
                    var seq = reader.ValueSequence;
                    rented = ArrayPool<byte>.Shared.Rent(checked((int)seq.Length));
                    seq.CopyTo(rented);

                    bytes = rented.AsSpan(0, (int)seq.Length);
                }
                else
                {
                    bytes = reader.ValueSpan;
                }

                if (!T.TryParse(bytes, null, out var result))
                {
                    throw new JsonException($"Failed to parse value as {typeof(T).Name}");
                }

                return result;
            }
            finally
            {
                if (rented is not null)
                {
                    ArrayPool<byte>.Shared.Return(rented);
                }
            }
        }

        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
            byte[]? rented = null;

            try
            {
                Span<byte> bytes = stackalloc byte[64];
                int written;
                if (value.TryFormat(bytes, out written, format: default, provider: default))
                {
                    writer.WriteStringValue(bytes.Slice(0, written));
                    return;
                }

                // we failed to format, so we need to rent a larger array
                rented = ArrayPool<byte>.Shared.Rent(4096);
                bytes = rented;

                if (value.TryFormat(bytes, out written, format: default, provider: default))
                {
                    writer.WriteStringValue(bytes.Slice(0, written));
                    return;
                }

                // we failed to format even to a large buffer, go through string instead
                WriteAsString(writer, value);
            }
            finally
            {
                if (rented is not null)
                {
                    ArrayPool<byte>.Shared.Return(rented);
                }
            }
        }
    }

    private class CharSpanConverter<T>
        : StringConverter<T>
        where T : ISpanParsable<T>, ISpanFormattable
    {
        public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return ReadAsString(ref reader);
        }

        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
            char[]? rented = null;

            try
            {
                Span<char> bytes = stackalloc char[32];
                int written;
                if (value.TryFormat(bytes, out written, format: default, provider: default))
                {
                    writer.WriteStringValue(bytes.Slice(0, written));
                    return;
                }

                // we failed to format, so we need to rent a larger array
                rented = ArrayPool<char>.Shared.Rent(4096);
                bytes = rented;

                if (value.TryFormat(bytes, out written, format: default, provider: default))
                {
                    writer.WriteStringValue(bytes.Slice(0, written));
                    return;
                }

                // we failed to format even to a large buffer, go through string instead
                WriteAsString(writer, value);
            }
            finally
            {
                if (rented is not null)
                {
                    ArrayPool<char>.Shared.Return(rented);
                }
            }
        }
    }

    private class StringConverter<T>
        : JsonConverter<T>
        where T : IParsable<T>, IFormattable
    {
        public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return ReadAsString(ref reader);
        }

        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }

        protected static T? ReadAsString(ref Utf8JsonReader reader)
        {
            var s = reader.GetString();

            if (!T.TryParse(s, null, out var result))
            {
                throw new JsonException($"Failed to parse value as {typeof(T).Name}");
            }

            return result;
        }

        protected static void WriteAsString(Utf8JsonWriter writer, T value)
        {
            writer.WriteStringValue(value.ToString(format: default, formatProvider: default));
        }
    }
}
