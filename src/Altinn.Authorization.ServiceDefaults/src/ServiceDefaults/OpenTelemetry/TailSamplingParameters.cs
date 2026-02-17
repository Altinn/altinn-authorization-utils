using System.Diagnostics;

namespace Altinn.Authorization.ServiceDefaults.OpenTelemetry;

/// <summary>
/// Parameters passed to <see cref="ITailSampler.ShouldRecord(in TailSamplingParameters)"/>.
/// </summary>
public readonly struct TailSamplingParameters
{
    private readonly Activity _activity;

    internal TailSamplingParameters(Activity activity)
    {
        _activity = activity;
    }

    /// <summary>
    /// Gets the <see cref="ActivityStatusCode"/> of the activity being evaluated for tail-sampling.
    /// </summary>
    public readonly ActivityStatusCode Status
        => _activity.Status;

    /// <summary>
    /// Gets the <see cref="ActivityKind"/> of the activity being evaluated for tail-sampling.
    /// </summary>
    public readonly ActivityKind Kind
        => _activity.Kind;

    /// <summary>
    /// Gets the tags of the activity being evaluated for tail-sampling.
    /// </summary>
    public readonly ActivityTags Tags
        => new(_activity);
}
