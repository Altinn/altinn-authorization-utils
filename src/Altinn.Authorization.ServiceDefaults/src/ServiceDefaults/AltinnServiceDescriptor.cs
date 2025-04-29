using CommunityToolkit.Diagnostics;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Altinn.Authorization.ServiceDefaults;

/// <summary>
/// A descriptor for an Altinn service.
/// </summary>
[ExcludeFromCodeCoverage]
[DebuggerDisplay("Name = {Name}, IsLocalDev = {IsLocalDev}")]
public sealed record AltinnServiceDescriptor
{

    /// <summary>
    /// Gets the name of the service.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets a value indicating whether the service is running in a local-dev mode.
    /// </summary>
    public bool IsLocalDev => Environment.IsLocalDev;

    /// <summary>
    /// Gets the altinn environment.
    /// </summary>
    public AltinnEnvironment Environment { get; }

    internal AltinnServiceDescriptor(string name, AltinnEnvironment environment)
    {
        Guard.IsNotNullOrWhiteSpace(name);
        if (name != name.ToLowerInvariant())
        {
            ThrowHelper.ThrowArgumentException("Service name must be in lowercase.", nameof(name));
        }

        Name = name;
        Environment = environment;
    }
}
