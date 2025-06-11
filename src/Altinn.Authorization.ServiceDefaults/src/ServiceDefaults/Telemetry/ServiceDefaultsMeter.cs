using System.Diagnostics.Metrics;
using System.Reflection;

namespace Altinn.Authorization.ServiceDefaults.Telemetry;

internal sealed class ServiceDefaultsMeter
{
    internal static readonly string MeterName = "Altinn.ServiceDefaults";

    private readonly Meter _meter;

    public ServiceDefaultsMeter(IMeterFactory meterFactory)
    {
        _meter = meterFactory.Create(new MeterOptions(MeterName)
        {
            Version = typeof(ServiceDefaultsMeter).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion,
        });
    }

    /// <inheritdoc cref="Meter.CreateCounter{T}(string, string?, string?)"/>
    public Counter<T> CreateCounter<T>(string name, string? unit = null, string? description = null)
        where T : struct
        => _meter.CreateCounter<T>(name, unit, description);
}
