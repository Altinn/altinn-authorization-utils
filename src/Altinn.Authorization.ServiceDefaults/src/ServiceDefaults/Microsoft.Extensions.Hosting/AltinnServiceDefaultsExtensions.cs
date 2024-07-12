using Altinn.Authorization.ServiceDefaults;
using Altinn.Authorization.ServiceDefaults.ApplicationInsights;
using Altinn.Authorization.ServiceDefaults.HealthChecks;
using Altinn.Authorization.ServiceDefaults.OpenTelemetry;
using Altinn.Authorization.ServiceDefaults.Options;
using Altinn.Authorization.ServiceDefaults.Telemetry;
using Azure.Identity;
using CommunityToolkit.Diagnostics;
using Microsoft.ApplicationInsights.AspNetCore.Extensions;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights.WindowsServer.TelemetryChannel;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Microsoft.Extensions.Hosting;

/// <summary>
/// Extension methods for adding default services to an Altinn service.
/// </summary>
[ExcludeFromCodeCoverage]
public static class AltinnServiceDefaultsExtensions
{
    internal readonly static string HealthEndpoint = "/health";
    internal readonly static string AliveEndpoint = "/alive";

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

        builder.AddAltinnConfiguration();

        // Note - this has to happen early due to a bug in Application Insights
        // See: https://github.com/microsoft/ApplicationInsights-dotnet/issues/2879
        builder.AddApplicationInsights(); 

        var isLocalDevelopment = builder.Environment.IsDevelopment() && builder.Configuration.GetValue<bool>("Altinn:LocalDev");

