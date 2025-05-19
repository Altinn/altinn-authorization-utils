using Altinn.Authorization.ServiceDefaults;

namespace Microsoft.Extensions.Hosting;

/// <summary>
/// Extension methods for configuring default Altinn services.
/// </summary>
public static class AltinnServiceDefaultsOptionsExtensions
{
    /// <summary>
    /// Configures which default Altinn services should be enabled.
    /// </summary>
    /// <param name="options">The <see cref="AltinnServiceDefaultOptions"/> instance to configure.</param>
    /// <param name="configureAddServices">An action to configure enabled services.</param>
    /// <returns>The same <see cref="AltinnServiceDefaultOptions"/> instance for chaining.</returns>
    public static AltinnServiceDefaultOptions ConfigureEnabledServices(this AltinnServiceDefaultOptions options, Action<AltinnServiceDefaultOptions.EnabledServicesOptions> configureAddServices)
    {
        configureAddServices(options.EnabledServices);
        return options;
    }

    /// <summary>
    /// Disables Application Insights.
    /// </summary>
    public static AltinnServiceDefaultOptions.EnabledServicesOptions DisableApplicationInsights(this AltinnServiceDefaultOptions.EnabledServicesOptions services)
    {
        services.ApplicationInsights = false;
        return services;
    }

    /// <summary>
    /// Disables OpenTelemetry.
    /// </summary>
    public static AltinnServiceDefaultOptions.EnabledServicesOptions DisableOpenTelemetry(this AltinnServiceDefaultOptions.EnabledServicesOptions services)
    {
        services.OpenTelemetry = false;
        return services;
    }

    /// <summary>
    /// Disables App Configuration.
    /// </summary>
    public static AltinnServiceDefaultOptions.EnabledServicesOptions DisableAppConfiguration(this AltinnServiceDefaultOptions.EnabledServicesOptions services)
    {
        services.AppConfiguration = false;
        return services;
    }

    /// <summary>
    /// Disables Health Checks.
    /// </summary>
    public static AltinnServiceDefaultOptions.EnabledServicesOptions DisableHealthCheck(this AltinnServiceDefaultOptions.EnabledServicesOptions services)
    {
        services.HealthCheck = false;
        return services;
    }
}
