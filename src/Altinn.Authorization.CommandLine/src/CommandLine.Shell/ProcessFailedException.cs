using System.Diagnostics;

namespace Altinn.Authorization.CommandLine.Shell;

/// <summary>
/// Represents an exception that is thrown when a process fails to execute successfully.
/// </summary>
public class ProcessFailedException
    : ProcessException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ProcessFailedException"/> class with the specified process and message.
    /// </summary>
    /// <param name="process">The process.</param>
    /// <param name="message">The error message.</param>
    public ProcessFailedException(Process process, string message)
        : base(process, message)
    {
    }
}
