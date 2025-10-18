using CommunityToolkit.Diagnostics;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Altinn.Authorization.ModelUtils.AspNet;

/// <summary>
/// Provides static methods for creating and inspecting strongly-typed models of enum values that support bitwise flag
/// operations.
/// </summary>
public sealed partial class FlagsEnum
{
    /// <summary>
    /// Creates a new instance of the <see cref="FlagsEnum{TEnum}"/> representing the specified enum value.
    /// </summary>
    /// <typeparam name="TEnum">The enum type to be wrapped. Must be a value type that is an enumeration.</typeparam>
    /// <param name="value">The enum value to be represented by the created <see cref="FlagsEnum{TEnum}"/> instance.</param>
    /// <returns>A <see cref="FlagsEnum{TEnum}"/> that encapsulates the specified enum value.</returns>
    public static FlagsEnum<TEnum> Create<TEnum>(TEnum value)
        where TEnum : struct, Enum
        => value;

    /// <summary>
    /// Determines whether the specified type is a constructed generic type of <see cref="FlagsEnum{TEnum}"/> and retrieves the
    /// underlying enum type if so.
    /// </summary>
    /// <remarks>This method is useful for identifying types that represent flag-based enum models and
    /// extracting the associated enum type for further processing.</remarks>
    /// <param name="type">The type to examine for compatibility with <see cref="FlagsEnum{TEnum}"/>.</param>
    /// <param name="enumType">When this method returns <see langword="true"/>, contains the underlying enum type parameter of the
    /// <see cref="FlagsEnum{TEnum}"/>.</param>
    /// <returns><see langword="true" /> if the specified type is a constructed generic type of <see cref="FlagsEnum{TEnum}"/>; otherwise, <see langword="false" />.</returns>
    public static bool IsFlagsEnumModelType(Type type, [NotNullWhen(true)] out Type? enumType)
    {
        if (type.IsConstructedGenericType && type.GetGenericTypeDefinition() == typeof(FlagsEnum<>))
        {
            enumType = type.GetGenericArguments()[0];
            return true;
        }

        enumType = null;
        return false;
    }
}

