using Altinn.Authorization.ServiceDefaults;
using Altinn.Authorization.ServiceDefaults.OpenTelemetry;
using Altinn.Authorization.ServiceDefaults.Options;
using CommunityToolkit.Diagnostics;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Extensions.Hosting;

/// <summary>
/// Extension methods for adding default services to an Altinn service.
/// </summary>
public static class AltinnServiceDefaultsExtensions
{
    /// <summary>
    /// Adds default services for an Altinn service.
    /// </summary>
    /// <param name="builder">The <see cref="IHostApplicationBuilder"/>.</param>
    /// <param name="name">The service name.</param>
    /// <returns><paramref name="builder"/>.</returns>
    public static IHostApplicationBuilder AddAltinnServiceDefaults(this IHostApplicationBuilder builder, string name)
    {
        Guard.IsNotNull(builder);
        Guard.IsNotNullOrEmpty(name);

        if (builder.Services.TryFindAltinnServiceDescription(out var serviceDescription))
        {
            Guard.IsEqualTo(name, serviceDescription.Name);

            // The service description has already been added - meaning that the defaults have already been added
            // Some of the defaults are not idempotent, so we can't add them again
            return builder;
        }

        var isLocalDevelopment = builder.Environment.IsDevelopment() && builder.Configuration.GetValue<bool>("Altinn:LocalDev");

        serviceDescription = new AltinnServiceDescriptor(name, isLocalDevelopment);
        builder.Services.AddSingleton(serviceDescription);
        builder.Services.AddSingleton<AltinnServiceResourceDetector>();
        builder.Services.Configure<AltinnClusterInfo>(builder.Configuration.GetSection("Altinn:ClusterInfo"));
        builder.Services.AddSingleton<IConfigureOptions<AltinnClusterInfo>, ConfigureAltinnClusterInfo>();
        builder.Services.AddOptions<ForwardedHeadersOptions>()
            .Configure((ForwardedHeadersOptions options, IOptionsMonitor<AltinnClusterInfo> clusterInfoOptions) =>
            {
                var clusterInfo = clusterInfoOptions.CurrentValue;
                if (clusterInfo.ClusterNetwork is { } clusterNetwork)
                {
                    options.KnownNetworks.Add(new AspNetCore.HttpOverrides.IPNetwork(clusterNetwork.BaseAddress, clusterNetwork.PrefixLength));
                }
            });

        builder.ConfigureOpenTelemetry();
        builder.AddDefaultHealthChecks();

        builder.Services.AddServiceDiscovery();
        builder.Services.ConfigureHttpClientDefaults(http =>
        {
            // Turn on resilience by default
            http.AddStandardResilienceHandler();

            // Turn on service discovery by default
            http.AddServiceDiscovery();
        });

        return builder;
    }

    /// <summary>
    /// Maps default Altinn endpoints.
    /// </summary>
    /// <remarks>
    /// Requires that <see cref="AddAltinnServiceDefaults(IHostApplicationBuilder, string)"/> has been called.
    /// </remarks>
    /// <param name="app">The <see cref="WebApplication"/>.</param>
    /// <returns><paramref name="app"/>.</returns>
    public static WebApplication MapDefaultAltinnEndpoints(this WebApplication app)
    {
        // All health checks must pass for app to be considered ready to accept traffic after starting
        app.MapHealthChecks("/health");

        // Only health checks tagged with the "live" tag must pass for app to be considered alive
        app.MapHealthChecks("/alive", new HealthCheckOptions
        {
            Predicate = static r => r.Tags.Contains("live"),
        });

        return app;
    }

    /// <summary>
    /// Gets the configured <see cref="AltinnServiceDescriptor"/>.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/>.</param>
    /// <returns>The <see cref="AltinnServiceDescriptor"/>.</returns>
    public static AltinnServiceDescriptor GetAltinnServiceDescriptor(this IServiceCollection services)
    {
        if (services.TryFindAltinnServiceDescription(out var serviceDescription))
        {
            return serviceDescription;
        }

        return ThrowHelper.ThrowInvalidOperationException<AltinnServiceDescriptor>("Service of type AltinnServiceDescription not registered - did you forget to call AddAltinnServiceDefaults?");
    }

