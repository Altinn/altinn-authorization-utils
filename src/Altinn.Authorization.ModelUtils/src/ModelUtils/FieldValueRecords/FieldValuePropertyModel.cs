using CommunityToolkit.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Altinn.Authorization.ModelUtils.FieldValueRecords;

/// <summary>
/// A model for a field-value property of a field-value-record.
/// </summary>
/// <typeparam name="TOwner">The field-value-record type.</typeparam>
/// <typeparam name="TValue">The field-value type.</typeparam>
internal class FieldValuePropertyModel<TOwner, TValue>
    : IFieldValueRecordPropertyModel<TOwner, TValue>
    where TOwner : class
    where TValue : notnull
{
    private readonly PropertyInfo _property;
    private readonly Func<TOwner, FieldValue<TValue>>? _read;
    private readonly Action<TOwner, FieldValue<TValue>>? _write;

    public FieldValuePropertyModel(PropertyInfo property)
    {
        if (property.PropertyType != typeof(FieldValue<TValue>))
        {
            ThrowHelper.ThrowInvalidOperationException("Property type mismatch.");
        }

        _property = property;
        _read = _property.GetGetMethod(true)?.CreateDelegate<Func<TOwner, FieldValue<TValue>>>();
        _write = _property.GetSetMethod(true)?.CreateDelegate<Action<TOwner, FieldValue<TValue>>>();
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

    public bool IsRequired => false;

    public bool IsNullable => true;

    public virtual FieldValue<TValue> Read(TOwner owner)
    {
        if (!CanRead)
        {
            ThrowHelper.ThrowInvalidOperationException($"Property {Name} is not readable.");
        }

        return _read(owner);
    }

    public virtual void Write(TOwner owner, FieldValue<TValue> value)
    {
        if (!CanWrite)
        {
            ThrowHelper.ThrowInvalidOperationException($"Property {Name} is not writable.");
        }

        _write(owner, value);
    }
}