/// <summary>
/// Represents a strongly-typed wrapper for an enum value marked with the <see cref="FlagsAttribute"/>, providing
/// parsing, formatting, and conversion operations for flag-based enumerations.
/// </summary>
/// <remarks>Use <see cref="FlagsEnum{TEnum}"/> to simplify working with flag-based enums, including parsing
/// from strings, formatting, and implicit conversion to and from the underlying enum type. This struct is immutable and
/// supports span-based parsing and formatting for efficient operations. Thread safety is guaranteed for all
/// members.</remarks>
/// <typeparam name="TEnum">The enumeration type to wrap. Must be a value type that derives from <see cref="Enum"/> and is typically decorated
/// with <see cref="FlagsAttribute"/>.</typeparam>
[DebuggerDisplay("{DebuggerDisplay,nq}")]
[JsonConverter(typeof(FlagsEnum.JsonConverterProvider))]
public readonly struct FlagsEnum<TEnum>
    : ISpanParsable<FlagsEnum<TEnum>>
    , ISpanFormattable
    , IEquatable<FlagsEnum<TEnum>>
    , IEquatable<TEnum>
    where TEnum : struct, Enum
{
    internal static readonly EnumUtils.FlagsEnumModel<TEnum> Model = EnumUtils.FlagsEnumModel.Create<TEnum>();

    private readonly TEnum _value;

    private FlagsEnum(TEnum value)
    {
        _value = value;
    }

    private readonly string DebuggerDisplay
        => Model.Format(_value);

    /// <summary>
    /// Gets the underlying enumeration value represented by this instance.
    /// </summary>
    public readonly TEnum Value => _value;

    /// <summary>
    /// Converts a <see cref="FlagsEnum{TEnum}"/> instance to its underlying enum value.
    /// </summary>
    /// <remarks>This operator enables implicit conversion from a <see cref="FlagsEnum{TEnum}"/> to the
    /// underlying <typeparamref name="TEnum"/> type, allowing seamless use of enum values in expressions and
    /// assignments.</remarks>
    /// <param name="flags">The <see cref="FlagsEnum{TEnum}"/> instance to convert.</param>
    public static implicit operator TEnum(FlagsEnum<TEnum> flags)
        => flags._value;

    /// <summary>
    /// Converts a value of the underlying enum type to a <see cref="FlagsEnum{TEnum}"/> instance.
    /// </summary>
    /// <remarks>This operator enables implicit conversion from the enum type to <see
    /// cref="FlagsEnum{TEnum}"/>, allowing seamless usage in expressions and assignments. The resulting <see
    /// cref="FlagsEnum{TEnum}"/> instance represents the specified enum value.</remarks>
    /// <param name="value">The enum value to convert to a <see cref="FlagsEnum{TEnum}"/>.</param>
    public static implicit operator FlagsEnum<TEnum>(TEnum value)
        => new(value);

    /// <inheritdoc cref="IParsable{TSelf}.Parse(string, IFormatProvider?)"/>
    public static FlagsEnum<TEnum> Parse(string s)
        => Parse(s, null);

    /// <inheritdoc cref="ISpanParsable{TSelf}.Parse(ReadOnlySpan{char}, IFormatProvider?)"/>
    public static FlagsEnum<TEnum> Parse(ReadOnlySpan<char> s)
        => Parse(s, null);

    /// <inheritdoc/>
    static FlagsEnum<TEnum> ISpanParsable<FlagsEnum<TEnum>>.Parse(ReadOnlySpan<char> s, IFormatProvider? provider)
        => Parse(s, provider);

    /// <inheritdoc/>
    static FlagsEnum<TEnum> IParsable<FlagsEnum<TEnum>>.Parse(string s, IFormatProvider? provider)
        => Parse(s, provider);

    private static FlagsEnum<TEnum> Parse(ReadOnlySpan<char> s, IFormatProvider? provider)
    {
        if (!TryParse(s, provider, out var result))
        {
            ThrowHelper.ThrowFormatException($"Failed to parse {nameof(FlagsEnum)}<{typeof(TEnum)}>");
        }

        return result;
    }

    /// <inheritdoc cref="IParsable{TSelf}.TryParse(string?, IFormatProvider?, out TSelf)"/>
    public static bool TryParse(string s, out FlagsEnum<TEnum> result)
        => TryParse(s, null, out result);

    /// <inheritdoc cref="ISpanParsable{TSelf}.TryParse(ReadOnlySpan{char}, IFormatProvider?, out TSelf)"/>
    public static bool TryParse(ReadOnlySpan<char> s, out FlagsEnum<TEnum> result)
        => TryParse(s, null, out result);

    /// <inheritdoc/>
    static bool ISpanParsable<FlagsEnum<TEnum>>.TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, [MaybeNullWhen(false)] out FlagsEnum<TEnum> result)
        => TryParse(s, provider, out result);

    /// <inheritdoc/>
    static bool IParsable<FlagsEnum<TEnum>>.TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, [MaybeNullWhen(false)] out FlagsEnum<TEnum> result)
        => TryParse(s, provider, out result);

    private static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, [MaybeNullWhen(false)] out FlagsEnum<TEnum> result)
    {
        var ret = Model.TryParse(s, out var enumResult);
        result = enumResult;
        return ret;
    }

    /// <inheritdoc/>
    bool ISpanFormattable.TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
        => Model.TryFormat(_value, destination, out charsWritten);

    /// <inheritdoc/>
    string IFormattable.ToString(string? format, IFormatProvider? formatProvider)
        => ToString();

    /// <inheritdoc/>
    public override string ToString()
        => Model.Format(_value);

    /// <inheritdoc/>
    public override int GetHashCode()
        => _value.GetHashCode();

    /// <inheritdoc/>
    public override bool Equals([NotNullWhen(true)] object? obj)
        => obj switch
        {
            FlagsEnum<TEnum> other => Equals(other),
            TEnum enumValue => Equals(enumValue),
            _ => false,
        };

    /// <inheritdoc/>
    public bool Equals(FlagsEnum<TEnum> other)
        => EqualityComparer<TEnum>.Default.Equals(_value, other._value);

    /// <inheritdoc/>
    public bool Equals(TEnum other)
        => EqualityComparer<TEnum>.Default.Equals(_value, other);
}
