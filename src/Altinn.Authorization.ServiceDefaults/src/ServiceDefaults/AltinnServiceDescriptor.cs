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
    public bool IsLocalDev { get; }

    internal AltinnServiceDescriptor(string name, bool isLocalDev)
    {
        Name = name;
        IsLocalDev = isLocalDev;
    }
}
