namespace Altinn.Authorization.ProblemDetails;

/// <summary>
/// An exception that represents a <see cref="Problem"/>.
/// </summary>
public class ProblemInstanceException
    : InvalidOperationException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ProblemInstanceException"/> class.
    /// </summary>
    /// <param name="problemInstance">The <see cref="Problem"/>.</param>
    public ProblemInstanceException(ProblemInstance problemInstance)
        : base(problemInstance.Detail)
    {
        Problem = problemInstance;
    }

    /// <summary>
    /// Gets the <see cref="Problem"/>.
    /// </summary>
    public ProblemInstance Problem { get; }
}
