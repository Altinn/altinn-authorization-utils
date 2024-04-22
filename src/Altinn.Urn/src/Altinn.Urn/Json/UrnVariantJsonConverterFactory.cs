using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Altinn.Urn.Json;

public sealed class UrnVariantJsonConverterFactory<TUrn, TVariants>
    : JsonConverterFactory
    where TUrn : IKeyValueUrn<TUrn, TVariants>
    where TVariants : struct, Enum
{
    //private static readonly UrnJsonConverter<TUrn> _baseConverter = new();
    private static readonly Dictionary<Type, JsonConverter> _converters
        = CreateConverters();

    public override bool CanConvert(Type typeToConvert)
        => _converters.ContainsKey(typeToConvert);

    public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        if (_converters.TryGetValue(typeToConvert, out var converter))
        {
            return converter;
        }

        return null;
    }

    static Dictionary<Type, JsonConverter> CreateConverters()
    {
        var dict = new Dictionary<Type, JsonConverter>();

        dict.Add(typeof(TUrn), new UrnJsonConverter<TUrn>());
        foreach (var variant in TUrn.Variants)
        {
            var variantType = TUrn.VariantTypeFor(variant);
            var valueType = TUrn.ValueTypeFor(variant);

            var converterType = typeof(VariantConverter<,>).MakeGenericType(typeof(TUrn), typeof(TVariants), variantType, valueType);
            var converter = (JsonConverter)Activator.CreateInstance(converterType)!;
            dict.Add(variantType, converter);
        }

        return dict;
    }

    private class VariantConverter<TVariant, TValue>
        : JsonConverter<TVariant>
        where TVariant : TUrn, IKeyValueUrnVariant<TVariant, TUrn, TVariants, TValue>
    {
        private static readonly string Name = $"{typeof(TUrn).Name}.{typeof(TVariant).Name}";
        private static readonly string InvalidUrn = $"Invalid {Name}";

        static VariantConverter()
        {
            Debug.Assert(Enum.GetUnderlyingType(typeof(TVariants)) == typeof(int));
        }

        public override TVariant? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            // We just try to parse it as the base urn and validate the the variant is correct for now
            var urn = UrnJsonConverter.ReadUrn<TUrn>(ref reader, Name, InvalidUrn);

            if (urn is null)
            {
                return default;
            }

            if (VariantValue(urn.UrnType) != VariantValue(TVariant.Variant))
            {
                throw new JsonException(InvalidUrn);
            }

            return (TVariant)urn;
        }

        public override void Write(Utf8JsonWriter writer, TVariant value, JsonSerializerOptions options)
        {
            UrnJsonConverter.WriteUrnString(writer, value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int VariantValue(TVariants variant)
        {
            return (int)(object)variant;
        }
    }
}
