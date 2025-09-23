using Altinn.Authorization.ServiceDefaults.HttpClient.PlatformAccessToken;
using Altinn.Common.AccessTokenClient.Services;
using CommunityToolkit.Diagnostics;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods for configuring http clients.
/// </summary>
public static class ServiceDefaultsHttpClientDependencyInjectionExtensions
{
    /// <summary>
    /// Add a platform access token provider if one has not already been registered.
    /// </summary>
    /// <typeparam name="TProvider">The provider type.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the service to.</param>
    /// <param name="lifetime">The service lifetime. Defaults to singleton.</param>
    /// <returns><paramref name="services"/>.</returns>
    public static IServiceCollection TryAddPlatformAccessTokenProvider<TProvider>(this IServiceCollection services, ServiceLifetime lifetime = ServiceLifetime.Singleton)
        where TProvider : class, IPlatformAccessTokenProvider
    {
        var descriptor = ServiceDescriptor.Describe(typeof(IPlatformAccessTokenProvider), typeof(TProvider), lifetime);
        services.TryAdd(descriptor);

        return services;
    }

    /// <summary>
    /// Configures the base address for HTTP requests made by the HttpClient instances created by the builder.
    /// </summary>
    /// <param name="builder">The <see cref="IHttpClientBuilder"/> to configure. Cannot be null.</param>
    /// <param name="baseAddress">The base address to use for HTTP requests. Cannot be null.</param>
    /// <returns><paramref name="builder"/>, so that configuration calls can be chained.</returns>
    public static IHttpClientBuilder ConfigureBaseAddress(this IHttpClientBuilder builder, Uri baseAddress)
        => builder.ConfigureHttpClient(client => client.BaseAddress = baseAddress);

    /// <summary>
    /// Configures the base address for HTTP requests made by the HttpClient instances created by the builder.
    /// </summary>
    /// <param name="builder">The <see cref="IHttpClientBuilder"/> to configure. Cannot be null.</param>
    /// <param name="baseAddress">The base address to use for HTTP requests. Cannot be null.</param>
    /// <returns><paramref name="builder"/>, so that configuration calls can be chained.</returns>
    public static IHttpClientBuilder ConfigureBaseAddress(this IHttpClientBuilder builder, string baseAddress)
        => builder.ConfigureBaseAddress(new Uri(baseAddress));

    /// <summary>
    /// Configures the base address of the HTTP client using a URI derived from the specified options type.
    /// </summary>
    /// <remarks>This method retrieves the options instance from the dependency injection container and sets
    /// the HTTP client's <see cref="System.Net.Http.HttpClient.BaseAddress"/> property using the provided delegate.
    /// This is useful for configuring HTTP clients with environment-specific or user-defined base addresses.</remarks>
    /// <typeparam name="TOptions">The type of options from which to retrieve the base URI. Must be a reference type.</typeparam>
    /// <param name="builder">The <see cref="IHttpClientBuilder"/> to configure. Cannot be null.</param>
    /// <param name="getBaseUri">A function that takes an instance of <typeparamref name="TOptions"/> and returns the base <see cref="Uri"/> to
    /// use for the HTTP client.</param>
    /// <returns><paramref name="builder"/>, so that configuration calls can be chained.</returns>
    public static IHttpClientBuilder ConfigureBaseAddressFromOptions<TOptions>(this IHttpClientBuilder builder, Func<TOptions, Uri> getBaseUri)
        where TOptions : class
        => builder.ConfigureHttpClient((services, client) =>
        {
            var settings = services.GetRequiredService<IOptions<TOptions>>().Value;
            client.BaseAddress = getBaseUri(settings);
        });

    /// <summary>
    /// Configures the base address of the HTTP client using a URI derived from the specified options type.
    /// </summary>
    /// <remarks>This method retrieves the options instance from the dependency injection container and sets
    /// the HTTP client's <see cref="System.Net.Http.HttpClient.BaseAddress"/> property using the provided delegate.
    /// This is useful for configuring HTTP clients with environment-specific or user-defined base addresses.</remarks>
    /// <typeparam name="TOptions">The type of options from which to retrieve the base URI. Must be a reference type.</typeparam>
    /// <param name="builder">The <see cref="IHttpClientBuilder"/> to configure. Cannot be null.</param>
    /// <param name="getBaseUri">A function that takes an instance of <typeparamref name="TOptions"/> and returns the base <see langword="string"/> to
    /// use for the HTTP client.</param>
    /// <returns><paramref name="builder"/>, so that configuration calls can be chained.</returns>
    public static IHttpClientBuilder ConfigureBaseAddressFromOptions<TOptions>(this IHttpClientBuilder builder, Func<TOptions, string> getBaseUri)
        where TOptions : class
        => builder.ConfigureBaseAddressFromOptions((TOptions options) => new Uri(getBaseUri(options)));

