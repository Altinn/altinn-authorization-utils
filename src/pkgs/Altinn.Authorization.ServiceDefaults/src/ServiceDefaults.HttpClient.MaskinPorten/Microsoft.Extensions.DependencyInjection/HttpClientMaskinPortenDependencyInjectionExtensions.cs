using Altinn.Authorization.ServiceDefaults.HttpClient.MaskinPorten;
using Altinn.Authorization.ServiceDefaults.HttpClient.MaskinPorten.Handlers;
using Altinn.Authorization.ServiceDefaults.HttpClient.MaskinPorten.Options;
using CommunityToolkit.Diagnostics;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Provides extension methods for registering MaskinPorten HTTP clients and handlers with dependency injection in an
/// ASP.NET Core application.
/// </summary>
public static class HttpClientMaskinPortenDependencyInjectionExtensions
{
    /// <summary>
    /// Adds a MaskinPorten HTTP client to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>A <see cref="IHttpClientBuilder"/> for configuring the HTTP client used to get access-tokens from MaskinPorten.</returns>
    public static IHttpClientBuilder AddMaskinPortenClient(this IServiceCollection services)
        => AddMaskinPortenClientDependencies(services);

    /// <summary>
    /// Validates that a named MaskinPorten client can retrieve an access token before hosted services start.
    /// </summary>
    /// <remarks>
    /// Failure to retrieve a token prevents the host from starting. Repeated registrations for the same client
    /// perform only one check.
    ///
    /// This method is not an extension method, and is typically only invoked as part of <see cref="AddMaskinPortenHandler(IHttpClientBuilder, string?, bool)"/>.
    /// It's public in order to allow explicit registration when needed.
    /// </remarks>
    /// <param name="services">The service collection.</param>
    /// <param name="clientName">The name (configuration key) of the client to validate.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddMaskinPortenClientValidationOnStart(IServiceCollection services, string clientName)
    {
        Guard.IsNotNull(services);
        Guard.IsNotNullOrEmpty(clientName);

        services.AddMaskinPortenClient();
        services.Configure<MaskinPortenStartupValidator.Options>(options => options.ClientNames.Add(clientName));
        services.AddHostedService<MaskinPortenStartupValidator>();
        return services;
    }

    /// <summary>
    /// Adds the MaskinPorten authentication handlers to the HTTP client pipeline for the specified client.
    /// </summary>
    /// <remarks>This method registers the MaskinPorten authentication handlers, ensuring that outgoing HTTP
    /// requests are authenticated using MaskinPorten. If a <paramref name="clientName"/> is provided, a client-specific
    /// configuration handler is added. If a <paramref name="clientName"/> is *NOT* provided, all requests must configure a
    /// client-name per request for the handler to take effect.</remarks>
    /// <param name="builder">The HTTP client builder to configure.</param>
    /// <param name="clientName">
    /// The logical name of the HTTP client to associate with the MaskinPorten configuration.
    /// If <see langword="null"/>, only applies the handler to requests where a client-name is set.
    /// </param>
    /// <param name="validateOnStart">
    /// Indicates whether the MaskinPorten handler should validate its configuration on start. Only applicable if <paramref name="clientName"/> is set.
    /// </param>
    /// <returns>The same instance of <see cref="IHttpClientBuilder"/> for chaining further configuration.</returns>
    public static IHttpClientBuilder AddMaskinPortenHandler(
        this IHttpClientBuilder builder,
        string? clientName = null,
        bool validateOnStart = true)
    {
        builder.Services.AddMaskinPortenClient();
        builder.ConfigureAdditionalHttpMessageHandlers((handlers, services) =>
        {
            RemoveAll<MaskinPortenHandler>(handlers);
            RemoveAll<MaskinPortenRequestConfigHandler>(handlers);

            var timeProvider = services.GetRequiredService<TimeProvider>();
            var client = services.GetRequiredService<IMaskinPortenClient>();

            if (clientName is not null)
            {
                Guard.IsNotEmpty(clientName);
                handlers.Insert(0, new MaskinPortenRequestConfigHandler(clientName));
            }

            handlers.Add(new MaskinPortenHandler(timeProvider, client));
        });

        if (clientName is not null && validateOnStart)
        {
            AddMaskinPortenClientValidationOnStart(builder.Services, clientName);
        }

        return builder;
    }

    private static IHttpClientBuilder AddMaskinPortenClientDependencies(IServiceCollection services)
    {
        var builder = services.AddHttpClient(MaskinPortenClient.MaskinPortenHttpClientName);

        if (services.Contains(Marker.ServiceDescriptor))
        {
            return builder;
        }

        services.Add(Marker.ServiceDescriptor);

        services.AddMemoryCache();
        services.TryAddSingleton<IMaskinPortenClient, MaskinPortenClient>();
        services.AddOptions<MaskinPortenCommonOptions>()
            .BindConfiguration("Altinn:MaskinPorten")
            .ValidateDataAnnotations();
        services.AddOptions<MaskinPortenClientOptions>().ValidateDataAnnotations();
        services.AddSingleton<IConfigureOptions<MaskinPortenClientOptions>, ConfigureMaskinPortenClientOptionsFromCommonOptions>();
        services.AddSingleton<IConfigureOptions<MaskinPortenClientOptions>, ConfigureMaskinPortenClientFromConfiguration>();

        services.ConfigureOpenTelemetryMeterProvider(c => c.AddMeter(MaskinPortenClientTelemetry.Name));
        services.ConfigureOpenTelemetryTracerProvider(c => c.AddSource(MaskinPortenClientTelemetry.Name));

        return builder;
    }

    private static void RemoveAll<THandler>(IList<DelegatingHandler> handlers)
        where THandler : DelegatingHandler
    {
        for (int i = handlers.Count - 1; i >= 0; i--)
        {
            if (handlers[i] is THandler)
            {
                handlers.RemoveAt(i);
            }
        }
    }

    private sealed class Marker
    {
        public static readonly ServiceDescriptor ServiceDescriptor = ServiceDescriptor.Singleton<Marker, Marker>();
    }
}
