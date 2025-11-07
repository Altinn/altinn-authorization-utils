using Microsoft.AspNetCore.Http;

namespace Altinn.Authorization.ServiceDefaults.Swashbuckle.Servers;

/// <summary>
/// Provides options for configuring Altinn servers in Swagger documentation.
/// </summary>
public sealed class AltinnServerOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether the localhost server should be included in OpenAPI document.
    /// </summary>
    public bool? IncludeLocalhostServer { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether acceptance test servers (AT) should be included in OpenAPI document.
    /// </summary>
    public bool? IncludeAcceptanceTestServers { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether performance test servers (YT) should be included in OpenAPI document.
    /// </summary>
    public bool? IncludePerformanceTestServer { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the integration test server (TT) should be included in OpenAPI document.
    /// </summary>
    public bool? IncludeIntegrationTestServer { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the production server should be included in OpenAPI document.
    /// </summary>
    public bool? IncludeProductionServers { get; set; }

    /// <summary>
    /// Gets or sets the path suffix to append to the server URL for all non-localhost servers.
    /// </summary>
    public PathString EnvironmentServerPathSuffix { get; set; }
}
