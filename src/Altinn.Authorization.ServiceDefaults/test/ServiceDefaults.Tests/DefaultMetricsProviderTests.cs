using Altinn.Authorization.ServiceDefaults.Telemetry;
using Altinn.Authorization.TestUtils;
using System.Diagnostics.Metrics;

[assembly: AssemblyMeterName("Altinn.Authorization.ServiceDefaults.Tests")]
[assembly: AssemblyMeterVersion("fake-version")]

namespace Altinn.Authorization.ServiceDefaults.Tests;

public class DefaultMetricsProviderTests
{
    [Fact]
    public void InstancesAreCached()
    {
        using var meterFactory = new TestMeterFactory();
        var sut = new DefaultMetricsProvider(meterFactory);

        var dummy1 = sut.Get<DummyMeters>();
        var dummy2 = sut.Get<DummyMeters>();

        dummy1.ShouldBeSameAs(dummy2);
    }

    [Fact]
    public void UsesCorrectMeterNameAndVersion()
    {
        using var meterFactory = new TestMeterFactory();
        var sut = new DefaultMetricsProvider(meterFactory);

        var dummy = sut.Get<DummyMeters>();
        dummy.Meter.Name.ShouldBe("Altinn.Authorization.ServiceDefaults.Tests");
        dummy.Meter.Version.ShouldBe("fake-version");
    }

    private sealed class DummyMeters
        : IMetrics<DummyMeters>
    {
        private static int _instanceCount = 0;

        public int InstanceNumber { get; }
        public Meter Meter { get; }

        public static DummyMeters Create(Meter meter)
        {
            var num = Interlocked.Increment(ref _instanceCount);
            return new DummyMeters(num, meter);
        }

        private DummyMeters(int instanceNumber, Meter meter)
        {
            InstanceNumber = instanceNumber;
            Meter = meter;
        }
    }
}
