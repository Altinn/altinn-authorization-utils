using System.Reflection;

namespace Altinn.Authorization.ModelUtils.FieldValueRecords;

/// <summary>
/// A model for a constructor parameter of a field-value-record that accepts a field-value.
/// </summary>
/// <typeparam name="TOwner">The field-value-record type.</typeparam>
/// <typeparam name="TValue">The constructor parameter type.</typeparam>
internal class FieldValueConstructorParameterModel<TOwner, TValue>
    : ConstructorParameterModel<TOwner, FieldValue<TValue>>
    where TOwner : class
    where TValue : notnull
{
    public FieldValueConstructorParameterModel(ParameterInfo parameter)
        : base(parameter)
    {
    }

    public override Type Type => typeof(TValue);
}
