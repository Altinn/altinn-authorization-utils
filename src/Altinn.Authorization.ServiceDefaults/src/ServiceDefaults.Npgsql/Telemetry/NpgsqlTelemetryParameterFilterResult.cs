namespace Altinn.Authorization.ServiceDefaults.Npgsql.Telemetry;

/// <summary>
/// Specifies the result of evaluating whether a parameter should be included in telemetry data for Npgsql operations.
/// </summary>
public enum NpgsqlTelemetryParameterFilterResult
    : byte
{
    /// <summary>
    /// This step in the chain does not specify whether parameters should be included in telemetry or not. The decision will be deferred to the parent node in the chain, and ultimately to the root node if no node in the chain specifies it.
    /// </summary>
    Default = default,

    /// <summary>
    /// Include the parameter (name and value) in telemetry.
    /// </summary>
    Include,

    /// <summary>
    /// Include the parameter with a redacted value in telemetry.
    /// </summary>
    RedactValue,

    /// <summary>
    /// Do not include the parameter in telemetry at all.
    /// </summary>
    Ignore,
}
