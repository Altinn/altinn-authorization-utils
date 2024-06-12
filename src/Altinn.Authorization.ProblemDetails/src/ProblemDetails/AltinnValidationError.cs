using System.Collections.Immutable;
using System.Text.Json.Serialization;

namespace Altinn.Authorization.ProblemDetails;

/// <summary>
/// Represents a validation error.
/// </summary>
public sealed class AltinnValidationError
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AltinnValidationError"/> class.
    /// </summary>
    /// <param name="descriptor">The validation error descriptor.</param>
    internal AltinnValidationError(ValidationErrorDescriptor descriptor)
    {
        ErrorCode = descriptor.ErrorCode;
        Detail = descriptor.Detail;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AltinnValidationError"/> class.
    /// </summary>
    /// <param name="instance">The validation error instance.</param>
    internal AltinnValidationError(ValidationErrorInstance instance)
    {
        ErrorCode = instance.ErrorCode;
        Detail = instance.Detail;
        Paths = instance.Paths;

        if (!instance.Extensions.IsDefaultOrEmpty)
        {
            foreach (var (key, value) in instance.Extensions)
            {
                Extensions[key] = value;
            }
        }
    }

    [JsonConstructor]
    private AltinnValidationError()
    {
    }

    /// <summary>
    /// Gets or sets the error code.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    [JsonPropertyOrder(-3)]
    [JsonPropertyName("code")]
    public ErrorCode ErrorCode { get; set; }

    /// <summary>
    /// Gets or sets the error details.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyOrder(-2)]
    [JsonPropertyName("detail")]
    public string? Detail { get; set; }

    /// <summary>
    /// Gets or sets the error paths.
    /// </summary>
    /// <remarks>
    /// This SHOULD be a set of JSON Pointer values that identify the path(s) to the erroneous field(s) within the request document or parameters.
    /// </remarks>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    [JsonPropertyOrder(-1)]
    [JsonPropertyName("paths")]
    public ImmutableArray<string> Paths { get; set; }

    /// <summary>
    /// Gets the <see cref="IDictionary{TKey, TValue}"/> for extension members.
    /// <para>
    /// Problem type definitions MAY extend the problem details object with additional members. Extension members appear in the same namespace as
    /// other members of a problem type.
    /// </para>
    /// </summary>
    /// <remarks>
    /// The round-tripping behavior for <see cref="Extensions"/> is determined by the implementation of the Input \ Output formatters.
    /// In particular, complex types or collection types may not round-trip to the original type when using the built-in JSON or XML formatters.
    /// </remarks>
    [JsonExtensionData]
    public IDictionary<string, object?> Extensions { get; set; } = new Dictionary<string, object?>(StringComparer.Ordinal);
}
