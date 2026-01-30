using CommunityToolkit.Diagnostics;
using System.Diagnostics;

namespace Altinn.Authorization.ProblemDetails;

/// <summary>
/// An immutable descriptor for a validation error.
/// </summary>
[DebuggerDisplay("{ErrorCode,nq}: {Title,nq}")]
public sealed record class ValidationErrorDescriptor
{
    /// <summary>
    /// Gets the error code.
    /// </summary>
    public ErrorCode ErrorCode { get; }

    /// <summary>
    /// Gets the error title.
    /// </summary>
    public string Title { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationErrorDescriptor"/> class.
    /// </summary>
    /// <param name="errorCode">The error code.</param>
    /// <param name="title">The error title.</param>
    internal ValidationErrorDescriptor(ErrorCode errorCode, string title)
    {
        Guard.IsNotDefault(errorCode);
        Guard.IsNotNullOrWhiteSpace(title);

        ErrorCode = errorCode;
        Title = title;
    }
}
