using CommunityToolkit.Diagnostics;
using System.Diagnostics;

namespace Altinn.Authorization.ProblemDetails;

/// <summary>
/// An immutable descriptor for a validation error.
/// </summary>
[DebuggerDisplay("{ErrorCode,nq}: {Detail,nq}")]
public sealed record class ValidationErrorDescriptor
{
    /// <summary>
    /// Gets the error code.
    /// </summary>
    public ErrorCode ErrorCode { get; }

    /// <summary>
    /// Gets the error details.
    /// </summary>
    public string Detail { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationErrorDescriptor"/> class.
    /// </summary>
    /// <param name="errorCode">The error code.</param>
    /// <param name="detail">The error description.</param>
    internal ValidationErrorDescriptor(ErrorCode errorCode, string detail)
    {
        Guard.IsNotDefault(errorCode);
        Guard.IsNotNullOrWhiteSpace(detail);

        ErrorCode = errorCode;
        Detail = detail;
    }
}
