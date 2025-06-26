using CommunityToolkit.Diagnostics;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Altinn.Authorization.ModelUtils.FieldValueRecords;

/// <summary>
/// A model for a field-value property of a field-value-record.
/// </summary>
/// <typeparam name="TOwner">The field-value-record type.</typeparam>
/// <typeparam name="TValue">The field-value type.</typeparam>
[DebuggerDisplay("{_property,nq}")]
internal sealed class FieldValuePropertyModel<TOwner, TValue>
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

    /// <inheritdoc/>
    public MemberInfo MemberInfo => _property;

    /// <inheritdoc/>
    public string Name => _property.Name;

    /// <inheritdoc/>
    public Type Type => typeof(TValue);

    /// <inheritdoc/>
    public T? GetCustomAttribute<T>(bool inherit)
        where T : Attribute
        => _property.GetCustomAttribute<T>(inherit);

    /// <inheritdoc/>
    [MemberNotNullWhen(true, nameof(_read))]
    public bool CanRead => _read is not null;

    /// <inheritdoc/>
    [MemberNotNullWhen(true, nameof(_write))]
    public bool CanWrite => _write is not null;

    /// <inheritdoc/>
    public bool IsRequired => false;

    /// <inheritdoc/>
    public bool IsNullable => true;

    /// <inheritdoc/>
    public bool IsUnsettable => true;

    /// <inheritdoc/>
    public FieldValue<TValue> Read(TOwner owner)
    {
        if (!CanRead)
        {
            ThrowHelper.ThrowInvalidOperationException($"Property {Name} is not readable.");
        }

        return _read(owner);
    }

    /// <inheritdoc/>
    public void Write(TOwner owner, FieldValue<TValue> value)
    {
        if (!CanWrite)
        {
            ThrowHelper.ThrowInvalidOperationException($"Property {Name} is not writable.");
        }

        _write(owner, value);
    }

    /// <inheritdoc/>
    public void WriteSlot(ref object? slot, FieldValue<TValue> value)
    {
        slot = value;
    }
}
