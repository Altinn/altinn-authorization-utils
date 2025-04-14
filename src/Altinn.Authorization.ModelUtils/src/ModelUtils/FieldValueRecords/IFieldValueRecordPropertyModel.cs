using System.Reflection;

namespace Altinn.Authorization.ModelUtils.FieldValueRecords;

/// <summary>
/// Represents a property of a field-value-record.
/// </summary>
public interface IFieldValueRecordPropertyModel
{
    /// <summary>
    /// Gets the name of the property.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the <see cref="PropertyInfo"/>.
    /// </summary>
    public PropertyInfo PropertyInfo { get; }

    /// <summary>
    /// Gets the type of the property.
    /// </summary>
    public Type Type { get; }

    /// <summary>
    /// Gets whether the property can be read.
    /// </summary>
    public bool CanRead { get; }

    /// <summary>
    /// Gets whether the property can be written to.
    /// </summary>
    public bool CanWrite { get; }

    /// <summary>
    /// Gets whether the property is mandatory.
    /// </summary>
    public bool IsRequired { get; }

    /// <summary>
    /// Gets whether the property is nullable.
    /// </summary>
    public bool IsNullable { get; }

    /// <summary>
    /// Gets whether the property can be unset.
    /// </summary>
    /// <remarks>
    /// Writing <see cref="FieldValue{T}.Unset"/> to a property that is not unsettable will not update the value.
    /// </remarks>
    public bool IsUnsettable { get; }

    /// <summary>
    /// Gets a custom attribute of the specified type.
    /// </summary>
    /// <typeparam name="T">The attribute type.</typeparam>
    /// <param name="inherit">Whether to include inherite attributes.</param>
    /// <returns>A <typeparamref name="T"/>, if it was found.</returns>
    public T? GetCustomAttribute<T>(bool inherit)
        where T : Attribute;
}

/// <summary>
/// Represents a property of a field-value-record.
/// </summary>
/// <typeparam name="TOwner">The field-value-record type.</typeparam>
public interface IFieldValueRecordPropertyModel<in TOwner>
    : IFieldValueRecordPropertyModel
    where TOwner : class
{
    /// <summary>
    /// Accepts a visitor for this property model.
    /// </summary>
    /// <typeparam name="TResult">The output type of the visitor.</typeparam>
    /// <param name="visitor">The visitor.</param>
    /// <returns>The result of calling <see cref="IFieldValueRecordPropertyModelVisitor{TOwner, TResult}.Visit{TValue}(IFieldValueRecordPropertyModel{TOwner, TValue})"/>.</returns>
    public TResult Accept<TResult>(IFieldValueRecordPropertyModelVisitor<TOwner, TResult> visitor);
}

/// <summary>
/// Represents a property of a field-value-record.
/// </summary>
/// <typeparam name="TOwner">The field-value-record type.</typeparam>
/// <typeparam name="TValue">The property type.</typeparam>
public interface IFieldValueRecordPropertyModel<in TOwner, TValue>
    : IFieldValueRecordPropertyModel<TOwner>
    where TOwner : class
    where TValue : notnull
{
    /// <summary>
    /// Reads the property value from the specified owner.
    /// </summary>
    /// <param name="owner">The owner.</param>
    /// <returns>The property value.</returns>
    public FieldValue<TValue> Read(TOwner owner);

    /// <summary>
    /// Writes the property value to the specified owner.
    /// </summary>
    /// <param name="owner">The owner.</param>
    /// <param name="value">The value.</param>
    public void Write(TOwner owner, FieldValue<TValue> value);

    /// <summary>
    /// Writes a value to a specified slot, typically used for later calling the constructor.
    /// </summary>
    /// <param name="slot">The value slot.</param>
    /// <param name="value">The value.</param>
    public void WriteSlot(ref object? slot, FieldValue<TValue> value);

    /// <inheritdoc/>
    Type IFieldValueRecordPropertyModel.Type => typeof(TValue);

    /// <inheritdoc/>
    TResult IFieldValueRecordPropertyModel<TOwner>.Accept<TResult>(IFieldValueRecordPropertyModelVisitor<TOwner, TResult> visitor)
        => visitor.Visit<TValue>(this);
}
