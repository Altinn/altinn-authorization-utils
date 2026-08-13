using System.Diagnostics;

namespace Altinn.Authorization.CommandLine.Shell;

/// <summary>
/// Represents an exception that occurs related to a process.
/// </summary>
public class ProcessException
    : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ProcessException"/> class with the specified process and message.
    /// </summary>
    /// <param name="process">The process.</param>
    /// <param name="message">The error message.</param>
    public ProcessException(Process process, string message)
        : base(message)
    {
        Process = process;
    }

    /// <summary>
    /// Gets the process associated with this exception.
    /// </summary>
    public Process Process { get; }
}
