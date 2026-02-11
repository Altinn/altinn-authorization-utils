using CommunityToolkit.Diagnostics;
using System.Diagnostics;

namespace Altinn.Authorization.ProblemDetails;

/// <summary>
/// Tracks error codes within a domain to ensure uniqueness during development.
/// This tracking is only active in DEBUG builds.
/// </summary>
internal sealed class ErrorCodeTracking
{
    private readonly HashSet<uint> _usedCodes = new();
#if NET9_0_OR_GREATER
    private readonly Lock _lock = new();
#else
    private readonly object _lock = new();
#endif

    /// <summary>
    /// Initializes a new instance of the <see cref="ErrorCodeTracking"/> class.
    /// </summary>
    public ErrorCodeTracking()
    {
    }

    /// <summary>
    /// Tracks an error code to ensure it hasn't been registered before.
    /// This method only executes in DEBUG builds.
    /// </summary>
    /// <param name="code">The numeric error code to track.</param>
    /// <param name="display">The error code display information for error reporting.</param>
    /// <exception cref="InvalidOperationException">Thrown when the error code has already been registered.</exception>
    [Conditional("DEBUG")]
    public void Track(uint code, ErrorCode display)
    {
        bool added;
        lock (_lock)
        {
            added = _usedCodes.Add(code);
        }

        if (!added)
        {
            ThrowHelper.ThrowInvalidOperationException($"Error code '{display}' has already been registered.");
        }
    }
}
