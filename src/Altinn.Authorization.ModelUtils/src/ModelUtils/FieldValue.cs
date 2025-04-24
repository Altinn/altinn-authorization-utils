using CommunityToolkit.Diagnostics;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.CompilerServices;
using static Altinn.Authorization.ModelUtils.FieldValue;

namespace Altinn.Authorization.ModelUtils;

/// <summary>
/// Static utility class for <see cref="FieldValue{T}"/>.
/// </summary>
public static class FieldValue
{
    /// <summary>
    /// Gets a value that represents an unset field value.
    /// </summary>
    public static readonly UnsetSentinel Unset = default;

    /// <summary>
    /// Gets a value that represents a null field value.
    /// </summary>
    public static readonly NullSentinel Null = default;

    /// <summary>
    /// Creates a <see cref="FieldValue{T}"/> from a nullable struct.
    /// </summary>
    /// <typeparam name="T">The field type.</typeparam>
    /// <param name="value">The value.</param>
    /// <returns>A <see cref="FieldValue{T}"/>.</returns>
    public static FieldValue<T> From<T>(T? value)
        where T : struct
        => value.HasValue ? value.Value : Null;

    /// <summary>
    /// Maps a <see cref="FieldValue{T}"/> to another type.
    /// </summary>
    /// <typeparam name="TSource">The source type.</typeparam>
    /// <typeparam name="TResult">The result type.</typeparam>
    /// <param name="value">The field value to map.</param>
    /// <param name="selector">The mapper.</param>
    /// <returns>The mapped field value.</returns>
    public static FieldValue<TResult> Select<TSource, TResult>(this FieldValue<TSource> value, Func<TSource, TResult> selector)
        where TSource : notnull
        where TResult : notnull
        => value switch
        {
            { HasValue: true } => selector(value.Value!),
            { IsNull: true } => Null,
            _ => Unset,
        };

    /// <summary>
    /// Maps a <see cref="FieldValue{T}"/> to another type.
    /// </summary>
    /// <typeparam name="TSource">The source type.</typeparam>
    /// <typeparam name="TState">The state type.</typeparam>
    /// <typeparam name="TResult">The result type.</typeparam>
    /// <param name="value">The field value to map.</param>
    /// <param name="state">The state to pass to the mapper.</param>
    /// <param name="selector">The mapper.</param>
    /// <returns>The mapped field value.</returns>
    public static FieldValue<TResult> Select<TSource, TState, TResult>(this FieldValue<TSource> value, TState state, Func<TSource, TState, TResult> selector)
        where TSource : notnull
        where TResult : notnull
        => value switch
        {
            { HasValue: true } => selector(value.Value!, state),
            { IsNull: true } => Null,
            _ => Unset,
        };

    /// <summary>
    /// Maps a <see cref="FieldValue{T}"/> to another type.
    /// </summary>
    /// <typeparam name="TSource">The source type.</typeparam>
    /// <typeparam name="TResult">The result type.</typeparam>
    /// <param name="value">The field value to map.</param>
    /// <param name="selector">The mapper.</param>
    /// <returns>The mapped field value.</returns>
    public static FieldValue<TResult> SelectFieldValue<TSource, TResult>(this FieldValue<TSource> value, Func<TSource, FieldValue<TResult>> selector)
        where TSource : notnull
        where TResult : notnull
        => value switch
        {
            { HasValue: true } => selector(value.Value!),
            { IsNull: true } => Null,
            _ => Unset,
        };

    /// <summary>
    /// Maps a <see cref="FieldValue{T}"/> to another type.
    /// </summary>
    /// <typeparam name="TSource">The source type.</typeparam>
    /// <typeparam name="TState">The state type.</typeparam>
    /// <typeparam name="TResult">The result type.</typeparam>
    /// <param name="value">The field value to map.</param>
    /// <param name="state">The state to pass to the mapper.</param>
    /// <param name="selector">The mapper.</param>
    /// <returns>The mapped field value.</returns>
    public static FieldValue<TResult> SelectFieldValue<TSource, TState, TResult>(this FieldValue<TSource> value, TState state, Func<TSource, TState, FieldValue<TResult>> selector)
        where TSource : notnull
        where TResult : notnull
        => value switch
        {
            { HasValue: true } => selector(value.Value!, state),
            { IsNull: true } => Null,
            _ => Unset,
        };

    /// <summary>
    /// Checks if a type is a <see cref="FieldValue{T}"/> and returns the field type.
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <param name="fieldType">The inner field type, if <paramref name="type"/> is a constructed <see cref="FieldValue{T}"/> type.</param>
    /// <returns><see langword="true"/> if <paramref name="type"/> is a constructed <see cref="FieldValue{T}"/> type, otherwise <see langword="false"/>.</returns>
    public static bool IsFieldValueType(Type type, [NotNullWhen(true)] out Type? fieldType)
    {
        if (type.IsConstructedGenericType && type.GetGenericTypeDefinition() == typeof(FieldValue<>))
        {
            fieldType = type.GetGenericArguments()[0];
            return true;
        }

        fieldType = null;
        return false;
    }

