using CommunityToolkit.Diagnostics;
using System.Diagnostics;
using System.Net;

namespace Altinn.Authorization.ProblemDetails;

/// <summary>
/// An immutable descriptor for a problem.
/// </summary>
[DebuggerDisplay("{ErrorCode,nq}: {Title,nq}")]
public sealed record class ProblemDescriptor
{
    /// <summary>
    /// Gets the error code.
    /// </summary>
    public ErrorCode ErrorCode { get; }

    /// <summary>
    /// Gets the status code.
    /// </summary>
    public HttpStatusCode StatusCode { get; }

    /// <summary>
    /// Gets the error title.
    /// </summary>
    public string Title { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ProblemDescriptor"/> class.
    /// </summary>
    /// <param name="errorCode">The error code.</param>
    /// <param name="statusCode">The HTTP status code.</param>
    /// <param name="title">The error title.</param>
    internal ProblemDescriptor(ErrorCode errorCode, HttpStatusCode statusCode, string title)
    {
        Guard.IsNotDefault(errorCode);
        Guard.IsNotNullOrWhiteSpace(title);

        ErrorCode = errorCode;
        StatusCode = statusCode;
        Title = title;
    }
}
