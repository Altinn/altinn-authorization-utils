namespace Altinn.Authorization.ServiceDefaults;

/// <summary>
/// A descriptor for an Altinn service.
/// </summary>
public sealed class AltinnServiceDescriptor
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
