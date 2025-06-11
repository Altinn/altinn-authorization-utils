using System.Collections.Immutable;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;

namespace Altinn.Authorization.TestUtils.Shouldly;

/// <summary>
/// Extensions for Shouldly to compare JSON structures and perform round-trip serialization checks.
/// </summary>
[ShouldlyMethods]
[DebuggerStepThrough]
[EditorBrowsable(EditorBrowsableState.Never)]
public static class ShouldlyJsonExtensions
{
    /// <summary>
    /// Verifies that the specified object can be serialized to JSON and deserialized back, producing a result that
    /// matches the provided expected JSON string.
    /// </summary>
    /// <remarks>This method performs a structural comparison of the JSON representations to ensure they are
    /// equivalent, and also verifies that deserializing the expected JSON produces an object equal to the
    /// original.</remarks>
    /// <typeparam name="T">The type of the object being tested for JSON round-tripping.</typeparam>
    /// <param name="actual">The object to be serialized and compared against the expected JSON.</param>
    /// <param name="expected">The JSON string representing the expected serialized form of the object.</param>
    /// <param name="customMessage">An optional custom message to include in the exception if the assertion fails.</param>
    /// <exception cref="ShouldAssertException">Thrown if the serialized form of <paramref name="actual"/> does not match <paramref name="expected"/>, or if
    /// deserializing <paramref name="expected"/> does not produce an object equal to <paramref name="actual"/>.</exception>
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

    /// <summary>
    /// Compares the JSON serialization of the specified object to an expected JSON string and throws an exception if
    /// they are not structurally equivalent.
    /// </summary>
    /// <remarks>This method performs a structural comparison of the JSON representation of the object and the
    /// expected JSON string. The comparison is case-sensitive and considers both the structure and values of the JSON
    /// elements.</remarks>
    /// <typeparam name="T">The type of the object to serialize and compare.</typeparam>
    /// <param name="actual">The object to serialize and compare against the expected JSON string.</param>
    /// <param name="expected">A JSON-formatted string representing the expected structure and values.</param>
    /// <param name="customMessage">An optional custom message to include in the exception if the comparison fails. If <see langword="null"/>, a
    /// default message will be used.</param>
    /// <returns>A <see cref="JsonDocument"/> representing the serialized JSON structure of the <paramref name="actual"/> object.</returns>
    /// <exception cref="ShouldAssertException">Thrown if the serialized JSON structure of <paramref name="actual"/> does not match the structure and values of
    /// the <paramref name="expected"/> JSON string.</exception>
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

    /// <summary>
    /// Asserts that the specified <see cref="JsonDocument"/> instances are structurally equivalent.
    /// </summary>
    /// <remarks>Structural equivalence compares the JSON structure, including keys and values, but may not
    /// account for differences in formatting or ordering of properties unless they affect the structure.</remarks>
    /// <param name="actual">The actual <see cref="JsonDocument"/> to compare.</param>
    /// <param name="expected">The expected <see cref="JsonDocument"/> to compare against.</param>
    /// <param name="customMessage">An optional custom message to include in the exception if the assertion fails. If not provided, a default
    /// message will be used.</param>
    /// <exception cref="ShouldAssertException">Thrown if the <paramref name="actual"/> and <paramref name="expected"/> documents are not structurally equivalent.</exception>
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

    /// <summary>
    /// Asserts that the specified <see cref="JsonDocument"/> is structurally equivalent to the given <see
    /// cref="JsonElement"/>.
    /// </summary>
    /// <remarks>Structural equivalence means that the JSON structure, including keys, values, and their
    /// hierarchy, matches between the <paramref name="actual"/> and <paramref name="expected"/>.</remarks>
    /// <param name="actual">The <see cref="JsonDocument"/> to be compared.</param>
    /// <param name="expected">The <see cref="JsonElement"/> to compare against.</param>
    /// <param name="customMessage">An optional custom message to include in the exception if the assertion fails. If null, a default message will
    /// be used.</param>
    /// <exception cref="ShouldAssertException">Thrown if the <paramref name="actual"/> document is not structurally equivalent to the <paramref
    /// name="expected"/> element.</exception>
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

    /// <summary>
    /// Asserts that the specified <see cref="JsonDocument"/> is structurally equivalent to the provided JSON string.
    /// </summary>
    /// <remarks>Structural equivalence checks ensure that the JSON structure, including keys and value types,
    /// matches between the actual and expected JSON. Differences in formatting, such as whitespace or property order, 
    /// are ignored.</remarks>
    /// <param name="actual">The <see cref="JsonDocument"/> to compare.</param>
    /// <param name="expected">A JSON string representing the expected structure.</param>
    /// <param name="customMessage">An optional custom message to include in the exception if the assertion fails.  If not provided, a default
    /// message will be used.</param>
    /// <exception cref="ShouldAssertException">Thrown if the structure of <paramref name="actual"/> does not match the structure of <paramref
    /// name="expected"/>.</exception>
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

    /// <summary>
    /// Asserts that the specified <see cref="JsonElement"/> is structurally equivalent to the root element of the given
    /// <see cref="JsonDocument"/>.
    /// </summary>
    /// <remarks>Structural equivalence means that the JSON structure, including keys, values, and their
    /// hierarchy, matches between the two elements. This method is typically used in testing scenarios to validate JSON
    /// content.</remarks>
    /// <param name="actual">The <see cref="JsonElement"/> to compare.</param>
    /// <param name="expected">The <see cref="JsonDocument"/> containing the root element to compare against.</param>
    /// <param name="customMessage">An optional custom message to include in the exception if the assertion fails. If not provided, a default
    /// message will be used.</param>
    /// <exception cref="ShouldAssertException">Thrown if the <paramref name="actual"/> <see cref="JsonElement"/> is not structurally equivalent to the root
    /// element of <paramref name="expected"/>.</exception>
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

    /// <summary>
    /// Asserts that the specified <see cref="JsonElement"/> instances are structurally equivalent.
    /// </summary>
    /// <remarks>Structural equivalence means that the JSON structure, including keys, values, and their
    /// types, matches between the two elements. Differences in formatting, such as whitespace or property order, are
    /// ignored.</remarks>
    /// <param name="actual">The actual <see cref="JsonElement"/> to compare.</param>
    /// <param name="expected">The expected <see cref="JsonElement"/> to compare against.</param>
    /// <param name="customMessage">An optional custom message to include in the exception if the assertion fails.</param>
    /// <exception cref="ShouldAssertException">Thrown if the <paramref name="actual"/> and <paramref name="expected"/> <see cref="JsonElement"/> instances are
    /// not structurally equivalent.</exception>
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

    /// <summary>
    /// Asserts that the specified <see cref="JsonElement"/> is structurally equivalent to the provided JSON string.
    /// </summary>
    /// <remarks>Structural equivalence checks ensure that the JSON structures are the same, including
    /// property names, value types, and array contents, but do not consider formatting or property order.</remarks>
    /// <param name="actual">The <see cref="JsonElement"/> to compare.</param>
    /// <param name="expected">A JSON string representing the expected structure.</param>
    /// <param name="customMessage">An optional custom message to include in the exception if the assertion fails. If null, a default message will
    /// be used.</param>
    /// <exception cref="ShouldAssertException">Thrown if the <paramref name="actual"/> JSON structure does not match the <paramref name="expected"/> structure.</exception>
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
