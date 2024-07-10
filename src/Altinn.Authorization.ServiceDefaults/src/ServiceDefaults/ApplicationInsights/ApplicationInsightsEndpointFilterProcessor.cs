using Altinn.Authorization.ServiceDefaults.Telemetry;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using System.Diagnostics.CodeAnalysis;

namespace Altinn.Authorization.ServiceDefaults.ApplicationInsights;

[ExcludeFromCodeCoverage]
internal class ApplicationInsightsEndpointFilterProcessor
    : ITelemetryProcessor
{
    private readonly ITelemetryProcessor _next;

    public ApplicationInsightsEndpointFilterProcessor(ITelemetryProcessor next)
    {
        _next = next;
    }

    /// <inheritdoc/>
    public void Process(ITelemetry item)
    {
        if (item is RequestTelemetry request 
            && request.Url is { } url
            && TelemetryHelpers.ShouldExclude(url))
        {
            return;
        }

        _next.Process(item);
    }
}
