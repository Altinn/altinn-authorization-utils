using System.Net;

namespace Altinn.Authorization.ServiceDefaults;

/// <summary>
/// Information about the Altinn cluster.
/// </summary>
public sealed class AltinnClusterInfo
{
    /// <summary>
    /// Gets or sets the cluster network.
    /// </summary>
    [Obsolete($"Use {nameof(TrustedProxies)} instead")]
    public IPNetwork? ClusterNetwork
    {
        get => field;
        set
        {
            field = value;
            if (value is { } network)
            {
                TrustedProxies.Add(network);
            }
        }
    }

    /// <summary>
    /// Gets or sets the list of proxy IP networks that are considered trusted when evaluating forwarded headers.
    /// </summary>
    public ISet<IPNetwork> TrustedProxies { get; set; } = new HashSet<IPNetwork>();
}
