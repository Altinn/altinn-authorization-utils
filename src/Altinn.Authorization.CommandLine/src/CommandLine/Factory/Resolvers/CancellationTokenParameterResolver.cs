namespace Altinn.Authorization.CommandLine.Factory.Resolvers;

/// <summary>
/// Defines a parameter resolver that resolves the value of a <see cref="CancellationToken"/> parameter.
/// </summary>
internal sealed class CancellationTokenParameterResolver
    : ICommandHandlerParameterResolver
{
    public static readonly CancellationTokenParameterResolver Instance = new();

    public Task<object?> ResolveParameterValue(CommandInvocationContext invocationContext, CancellationToken cancellationToken)
    {
        return Task.FromResult<object?>(cancellationToken);
    }
}
