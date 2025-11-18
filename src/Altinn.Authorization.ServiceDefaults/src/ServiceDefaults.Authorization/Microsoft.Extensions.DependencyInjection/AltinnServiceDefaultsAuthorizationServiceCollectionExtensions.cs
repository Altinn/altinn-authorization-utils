using Altinn.Authorization.ServiceDefaults.Authorization.Scopes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Altinn authorization <see cref="IServiceCollection"/> extensions.
/// </summary>
public static class AltinnServiceDefaultsAuthorizationServiceCollectionExtensions
{
    /// <summary>
    /// Registers Altinn scopes-based authorization handlers with the specified service collection.
    /// </summary>
    /// <param name="services">The service collection to which the authorization handler will be added. Cannot be null.</param>
    /// <returns><paramref name="services"/>.</returns>
    public static IServiceCollection AddAltinnScopesAuthorizationHandlers(this IServiceCollection services)
    {
        services.AddScopeAnyOfAuthorizationHandler();

        return services;
    }

    /// <summary>
    /// Registers the ScopeAnyOfAuthorizationHandler for scope-based authorization in the application's dependency
    /// injection container.
    /// </summary>
    /// <param name="services">The service collection to which the authorization handler will be added. Cannot be null.</param>
    /// <returns><paramref name="services"/>.</returns>
    public static IServiceCollection AddScopeAnyOfAuthorizationHandler(this IServiceCollection services)
    {
        services.AddAuthorizationScopeProvider();
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IAuthorizationHandler, ScopeAnyOfAuthorizationHandler>());

        return services;
    }

    /// <summary>
    /// Registers the default implementation of <see cref="IAuthorizationScopeProvider"/> in <paramref name="services"/>.
    /// </summary>
    /// <param name="services">The service collection to which the authorization handler will be added. Cannot be null.</param>
    /// <returns><paramref name="services"/>.</returns>
    public static IServiceCollection AddAuthorizationScopeProvider(this IServiceCollection services)
    {
        services.TryAddSingleton<IAuthorizationScopeProvider, DefaultAuthorizationScopeProvider>();

        return services;
    }
}
