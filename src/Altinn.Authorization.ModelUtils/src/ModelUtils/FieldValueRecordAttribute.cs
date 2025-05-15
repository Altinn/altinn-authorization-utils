using Altinn.Authorization.ModelUtils.FieldValueRecords;
using Altinn.Authorization.ModelUtils.FieldValueRecords.Converters;
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
    private static readonly ConcurrentDictionary<IFieldValueRecordModel, JsonConverterFactory> _cache = new();
    private static readonly Func<IFieldValueRecordModel, JsonConverterFactory> _createFactory = CreateFactory;

    /// <summary>
    /// Gets a <see cref="JsonConverter"/> for the specified model.
    /// </summary>
    /// <param name="model">The model to get a json converter for.</param>
    /// <returns>A <see cref="JsonConverter"/> for the given model.</returns>
    internal static JsonConverterFactory GetConverterFactoryForModel(IFieldValueRecordModel model)
        => _cache.GetOrAdd(model, _createFactory);

    /// <summary>
    /// Gets a <see cref="JsonConverter"/> for the specified model.
    /// </summary>
    /// <param name="model">The model to get a json converter for.</param>
    /// <param name="options">The <see cref="JsonSerializerOptions"/>.</param>
    /// <returns>A <see cref="JsonConverter"/> for the given model.</returns>
    internal static FieldValueRecordBaseConverter<T> GetConverterForModel<T>(IFieldValueRecordModel<T> model, JsonSerializerOptions options)
        where T : class
        => (FieldValueRecordBaseConverter<T>)_cache.GetOrAdd(model, _createFactory).CreateConverter(model.Type, options)!;

    /// <inheritdoc/>
    public override JsonConverter? CreateConverter(Type typeToConvert)
    {
        var model = FieldValueRecordModel.For(typeToConvert);
        Debug.Assert(model.Type == typeToConvert);

        return GetConverterFactoryForModel(model);
    }

    private static JsonConverterFactory CreateFactory(IFieldValueRecordModel model)
    {
        var factoryType = typeof(Factory<>).MakeGenericType(model.Type);
        var factory = Activator.CreateInstance(factoryType, model);
        return (JsonConverterFactory)factory!;
    }

    private sealed class Factory<T>
        : JsonConverterFactory
        where T : class
    {
        private readonly IFieldValueRecordModel<T> _model;

        public Factory(IFieldValueRecordModel<T> model)
        {
            _model = model;
        }

        public override bool CanConvert(Type typeToConvert)
            => typeToConvert == typeof(T);

        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            Debug.Assert(typeToConvert == typeof(T));

            return _model.Constructor.Parameters.Length switch
            {
                0 => new FieldValueRecordWithEmptyConstructorConverter<T>(_model, options),
                _ => new FieldValueRecordWithParameterizedConstructorConverter<T>(_model, options),
            };
        }
    }
}
