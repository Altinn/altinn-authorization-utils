using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.IO.Pipelines;

namespace Altinn.Authorization.ServiceDefaults.HealthChecks;

internal abstract class HealthReportFormatter
{
    internal protected abstract bool Handles(MediaType accepts);

    internal protected abstract Task WriteAsync(
        HealthReport report, 
        string contentType,
        PipeWriter writer, 
        CancellationToken cancellationToken);
}
