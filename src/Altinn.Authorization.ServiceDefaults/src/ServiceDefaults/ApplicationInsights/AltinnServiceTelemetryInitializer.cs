using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;
using System.Diagnostics.CodeAnalysis;

namespace Altinn.Authorization.ServiceDefaults.ApplicationInsights;

[ExcludeFromCodeCoverage]
internal class AltinnServiceTelemetryInitializer
    : ITelemetryInitializer
{
    private readonly string _serviceName;

    private int _logged = 0;

    public AltinnServiceTelemetryInitializer(AltinnServiceDescriptor serviceDescription)
    {
        _serviceName = serviceDescription.Name;
    }

    public void Initialize(ITelemetry telemetry)
    {
        if (string.IsNullOrEmpty(telemetry.Context.Cloud.RoleName))
        {
            telemetry.Context.Cloud.RoleName = _serviceName;
        }

        if (_logged == 0)
        {
            if (Interlocked.CompareExchange(ref _logged, 1, 0) == 0)
            {
                Console.WriteLine($"Cloud.RoleName: {telemetry.Context.Cloud.RoleName}");
            }
        }
    }
}
