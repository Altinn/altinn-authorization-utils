using CommunityToolkit.Diagnostics;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace Altinn.Authorization.ModelUtils.FieldValueRecords;

/// <summary>
/// A model for a field-value-record field.
/// </summary>
/// <typeparam name="TOwner">The field-value-record type.</typeparam>
/// <typeparam name="TValue">The field type.</typeparam>
[DebuggerDisplay("{_field,nq}")]
internal sealed class FieldModel<TOwner, TValue>
    : IFieldValueRecordPropertyModel<TOwner, TValue>
    where TOwner : class
    where TValue : notnull
{
    private static readonly NullabilityInfoContext _nullabilityInfoContext = new();

    private readonly FieldInfo _field;
    private readonly bool _isRequired;
    private readonly bool _isNullable;
    private readonly Func<TOwner, TValue?> _read;
    private readonly Action<TOwner, TValue?> _write;

    public FieldModel(FieldInfo field)
    {
        if (field.FieldType != typeof(TValue))
        {
            ThrowHelper.ThrowInvalidOperationException("Property type mismatch.");
        }

        _field = field;
        _read = CreateGetter(_field);
        _write = CreateSetter(_field);

        _isNullable = _nullabilityInfoContext.Create(_field).WriteState != NullabilityState.NotNull;
        _isRequired = _field.GetCustomAttribute<JsonRequiredAttribute>() is not null
            || _field.GetCustomAttribute<RequiredAttribute>() is not null
            || _field.GetCustomAttribute<RequiredMemberAttribute>() is not null;
    }

    /// <inheritdoc/>
    public MemberInfo MemberInfo => _field;

    /// <inheritdoc/>
    public string Name => _field.Name;

    /// <inheritdoc/>
    public Type Type => typeof(TValue);

    /// <inheritdoc/>
    public T? GetCustomAttribute<T>(bool inherit)
        where T : Attribute
        => _field.GetCustomAttribute<T>(inherit);

    /// <inheritdoc/>
    public bool CanRead => true;

    /// <inheritdoc/>
    public bool CanWrite => true;

    /// <inheritdoc/>
    public bool IsRequired => _isRequired;

    /// <inheritdoc/>
    public bool IsNullable => _isNullable;

    /// <inheritdoc/>
    public bool IsUnsettable => false;

    /// <inheritdoc/>
    public FieldValue<TValue> Read(TOwner owner)
    {
        if (!CanRead)
        {
            ThrowHelper.ThrowInvalidOperationException($"Property {Name} is not readable.");
        }

        return _read(owner) switch
        {
            null => FieldValue.Null,
            var value => value,
        };
    }

    /// <inheritdoc/>
    public void Write(TOwner owner, FieldValue<TValue> value)
    {
        if (!CanWrite)
        {
            ThrowHelper.ThrowInvalidOperationException($"Property {Name} is not writable.");
        }

        if (value.IsUnset)
        {
            ThrowHelper.ThrowInvalidOperationException($"Property {Name} is not unsettable.");
        }

        if (value.IsNull && !_isNullable)
        {
            ThrowHelper.ThrowInvalidOperationException($"Property {Name} is not nullable.");
        }

        _write(owner, value.Value);
    }

    /// <inheritdoc/>
    public void WriteSlot(ref object? slot, FieldValue<TValue> value)
    {
        if (value.IsUnset)
        {
            ThrowHelper.ThrowInvalidOperationException($"Property {Name} is not unsettable.");
        }

        if (value.IsNull && !_isNullable)
        {
            ThrowHelper.ThrowInvalidOperationException($"Property {Name} is not nullable.");
        }

        slot = value.Value;
    }

    private static Func<TOwner, TValue?> CreateGetter(FieldInfo field)
    {
        var owner = Expression.Parameter(typeof(TOwner), "owner");
        var fieldAccess = Expression.Field(owner, field);
        var lambda = Expression.Lambda<Func<TOwner, TValue?>>(fieldAccess, owner);

        return lambda.Compile();
    }

    private static Action<TOwner, TValue?> CreateSetter(FieldInfo field)
    {
        var meth = new DynamicMethod($"set_{field.Name}", typeof(void), [typeof(TOwner), typeof(TValue?)], typeof(FieldModel<,>).Module, skipVisibility: true);
        var il = meth.GetILGenerator();
        il.Emit(OpCodes.Ldarg_0); // Load owner
        il.Emit(OpCodes.Ldarg_1); // Load value
        il.Emit(OpCodes.Stfld, field); // Store value in field
        il.Emit(OpCodes.Ret); // Return

        return meth.CreateDelegate<Action<TOwner, TValue?>>();
    }
}
