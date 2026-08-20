using Microsoft.Extensions.DependencyInjection;

namespace Altinn.Authorization.CommandLine.Factory.Resolvers;

/// <summary>
/// Defines a parameter resolver that resolves the value of a service parameter from the <see cref="IServiceProvider"/>.
/// </summary>
internal abstract class ServiceParameterResolver
    : ICommandHandlerParameterResolver
{
    public static ServiceParameterResolver Create(Type serviceType, object? serviceKey, bool isRequired)
        => (isRequired, serviceKey) switch
        {
            (true, null) => (ServiceParameterResolver)Activator.CreateInstance(typeof(RequiredService<>).MakeGenericType(serviceType))!,
            (false, null) => (ServiceParameterResolver)Activator.CreateInstance(typeof(Service<>).MakeGenericType(serviceType))!,
            (true, _) => (ServiceParameterResolver)Activator.CreateInstance(typeof(RequiredKeyedService<>).MakeGenericType(serviceType), serviceKey)!,
            (false, _) => (ServiceParameterResolver)Activator.CreateInstance(typeof(KeyedService<>).MakeGenericType(serviceType), serviceKey)!,
        };

    private protected abstract void SetValue(CommandHandlerParameterResolverContext context);

    public Task ResolveParameterValue(CommandHandlerParameterResolverContext context, CancellationToken cancellationToken)
    {
        SetValue(context);
        return Task.CompletedTask;
    }

    private sealed class Service<T>
        : ServiceParameterResolver
        where T : notnull
    {
        private protected override void SetValue(CommandHandlerParameterResolverContext context)
            => context.SetParameterValue(context.ApplicationServices.GetService<T>());
    }

    private sealed class RequiredService<T>
        : ServiceParameterResolver
        where T : notnull
    {
        private protected override void SetValue(CommandHandlerParameterResolverContext context)
            => context.SetParameterValue(context.ApplicationServices.GetRequiredService<T>());
    }

    private sealed class KeyedService<T>(object? serviceKey)
        : ServiceParameterResolver
        where T : notnull
    {
        private protected override void SetValue(CommandHandlerParameterResolverContext context)
            => context.SetParameterValue(context.ApplicationServices.GetKeyedService<T>(serviceKey));
    }

    private sealed class RequiredKeyedService<T>(object? serviceKey)
        : ServiceParameterResolver
        where T : notnull
    {
        private protected override void SetValue(CommandHandlerParameterResolverContext context)
            => context.SetParameterValue(context.ApplicationServices.GetRequiredKeyedService<T>(serviceKey));
    }
}
