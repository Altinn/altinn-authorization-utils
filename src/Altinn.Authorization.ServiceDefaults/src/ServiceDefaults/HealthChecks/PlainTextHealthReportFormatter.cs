using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Buffers;
using System.IO.Pipelines;

namespace Altinn.Authorization.ServiceDefaults.HealthChecks;

internal class PlainTextHealthReportFormatter
    : HealthReportFormatter
{
    private static readonly string _contentType = "text/plain";

    private static readonly MediaType _mediaType = new(_contentType);

    internal static string ContentType => _contentType;

    internal protected override bool Handles(MediaType accepts) 
        => _mediaType.IsSubsetOf(accepts);

    internal protected override Task WriteAsync(
        HealthReport report,
        string contentType,
        PipeWriter writer,
        CancellationToken cancellationToken)
    {
        writer.Write(report.Status switch
        {
            HealthStatus.Degraded => "Degraded"u8,
            HealthStatus.Unhealthy => "Unhealthy"u8,
            HealthStatus.Healthy => "Healthy"u8,
            _ => "Unknown"u8,
        });

        return writer.FlushAsync(cancellationToken).AsTask();
    }
}
