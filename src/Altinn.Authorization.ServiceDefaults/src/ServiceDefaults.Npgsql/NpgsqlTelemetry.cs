using Altinn.Authorization.ServiceDefaults.Npgsql.Telemetry;

namespace Altinn.Authorization.ServiceDefaults.Npgsql;

/// <summary>
/// Telemetry utilities for Npgsql.
/// </summary>
public static class NpgsqlTelemetry
{
    /// <summary>
    /// Configures npgsql telemetry within the current scope. The configuration will be applied to all
    /// npgsql operations executed within the scope, including those executed in child scopes. If no
    /// configuration is provided, the default configuration will be used. The configuration will be
    /// reverted when the scope is disposed.
    /// </summary>
    /// <remarks>
    /// Configuration is additive, meaning that if multiple configurations are applied within the same
    /// scope, all of them will be applied.
    /// </remarks>
    /// <param name="configure">A delegate to configure this scope.</param>
    /// <returns>A <see cref="IDisposable"/> that reverts the configuration when disposed.</returns>
    public static IDisposable? Configure(Action<INpgsqlScopeTelemetryOptions> configure)
        => AltinnNpgsqlTelemetry.StartScope(configure);

    /// <summary>
    /// Temporarily disables tracing for the current scope.
    /// </summary>
    /// <remarks>Use the returned <see cref="IDisposable"/> in a <c>using</c> statement to ensure that tracing
    /// is re-enabled when the scope ends.</remarks>
    /// <returns>An <see cref="IDisposable"/> instance that, when disposed, restores the previous tracing state.</returns>
    public static IDisposable? DisableTracing()
        => Configure(static o => o.ShouldTrace = false);

    /// <inheritdoc cref="Configure(Action{INpgsqlScopeTelemetryOptions})" path="/summary"/>
    /// <inheritdoc cref="Configure(Action{INpgsqlScopeTelemetryOptions})" path="/remarks"/>
    /// <param name="spanName">Optional span name to use for operations in this scope.</param>
    /// <param name="traceParameterNames">Optional additional parameter names to include in traces.</param>
    /// <param name="traceParameterTypes">Optional additional parameter types to include in traces.</param>
    /// <returns></returns>
    public static IDisposable? Configure(
        string? spanName = null,
        IEnumerable<string>? traceParameterNames = null,
        IEnumerable<Type>? traceParameterTypes = null)
    {
        if (spanName is null && traceParameterNames is null && traceParameterTypes is null)
        {
            return null;
        }

        return Configure(o =>
        {
            if (spanName is not null)
            {
                o.SpanName = spanName;
            }

            if (traceParameterNames is not null)
            {
                foreach (var name in traceParameterNames)
                {
                    o.SetParameterFilter(name, NpgsqlTelemetryParameterFilterResult.Include);
                }
            }
            
            if (traceParameterTypes is not null)
            {
                foreach (var type in traceParameterTypes)
                {
                    o.SetParameterFilter(type, NpgsqlTelemetryParameterFilterResult.Include);
                }
            }
        });
    }
}
