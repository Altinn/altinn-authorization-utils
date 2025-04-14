namespace Altinn.Authorization.ModelUtils.FieldValueRecords;

/// <summary>
/// A visitor for <see cref="IFieldValueRecordPropertyModel{TOwner}"/>.
/// </summary>
/// <typeparam name="TOwner">The property owner type.</typeparam>
/// <typeparam name="TResult">The output produced by the visitor.</typeparam>
public interface IFieldValueRecordPropertyModelVisitor<out TOwner, TResult>
    where TOwner : class
{
    /// <summary>
    /// Visits a property model with a known value type.
    /// </summary>
    /// <typeparam name="TValue">The value type.</typeparam>
    /// <param name="property">The property model.</param>
    /// <returns><typeparamref name="TResult"/>.</returns>
    public TResult Visit<TValue>(IFieldValueRecordPropertyModel<TOwner, TValue> property)
        where TValue : notnull;
}
