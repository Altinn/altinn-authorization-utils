using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.IO.Pipelines;
using System.Text.Json;

namespace Altinn.Authorization.ServiceDefaults.HealthChecks;

internal abstract class JsonHealthReportFormatter
    : HealthReportFormatter
{
    private static readonly MediaType _mediaType = new("application/json");

    protected readonly bool _includeData;
    protected readonly bool _includeTags;
    protected readonly HealthReportWriterSettings.ExceptionHandling _exceptionHandling;
    protected readonly JsonSerializerOptions _jsonSerializerOptions;

    protected JsonHealthReportFormatter(HealthReportWriterSettings settings)
    {
        _includeData = settings.IncludeData;
        _includeTags = settings.IncludeTags;
        _exceptionHandling = settings.Exceptions;
        _jsonSerializerOptions = settings.JsonSerializerOptions;
    }

    protected internal override bool Handles(MediaType accepts)
        => _mediaType.IsSubsetOf(accepts);

    protected internal sealed override async Task WriteAsync(HealthReport report, string contentType, PipeWriter writer, CancellationToken cancellationToken)
    {
        {
            await using var jsonWriter = new Utf8JsonWriter(writer, new JsonWriterOptions
            {
                Indented = _jsonSerializerOptions.WriteIndented,
                MaxDepth = _jsonSerializerOptions.MaxDepth,
                Encoder = _jsonSerializerOptions.Encoder,
            });

            await WriteJsonReport(jsonWriter, report, cancellationToken);
        }

        await writer.FlushAsync(cancellationToken);
    }

    protected abstract Task WriteJsonReport(Utf8JsonWriter writer, HealthReport report, CancellationToken cancellationToken);
}
