namespace Altinn.Authorization.CommandLine;

/// <summary>
/// Interface marking attributes that specify a parameter should be bound from a service.
/// </summary>
public interface IFromServiceMetadata
{
    /// <summary>
    /// The service key to bind from. If null, uses unkeyed services.
    /// </summary>
    object? ServiceKey { get; }
}
