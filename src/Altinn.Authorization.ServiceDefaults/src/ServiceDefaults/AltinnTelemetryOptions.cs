using System.ComponentModel.DataAnnotations;

namespace Altinn.Authorization.ServiceDefaults;

/// <summary>
/// Represents configuration options for telemetry collection in Altinn applications.
/// </summary>
public class AltinnTelemetryOptions
{
    /// <summary>
    /// Gets or sets the proportion of items to include during sampling.
    /// </summary>
    /// <remarks>The value must be between 0.0 and 1.0, inclusive. A value of 0.0 means no items are sampled;
    /// a value of 1.0 means all items are included.</remarks>
    [Range(0.0, 1.0)]
    public double SamplingRatio { get; set; } = 1.0;
}
