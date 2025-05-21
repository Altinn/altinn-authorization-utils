namespace Altinn.Authorization.ModelUtils.FieldValueRecords.Polymorphic;

/// <summary>
/// A model for a root polymorphic record type that consists of <see cref="FieldValue{T}"/>s.
/// </summary>
public interface IPolymorphicRootFieldValueRecordModel
    : IPolymorphicFieldValueRecordModel
{
    /// <summary>
    /// Gets the model for the specified type.
    /// </summary>
    /// <param name="type">The type.</param>
    /// <returns>A <see cref="FieldValueRecordModel"/> for <paramref name="type"/>.</returns>
    public IPolymorphicFieldValueRecordModel ModelFor(Type type);
}

/// <summary>
/// A model for a root polymorphic record type that consists of <see cref="FieldValue{T}"/>s.
/// </summary>
/// <typeparam name="TDiscriminator">The discriminator type.</typeparam>
public interface IPolymorphicRootFieldValueRecordModel<TDiscriminator>
    : IPolymorphicFieldValueRecordModel<TDiscriminator>
    , IPolymorphicRootFieldValueRecordModel
    where TDiscriminator : struct, Enum
{
    /// <summary>
    /// Gets the model for the specified type.
    /// </summary>
    /// <param name="type">The type.</param>
    /// <returns>A <see cref="FieldValueRecordModel"/> for <paramref name="type"/>.</returns>
    public new IPolymorphicFieldValueRecordModel<TDiscriminator> ModelFor(Type type);

    /// <inheritdoc/>
    IPolymorphicFieldValueRecordModel IPolymorphicRootFieldValueRecordModel.ModelFor(Type type)
        => ModelFor(type);
}

/// <summary>
/// A model for a root polymorphic record type that consists of <see cref="FieldValue{T}"/>s.
/// </summary>
/// <typeparam name="TDiscriminator">The discriminator type.</typeparam>
/// <typeparam name="T">The type of the record.</typeparam>
public interface IPolymorphicRootFieldValueRecordModel<TDiscriminator, T>
    : IPolymorphicFieldValueRecordModel<TDiscriminator, T>
    , IPolymorphicRootFieldValueRecordModel
    where TDiscriminator : struct, Enum
    where T : class
{
}
