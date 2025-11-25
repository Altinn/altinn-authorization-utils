namespace Altinn.Authorization.ServiceDefaults.Telemetry;

/// <summary>
/// Defines a contract for obtaining strongly typed metrics objects.
/// </summary>
public interface IMetricsProvider
{
    /// <summary>
    /// Gets or creates a strongly typed metrics object.
    /// </summary>
    /// <typeparam name="T">The type of the metrics object.</typeparam>
    /// <returns>An instance of <typeparamref name="T"/>.</returns>
    public T Get<T>()
        where T : IMetrics<T>;
}
