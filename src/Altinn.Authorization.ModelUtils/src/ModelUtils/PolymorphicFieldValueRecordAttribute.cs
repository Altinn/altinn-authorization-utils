using Altinn.Authorization.ModelUtils.FieldValueRecords;
using Altinn.Authorization.ModelUtils.FieldValueRecords.Converters;
using Altinn.Authorization.ModelUtils.FieldValueRecords.Polymorphic;
using CommunityToolkit.Diagnostics;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Altinn.Authorization.ModelUtils;

/// <summary>
/// Uses polymorphic field-value-record semantics for JSON serialization.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class PolymorphicFieldValueRecordAttribute
    : JsonConverterAttribute
{
    private static readonly ConcurrentDictionary<IPolymorphicFieldValueRecordModel, JsonConverterFactory> _cache = new();
    private static readonly Func<IPolymorphicFieldValueRecordModel, JsonConverterFactory> _createFactory = CreateFactory;

    /// <summary>
    /// Gets a <see cref="JsonConverter"/> for the specified model.
    /// </summary>
    /// <param name="model">The model to get a json converter for.</param>
    /// <returns>A <see cref="JsonConverter"/> for the given model.</returns>
    internal static JsonConverterFactory GetConverterFactoryForModel(IPolymorphicFieldValueRecordModel model)
        => _cache.GetOrAdd(model, _createFactory);

    /// <summary>
    /// Gets whether the record is the root of the polymorphic hierarchy.
    /// </summary>
    public bool IsRoot { get; init; } = false;

    /// <inheritdoc/>
    public override JsonConverter? CreateConverter(Type typeToConvert)
    {
        var model = PolymorphicFieldValueRecordModel.For(typeToConvert);
        Debug.Assert(model.Type == typeToConvert);

        return GetConverterFactoryForModel(model);
    }

    private static JsonConverterFactory CreateFactory(IPolymorphicFieldValueRecordModel model)
    {
        var factoryType = typeof(Factory<,>).MakeGenericType(model.Type, model.DiscriminatorType);
        var factory = Activator.CreateInstance(factoryType, model);
        return (JsonConverterFactory)factory!;
    }

    private sealed class Factory<T, TDiscriminator>
        : JsonConverterFactory
        where T : class
        where TDiscriminator : struct, Enum
    {
        private readonly IPolymorphicFieldValueRecordModel<TDiscriminator, T> _model;

        public Factory(IPolymorphicFieldValueRecordModel<TDiscriminator, T> model)
        {
            _model = model;
        }

        public override bool CanConvert(Type typeToConvert)
            => typeToConvert == typeof(T);

        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            Debug.Assert(typeToConvert == typeof(T));

            var inner = FieldValueRecordAttribute.GetConverterForModel(_model, options);
            return _model.Descendants.Length switch
            {
                0 => inner,
                _ => new PolymorphicFieldValueRecordConverter<T, TDiscriminator>(_model, inner, options),
            };
        }
    }
}
