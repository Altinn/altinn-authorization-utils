namespace Altinn.Authorization.ServiceDefaults.Npgsql.Telemetry;

/// <summary>
/// Scoped telemetry options for Npgsql operations.
/// </summary>
public interface INpgsqlScopeTelemetryOptions
    : INpgsqlTelemetryOptions
{
    /// <summary>
    /// Gets or sets the summary to use for Npgsql operations that occur within this scope.
    /// </summary>
    public string? Summary { get; set; }

    /// <summary>
    /// Gets or sets the name of the span to use for Npgsql operations that occur within this scope.
    /// </summary>
    public string? SpanName { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether Npgsql operations that occur within this scope should be traced or not.
    /// </summary>
    public bool? ShouldTrace { get; set; }
}
