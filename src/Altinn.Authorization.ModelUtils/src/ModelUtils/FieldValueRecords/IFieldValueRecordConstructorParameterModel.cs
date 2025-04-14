namespace Altinn.Authorization.ModelUtils.FieldValueRecords;

/// <summary>
/// Represents a model for a constructor parameter of a field-value-record.
/// </summary>
public interface IFieldValueRecordConstructorParameterModel
{
    /// <summary>
    /// Gets the name of the constructor parameter.
    /// </summary>
    public string? Name { get; }

    /// <summary>
    /// Gets the type of the constructor parameter.
    /// </summary>
    public Type Type { get; }

    /// <summary>
    /// Gets the default value of the constructor parameter.
    /// </summary>
    public FieldValue<object> DefaultValue { get; }
}

/// <summary>
/// Represents a model for a constructor parameter of a field-value-record.
/// </summary>
/// <typeparam name="TOwner">The field-value-record type.</typeparam>
public interface IFieldValueRecordConstructorParameterModel<in TOwner>
    : IFieldValueRecordConstructorParameterModel
    where TOwner : class
{
}

/// <summary>
/// Represents a model for a constructor parameter of a field-value-record.
/// </summary>
/// <typeparam name="TOwner">The field-value-record type.</typeparam>
/// <typeparam name="TValue">The value type.</typeparam>
public interface IFieldValueRecordConstructorParameterModel<in TOwner, TValue>
    : IFieldValueRecordConstructorParameterModel<TOwner>
    where TOwner : class
    where TValue : notnull
{
    /// <summary>
    /// Gets the default value of the constructor parameter.
    /// </summary>
    public new FieldValue<TValue> DefaultValue { get; }

    FieldValue<object> IFieldValueRecordConstructorParameterModel.DefaultValue
        => DefaultValue.Select(static v => (object)v);
}
