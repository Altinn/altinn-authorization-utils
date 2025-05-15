using CommunityToolkit.Diagnostics;
using System.Reflection;

namespace Altinn.Authorization.ModelUtils.FieldValueRecords.Polymorphic;

internal class NonExhaustiveDiscriminatorPropertyModelWrapper<TOwner, TValue>
    : IFieldValueRecordPropertyModel<TOwner, NonExhaustiveEnum<TValue>>
    where TOwner : class
    where TValue : struct, Enum
{
    private readonly IFieldValueRecordPropertyModel<TOwner, TValue> _innerModel;

    public NonExhaustiveDiscriminatorPropertyModelWrapper(IFieldValueRecordPropertyModel<TOwner, TValue> innerModel)
    {
        _innerModel = innerModel;
    }

    public string Name => _innerModel.Name;

    public PropertyInfo PropertyInfo => _innerModel.PropertyInfo;

    public bool CanRead => _innerModel.CanRead;

    public bool CanWrite => _innerModel.CanWrite;

    public bool IsRequired => _innerModel.IsRequired;

    public bool IsNullable => _innerModel.IsNullable;

    public bool IsUnsettable => _innerModel.IsUnsettable;

    public T? GetCustomAttribute<T>(bool inherit) 
        where T : Attribute
        => _innerModel.GetCustomAttribute<T>(inherit);

    public FieldValue<NonExhaustiveEnum<TValue>> Read(TOwner owner)
        => _innerModel.Read(owner).Select(static value => NonExhaustiveEnum.Create(value));

    public void Write(TOwner owner, FieldValue<NonExhaustiveEnum<TValue>> value)
    {
        if (value.HasValue && value.Value.IsUnknown)
        {
            ThrowHelper.ThrowInvalidOperationException($"The value '{value.Value}' is not a valid value for '{Name}'.");
        }

        _innerModel.Write(owner, value.Select(static v => v.Value));
    }

    public void WriteSlot(ref object? slot, FieldValue<NonExhaustiveEnum<TValue>> value)
    {
        if (value.HasValue && value.Value.IsUnknown)
        {
            ThrowHelper.ThrowInvalidOperationException($"The value '{value.Value}' is not a valid value for '{Name}'.");
        }

        _innerModel.WriteSlot(ref slot, value.Select(static v => v.Value));
    }
}
