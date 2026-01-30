using System.Diagnostics;
using System.Text.Json.Serialization;

namespace Altinn.Authorization.ProblemDetails;

/// <summary>
/// A <see cref="Microsoft.AspNetCore.Mvc.ProblemDetails"/> with an <see cref="ErrorCode"/>.
/// </summary>
public class AltinnProblemDetails
    : Microsoft.AspNetCore.Mvc.ProblemDetails
    , IJsonOnDeserializing
{
    private string? _statusDescription;

    /// <summary>
    /// Initializes a new instance of the <see cref="AltinnProblemDetails"/> class.
    /// </summary>
    /// <param name="descriptor">The problem descriptor.</param>
    internal AltinnProblemDetails(ProblemDescriptor descriptor)
    {
        Type = $"urn:altinn:error:{descriptor.ErrorCode}";
        ErrorCode = descriptor.ErrorCode;
        Status = (int)descriptor.StatusCode;
        Title = descriptor.Title;
        TraceId = Activity.Current?.Id;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AltinnProblemDetails"/> class.
    /// </summary>
    /// <param name="instance">The problem instance.</param>
    internal AltinnProblemDetails(ProblemInstance instance)
        : this(instance.Descriptor)
    {
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

    /// <summary>
    /// Gets or sets the unique identifier used to trace a request or operation across system boundaries.
    /// </summary>
    [JsonPropertyName("traceId")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyOrder(1)]
    public string? TraceId { get; set; }

    /// <summary>
    /// Gets or sets a human-readable description of the (HTTP) status code.
    /// </summary>
    [JsonPropertyName("statusDescription")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyOrder(2)]
    public string? StatusDescription
    {
        get => _statusDescription ??= StatusDescriptions.GetStatusDescription(Status);
        set => _statusDescription = value;
    }

    /// <inheritdoc/>
    void IJsonOnDeserializing.OnDeserializing()
    {
        // reset values set by subclasses that hard-code a ProblemDescriptor
        ErrorCode = default;
        Status = default;
        Detail = default;
    }
}
