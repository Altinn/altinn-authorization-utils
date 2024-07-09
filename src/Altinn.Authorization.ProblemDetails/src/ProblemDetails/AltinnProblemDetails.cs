using System.Text.Json.Serialization;

namespace Altinn.Authorization.ProblemDetails;

/// <summary>
/// A <see cref="Microsoft.AspNetCore.Mvc.ProblemDetails"/> with an <see cref="ErrorCode"/>.
/// </summary>
public class AltinnProblemDetails
    : Microsoft.AspNetCore.Mvc.ProblemDetails
    , IJsonOnDeserializing
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AltinnProblemDetails"/> class.
    /// </summary>
    /// <param name="descriptor">The problem descriptor.</param>
    internal AltinnProblemDetails(ProblemDescriptor descriptor)
    {
        ErrorCode = descriptor.ErrorCode;
        Status = (int)descriptor.StatusCode;
        Detail = descriptor.Detail;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AltinnProblemDetails"/> class.
    /// </summary>
    /// <param name="instance">The problem instance.</param>
    internal AltinnProblemDetails(ProblemInstance instance)
    {
        ErrorCode = instance.ErrorCode;
        Status = (int)instance.StatusCode;
        Detail = instance.Detail;

        if (!instance.Extensions.IsDefaultOrEmpty)
        {
            Extensions = instance.Extensions;
        }
    }

    [JsonConstructor]
    private AltinnProblemDetails()
        : base()
    {
    }

    /// <summary>
    /// Gets or sets the error code.
    /// </summary>
    [JsonPropertyName("code")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    [JsonPropertyOrder(0)]
    public ErrorCode ErrorCode { get; set; }

    /// <inheritdoc/>
    void IJsonOnDeserializing.OnDeserializing()
    {
        // reset values set by subclasses that hard-code a ProblemDescriptor
        ErrorCode = default;
        Status = default;
        Detail = default;
    }
}
