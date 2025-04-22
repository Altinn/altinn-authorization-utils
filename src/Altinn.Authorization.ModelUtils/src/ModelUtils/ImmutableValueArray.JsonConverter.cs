using System.Collections.Immutable;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Altinn.Authorization.ModelUtils;

public static partial class ImmutableValueArray
{
    [AttributeUsage(AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
    internal sealed class ImmutableValueArrayJsonConverterAttribute
        : JsonConverterAttribute
    {
        public override JsonConverter? CreateConverter(Type typeToConvert)
        {
            Debug.Assert(typeToConvert.IsConstructedGenericType && typeToConvert.GetGenericTypeDefinition() == typeof(ImmutableValueArray<>));
            var itemType = typeToConvert.GenericTypeArguments[0];
            var converterType = typeof(ImmutableValueArray<>.JsonConverter).MakeGenericType(itemType);
            var converter = (JsonConverter?)Activator.CreateInstance(converterType);

            return converter;
        }
    }
}

[ImmutableValueArray.ImmutableValueArrayJsonConverter]
public readonly partial struct ImmutableValueArray<T> 
{
    internal sealed class JsonConverter
        : JsonConverter<ImmutableValueArray<T>>
    {
        public override ImmutableValueArray<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var inner = JsonSerializer.Deserialize<ImmutableArray<T>>(ref reader, options);
            return new(inner);
        }

        public override void Write(Utf8JsonWriter writer, ImmutableValueArray<T> value, JsonSerializerOptions options)
        {
            JsonSerializer.Serialize(writer, value._inner, options);
        }
    }
}
