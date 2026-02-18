namespace Altinn.Authorization.ServiceDefaults.Npgsql.Telemetry;

/// <summary>
/// Telemetry options for a <see cref="NpgsqlDataSource"/>.
/// </summary>
public interface INpgsqlDataSourceTelemetryOptions
    : INpgsqlTelemetryOptions
{
    /// <summary>
    /// Excludes the specified query from telemetry.
    /// </summary>
    /// <param name="query">The query to exclude from telemetry.</param>
    public void ExcludeQuery(string query);

    /// <summary>
    /// Excludes queries from telemetry based on a custom predicate function.
    /// </summary>
    /// <remarks>Use this method to dynamically filter out items from the query results according to custom
    /// logic. Ensure that the predicate function is efficient. If possible, preffer using the
    /// <see cref="ExcludeQuery(string)"/> overload instead.</remarks>
    /// <param name="excludePredicate">A function that receives the query under consideration for inclusion
    /// in telemetry. Returns <see langword="true"/> to exclude the item; otherwise, <see langword="false"/>.</param>
    public void ExcludeQuery(Func<string, bool> excludePredicate);
}
