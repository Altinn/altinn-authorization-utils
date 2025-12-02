using Altinn.Authorization.ServiceDefaults.Utils;
using CommunityToolkit.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Altinn.Authorization.ServiceDefaults.Options;

internal partial class ConfigureAltinnClusterInfo
    : IConfigureOptions<AltinnClusterInfo>
{
    private readonly IConfiguration _config;
    private readonly ILogger<ConfigureAltinnClusterInfo> _logger;

    public ConfigureAltinnClusterInfo(
        IConfiguration config,
        ILogger<ConfigureAltinnClusterInfo> logger)
    {
        _config = config;
        _logger = logger;
    }

    public void Configure(AltinnClusterInfo options)
    {
        ConfigureClusterNetwork(options);
        ConfigureTrustedProxies(options);
    }

    private void ConfigureClusterNetwork(AltinnClusterInfo options)
    {
        var network = _config.GetValue<string>("Altinn:ClusterInfo:ClusterNetwork");
        if (string.IsNullOrEmpty(network))
        {
            return;
        }

        if (!IPNetworkUtils.TryParseIPNetwork(network, out var ipNetwork, out _))
        {
            ThrowHelper.ThrowFormatException("Invalid CIDR format for Altinn:ClusterInfo:ClusterNetwork");
        }

#pragma warning disable CS0618 // Type or member is obsolete
        options.ClusterNetwork = ipNetwork;
#pragma warning restore CS0618 // Type or member is obsolete

        Log.ClusterNetworkDeprecated(_logger);
    }

    private void ConfigureTrustedProxies(AltinnClusterInfo options)
    {
        var proxies = _config.GetValue<string>("Altinn:ClusterInfo:TrustedProxies");
        if (string.IsNullOrEmpty(proxies))
        {
            return;
        }

        var span = proxies.AsSpan();
        foreach (var range in span.Split(','))
        {
            var segment = span[range].Trim();
            if (segment.IsEmpty)
            {
                continue;
            }

            if (!IPNetworkUtils.TryParseIPNetwork(segment, out var ipNetwork, out _))
            {
                ThrowHelper.ThrowFormatException($"Invalid CIDR format in Altinn:ClusterInfo:TrustedProxies. Got: {segment}");
            }

            options.TrustedProxies.Add(ipNetwork);
        }
    }

    private static partial class Log
    {
        [LoggerMessage(0, LogLevel.Warning, "'Altinn:ClusterInfo:ClusterNetwork' is deprecated, use 'Altinn:ClusterInfo:TrustedProxies' instead")]
        public static partial void ClusterNetworkDeprecated(ILogger logger);
    }
}
