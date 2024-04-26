using Altinn.Urn.Visit;
using System.Buffers;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Altinn.Urn.Json;

internal sealed class DictionaryUrnJsonConverter
    : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert)
    {
        if (!typeToConvert.IsConstructedGenericType)
        {
            return false;
        }

        var genericTypeDefinition = typeToConvert.GetGenericTypeDefinition();
        if (genericTypeDefinition != typeof(KeyValueUrnDictionary<,>))
        {
            return false;
        }

        var urnType = typeToConvert.GetGenericArguments()[0];
        if (!urnType.IsAssignableTo(typeof(IVisitableKeyValueUrn)))
        {
            return false;
        }

        return true;
    }

    public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var genericArguments = typeToConvert.GetGenericArguments();
        var converterType = typeof(DictionaryUrnJsonConverter<,>).MakeGenericType(genericArguments);
        return (JsonConverter)Activator.CreateInstance(converterType)!;
    }
}

internal sealed class DictionaryUrnJsonConverter<TUrn, TVariants>
    : JsonConverter<KeyValueUrnDictionary<TUrn, TVariants>>
    where TUrn : IKeyValueUrn<TUrn, TVariants>, IVisitableKeyValueUrn
    where TVariants : struct, Enum
{
    private static Dictionary<TVariants, VariantValueJsonConverter> _variantConverters
        = CreateVariantConverters();

    static DictionaryUrnJsonConverter()
    {
        Debug.Assert(Enum.GetUnderlyingType(typeof(TVariants)) == typeof(int));
    }

    public override KeyValueUrnDictionary<TUrn, TVariants>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            return null;
        }

        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException($"Expected object, but got {reader.TokenType}");
        }

        var dict = new KeyValueUrnDictionary<TUrn, TVariants>();

        // keep track of which variants we have seen a canonical value for
        var canonical = ArrayPool<bool>.Shared.Rent(TUrn.Variants.Length + 1);

        try
        {
            Array.Clear(canonical);

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                {
                    return dict;
                }

                if (reader.TokenType != JsonTokenType.PropertyName)
                {
                    throw new JsonException($"Expected property name, but got {reader.TokenType}");
                }

                var propertyName = reader.GetString();
                if (propertyName is null)
                {
                    throw new JsonException("Property name is null");
                }

                if (!reader.Read())
                {
                    throw new JsonException("Unexpected end of input");
                }

                if (!TUrn.TryGetVariant(propertyName, out var variant))
                {
                    throw new JsonException($"Unknown variant '{propertyName}' for URN {typeof(TUrn).Name}");
                }

                ref var seenCanonical = ref canonical[(int)(object)variant];
                var isCanonical = string.Equals(propertyName, TUrn.CanonicalPrefixFor(variant), StringComparison.Ordinal);

                // Skip non-canonical values if we have already seen a canonical value
                if (!isCanonical && seenCanonical)
                {
                    reader.Skip();
                    continue;
                }

                var converter = _variantConverters[variant];
                var urn = converter.Read(ref reader, typeof(TUrn), options)
                    ?? throw new JsonException($"Failed to read URN for variant {variant}");

                if (isCanonical)
                {
                    seenCanonical = true;
                }

                dict.Add(urn, overwrite: true);
            }
        }
        finally
        {
            ArrayPool<bool>.Shared.Return(canonical);
        }

        return dict;
    }

    public override void Write(Utf8JsonWriter writer, KeyValueUrnDictionary<TUrn, TVariants> value, JsonSerializerOptions options)
    {
        if (value is null)
        {
            writer.WriteNullValue();
            return;
        }

        writer.WriteStartObject();
        foreach (var urn in value)
        {
            var valueConverter = _variantConverters[urn.UrnType];
            foreach (var prefix in valueConverter.Prefixes)
            {
                writer.WritePropertyName(prefix);
                valueConverter.Write(writer, urn, options);
            }
        }
        writer.WriteEndObject();
    }

    private static Dictionary<TVariants, VariantValueJsonConverter> CreateVariantConverters()
    {
        var dict = new Dictionary<TVariants, VariantValueJsonConverter>();
        foreach (var variant in TUrn.Variants)
        {
            var type = TUrn.VariantTypeFor(variant);
            var iface = type.GetInterfaces().SingleOrDefault(i => i.IsConstructedGenericType && i.GetGenericTypeDefinition() == typeof(IKeyValueUrnVariant<,,,>))
                ?? throw new InvalidOperationException($"Type {type} does not implement IKeyValueUrnVariant");

            var args = iface.GetGenericArguments();
            var variantType = args[0];
            var urnType = args[1];
            var variantsEnumType = args[2];
            var valueType = args[3];

            Debug.Assert(variantType == type);
            Debug.Assert(urnType == typeof(TUrn));
            Debug.Assert(variantsEnumType == typeof(TVariants));

            var converterType = typeof(VariantValueJsonConverter<,>).MakeGenericType(urnType, variantsEnumType, variantType, valueType);
            var converter = (VariantValueJsonConverter?)Activator.CreateInstance(converterType)
                ?? throw new InvalidOperationException($"Failed to create converter for {type}");

            dict.Add(variant, converter);
        }

        return dict;
    }

    internal abstract class VariantValueJsonConverter
        : JsonConverter<TUrn>
    {
        public ImmutableArray<JsonEncodedText> Prefixes { get; }

        protected VariantValueJsonConverter(ImmutableArray<JsonEncodedText> prefixes)
        {
            Prefixes = prefixes;
        }

        protected static ImmutableArray<JsonEncodedText> EncodePrefixes(ReadOnlySpan<string> prefixes)
        {
            var builder = ImmutableArray.CreateBuilder<JsonEncodedText>(prefixes.Length);
            foreach (var prefix in prefixes)
            {
                builder.Add(JsonEncodedText.Encode(prefix));
            }

            return builder.MoveToImmutable();
        }
    }

    internal sealed class VariantValueJsonConverter<TVariant, TValue>
        : VariantValueJsonConverter
        where TVariant : TUrn, IKeyValueUrnVariant<TVariant, TUrn, TVariants, TValue>
    {
        public VariantValueJsonConverter()
            : base(EncodePrefixes(TVariant.Prefixes))
        {
        }

        public override TUrn? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var value = JsonSerializer.Deserialize<TValue>(ref reader, options);
            if (value is null)
            {
                throw new JsonException("URN value is null");
            }

            return TVariant.Create(value);
        }

        public override void Write(Utf8JsonWriter writer, TUrn value, JsonSerializerOptions options)
        {
            var typed = (TVariant)value;
            JsonSerializer.Serialize(writer, typed.Value, options);
        }
    }
}
