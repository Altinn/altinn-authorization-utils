using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.IO.Pipelines;
using System.Text.Json;

namespace Altinn.Authorization.ServiceDefaults.HealthChecks;

/// <summary>
/// Settings for configuring the behavior of the <see cref="HealthReportWriter"/>.
/// </summary>
public class HealthReportWriterSettings
{
    /// <summary>
    /// Gets or sets whether to include "data" in the health report.
    /// </summary>
    public bool IncludeData { get; set; } = false;

    /// <summary>
    /// Gets or sets whether to include "tags" in the health report.
    /// </summary>
    public bool IncludeTags { get; set; } = true;

    /// <summary>
    /// Gets or sets how exceptions should be handled in the health report.
    /// </summary>
    public ExceptionHandling Exceptions { get; set; } = ExceptionHandling.None;

    /// <summary>
    /// Gets or sets the <see cref="JsonSerializerOptions"/> to use when serializing the health report.
    /// </summary>
    public JsonSerializerOptions JsonSerializerOptions { get; set; } = new JsonSerializerOptions(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
#if DEBUG
        WriteIndented = true,
#endif
    };

    /// <summary>
    /// Exception handling options for health report serialization.
    /// </summary>
    [Flags]
    public enum ExceptionHandling
        : byte
    {
        /// <summary>
        /// Do not include any exception information in the health report.
        /// </summary>
        None = 0,

        /// <summary>
        /// Include the exception in the health report.
        /// </summary>
        Include = 1 << 0,

        /// <summary>
        /// Include the exception stack trace in the health report.
        /// </summary>
        IncludeStackTrace = (1 << 1) | Include,

        /// <summary>
        /// Include the inner exception in the health report.
        /// </summary>
        IncludeInnerException = (1 << 2) | Include,
    }
}

internal class HealthReportWriter
{
    private static readonly string ContentType = "application/json";

    private bool _includeData;
    private bool _includeTags;
    private readonly HealthReportWriterSettings.ExceptionHandling _exceptionHandling;
    private readonly JsonSerializerOptions _jsonSerializerOptions;

    public HealthReportWriter(
        IOptions<HealthReportWriterSettings> options)
    {
        var settings = options.Value;

        _includeData = settings.IncludeData;
        _includeTags = settings.IncludeTags;
        _exceptionHandling = settings.Exceptions;
        _jsonSerializerOptions = settings.JsonSerializerOptions;
    }

    public static implicit operator Func<HttpContext, HealthReport?, Task>(HealthReportWriter writer)
        => writer.WriteHealthCheckReport;

    public Task WriteHealthCheckReport(HttpContext context, HealthReport? report)
    {
        context.Response.ContentType = ContentType;

        if (report is null)
        {
            context.Response.BodyWriter.Write("{}"u8);
            return context.Response.BodyWriter.FlushAsync(context.RequestAborted).AsTask();
        }

        WriteReport(report, context.Response.BodyWriter);
        return context.Response.BodyWriter.FlushAsync(context.RequestAborted).AsTask();
    }

    private void WriteReport(HealthReport report, PipeWriter pipeWriter)
    {
        using var writer = new Utf8JsonWriter(pipeWriter, new JsonWriterOptions
        {
            Indented = _jsonSerializerOptions.WriteIndented,
            MaxDepth = _jsonSerializerOptions.MaxDepth,
            Encoder = _jsonSerializerOptions.Encoder,
        });
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
