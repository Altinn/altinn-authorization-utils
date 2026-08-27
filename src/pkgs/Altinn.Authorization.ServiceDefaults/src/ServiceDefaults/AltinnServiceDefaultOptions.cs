namespace Altinn.Authorization.ServiceDefaults;

/// <summary>
/// Provides configuration options for setting up default Altinn services.
/// </summary>
public class AltinnServiceDefaultOptions
{
    /// <summary>
    /// Configuration for which services should be enabled.
    /// </summary>
    internal EnabledServicesOptions EnabledServices { get; } = new();

    /// <summary>
    /// Options for specifying which services to enable during initialization.
    /// </summary>
    public class EnabledServicesOptions
    {
        /// <summary>
        /// Indicates whether Application Insights should be enabled. Default is <c>true</c>.
        /// </summary>
        public bool ApplicationInsights { get; set; } = true;

        /// <summary>
        /// Indicates whether OpenTelemetry should be enabled. Default is <c>true</c>.
        /// </summary>
        public bool OpenTelemetry { get; set; } = true;

        /// <summary>
        /// Indicates whether default appsettings configuration should be enabled. Default is <c>true</c>.
        /// </summary>
        public bool AppConfiguration { get; set; } = true;

        /// <summary>
        /// Indicates whether Health Checks should be enabled. Default is <c>true</c>.
        /// </summary>
        public bool HealthCheck { get; set; } = true;
    }
}
