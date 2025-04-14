using CommunityToolkit.Diagnostics;
using System.Reflection;

namespace Altinn.Authorization.ModelUtils.FieldValueRecords;

/// <summary>
/// A model for a constructor parameter of a field-value-record.
/// </summary>
/// <typeparam name="TOwner">The field-value-record type.</typeparam>
/// <typeparam name="TValue">The constructor parameter type.</typeparam>
internal class ConstructorParameterModel<TOwner, TValue>
    : IFieldValueRecordConstructorParameterModel<TOwner, TValue>
    where TOwner : class
    where TValue : notnull
{
    private readonly ParameterInfo _parameter;
    private readonly FieldValue<TValue> _defaultValue;

    public ConstructorParameterModel(ParameterInfo parameter)
    {
        if (parameter.ParameterType != typeof(TValue))
        {
            ThrowHelper.ThrowInvalidOperationException("Property type mismatch.");
        }

        _parameter = parameter;
        if (parameter.HasDefaultValue)
        {
            var defaultValue = parameter.DefaultValue;
            if (defaultValue is null)
            {
                _defaultValue = FieldValue<TValue>.Null;
            }
            else
            {
                _defaultValue = (TValue)defaultValue;
            }
        }
    }

    public string? Name => _parameter.Name;

    public virtual Type Type => typeof(TValue);

    public FieldValue<TValue> DefaultValue => _defaultValue;
}