    /// <summary>
    /// A value that implicitly converts to any <see cref="FieldValue{T}"/> in the unset state.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public readonly struct UnsetSentinel
    {
    }

    /// <summary>
    /// A value that implicitly converts to any <see cref="FieldValue{T}"/> in the null state.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public readonly struct NullSentinel
    {
    }

    internal enum FieldState : byte
    {
        Unset = default,
        Null,
        NonNull,
    }
}

/// <summary>
/// Represents a field value (typically a database field or json property).
/// 
/// This is similar to <see cref="Nullable{T}"/>, but with an additional state for unset values.
/// </summary>
/// <typeparam name="T">The value type.</typeparam>
[DebuggerDisplay("{DebuggerDisplay,nq}")]
public readonly struct FieldValue<T>
    : IEqualityOperators<FieldValue<T>, FieldValue<T>, bool>
    where T : notnull
{
    /// <summary>
    /// Represents an unset field value.
    /// </summary>
    public static readonly FieldValue<T> Unset = new(FieldState.Unset, default);

    /// <summary>
    /// Represents a null field value.
    /// </summary>
    public static readonly FieldValue<T> Null = new(FieldState.Null, default);

    private readonly FieldState _state;
    private readonly T? _value;

    private FieldValue(FieldState state, T? value)
    {
        _state = state;
        _value = value;
    }

    /// <summary>
    /// Gets whether the field is unset.
    /// </summary>
    public bool IsUnset => _state == FieldState.Unset;

    /// <summary>
    /// Gets whether the field is set.
    /// </summary>
    public bool IsSet => _state != FieldState.Unset;

    /// <summary>
    /// Gets whether the field is null.
    /// </summary>
    public bool IsNull => _state == FieldState.Null;

    /// <summary>
    /// Gets whether the field has a value.
    /// </summary>
    [MemberNotNullWhen(true, nameof(Value))]
    public bool HasValue => _state == FieldState.NonNull;

    /// <summary>
    /// Gets the field value.
    /// </summary>
    public T? Value => _value;

    /// <summary>
    /// Gets the field value or a default value if the field is null/unset.
    /// </summary>
    /// <returns>The field value, or <see langword="default"/>.</returns>
    public T? OrDefault(T? defaultValue = default)
        => HasValue ? _value : defaultValue;

    /// <inheritdoc/>
    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        if (IsNull && obj is null)
        {
            return true;
        }

        return obj is FieldValue<T> other && Equals(this, other);

        static bool Equals(FieldValue<T> left, FieldValue<T> right)
        {
            if (left._state != right._state)
            {
                return false;
            }

            if (left._state == FieldState.NonNull)
            {
                return EqualityComparer<T>.Default.Equals(left._value!, right._value!);
            }

            return true;
        }
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        if (IsNull)
        {
            return 0;
        }

        if (IsUnset)
        {
            return -1;
        }

        return _value!.GetHashCode();
    }

    /// <inheritdoc/>
    public override string? ToString()
        => _state switch
        {
            FieldState.Unset => "<unset>",
            FieldState.Null => "<null>",
            FieldState.NonNull => _value!.ToString() ?? string.Empty,
            _ => Unreachable<string?>(),
        };

    private string DebuggerDisplay
        => _state switch
        {
            FieldState.Unset => "<unset>",
            FieldState.Null => "<null>",
            FieldState.NonNull => _value!.ToString() ?? string.Empty,
            _ => Unreachable<string>(),
        };

    /// <summary>
    /// Converts from a <see cref="UnsetSentinel"/> to a <see cref="FieldValue{T}"/> in the unset state.
    /// </summary>
    public static implicit operator FieldValue<T>(UnsetSentinel _)
        => Unset;

    /// <summary>
    /// Converts from a <see cref="NullSentinel"/> to a <see cref="FieldValue{T}"/> in the null state.
    /// </summary>
    public static implicit operator FieldValue<T>(NullSentinel _)
        => Null;

    /// <summary>
    /// Converts from a <typeparamref name="T"/> to a <see cref="FieldValue{T}"/> in the set or null state.
    /// </summary>
    /// <param name="value">The field value.</param>
    public static implicit operator FieldValue<T>(T? value)
        => value is null ? Null : new FieldValue<T>(FieldState.NonNull, value);

    /// <summary>
    /// Converts from a <see cref="FieldValue{T}"/> to a <typeparamref name="T"/>.
    /// </summary>
    /// <param name="value">The <see cref="FieldValue{T}"/>.</param>
    /// <exception cref="InvalidOperationException">Thrown if the field value is unset or null.</exception>
    public static explicit operator T(FieldValue<T> value)
    {
        if (!value.HasValue)
        {
            ThrowHelper.ThrowInvalidOperationException("FieldValue has no value");
        }

        return value.Value;
    }

    /// <inheritdoc/>
    public static bool operator ==(FieldValue<T> left, FieldValue<T> right)
        => left.Equals(right);

    /// <inheritdoc/>
    public static bool operator !=(FieldValue<T> left, FieldValue<T> right)
        => !left.Equals(right);

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static TRet Unreachable<TRet>()
        => throw new UnreachableException();
}
