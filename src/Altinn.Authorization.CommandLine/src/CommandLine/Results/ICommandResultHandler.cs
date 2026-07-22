namespace Altinn.Authorization.CommandLine.Results;

/// <summary>
/// Interface for handling command results of a specific type.
/// </summary>
/// <typeparam name="T">The result type.</typeparam>
public interface ICommandResultHandler<T>
{
    /// <summary>
    /// Handles the command result of type <typeparamref name="T"/>.
    /// </summary>
    /// <param name="result">The command result.</param>
    /// <param name="context">The command invocation context.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task HandleResult(T result, CommandInvocationContext context, CancellationToken cancellationToken = default);
}
