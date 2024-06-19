using Altinn.Authorization.ServiceDefaults.Utils;
using CommunityToolkit.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Altinn.Authorization.ServiceDefaults.Options;

internal class ConfigureAltinnClusterInfo
    : IConfigureOptions<AltinnClusterInfo>
{
    private readonly IConfiguration _config;

    public ConfigureAltinnClusterInfo(IConfiguration config)
    {
        _config = config;
    }

    public void Configure(AltinnClusterInfo options)
    {
        ConfigureClusterNetwork(options);
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

        options.ClusterNetwork = ipNetwork;
    }
}
