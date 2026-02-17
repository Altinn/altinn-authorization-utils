using System.Diagnostics;

namespace Altinn.Authorization.ServiceDefaults.OpenTelemetry;

/// <summary>
/// A <see cref="ITailSampler"/> that records activities with <see cref="ActivityStatusCode.Error"/> status.
/// </summary>
internal sealed class ErrorTailSampler
    : ITailSampler
{
    public RecordDecision ShouldRecord(in TailSamplingParameters parameters)
        => parameters.Status switch
        {
            ActivityStatusCode.Error => RecordDecision.Record,
            _ => RecordDecision.Undecided,
        };
}
