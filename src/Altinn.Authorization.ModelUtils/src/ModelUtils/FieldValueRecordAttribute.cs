using Altinn.Authorization.ModelUtils.FieldValueRecords;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Altinn.Authorization.ModelUtils;

/// <summary>
/// Uses field-value-record semantics for JSON serialization.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class FieldValueRecordAttribute
    : JsonConverterAttribute
{
    private static readonly ConcurrentDictionary<Type, JsonConverter> _cache = new();

    /// <inheritdoc/>
    public override JsonConverter? CreateConverter(Type typeToConvert)
            => _cache.GetOrAdd(typeToConvert, CreateFactory);

    private static JsonConverter CreateFactory(Type typeToConvert)
    {
        var factoryType = typeof(Factory<>).MakeGenericType(typeToConvert);
        var factory = Activator.CreateInstance(factoryType);
        return (JsonConverter)factory!;
    }

    private sealed class Factory<T>
        : JsonConverterFactory
        where T : class
    {
        public override bool CanConvert(Type typeToConvert)
            => typeToConvert == typeof(T);

        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            Debug.Assert(typeToConvert == typeof(T));
            var model = FieldValueRecordModel.For<T>();

            return model.Constructor.Parameters.Length switch
            {
                0 => new FieldValueRecordConverter<T>(model, options),
                _ => new FieldValueRecordWithParameterizedConstructorConverter<T>(model, options),
            };
        }
    }
}
