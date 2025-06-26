using Altinn.Authorization.ModelUtils.FieldValueRecords.Polymorphic;
using CommunityToolkit.Diagnostics;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Altinn.Authorization.ModelUtils.FieldValueRecords.Converters;

internal sealed class PolymorphicLeafFieldValueRecordConverter<T, TDiscriminator>
    : JsonConverter<T>
    , IPolymorphicFieldValueRecordJsonConverter
    , IGenericJsonConverter
    where T : class
{
    private readonly IPolymorphicFieldValueRecordModel _model;
    private readonly FieldValueRecordBaseConverter<T> _inner;

    public PolymorphicLeafFieldValueRecordConverter(
        IPolymorphicFieldValueRecordModel model,
        FieldValueRecordBaseConverter<T> inner)
    {
        Debug.Assert(model.Descendants.Length == 0);

        _model = model;
        _inner = inner;
    }

    /// <inheritdoc/>
    public IPolymorphicFieldValueRecordModel Model => _model;

    /// <inheritdoc/>
    string IPolymorphicFieldValueRecordJsonConverter.DiscriminatorPropertyName 
        => ThrowHelper.ThrowNotSupportedException<string>();

    /// <inheritdoc/>
    bool IPolymorphicFieldValueRecordJsonConverter.TryFindPropertyModel(string name, [NotNullWhen(true)] out IFieldValueRecordPropertyModel? model)
        => ((IFieldValueRecordJsonConverter)_inner).TryFindPropertyModel(name, out model);

    /// <inheritdoc/>
    bool IPolymorphicFieldValueRecordJsonConverter.IsDiscriminatorProperty(IFieldValueRecordPropertyModel model)
        => model.MemberInfo == _model.DiscriminatorProperty.MemberInfo;

    public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => _inner.Read(ref reader, typeToConvert, options);

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        => _inner.Write(writer, value, options);

    /// <inheritdoc/>
    void IGenericJsonConverter.WriteGeneric<T1>(Utf8JsonWriter writer, T1 value, JsonSerializerOptions options)
        where T1 : class
    {
        Debug.Assert(value.GetType().IsAssignableTo(typeof(T)));

        Write(writer, (T)(object)value, options);
    }

    /// <inheritdoc/>
    T1? IGenericJsonConverter.ReadGeneric<T1>(ref Utf8JsonReader reader, JsonSerializerOptions options)
        where T1 : class
    {
        Debug.Assert(typeof(T).IsAssignableTo(typeof(T1)));

        return (T1?)(object?)Read(ref reader, typeof(T1), options);
    }
}
