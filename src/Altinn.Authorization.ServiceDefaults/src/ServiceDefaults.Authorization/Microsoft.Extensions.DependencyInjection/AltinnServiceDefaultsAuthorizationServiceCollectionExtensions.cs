using Altinn.Authorization.ServiceDefaults.Authorization.Scopes;
using Altinn.Authorization.ServiceDefaults.Authorization.Scopes.PlatformAccessToken;
using Azure.Core;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

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
        services.AddPlatformAccessTokenHandler();

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
    /// Adds an authorization handler that validates platform access tokens.
    /// </summary>
    /// <param name="services">The service collection to which the authorization handler will be added. Cannot be null.</param>
    /// <param name="configure">Optional configuration delegate.</param>
    /// <returns><paramref name="services"/>.</returns>
    public static IServiceCollection AddPlatformAccessTokenHandler(this IServiceCollection services, Action<PlatformAccessTokenSettings>? configure = null)
    {
        var options = services.AddOptions<PlatformAccessTokenSettings>();
        if (configure is not null)
        { 
            options.Configure(configure);
        }

        services.TryAddSingleton<IPlatformAccessTokenSigningKeyProvider, DefaultPlatformAccessTokenSigningKeyProvider>();
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IAuthorizationHandler, DefaultPlatformAccessTokenHandler>());

#if NET9_0_OR_GREATER
        services.AddHybridCache();
#else
        services.AddMemoryCache();
#endif

        services.AddOptions<PlatformAccessTokenKeyVaultSettings>().ValidateDataAnnotations().BindConfiguration("kvSetting");
        services.TryAddKeyedSingleton<TokenCredential>(
            serviceKey: typeof(PlatformAccessTokenSettings),
            (services, _) =>
            {
                var settings = services.GetRequiredService<IOptions<PlatformAccessTokenKeyVaultSettings>>().Value;

                List<TokenCredential> credentialList = [];
                if (!string.IsNullOrEmpty(settings.ClientId)
                    && !string.IsNullOrEmpty(settings.TenantId)
                    && !string.IsNullOrEmpty(settings.ClientSecret))
                {
                    credentialList.Add(new ClientSecretCredential(
                        tenantId: settings.TenantId,
                        clientId: settings.ClientId,
                        clientSecret: settings.ClientSecret));
                }

                credentialList.Add(new EnvironmentCredential());
                credentialList.Add(new WorkloadIdentityCredential());
                credentialList.Add(new ManagedIdentityCredential());

                return new ChainedTokenCredential([.. credentialList]);
            });

        services.TryAddKeyedSingleton<SecretClient>(
            serviceKey: typeof(PlatformAccessTokenSettings),
            (services, _) =>
            {
                var settings = services.GetRequiredService<IOptions<PlatformAccessTokenKeyVaultSettings>>().Value;
                var credentials = services.GetRequiredKeyedService<TokenCredential>(serviceKey: typeof(PlatformAccessTokenSettings));

                return new SecretClient(settings.SecretUri, credentials);
            });

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
