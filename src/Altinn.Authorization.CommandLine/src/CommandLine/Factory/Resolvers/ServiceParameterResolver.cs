using Microsoft.Extensions.DependencyInjection;

namespace Altinn.Authorization.CommandLine.Factory.Resolvers;

/// <summary>
/// Defines a parameter resolver that resolves the value of a service parameter from the <see cref="IServiceProvider"/>.
/// </summary>
internal sealed class ServiceParameterResolver
    : ICommandHandlerParameterResolver
{
    private readonly Type _serviceType;
    private readonly object? _serviceKey;
    private readonly bool _isRequired;

    public ServiceParameterResolver(Type serviceType, object? serviceKey, bool isRequired)
    {
        _serviceType = serviceType;
        _serviceKey = serviceKey;
        _isRequired = isRequired;
    }

    public Task<object?> ResolveParameterValue(
        CommandInvocationContext invocationContext,
        CancellationToken cancellationToken)
    {
        var service = (_isRequired, _serviceKey) switch
        {
            (false, null) => invocationContext.ApplicationServices.GetService(_serviceType),
            (false, _) => invocationContext.ApplicationServices.GetKeyedService(_serviceType, _serviceKey),
            (true, null) => invocationContext.ApplicationServices.GetRequiredService(_serviceType),
            (true, _) => invocationContext.ApplicationServices.GetRequiredKeyedService(_serviceType, _serviceKey),
        };

        return Task.FromResult(service);
    }
}
