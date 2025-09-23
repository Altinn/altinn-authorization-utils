using System.Diagnostics;

namespace Altinn.Authorization.ServiceDefaults.HttpClient;

internal static class HttpClientTelemetry
{
    public static ActivitySource ActivitySource { get; } = new("Altinn.Authorization.ServiceDefaults.HttpClient");
}
