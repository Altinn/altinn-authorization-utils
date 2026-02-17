using OpenTelemetry.Trace;

namespace Altinn.Authorization.ServiceDefaults.OpenTelemetry;

/// <summary>
/// Provides a sampler that always records samples, regardless of the sampling parameters or context.
/// </summary>
public sealed class AlwaysRecordSampler
    : Sampler
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AlwaysRecordSampler"/> class.
    /// </summary>
    public AlwaysRecordSampler()
    {
        Description = "AlwaysRecord";
    }

    /// <inheritdoc/>
    public override SamplingResult ShouldSample(in SamplingParameters samplingParameters)
        => new(SamplingDecision.RecordOnly);
}
