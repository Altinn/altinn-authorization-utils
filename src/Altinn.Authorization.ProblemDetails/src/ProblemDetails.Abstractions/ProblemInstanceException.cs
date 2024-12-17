namespace Altinn.Authorization.ProblemDetails;

/// <summary>
/// An exception that represents a <see cref="ProblemInstance"/>.
/// </summary>
public class ProblemInstanceException
    : InvalidOperationException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ProblemInstanceException"/> class.
    /// </summary>
    /// <param name="problemInstance">The <see cref="ProblemInstance"/>.</param>
    public ProblemInstanceException(ProblemInstance problemInstance)
        : base(problemInstance.Detail)
    {
        ProblemInstance = problemInstance;
    }

    /// <summary>
    /// Gets the <see cref="ProblemInstance"/>.
    /// </summary>
    public ProblemInstance ProblemInstance { get; }
}
