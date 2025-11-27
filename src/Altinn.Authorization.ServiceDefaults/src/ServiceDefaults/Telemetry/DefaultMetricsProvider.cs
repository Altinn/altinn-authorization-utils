using System.Collections.Concurrent;
using System.Diagnostics.Metrics;

namespace Altinn.Authorization.ServiceDefaults.Telemetry;

internal sealed class DefaultMetricsProvider
    : IMetricsProvider
{
    private readonly ConcurrentDictionary<Type, object> _instances = new();
    private readonly IMeterFactory _factory;

    public DefaultMetricsProvider(IMeterFactory factory)
    {
        _factory = factory;
    }

    public T Get<T>()
        where T : IMetrics<T>
        => (T)_instances.GetOrAdd(
            typeof(T),
            static (_, state) => state.TypeFactory.Create(state.MeterFactory),
            (TypeFactory: Factory<T>.Instance, MeterFactory: _factory));

    private abstract class Factory
    {
        public abstract object Create(IMeterFactory factory);
    }

    private sealed class Factory<T>
        : Factory
        where T : IMetrics<T>
    {
        public static Factory Instance { get; } = new Factory<T>();

        private Factory()
        {
        }

        public override object Create(IMeterFactory factory)
        {
            var options = new MeterOptions(T.MeterName)
            {
                Version = T.MeterVersion,
            };

            var meter = factory.Create(options);
            return T.Create(meter);
        }
    }
}
