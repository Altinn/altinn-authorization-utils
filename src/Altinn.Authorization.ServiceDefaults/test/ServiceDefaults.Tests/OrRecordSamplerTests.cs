using Altinn.Authorization.ServiceDefaults.OpenTelemetry;
using OpenTelemetry.Trace;

namespace Altinn.Authorization.ServiceDefaults.Tests;

public class OrRecordSamplerTests
{
    [Fact]
    public void Description_WrapsInner()
    {
        var inner = new AlwaysOnSampler();
        var sut = new OrRecordSampler(inner);

        sut.Description.ShouldBe($"OrRecord({inner.Description})");
    }

    [Fact]
    public void Result_IsOriginal_IfSampled()
    {
        var attributes = new Dictionary<string, object> { { "foo", "bar" } };
        var inner = new TestSampler(new SamplingResult(SamplingDecision.RecordAndSample, attributes, "state1=foo"));
        var sut = new OrRecordSampler(inner);

        var result = sut.ShouldSample(default);
        result.Decision.ShouldBe(SamplingDecision.RecordAndSample);
        result.Attributes.ShouldBeSameAs(attributes);
        result.TraceStateString.ShouldBe("state1=foo");
    }

    [Fact]
    public void Result_IsOriginal_IfRecorded()
    {
        var attributes = new Dictionary<string, object> { { "foo", "bar" } };
        var inner = new TestSampler(new SamplingResult(SamplingDecision.RecordOnly, attributes, "state2=foo"));
        var sut = new OrRecordSampler(inner);

        var result = sut.ShouldSample(default);
        result.Decision.ShouldBe(SamplingDecision.RecordOnly);
        result.Attributes.ShouldBeSameAs(attributes);
        result.TraceStateString.ShouldBe("state2=foo");
    }

    [Fact]
    public void Result_IsRecorded_IfDropped()
    {
        var attributes = new Dictionary<string, object> { { "foo", "bar" } };
        var inner = new TestSampler(new SamplingResult(SamplingDecision.Drop, attributes, "state1=foo"));
        var sut = new OrRecordSampler(inner);

        var result = sut.ShouldSample(default);
        result.Decision.ShouldBe(SamplingDecision.RecordOnly);
        result.Attributes.ShouldBeSameAs(attributes);
        result.TraceStateString.ShouldBe("state1=foo");
    }

    private sealed class TestSampler(SamplingResult result)
        : Sampler
    {
        public override SamplingResult ShouldSample(in SamplingParameters samplingParameters)
            => result;
    }
}
