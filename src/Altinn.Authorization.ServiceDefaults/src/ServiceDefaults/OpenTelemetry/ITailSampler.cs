namespace Altinn.Authorization.ServiceDefaults.OpenTelemetry;

/// <summary>
/// Defines a sampler that makes a decision on whether to record an activity based on its parameters at the point where the activity is completed.
/// </summary>
public interface ITailSampler
{
    /// <summary>
    /// Determines whether a record should be created based on the specified sampling parameters.
    /// </summary>
    /// <param name="parameters">Activity information.</param>
    /// <returns>A <see cref="RecordDecision"/>.</returns>
    RecordDecision ShouldRecord(in TailSamplingParameters parameters);
}
