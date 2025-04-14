using CommunityToolkit.Diagnostics;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace Altinn.Authorization.ModelUtils.FieldValueRecords;

/// <summary>
/// A model for a field-value-record property.
/// </summary>
/// <typeparam name="TOwner">The field-value-record type.</typeparam>
/// <typeparam name="TValue">The proeprty type.</typeparam>
internal class PropertyModel<TOwner, TValue>
    : IFieldValueRecordPropertyModel<TOwner, TValue>
    where TOwner : class
    where TValue : notnull
{
    private static readonly NullabilityInfoContext _nullabilityInfoContext = new();

    private readonly PropertyInfo _property;
    private readonly bool _isRequired;
    private readonly bool _isNullable;
    private readonly Func<TOwner, TValue>? _read;
    private readonly Action<TOwner, TValue>? _write;

    public PropertyModel(PropertyInfo property)
    {
        if (property.PropertyType != typeof(TValue))
        {
            ThrowHelper.ThrowInvalidOperationException("Property type mismatch.");
        }

        _property = property;
        _read = _property.GetGetMethod(true)?.CreateDelegate<Func<TOwner, TValue>>();
        _write = _property.GetSetMethod(true)?.CreateDelegate<Action<TOwner, TValue>>();

        _isNullable = _nullabilityInfoContext.Create(_property).WriteState != NullabilityState.NotNull;
        _isRequired = _property.GetCustomAttribute<JsonRequiredAttribute>() is not null
            || _property.GetCustomAttribute<RequiredAttribute>() is not null
            || _property.GetCustomAttribute<RequiredMemberAttribute>() is not null;
    }

    public string Name => _property.Name;

    public virtual Type Type => typeof(TValue);

    public T? GetCustomAttribute<T>(bool inherit) 
        where T : Attribute
        => _property.GetCustomAttribute<T>(inherit);

    [MemberNotNullWhen(true, nameof(_read))]
    public bool CanRead => _read is not null;

    [MemberNotNullWhen(true, nameof(_write))]
    public bool CanWrite => _write is not null;

    public bool IsRequired => _isRequired;

    public bool IsNullable => _isNullable;

    public virtual FieldValue<TValue> Read(TOwner owner)
    {
        if (!CanRead)
        {
            ThrowHelper.ThrowInvalidOperationException($"Property {Name} is not readable.");
        }

        return _read(owner) switch
        {
            null => FieldValue.Null,
            TValue value => value,
        };
    }

    public virtual void Write(TOwner owner, FieldValue<TValue> value)
    {
        if (!CanWrite)
        {
            ThrowHelper.ThrowInvalidOperationException($"Property {Name} is not writable.");
        }

        if (value.HasValue)
        {
            _write(owner, value.Value);
        }
    }
}
