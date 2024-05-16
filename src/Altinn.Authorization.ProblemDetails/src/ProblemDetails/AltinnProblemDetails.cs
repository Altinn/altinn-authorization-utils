using System.Net;
using System.Text.Json.Serialization;

namespace Altinn.Authorization.ProblemDetails;

/// <summary>
/// A <see cref="Microsoft.AspNetCore.Mvc.ProblemDetails"/> with an <see cref="ErrorCode"/>.
/// </summary>
public sealed class AltinnProblemDetails
    : Microsoft.AspNetCore.Mvc.ProblemDetails
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AltinnProblemDetails"/> class.
    /// </summary>
    /// <param name="errorCode">The error code.</param>
    /// <param name="status">The <see cref="HttpStatusCode"/>.</param>
    /// <param name="detail">Error details (message).</param>
    public AltinnProblemDetails(ErrorCode errorCode, HttpStatusCode status, string detail)
    {
        ErrorCode = errorCode;
        Status = (int)status;
        Detail = detail;
    }

    [JsonConstructor]
    private AltinnProblemDetails()
    {
    }

    /// <summary>
    /// Gets or sets the error code.
    /// </summary>
    [JsonPropertyName("code")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public ErrorCode ErrorCode { get; set; }
}
