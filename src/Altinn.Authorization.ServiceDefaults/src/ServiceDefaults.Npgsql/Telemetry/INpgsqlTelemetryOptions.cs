namespace Altinn.Authorization.ServiceDefaults.Npgsql.Telemetry;

/// <summary>
/// Telemetry options for Npgsql operations.
/// </summary>
public interface INpgsqlTelemetryOptions
{
    /// <summary>
    /// Sets the filter result for a parameter with the given name. This will determine whether the parameter is included in telemetry data for Npgsql operations that occur within this scope, and if included, whether its value is redacted or not.
    /// </summary>
    /// <param name="parameterName">The parameter name.</param>
    /// <param name="filterResult">The filter result.</param>
    public void SetParameterFilter(string parameterName, NpgsqlTelemetryParameterFilterResult filterResult);

    /// <summary>
    /// Sets the filter result to apply for parameters of the specified type when collecting telemetry data.
    /// </summary>
    /// <remarks>Use this method to control how telemetry data is collected for specific parameter types. This
    /// can be useful for excluding sensitive information or customizing telemetry behavior for certain types.</remarks>
    /// <param name="parameterType">The type of parameter for which the filter result will be set. Cannot be null.</param>
    /// <param name="filterResult">The filter result to apply to parameters of the specified type.</param>
    public void SetParameterFilter(Type parameterType, NpgsqlTelemetryParameterFilterResult filterResult);
}