    /// <summary>
    /// Gets the configured <see cref="AltinnServiceDescriptor"/>.
    /// </summary>
    /// <param name="builder">The <see cref="IHostApplicationBuilder"/>.</param>
    /// <returns>The <see cref="AltinnServiceDescriptor"/>.</returns>
    public static AltinnServiceDescriptor GetAltinnServiceDescriptor(this IHostApplicationBuilder builder)
        => builder.Services.GetAltinnServiceDescriptor();

    /// <summary>
    /// Gets whether the service is running in local development mode.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/>.</param>
    /// <returns><see langword="true"/> if currently running in local-dev mode, otherwise <see langword="false"/>.</returns>
    public static bool IsLocalDevelopment(this IServiceCollection services)
        => services.GetAltinnServiceDescriptor().IsLocalDev;

    /// <summary>
    /// Gets whether the service is running in local development mode.
    /// </summary>
    /// <param name="builder">The <see cref="IHostApplicationBuilder"/>.</param>
    /// <returns><see langword="true"/> if currently running in local-dev mode, otherwise <see langword="false"/>.</returns>
    public static bool IsLocalDevelopment(this IHostApplicationBuilder builder)
        => builder.Services.IsLocalDevelopment();

    private static bool TryFindAltinnServiceDescription(this IServiceCollection services, [NotNullWhen(true)] out AltinnServiceDescriptor? serviceDescription)
    {
        var descriptor = services.SingleOrDefault(s => s.ServiceType == typeof(AltinnServiceDescriptor));
        if (descriptor is null)
        {
            serviceDescription = null;
            return false;
        }

        if (descriptor.Lifetime != ServiceLifetime.Singleton)
        {
            ThrowHelper.ThrowInvalidOperationException("Service of type AltinnServiceDescription registered as non-singleton");
        }

        if (descriptor.ImplementationInstance is AltinnServiceDescriptor instance)
        {
            serviceDescription = instance;
            return true;
        }

        serviceDescription = null;
        return ThrowHelper.ThrowInvalidOperationException<bool>("Service of type AltinnServiceDescription registered without an instance");
    }

    private static IHostApplicationBuilder ConfigureOpenTelemetry(this IHostApplicationBuilder builder)
    {
        builder.Logging.AddOpenTelemetry(logging =>
        {
            logging.IncludeFormattedMessage = true;
            logging.IncludeScopes = true;
        });

        builder.Services.AddOpenTelemetry()
            .ConfigureResource(resource =>
            {
                resource.AddDetector(services => services.GetRequiredService<AltinnServiceResourceDetector>());
            })
            .WithMetrics(metrics =>
            {
                metrics.AddRuntimeInstrumentation()
                       .AddBuiltInMeters();
            })
            .WithTracing(tracing =>
            {
                if (builder.Environment.IsDevelopment())
                {
                    // We want to view all traces in development
                    tracing.SetSampler(new AlwaysOnSampler());
                }

                tracing.AddAspNetCoreInstrumentation()
                       .AddHttpClientInstrumentation();
            });

        builder.AddOpenTelemetryExporters();

        return builder;
    }

    private static IHostApplicationBuilder AddOpenTelemetryExporters(this IHostApplicationBuilder builder)
    {
        var useOtlpExporter = !string.IsNullOrWhiteSpace(builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]);

        if (useOtlpExporter)
        {
            builder.Services.Configure<OpenTelemetryLoggerOptions>(logging => logging.AddOtlpExporter());
            builder.Services.ConfigureOpenTelemetryMeterProvider(metrics => metrics.AddOtlpExporter());
            builder.Services.ConfigureOpenTelemetryTracerProvider(tracing => tracing.AddOtlpExporter());
        }

        // Uncomment the following lines to enable the Azure Monitor exporter (requires the Azure.Monitor.OpenTelemetry.AspNetCore package)
        // builder.Services.AddOpenTelemetry()
        //    .UseAzureMonitor();

        return builder;
    }

    private static MeterProviderBuilder AddBuiltInMeters(this MeterProviderBuilder meterProviderBuilder) =>
        meterProviderBuilder.AddMeter(
            "Microsoft.AspNetCore.Hosting",
            "Microsoft.AspNetCore.Server.Kestrel",
            "System.Net.Http");

    private static IHostApplicationBuilder AddDefaultHealthChecks(this IHostApplicationBuilder builder)
    {
        builder.Services.AddHealthChecks()
            // Add a default liveness check to ensure app is responsive
            .AddCheck("self", static () => HealthCheckResult.Healthy(), ["live"]);

        return builder;
    }
}
