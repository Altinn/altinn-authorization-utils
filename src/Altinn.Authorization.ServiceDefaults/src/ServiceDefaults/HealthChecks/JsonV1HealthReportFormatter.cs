using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace Altinn.Authorization.ServiceDefaults.HealthChecks;

internal class JsonV1HealthReportFormatter(HealthReportWriterSettings settings)
    : JsonHealthReportFormatter(settings)
{
    private static readonly string _contentType = "application/vnd.altinn.health.v1+json";

    private static readonly MediaType _mediaType = new(_contentType);

    internal static string ContentType => _contentType;

    protected internal override bool Handles(MediaType accepts)
        => _mediaType.IsSubsetOf(accepts)
        || base.Handles(accepts);

    protected override Task WriteJsonReport(Utf8JsonWriter writer, HealthReport report, CancellationToken cancellationToken)
    {
        Span<byte> scratch = stackalloc byte[32];

        writer.WriteStartObject();
        {
            WriteStatus(writer, "status"u8, report.Status);
            WriteDuration(writer, "totalDuration"u8, report.TotalDuration, scratch);

            writer.WriteStartObject("entries"u8);
            foreach (var (name, entry) in report.Entries)
            {
                writer.WriteStartObject(name);
                WriteEntry(writer, in entry, scratch);
                writer.WriteEndObject();
            }
            writer.WriteEndObject();
        }
        writer.WriteEndObject();

        return writer.FlushAsync(cancellationToken);
    }

    private void WriteEntry(Utf8JsonWriter writer, in HealthReportEntry entry, Span<byte> scratch)
    {
        WriteStatus(writer, "status"u8, entry.Status);
        WriteDuration(writer, "duration"u8, entry.Duration, scratch);

        var description = entry.Description;

        if (ShouldIncludeExceptions(_exceptionHandling) && entry.Exception is { } exn)
        {
            description ??= exn.Message;
            WriteException(writer, "exception"u8, exn);
        }

        if (description is not null)
        {
            writer.WriteString("description"u8, description);
        }

        // write data
        if (_includeData && entry.Data.Count > 0)
        {
            writer.WritePropertyName("data"u8);
            JsonSerializer.Serialize(writer, entry.Data, _jsonSerializerOptions);
        }
        // end data

        // write tags
        if (_includeTags)
        {
            var first = true;
            foreach (var tag in entry.Tags)
            {
                if (first)
                {
                    writer.WriteStartArray("tags"u8);
                    first = false;
                }

                writer.WriteStringValue(tag);
            }

            if (!first)
            {
                writer.WriteEndArray();
            }
        }
        // end tags
    }

    private static void WriteException(Utf8JsonWriter writer, ReadOnlySpan<byte> utf8PropertyName, Exception exn)
    {
        writer.WriteStartObject(utf8PropertyName);
        writer.WriteString("message"u8, exn.Message);

        if (exn.StackTrace is { } stackTrace)
        {
            writer.WriteString("stackTrace"u8, stackTrace);
        }

        if (exn.InnerException is { } inner)
        {
            WriteException(writer, "innerException"u8, inner);
        }

        writer.WriteEndObject();
    }

    private static void WriteStatus(Utf8JsonWriter writer, ReadOnlySpan<byte> utf8PropertyName, HealthStatus status)
    {
        writer.WriteString(utf8PropertyName, status switch
        {
            HealthStatus.Unhealthy => "unhealthy"u8,
            HealthStatus.Degraded => "degraded"u8,
            HealthStatus.Healthy => "healthy"u8,
            _ => "unknown"u8,
        });
    }

    private static void WriteDuration(Utf8JsonWriter writer, ReadOnlySpan<byte> utf8PropertyName, TimeSpan duration, Span<byte> scratch)
    {
        if (!duration.TryFormat(scratch, out var written, "c"))
        {
            // c requires 16 bytes at most
            Unreachable();
        }

        writer.WriteString(utf8PropertyName, scratch[..written]);
    }

    private static bool ShouldIncludeExceptions(HealthReportWriterSettings.ExceptionHandling handling)
        => handling != HealthReportWriterSettings.ExceptionHandling.None;

    [DoesNotReturn]
    private static void Unreachable()
        => throw new InvalidOperationException("Unreachable code executed.");
}
