using Altinn.Authorization.ServiceDefaults.Telemetry;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using System.Reflection;

namespace Microsoft.Extensions.Hosting;

/// <summary>
/// Extension methods for <see cref="IOpenTelemetryBuilder"/>.
/// </summary>
public static class AltinnOpenTelemetryBuilderExtensions
{
    /// <param name="builder">The <see cref="IOpenTelemetryBuilder"/>.</param>
    extension(IOpenTelemetryBuilder builder)
    {
        /// <summary>
        /// Adds the meter associated with the specified assembly to the OpenTelemetry pipeline.
        /// </summary>
        /// <param name="assembly">The assembly whose meter name is to be added.</param>
        /// <returns>The current instance of IOpenTelemetryBuilder for chaining further configuration.</returns>
        public IOpenTelemetryBuilder AddAssemblyMetrics(Assembly assembly)
        {
            var name = AssemblyMeterDescriptor.For(assembly).Name;

            builder.Services.ConfigureOpenTelemetryMeterProvider(options =>
            {
                options.AddMeter(name);
            });

            return builder;
        }

        /// <summary>
        /// Adds the meter associated with the specified assembly to the OpenTelemetry pipeline.
        /// </summary>
        /// <param name="type">The type which assembly whose meter name is to be added.</param>
        /// <returns>The current instance of IOpenTelemetryBuilder for chaining further configuration.</returns>
        public IOpenTelemetryBuilder AddAssemblyMetrics(Type type)
            => builder.AddAssemblyMetrics(type.Assembly);

        /// <summary>
        /// Adds the meter associated with the specified assembly to the OpenTelemetry pipeline.
        /// </summary>
        /// <typeparam name="T">The type which assembly whose meter name is to be added.</typeparam>
        /// <returns>The current instance of IOpenTelemetryBuilder for chaining further configuration.</returns>
        public IOpenTelemetryBuilder AddAssemblyMetrics<T>()
            => builder.AddAssemblyMetrics(typeof(T));

        /// <summary>
        /// Adds the meter associated with the specified metrics type to the OpenTelemetry pipeline.
        /// </summary>
        /// <typeparam name="T">The metrics type that implements the <see cref="IMetrics{TSelf}"/> interface. The meter name is obtained from this type.</typeparam>
        /// <returns>The current instance of IOpenTelemetryBuilder for chaining further configuration.</returns>
        /// <remarks>
        /// This method should only be used for meters that override the default <see cref="IMetrics{TSelf}.MeterName"/>.
        /// If this is not overridden, consider using <see cref="AddAssemblyMetrics{T}(IOpenTelemetryBuilder)"/> to reduce the number of register calls needed.
        /// </remarks>
        public IOpenTelemetryBuilder AddMetrics<T>()
            where T : IMetrics<T>
        {
            builder.Services.ConfigureOpenTelemetryMeterProvider(options =>
            {
                options.AddMeter(T.MeterName);
            });

            return builder;
        }
    }
}
