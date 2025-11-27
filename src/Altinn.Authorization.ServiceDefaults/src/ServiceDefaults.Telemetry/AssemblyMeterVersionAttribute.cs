namespace Altinn.Authorization.ServiceDefaults.Telemetry;

/// <summary>
/// Overrides the default meter version for an assembly.
/// </summary>
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false, Inherited = true)]
public class AssemblyMeterVersionAttribute
    : Attribute
{
    /// <summary>
    /// Gets the default meter version for meters defined in this assembly.
    /// </summary>
    public string MeterVersion { get; }

    /// <summary>
    /// Initializes a new instance of <see cref="AssemblyMeterVersionAttribute"/>.
    /// </summary>
    /// <param name="version">The meter version.</param>
    public AssemblyMeterVersionAttribute(string version)
    {
        MeterVersion = version;
    }
}
