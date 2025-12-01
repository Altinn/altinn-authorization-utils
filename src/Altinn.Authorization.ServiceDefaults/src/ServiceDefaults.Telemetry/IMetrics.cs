using System.Diagnostics.Metrics;
using System.Reflection;

namespace Altinn.Authorization.ServiceDefaults.Telemetry;

/// <summary>
/// A generic interface for metrics where the meter options is derived from the specified
/// <typeparamref name="TSelf"/>.
/// </summary>
/// <typeparam name="TSelf">Self type.</typeparam>
public interface IMetrics<TSelf>
    where TSelf : IMetrics<TSelf>
{
    /// <summary>
    /// Gets the name used for <see cref="MeterOptions.Name"/> by <see cref="IMeterFactory.Create(MeterOptions)"/>.
    /// </summary>
    /// <remarks>
    /// Defaults to (in decreasing order of priority):
    /// <list type="number">
    ///   <item><see cref="AssemblyMeterNameAttribute"/> on the assembly that defines <typeparamref name="TSelf"/>.</item>
    ///   <item>The name of the assembly that defines <typeparamref name="TSelf"/>.</item>
    /// </list>
    /// </remarks>
    public static virtual string MeterName
        => AssemblyMeterDescriptor.For<TSelf>().Name;

    /// <summary>
    /// Gets the version used for <see cref="MeterOptions.Version"/> by <see cref="IMeterFactory.Create(MeterOptions)"/>.
    /// </summary>
    /// <remarks>
    /// Defaults to (in decreasing order of priority):
    /// <list type="number">
    ///   <item><see cref="AssemblyMeterVersionAttribute"/> on the assembly that defines <typeparamref name="TSelf"/>.</item>
    ///   <item><see cref="AssemblyInformationalVersionAttribute"/> on the assembly that defines <typeparamref name="TSelf"/>.</item>
    ///   <item><see langword="null"/> if none of the above are present.</item>
    /// </list>
    /// </remarks>
    public static virtual string? MeterVersion
        => AssemblyMeterDescriptor.For<TSelf>().Version;

    /// <summary>
    /// Creates a new instance of self.
    /// </summary>
    /// <param name="meter">The <see cref="Meter"/>.</param>
    /// <returns>A new instance of self.</returns>
    /// <remarks><paramref name="meter"/> does not need to be disposed.</remarks>
    public static abstract TSelf Create(Meter meter);
}
