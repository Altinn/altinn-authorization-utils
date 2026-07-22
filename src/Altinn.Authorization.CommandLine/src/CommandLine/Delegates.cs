namespace Altinn.Authorization.CommandLine;

/// <summary>
/// Represents a delegate that handles the invocation of a command, given a <see cref="CommandInvocationContext"/>.
/// </summary>
/// <param name="context">The context of the command invocation.</param>
/// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
public delegate Task CommandHandlerDelegate(CommandInvocationContext context, CancellationToken cancellationToken = default);

/// <summary>
/// Represents a delegate that handles the invocation of a command, given a <see cref="CommandInvocationContext"/> and a next delegate in the pipeline.
/// </summary>
/// <param name="context">The context of the command invocation.</param>
/// <param name="next">The next delegate to invoke.</param>
/// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
public delegate Task CommandHandlerMiddlewareDelegate(CommandInvocationContext context, CommandHandlerDelegate next, CancellationToken cancellationToken = default);
