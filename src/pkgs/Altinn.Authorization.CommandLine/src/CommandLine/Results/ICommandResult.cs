namespace Altinn.Authorization.CommandLine.Results;

/// <summary>
/// Defines a contract that represents the result of a command execution, which can be processed and written to the console.
/// </summary>
public interface ICommandResult
{
    /// <summary>
    /// Write the result to the console.
    /// </summary>
    /// <param name="context">The command invocation context.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    Task Execute(CommandInvocationContext context, CancellationToken cancellationToken = default);
}
