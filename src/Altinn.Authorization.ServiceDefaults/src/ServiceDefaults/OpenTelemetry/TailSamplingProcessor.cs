using OpenTelemetry;
using System.Collections.Immutable;
using System.Diagnostics;

namespace Altinn.Authorization.ServiceDefaults.OpenTelemetry;

internal sealed class TailSamplingProcessor
    : BaseProcessor<Activity>
{
    private readonly ImmutableArray<ITailSampler> _tailSamplers;

    public TailSamplingProcessor(IEnumerable<ITailSampler> tailSamplers)
    {
        _tailSamplers = tailSamplers.ToImmutableArray();
    }

    public override void OnEnd(Activity activity)
    {
        if (activity.ActivityTraceFlags.HasFlag(ActivityTraceFlags.Recorded)) 
        {
            // If the activity is already sampled, we don't need to do anything
            return;
        }

        var parameters = new TailSamplingParameters(activity);
        foreach (var sampler in _tailSamplers)
        {
            switch (sampler.ShouldRecord(in parameters))
            {
                case RecordDecision.Record:
                    activity.ActivityTraceFlags |= ActivityTraceFlags.Recorded;
                    return;

                case RecordDecision.Drop:
                    // this is already the case, given the check above
                    return;

                default:
                    continue;
            }
        }
    }
}
