using System.Collections.Immutable;

namespace Altinn.Authorization.ModelUtils.FieldValueRecords;

/// <summary>
/// A model for a record type that consists of <see cref="FieldValue{T}"/>s.
/// </summary>
public interface IFieldValueRecordModel
{
    /// <summary>
    /// Gets the type of the record.
    /// </summary>
    public Type Type { get; }

    /// <summary>
    /// Gets the parent model, if any.
    /// </summary>
    public IFieldValueRecordModel? Parent { get; }

    /// <summary>
    /// Gets the properties of the record.
    /// </summary>
    /// <param name="includeInherited">Whether or not to include inherited properties.</param>
    /// <returns>The properties of the model.</returns>
    public ImmutableArray<IFieldValueRecordPropertyModel> Properties(bool includeInherited = true);

    /// <summary>
    /// Gets the constructor of the record.
    /// </summary>
    public IFieldValueRecordConstructorModel Constructor { get; }
}

/// <summary>
/// A model for a record type that consists of <see cref="FieldValue{T}"/>s.
/// </summary>
/// <typeparam name="T">The type of the record.</typeparam>
public interface IFieldValueRecordModel<T>
    : IFieldValueRecordModel
    where T : class
{
    /// <summary>
    /// Gets the properties of the record.
    /// </summary>
    /// <param name="includeInherited">Whether or not to include inherited properties.</param>
    /// <returns>The properties of the model.</returns>
    public new ImmutableArray<IFieldValueRecordPropertyModel<T>> Properties(bool includeInherited = true);

    /// <inheritdoc/>
    ImmutableArray<IFieldValueRecordPropertyModel> IFieldValueRecordModel.Properties(bool includeInherited)
        => ImmutableArray<IFieldValueRecordPropertyModel>.CastUp(Properties(includeInherited));

    /// <summary>
    /// Gets the constructor of the record.
    /// </summary>
    public new IFieldValueRecordConstructorModel<T> Constructor { get; }

    /// <inheritdoc/>
    IFieldValueRecordConstructorModel IFieldValueRecordModel.Constructor
        => Constructor;
}
