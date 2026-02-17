using OpenTelemetry.Trace;

namespace Altinn.Authorization.ServiceDefaults.OpenTelemetry;

/// <summary>
/// Provides a sampler that records samples when the inner sampler decides to drop them.
/// </summary>
/// <remarks>This sampler wraps another sampler and modifies its behavior to ensure that if the inner sampler
/// drops a sample, it will instead record it. This can be useful in scenarios where you want to ensure that certain
/// samples are always recorded, this allows running tail-sampling on all activities.</remarks>
public sealed class OrRecordSampler
    : Sampler
{
    private readonly Sampler _inner;

    /// <summary>
    /// Initializes a new instance of the <see cref="OrRecordSampler"/> class with the specified inner sampler.
    /// </summary>
    /// <param name="inner">The inner sampler whose decisions will be modified by this sampler.</param>
    public OrRecordSampler(Sampler inner)
    {
        _inner = inner;

        Description = $"OrRecord({_inner.Description})";
    }

    /// <inheritdoc/>
    public override SamplingResult ShouldSample(in SamplingParameters samplingParameters)
    {
        var result = _inner.ShouldSample(samplingParameters);
        if (result.Decision == SamplingDecision.Drop)
        {
            result = new(SamplingDecision.RecordOnly, result.Attributes, result.TraceStateString);
        }

        return result;
    }
}
