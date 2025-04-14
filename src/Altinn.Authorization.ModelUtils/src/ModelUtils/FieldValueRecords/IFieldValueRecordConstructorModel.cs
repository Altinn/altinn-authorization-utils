using System.Collections.Immutable;

namespace Altinn.Authorization.ModelUtils.FieldValueRecords;

/// <summary>
/// Represents a constructor of a field-value-record.
/// </summary>
public interface IFieldValueRecordConstructorModel
{
    /// <summary>
    /// Gets the parameters of the constructor.
    /// </summary>
    public ImmutableArray<IFieldValueRecordConstructorParameterModel> Parameters { get; }

    /// <summary>
    /// Invokes the constructor, creating a new instance of the field-value-record type.
    /// </summary>
    /// <param name="parameters">The list of parameters to call the constructor with.</param>
    /// <returns>The newly created field-value-record.</returns>
    public object Invoke(Span<object?> parameters);
}

/// <summary>
/// Represents a constructor of a field-value-record.
/// </summary>
/// <typeparam name="T">The field-value-record type.</typeparam>
public interface IFieldValueRecordConstructorModel<out T>
    : IFieldValueRecordConstructorModel
    where T : notnull
{
    /// <inheritdoc cref="IFieldValueRecordConstructorModel.Invoke(Span{object?})"/>
    public new T Invoke(Span<object?> parameters);

    object IFieldValueRecordConstructorModel.Invoke(Span<object?> parameters)
        => Invoke(parameters);
}
