using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.ServiceDiscovery;

namespace Altinn.Authorization.TestUtils.Http;

/// <summary>
/// Extension methods for <see cref="IServiceCollection"/>.
/// </summary>
public static class FakeHttpServiceCollectionExtensions
{
    private static ServiceDescriptor _endpointProviderFactory = ServiceDescriptor.Singleton<IServiceEndpointProviderFactory, FakeHttpHandlerServiceEndpointProviderFactory>();

    /// <summary>
    /// Configures the <see cref="IHttpClientFactory"/> to use <paramref name="handlers"/> for all clients.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="handlers">The <see cref="FakeHttpHandlers"/>.</param>
    /// <returns><paramref name="services"/>.</returns>
    public static IServiceCollection AddFakeHttpHandlers(this IServiceCollection services, FakeHttpHandlers? handlers = null)
    {
        services.AddSingleton(handlers ?? new());
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IConfigureOptions<HttpClientFactoryOptions>, ConfigureFakeHandlers>());

        if (!services.Contains(_endpointProviderFactory))
        {
            services.Add(_endpointProviderFactory);
        }

        return services;
    }

    private class ConfigureFakeHandlers
        : IConfigureNamedOptions<HttpClientFactoryOptions>
    {
        private readonly FakeHttpHandlers _handlers;

        public ConfigureFakeHandlers(FakeHttpHandlers handlers)
        {
            _handlers = handlers;
        }

        public void Configure(string? name, HttpClientFactoryOptions options)
        {
            var handler = _handlers.For(name ?? Options.DefaultName);

            options.HttpMessageHandlerBuilderActions.Insert(0, b => b.PrimaryHandler = handler);
            options.HttpClientActions.Insert(0, c => c.BaseAddress = FakeHttpEndpoint.HttpsUri);
        }

        public void Configure(HttpClientFactoryOptions options)
            => Configure(Options.DefaultName, options);
    }
}
