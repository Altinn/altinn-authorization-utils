using System.Text;

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
    /// <param name="problemInstance">The <see cref="ProblemInstance"/>.</param>
    public ProblemInstanceException(ProblemInstance problemInstance)
        : this(null, problemInstance)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ProblemInstanceException"/> class.
    /// </summary>
    /// <param name="message">
    /// The message. If <see langword="null"/>, the message is generated from <paramref name="problemInstance"/>.
    /// </param>
    /// <param name="problemInstance">The <see cref="ProblemInstance"/>.</param>
    public ProblemInstanceException(string? message, ProblemInstance problemInstance)
        : this(message, null, problemInstance)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ProblemInstanceException"/> class.
    /// </summary>
    /// <param name="message">
    /// The message. If <see langword="null"/>, the message is generated from <paramref name="problemInstance"/>.
    /// </param>
    /// <param name="innerException">
    /// The inner exception. If <see langword="null"/>, the exception is taken from <paramref name="problemInstance"/>, if it has one.
    /// </param>
    /// <param name="problemInstance">The <see cref="ProblemInstance"/>.</param>
    public ProblemInstanceException(string? message, Exception? innerException, ProblemInstance problemInstance)
        : base(message ?? CreateErrorMessage(problemInstance), innerException ?? problemInstance.Exception)
    {
        Problem = problemInstance;
    }

    /// <summary>
    /// Gets the <see cref="Problem"/>.
    /// </summary>
    public ProblemInstance Problem { get; }

    private static string CreateErrorMessage(ProblemInstance problemInstance)
    {
        var sb = new StringBuilder(problemInstance.Title);
        if (!string.IsNullOrEmpty(problemInstance.Detail))
        {
            sb.Append(" - ");
            sb.AppendLine(problemInstance.Detail);
        }

        sb.AppendLine();
        sb.AppendLine($"code: {problemInstance.ErrorCode}");

        problemInstance.AddExceptionDetails(sb, string.Empty);

        return sb.ToString();
    }
}
