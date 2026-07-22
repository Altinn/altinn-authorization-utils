namespace Altinn.Authorization.CommandLine.Results;

/// <summary>
/// Interface for handling command results.
/// </summary>
public interface ICommandResultHandler
{
    /// <summary>
    /// Handles the command result.
    /// </summary>
    /// <param name="result">The command result.</param>
    /// <param name="context">The command invocation context.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task HandleResult(object? result, CommandInvocationContext context, CancellationToken cancellationToken = default);
}