    /// <summary>
    /// Adds a message handler that automatically attaches a platform access token to outgoing HTTP requests.
    /// </summary>
    /// <remarks>This method adds a PlatformAccessTokenHandler to the HTTP client's message handler pipeline.
    /// Use this extension to ensure that all requests sent by the configured HTTP client include the necessary
    /// platform access token for authentication.</remarks>
    /// <param name="builder">The <see cref="IHttpClientBuilder"/> to configure. Cannot be null.</param>
    /// <returns><paramref name="builder"/>, so that configuration calls can be chained.</returns>
    public static IHttpClientBuilder AddPlatformAccessTokenHandler(this IHttpClientBuilder builder)
    {
        builder.Services.TryAddTransient<PlatformAccessTokenHandler>();

        return builder.AddHttpMessageHandler<PlatformAccessTokenHandler>();
    }

    /// <summary>
    /// Adds a test platform access token provider for local development to the service collection.
    /// </summary>
    /// <remarks>This method is intended for use only in local development environments. It throws an
    /// exception if called outside of a local development context. The test token generator provider enables the
    /// generation of platform access tokens for testing purposes.</remarks>
    /// <param name="services">The service collection to which the test token generator provider will be added.</param>
    /// <returns>The same service collection instance, to support method chaining.</returns>
    public static IServiceCollection TryAddTestPlatformTokenGeneratorProvider(this IServiceCollection services)
    {
        var serviceDescriptor = services.GetAltinnServiceDescriptor();
        if (!serviceDescriptor.IsLocalDev)
        {
            ThrowHelper.ThrowInvalidOperationException("Test token generator can only be used during local-dev");
        }

        var descriptor = ServiceDescriptor.Transient<IPlatformAccessTokenProvider>(s => s.GetRequiredService<TestTokenGeneratorPlatformAccessTokenProvider>());
        services.TryAdd(descriptor);
        if (!services.Contains(descriptor))
        {
            // Already contains a provider for IPlatformAccessTokenProvider
            return services;
        }

        services.AddOptions<AltinnTestTokenGeneratorSettings>()
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddSingleton<IConfigureOptions<AltinnTestTokenGeneratorSettings>, ConfigureAltinnTestTokenGeneratorSettingsFromConfiguration>();

        services.AddHttpClient<TestTokenGeneratorPlatformAccessTokenProvider>()
            .ConfigureBaseAddressFromOptions((AltinnTestTokenGeneratorSettings settings) => settings.Url!);

        return services;
    }

    /// <summary>
    /// Adds a platform access-token provider that uses <see cref="IAccessTokenGenerator"/> for tokens.
    /// </summary>
    /// <param name="services">The service collection to which the test token generator provider will be added.</param>
    /// <returns>The same service collection instance, to support method chaining.</returns>
    public static IServiceCollection TryAddAccessTokenClientTokenProvider(this IServiceCollection services)
    {
        var descriptor = ServiceDescriptor.Transient<IPlatformAccessTokenProvider, AccessTokenClientTokenProvider>();
        services.TryAdd(descriptor);
        if (!services.Contains(descriptor))
        {
            // Already contains a provider for IPlatformAccessTokenProvider
            return services;
        }

        services.AddOptions<AltinnPlatformAccessTokenSettings>()
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddSingleton<IConfigureOptions<AltinnPlatformAccessTokenSettings>, ConfigureAltinnPlatformAccessTokenSettingsFromConfiguration>();

        return services;
    }

    /// <summary>
    /// Attempts to add a platform token provider to the specified service collection, selecting the appropriate
    /// implementation based on the current environment.
    /// </summary>
    /// <remarks>In local development environments, a test platform token generator provider is added. In
    /// other environments, an access token client token provider is registered. This method is intended to be used
    /// during application startup to ensure the correct token provider is available for authentication
    /// scenarios.</remarks>
    /// <param name="services">The service collection to which the test token generator provider will be added.</param>
    /// <returns>The same service collection instance, to support method chaining.</returns>
    public static IServiceCollection TryAddPlatformTokenProvider(this IServiceCollection services)
    {
        var serviceDescriptor = services.GetAltinnServiceDescriptor();
        if (serviceDescriptor.IsLocalDev)
        {
            services.TryAddTestPlatformTokenGeneratorProvider();
        }
        else
        {
            services.TryAddAccessTokenClientTokenProvider();
        }

        return services;
    }
}
