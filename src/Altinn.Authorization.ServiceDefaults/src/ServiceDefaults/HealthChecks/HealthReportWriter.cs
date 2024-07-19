using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace Altinn.Authorization.ServiceDefaults.HealthChecks;

/// <summary>
/// Settings for configuring the behavior of the <see cref="HealthReportWriter"/>.
/// </summary>
public class HealthReportWriterSettings
{
    /// <summary>
    /// Gets or sets what format the health report should be written in.
    /// </summary>
    public HealthReportFormat Format { get; set; } = HealthReportFormat.Auto;

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

    /// <summary>
    /// Formats for health report serialization.
    /// </summary>
    public enum HealthReportFormat
    {
        /// <summary>
        /// Automatically determine the format based on the request.
        /// </summary>
        Auto = 0,

        /// <summary>
        /// Write the health report as plain text. This just writes the health status as a string.
        /// </summary>
        PlainText,

        /// <summary>
        /// Write the health report as a JSON object. This is the default format.
        /// </summary>
        JsonV1,
    }
}

internal class HealthReportWriter
{
    private static readonly Comparison<MediaTypeSegmentWithQuality> _sortFunction = (left, right) =>
    {
        return left.Quality > right.Quality ? -1 : (left.Quality == right.Quality ? 0 : 1);
    };

    private static readonly HealthReportFormatter _plain = new PlainTextHealthReportFormatter();

    private readonly HealthReportFormatter _jsonV1;
    private readonly HealthReportWriterSettings.HealthReportFormat _format;
    private readonly ImmutableArray<HealthReportFormatter> _formatters;

    public HealthReportWriter(
        IOptions<HealthReportWriterSettings> options)
    {
        var settings = options.Value;

        _format = settings.Format;
        _jsonV1 = new JsonV1HealthReportFormatter(settings);

        // Note: order matters here, the first formatter that can handle the media type will be used
        _formatters = [
            _jsonV1,
            _plain,
        ];
    }

    public static implicit operator Func<HttpContext, HealthReport, Task>(HealthReportWriter writer)
        => writer.WriteHealthCheckReport;

    public Task WriteHealthCheckReport(HttpContext context, HealthReport report)
    {
        var (formatter, contentType) = _format switch
        {
            HealthReportWriterSettings.HealthReportFormat.PlainText => (_plain, PlainTextHealthReportFormatter.ContentType),
            HealthReportWriterSettings.HealthReportFormat.JsonV1 => (_jsonV1, JsonV1HealthReportFormatter.ContentType),
            _ => SelectFormatter(context.Request.Headers.Accept),
        };

        context.Response.ContentType = contentType;
        return formatter.WriteAsync(report, contentType, context.Response.BodyWriter, context.RequestAborted);
    }

    private (HealthReportFormatter Formatter, string ContentType) SelectFormatter(StringValues acceptHeader)
    {
        if (!SelectFormatter(acceptHeader, out var formatter, out var contentType))
        {
            // default to plain text
            formatter = _plain;
            contentType = PlainTextHealthReportFormatter.ContentType;
        }

        return (formatter, contentType);
    }

    private bool SelectFormatter(
        StringValues acceptHeader, 
        [NotNullWhen(true)] out HealthReportFormatter? formatter, 
        [NotNullWhen(true)] out string? selectedContentType)
    {
        var acceptableMediaTypes = new List<MediaTypeSegmentWithQuality>();
        AcceptHeaderParser.ParseAcceptHeader(acceptHeader, acceptableMediaTypes);

        acceptableMediaTypes.Sort(_sortFunction);
        
        if (acceptableMediaTypes.Count == 0)
        {
            formatter = null;
            selectedContentType = null;
            return false;
        }

        foreach (var mediaType in acceptableMediaTypes)
        {
            foreach (var f in _formatters)
            {
                if (f.Handles(new MediaType(mediaType.MediaType)))
                {
                    selectedContentType = mediaType.MediaType.ToString();
                    formatter = f;
                    return true;
                }
            }
        }

        formatter = null;
        selectedContentType = null;
        return false;
    }
}
