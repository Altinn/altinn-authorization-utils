using System.Collections;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Altinn.Authorization.TestUtils;

/// <summary>
/// General string helpers for tests.
/// </summary>
public static partial class StringHelpers
{
    [GeneratedRegex("([A-Z])")]
    private static partial Regex PascalCaseRegex();

    /// <summary>
    /// Converts a PascalCase string into a spaced string with lowercase words.
    /// </summary>
    /// <param name="input">The PascalCase string to convert. Cannot be null.</param>
    /// <returns>A string where each word from the PascalCase input is separated by a space and converted to lowercase.</returns>
    public static string PascalToSpaced(string input)
        => PascalCaseRegex().Replace(input, static match => " " + match.Value.ToLower()).Trim();

    /// <summary>
    /// Converts the specified object to a string representation with additional formatting for certain types.
    /// </summary>
    /// <remarks>This method provides a consistent and human-readable string representation for a variety of
    /// object types, making it useful for debugging or logging purposes.</remarks>
    /// <param name="value">The object to convert to a string. Can be <see langword="null"/>.</param>
    /// <returns>A string representation of the object.</returns>
    public static string? ToStringAwesomely(object? value)
    {
        if (value == null)
        {
            return "null";
        }

        if (value is string)
        {
            return "\"" + value + "\"";
        }

        if (value is decimal)
        {
            return value + "m";
        }

        if (value is double)
        {
            return value + "d";
        }

        if (value is float)
        {
            return value + "f";
        }

        if (value is long)
        {
            return value + "L";
        }

        if (value is uint)
        {
            return value + "u";
        }

        if (value is ulong)
        {
            return value + "uL";
        }

        if (value is Type typeValue)
        {
            return TypeNameHelper.GetTypeDisplayName(typeValue);
        }

        var objectType = value.GetType();

        if (value is IEnumerable enumerable)
        {
            var objects = enumerable.Cast<object>();
            return "[" + string.Join(", ", objects.Select(ToStringAwesomely)) + "]";
        }

        if (value is Enum enumValue)
        {
            return ToStringEnum(enumValue);
        }

        if (value is DateTime dateTime)
        {
            return ToStringDateTime(dateTime);
        }

        if (value is ConstantExpression constantExpression)
        {
            return ToStringAwesomely(constantExpression.Value);
        }

        if (value is MemberExpression { Expression: ConstantExpression constant, Member: FieldInfo info })
        {
            return ToStringAwesomely(info.GetValue(constant.Value));
        }

        if (objectType.IsGenericType && objectType.GetGenericTypeDefinition() == typeof(KeyValuePair<,>))
        {
            var key = objectType.GetProperty("Key")!.GetValue(value, null);
            var v = objectType.GetProperty("Value")!.GetValue(value, null);
            return $"[{ToStringAwesomely(key)} => {ToStringAwesomely(v)}]";
        }

        var toString = value.ToString();
        if (toString == objectType.FullName)
        {
            return $"{value} ({value.GetHashCode():D6})";
        }

        return toString; // ToString() may return null.

        static string ToStringEnum(Enum value) =>
            value.GetType().Name + "." + value;

        static string ToStringDateTime(DateTime value) =>
            value.ToString("o");
    }
}
