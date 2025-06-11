using System.Collections.Immutable;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;

namespace Altinn.Authorization.TestUtils.Shouldly;

[ShouldlyMethods]
[DebuggerStepThrough]
[ExcludeFromCodeCoverage]
[EditorBrowsable(EditorBrowsableState.Never)]
internal static class ShouldlyJsonExtensions
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void ShouldJsonRoundTripAs<T>(
        this T actual,
        [StringSyntax(StringSyntaxAttribute.Json)] string expected,
        string? customMessage = null)
    {
        using var actualDoc = Json.SerializeToDocument(actual);
        using var expectedDoc = JsonDocument.Parse(expected);

        if (!StructurallyCompare(actualDoc.RootElement, expectedDoc.RootElement, out var errors))
        {
            throw new ShouldAssertException(new JsonActualShouldlyMessage(actualDoc.RootElement, expectedDoc.RootElement, errors, customMessage).ToString());
        }

        var deserialized = Json.Deserialize<T>(expectedDoc);
        if (!EqualityComparer<T>.Default.Equals(actual, deserialized))
        {
            throw new ShouldAssertException(new ExpectedActualShouldlyMessage(actual, deserialized, customMessage).ToString());
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static JsonDocument ShouldJsonSerializeAs<T>(
        this T actual,
        [StringSyntax(StringSyntaxAttribute.Json)] string expected,
        string? customMessage = null)
    {
        var actualDoc = Json.SerializeToDocument(actual);
        using var expectedDoc = JsonDocument.Parse(expected);

        if (!StructurallyCompare(actualDoc.RootElement, expectedDoc.RootElement, out var errors))
        {
            throw new ShouldAssertException(new JsonActualShouldlyMessage(actualDoc.RootElement, expectedDoc.RootElement, errors, customMessage).ToString());
        }

        return actualDoc;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void ShouldBeStructurallyEquivalentTo(
        this JsonDocument actual,
        JsonDocument expected,
        string? customMessage = null)
    {
        if (!StructurallyCompare(actual.RootElement, expected.RootElement, out var errors))
        {
            throw new ShouldAssertException(new JsonActualShouldlyMessage(actual.RootElement, expected.RootElement, errors, customMessage).ToString());
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void ShouldBeStructurallyEquivalentTo(
        this JsonDocument actual,
        JsonElement expected,
        string? customMessage = null)
    {
        if (!StructurallyCompare(actual.RootElement, expected, out var errors))
        {
            throw new ShouldAssertException(new JsonActualShouldlyMessage(actual.RootElement, expected, errors, customMessage).ToString());
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void ShouldBeStructurallyEquivalentTo(
        this JsonDocument actual,
        string expected,
        string? customMessage = null)
    {
        using var expectedDoc = JsonDocument.Parse(expected);
        if (!StructurallyCompare(actual.RootElement, expectedDoc.RootElement, out var errors))
        {
            throw new ShouldAssertException(new JsonActualShouldlyMessage(actual.RootElement, expectedDoc.RootElement, errors, customMessage).ToString());
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void ShouldBeStructurallyEquivalentTo(
        this JsonElement actual,
        JsonDocument expected,
        string? customMessage = null)
    {
        if (!StructurallyCompare(actual, expected.RootElement, out var errors))
        {
            throw new ShouldAssertException(new JsonActualShouldlyMessage(actual, expected.RootElement, errors, customMessage).ToString());
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void ShouldBeStructurallyEquivalentTo(
        this JsonElement actual,
        JsonElement expected,
        string? customMessage = null)
    {
        if (!StructurallyCompare(actual, expected, out var errors))
        {
            throw new ShouldAssertException(new JsonActualShouldlyMessage(actual, expected, errors, customMessage).ToString());
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void ShouldBeStructurallyEquivalentTo(
        this JsonElement actual,
        string expected,
        string? customMessage = null)
    {
        using var expectedDoc = JsonDocument.Parse(expected);
        if (!StructurallyCompare(actual, expectedDoc.RootElement, out var errors))
        {
            throw new ShouldAssertException(new JsonActualShouldlyMessage(actual, expectedDoc.RootElement, errors, customMessage).ToString());
        }
    }

    private static bool StructurallyCompare(
        JsonElement actual,
        JsonElement expected,
        out ImmutableArray<JsonStructureError> errors)
    {
        var pathBuilder = new StringBuilder();
        var errorsBuilder = ImmutableArray.CreateBuilder<JsonStructureError>();
        StructurallyCompare(actual, expected, pathBuilder, errorsBuilder);

        if (errorsBuilder.Count > 0)
        {
            errors = errorsBuilder.ToImmutable();
            return false;
        }

        errors = [];
        return true;
    }

    private static void StructurallyCompare(
        JsonElement actual,
        JsonElement expected,
        StringBuilder path,
        ImmutableArray<JsonStructureError>.Builder errors)
    {
        if (actual.ValueKind != expected.ValueKind)
        {
            errors.Add(JsonStructureError.Create(path, $"Expected a {expected.ValueKind}, but received a {actual.ValueKind}."));
            return;
        }

        switch (actual.ValueKind)
        {
            case JsonValueKind.Object:
                StructurallyCompareObjects(actual, expected, path, errors);
                break;

            case JsonValueKind.Array:
                StructurallyCompareArrays(actual, expected, path, errors);
                break;

            case JsonValueKind.String:
                StructurallyCompareStrings(actual, expected, path, errors);
                break;

            case JsonValueKind.Number:
                StructurallyCompareNumbers(actual, expected, path, errors);
                break;

            case JsonValueKind.True:
            case JsonValueKind.False:
            case JsonValueKind.Null:
            case JsonValueKind.Undefined:
                // These only require the type to be the same as they carry no data.
                break;

            default:
                Unreachable();
                break;
        }
    }

    private static void StructurallyCompareStrings(
        JsonElement actual,
        JsonElement expected,
        StringBuilder path,
        ImmutableArray<JsonStructureError>.Builder errors)
    {
        Debug.Assert(actual.ValueKind == JsonValueKind.String);
        Debug.Assert(expected.ValueKind == JsonValueKind.String);

        if (!string.Equals(actual.GetString(), expected.GetString(), StringComparison.Ordinal))
        {
            errors.Add(JsonStructureError.Create(path, $"Expected string '{expected.GetString()}', but received '{actual.GetString()}'."));
        }
    }

    private static void StructurallyCompareNumbers(
        JsonElement actual,
        JsonElement expected,
        StringBuilder path,
        ImmutableArray<JsonStructureError>.Builder errors)
    {
        Debug.Assert(actual.ValueKind == JsonValueKind.Number);
        Debug.Assert(expected.ValueKind == JsonValueKind.Number);

        if (actual.TryGetInt64(out var actualI64)
            && expected.TryGetInt64(out var expectedI64))
        {
            if (actualI64 != expectedI64)
            {
                errors.Add(JsonStructureError.Create(path, $"Expected number '{expectedI64}', but received '{actualI64}'."));
            }

            return;
        }

        if (actual.TryGetUInt64(out var actualU64)
            && expected.TryGetUInt64(out var expectedU64))
        {
            if (actualU64 != expectedU64)
            {
                errors.Add(JsonStructureError.Create(path, $"Expected number '{expectedU64}', but received '{actualU64}'."));
            }

            return;
        }

        if (actual.TryGetDecimal(out var actualDecimal)
            && expected.TryGetDecimal(out var expectedDecimal))
        {
            if (actualDecimal != expectedDecimal)
            {
                errors.Add(JsonStructureError.Create(path, $"Expected number '{expectedDecimal}', but received '{actualDecimal}'."));
            }

            return;
        }

        if (actual.TryGetDouble(out var actualDouble)
            && expected.TryGetDouble(out var expectedDouble))
        {
            if (actualDouble != expectedDouble)
            {
                errors.Add(JsonStructureError.Create(path, $"Expected number '{expectedDouble}', but received '{actualDouble}'."));
            }

            return;
        }

        var actualText = actual.GetRawText();
        var expectedText = expected.GetRawText();
        if (!string.Equals(actualText, expectedText, StringComparison.Ordinal))
        {
            errors.Add(JsonStructureError.Create(path, $"Expected number '{expectedText}', but received '{actualText}'."));
        }
    }

    private static void StructurallyCompareArrays(
        JsonElement actual,
        JsonElement expected,
        StringBuilder path,
        ImmutableArray<JsonStructureError>.Builder errors)
    {
        Debug.Assert(actual.ValueKind == JsonValueKind.Array);
        Debug.Assert(expected.ValueKind == JsonValueKind.Array);

        var actualLength = actual.GetArrayLength();
        var expectedLength = expected.GetArrayLength();
        var minLength = Math.Min(actualLength, expectedLength);

        if (actualLength != expectedLength)
        {
            errors.Add(JsonStructureError.Create(path, $"Expected array of length {expectedLength}, but received {actualLength}."));
        }

        var actualEnumerator = actual.EnumerateArray().GetEnumerator();
        var expectedEnumerator = expected.EnumerateArray().GetEnumerator();

        for (var i = 0; i < minLength; i++)
        {
            if (!actualEnumerator.MoveNext() || !expectedEnumerator.MoveNext())
            {
                Unreachable();
            }

            var actualElement = actualEnumerator.Current;
            var expectedElement = expectedEnumerator.Current;
            var pathSuffix = $"/{i}";

            path.Append(pathSuffix);
            StructurallyCompare(actualElement, expectedElement, path, errors);
            path.Length -= pathSuffix.Length;
        }
    }

    private static void StructurallyCompareObjects(
        JsonElement actual,
        JsonElement expected,
        StringBuilder path,
        ImmutableArray<JsonStructureError>.Builder errors)
    {
        Debug.Assert(actual.ValueKind == JsonValueKind.Object);
        Debug.Assert(expected.ValueKind == JsonValueKind.Object);

        var actualProperties = GetProperties(actual);
        var expectedProperties = GetProperties(expected);

        foreach (var propertyName in actualProperties.Keys)
        {
            if (!expectedProperties.ContainsKey(propertyName))
            {
                errors.Add(JsonStructureError.Create(path, $"Unexpected property '{propertyName}' in actual JSON."));
            }
        }

        foreach (var (propertyName, expectedValue) in expectedProperties)
        {
            if (!actualProperties.TryGetValue(propertyName, out var actualValue))
            {
                errors.Add(JsonStructureError.Create(path, $"Missing property '{propertyName}' in actual JSON."));
                continue;
            }

            var pathSuffix = $"/{propertyName}";
            path.Append(pathSuffix);
            StructurallyCompare(actualValue, expectedValue, path, errors);
            path.Length -= pathSuffix.Length;
        }

        static Dictionary<string, JsonElement> GetProperties(JsonElement element)
        {
            var result = new Dictionary<string, JsonElement>(StringComparer.Ordinal);
            foreach (var property in element.EnumerateObject())
            {
                // later properties override earlier ones
                result[property.Name] = property.Value;
            }

            return result;
        }
    }

    private sealed class JsonActualShouldlyMessage
        : ShouldlyMessage
    {
        private readonly JsonElement _actual;
        private readonly JsonElement _expected;
        private readonly ImmutableArray<JsonStructureError> _errors;

        public JsonActualShouldlyMessage(
            JsonElement actual,
            JsonElement expected,
            ImmutableArray<JsonStructureError> errors,
            string? customMessage,
            [CallerMemberName] string shouldlyMethod = null!)
        {
            _actual = actual;
            _expected = expected;
            _errors = errors;

            ShouldlyAssertionContext = new ShouldlyAssertionContext(shouldlyMethod)
            {
            };
        }

        public override string ToString()
        {
            var context = ShouldlyAssertionContext;
            var codePart = context.CodePart;

            var actualString =
                $"""

                {PrettyPrint(_actual)}
                """;

            var expectedString =
                $"""

                {PrettyPrint(_expected)}
                """;

            var errors = string.Join(Environment.NewLine, _errors.Select(e => $"  - {e.Path}: {e.Message}"));

            var message =
                $"""
                 {codePart}
                     Should be structurally equivalent to{expectedString}
                     but was{actualString}
                
                 Errors:
                 {errors}
                 """;

            return message;
        }

        private static readonly JsonSerializerOptions _writerOptions = new JsonSerializerOptions { IndentCharacter = ' ', IndentSize = 2, WriteIndented = true };
        private static string PrettyPrint(JsonElement element)
        {
            if (element.ValueKind == JsonValueKind.Undefined)
            {
                return "";
            }

            return JsonSerializer.Serialize(element, _writerOptions);
        }
    }

    private readonly record struct JsonStructureError
    {
        public static JsonStructureError Create(StringBuilder path, string message)
        {
            var pathString =
                path.Length == 0
                ? "/"
                : path.ToString();

            return new JsonStructureError
            {
                Path = pathString,
                Message = message,
            };
        }

        public required string Path { get; init; }

        public required string Message { get; init; }
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void Unreachable()
        => throw new UnreachableException();
}