        serviceDescription = new AltinnServiceDescriptor(name, isLocalDevelopment);
        builder.Services.AddSingleton(serviceDescription);
        builder.Services.AddSingleton<AltinnServiceResourceDetector>();
        builder.Services.Configure<AltinnClusterInfo>(builder.Configuration.GetSection("Altinn:ClusterInfo"));
        builder.Services.AddSingleton<IConfigureOptions<AltinnClusterInfo>, ConfigureAltinnClusterInfo>();
        builder.Services.AddOptions<ForwardedHeadersOptions>()
            .Configure((ForwardedHeadersOptions options, IOptionsMonitor<AltinnClusterInfo> clusterInfoOptions) =>
            {
                options.ForwardedHeaders = ForwardedHeaders.All;

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
    /// Adds default Altinn middleware.
    /// </summary>
    /// <remarks>
    /// Requires that <see cref="AddAltinnServiceDefaults(IHostApplicationBuilder, string)"/> has been called.
    /// </remarks>
    /// <param name="app">The <see cref="WebApplication"/>.</param>
    /// <param name="errorHandlingPath">The path to use for error handling in production environments.</param>
    /// <returns><paramref name="app"/>.</returns>
    public static WebApplication AddDefaultAltinnMiddleware(this WebApplication app, string errorHandlingPath)
    {
        Log("Startup // Configure");

        if (app.Environment.IsDevelopment() || app.Environment.IsStaging())
        {
            Log("IsDevelopment || IsStaging => Using developer exception page");

            app.UseDeveloperExceptionPage();
        }
        else
        {
            Log("Production => Using exception handler");

            app.UseExceptionHandler(errorHandlingPath);
        }

        app.UseForwardedHeaders();

        return app;
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
        var writer = app.Services.GetRequiredService<HealthReportWriter>();

        // All health checks must pass for app to be considered ready to accept traffic after starting
        app.MapHealthChecks(HealthEndpoint, new HealthCheckOptions
        {
            ResponseWriter = writer,
        });

        // Only health checks tagged with the "live" tag must pass for app to be considered alive
        app.MapHealthChecks(AliveEndpoint, new HealthCheckOptions
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
    /// Gets the configured <see cref="AltinnServiceDescriptor"/>.
    /// </summary>
    /// <param name="builder">The <see cref="IHost"/>.</param>
    /// <returns>The <see cref="AltinnServiceDescriptor"/>.</returns>
    public static AltinnServiceDescriptor GetAltinnServiceDescriptor(this IHost builder)
        => builder.Services.GetRequiredService<AltinnServiceDescriptor>();

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

    /// <summary>
    /// Gets whether the service is running in local development mode.
    /// </summary>
    /// <param name="builder">The <see cref="IHost"/>.</param>
    /// <returns><see langword="true"/> if currently running in local-dev mode, otherwise <see langword="false"/>.</returns>
    public static bool IsLocalDevelopment(this IHost builder)
        => builder.GetAltinnServiceDescriptor().IsLocalDev;

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

                tracing.AddAspNetCoreInstrumentation(o =>
                {
                    o.Filter = (httpContext) =>
                    {
                        if (TelemetryHelpers.ShouldExclude(httpContext.Request.Path))
                        {
                            return false;
                        }

                        return true;
                    };

                    o.EnrichWithHttpRequest = (activity, request) =>
                    {
                        TelemetryHelpers.EnrichFromRequest(new ActivityTags(activity), request.HttpContext);
                    };
                });
                
                tracing.AddHttpClientInstrumentation(); 
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
        builder.Services.AddSingleton<HealthReportWriter>();
        builder.Services.AddOptions<HealthReportWriterSettings>()
            .Configure((HealthReportWriterSettings opts, IHostEnvironment env) =>
            {
                if (!env.IsProduction())
                {
                    opts.Exceptions |= HealthReportWriterSettings.ExceptionHandling.Include;
                }

                if (env.IsDevelopment())
                {
                    opts.Exceptions |= HealthReportWriterSettings.ExceptionHandling.IncludeStackTrace;
                }
            });

        builder.Services.AddHealthChecks()
            // Add a default liveness check to ensure app is responsive
            .AddCheck("self", static () => HealthCheckResult.Healthy(), ["live"]);

        return builder;
    }

    private static IHostApplicationBuilder AddApplicationInsights(this IHostApplicationBuilder builder)
    {
        var applicationInsightsInstrumentationKey = builder.Configuration.GetValue<string>("ApplicationInsights:InstrumentationKey");

        if (!string.IsNullOrEmpty(applicationInsightsInstrumentationKey))
        {
            var applicationInsightsConnectionString = $"InstrumentationKey={applicationInsightsInstrumentationKey}";
            builder.Configuration.AddInMemoryCollection([
                KeyValuePair.Create("ApplicationInsights:ConnectionString", (string?)applicationInsightsConnectionString),
                KeyValuePair.Create("ConnectionStrings:ApplicationInsights", (string?)applicationInsightsConnectionString),
            ]);

            // NOTE: due to a bug in application insights, this must be registered before anything else
            // See https://github.com/microsoft/ApplicationInsights-dotnet/issues/2879
            builder.Services.AddSingleton(typeof(ITelemetryChannel), new ServerTelemetryChannel() { StorageFolder = "/tmp/logtelemetry" });
            builder.Services.AddApplicationInsightsTelemetry(new ApplicationInsightsServiceOptions
            {
                ConnectionString = applicationInsightsConnectionString,
            });

            builder.Services.AddApplicationInsightsTelemetryProcessor<ApplicationInsightsEndpointFilterProcessor>();
            builder.Services.AddApplicationInsightsTelemetryProcessor<ApplicationInsightsRequestTelemetryEnricherProcessor>();
            builder.Services.AddSingleton<ITelemetryInitializer, AltinnServiceTelemetryInitializer>();

            Log($"ApplicationInsightsConnectionString = {applicationInsightsConnectionString}");
        }
        else
        {
            Log("No ApplicationInsights:InstrumentationKey found - skipping Application Insights");
        }

        return builder;
    }

    private static IHostApplicationBuilder AddAltinnConfiguration(this IHostApplicationBuilder builder)
    {
        builder.Configuration.AddAltinnDbSecretsJson();
        builder.Configuration.AddAltinnKeyVault();

        return builder;
    }

    private static IConfigurationBuilder AddAltinnDbSecretsJson(this IConfigurationBuilder builder)
    {
        var parentDir = Path.GetDirectoryName(Environment.CurrentDirectory);
        if (parentDir is null)
        {
            Log("No parent directory found - skipping altinn-dbsettings-secret.json");
            return builder;
        }

        var altinnDbSecretsConfigFile = Path.Combine(
            parentDir,
            "altinn-appsettings",
            "altinn-dbsettings-secret.json");

        if (!File.Exists(altinnDbSecretsConfigFile))
        {
            Log($"No altinn-dbsettings-secret.json found at \"{altinnDbSecretsConfigFile}\" - skipping altinn-dbsettings-secret.json");
            return builder;
        }

        Log($"Loading configuration from \"{altinnDbSecretsConfigFile}\"");
        builder.AddJsonFile(altinnDbSecretsConfigFile, optional: false, reloadOnChange: true);
        return builder;
    }

    private static IConfigurationBuilder AddAltinnKeyVault(this IConfigurationManager manager)
    {
        var clientId = manager.GetValue<string>("kvSetting:ClientId");
        var tenantId = manager.GetValue<string>("kvSetting:TenantId");
        var clientSecret = manager.GetValue<string>("kvSetting:ClientSecret");
        var keyVaultUri = manager.GetValue<string>("kvSetting:SecretUri");

        if (!string.IsNullOrEmpty(clientId)
            && !string.IsNullOrEmpty(tenantId)
            && !string.IsNullOrEmpty(clientSecret)
            && !string.IsNullOrEmpty(keyVaultUri))
        {
            Log($"adding config from keyvault using client-secret credentials");
            var credential = new ClientSecretCredential(
                tenantId: tenantId,
                clientId: clientId,
                clientSecret: clientSecret);
            manager.AddAzureKeyVault(new Uri(keyVaultUri), credential);
        }
        else
        {
            Log($"Missing keyvault settings - skipping adding keyvault to configuration");
        }

        return manager;
    }

    private static void Log(
        string message,
        [CallerMemberName] string callerMemberName = "")
    {
        Console.WriteLine($"// {nameof(AltinnServiceDefaultsExtensions)}.{callerMemberName}: {message}");
    }
}
