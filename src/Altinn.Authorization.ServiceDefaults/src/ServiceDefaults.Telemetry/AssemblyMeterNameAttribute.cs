namespace Altinn.Authorization.ServiceDefaults.Telemetry;

/// <summary>
/// Overrides the default meter name for an assembly.
/// </summary>
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false, Inherited = true)]
public class AssemblyMeterNameAttribute
    : Attribute
{
    /// <summary>
    /// Gets the default meter name for meters defined in this assembly.
    /// </summary>
    public string MeterName { get; }

    /// <summary>
    /// Initializes a new instance of <see cref="AssemblyMeterNameAttribute"/>.
    /// </summary>
    /// <param name="name">The meter name.</param>
    public AssemblyMeterNameAttribute(string name)
    {
        MeterName = name;
    }
}
