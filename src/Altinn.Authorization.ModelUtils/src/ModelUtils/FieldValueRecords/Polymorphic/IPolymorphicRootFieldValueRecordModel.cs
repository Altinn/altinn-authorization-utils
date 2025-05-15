namespace Altinn.Authorization.ModelUtils.FieldValueRecords.Polymorphic;

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

public interface IPolymorphicRootFieldValueRecordModel<TDiscriminator, T>
    : IPolymorphicFieldValueRecordModel<TDiscriminator, T>
    , IPolymorphicRootFieldValueRecordModel
    where TDiscriminator : struct, Enum
    where T : class
{
}
