namespace Altinn.Authorization.CommandLine.Factory;

/// <summary>
/// Defines a service that can resolve the value of a command handler parameter.
/// </summary>
public interface ICommandHandlerParameterResolver
{
    /// <summary>
    /// Resolves the value of the parameter from the specified <see cref="CommandInvocationContext"/> and <see cref="CancellationToken"/>.
    /// </summary>
    /// <param name="context">The parameter resolver context.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The resolved parameter value.</returns>
    Task ResolveParameterValue(CommandHandlerParameterResolverContext context, CancellationToken cancellationToken);
}
