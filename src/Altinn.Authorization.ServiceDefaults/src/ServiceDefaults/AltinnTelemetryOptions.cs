using CommunityToolkit.Diagnostics;
using Microsoft.Extensions.Options;
using OpenTelemetry.Trace;
using System.ComponentModel.DataAnnotations;

namespace Altinn.Authorization.ServiceDefaults;

/// <summary>
/// Represents configuration options for telemetry collection in Altinn applications.
/// </summary>
public sealed class AltinnTelemetryOptions
{
    /// <summary>
    /// Gets or sets the sampling options.
    /// </summary>
    [ValidateObjectMembers]
    public SamplingOptions Sampling { get; set; } = new();

    /// <summary>
    /// Represents configuration options for controlling sampling behavior in telemetry collection.
    /// </summary>
    public sealed class SamplingOptions 
    {
        /// <summary>
        /// Gets or sets the configuration options for the root sampler, which determines the sampling behavior for telemetry items that do not have a parent context.
        /// </summary>
        [ValidateObjectMembers]
        public SamplerOptions Root { get; set; } = new() { SamplingRatio = 1.0 };

        /// <summary>
        /// Gets or sets the configuration options for the remote-parent-sampled sampler, which determines the sampling behavior for telemetry items that have a remote parent context that was sampled.
        /// </summary>
        [ValidateObjectMembers]
        public SamplerOptions RemoteParentSampled { get; set; } = new() { SamplingRatio = 1.0 };

        /// <summary>
        /// Gets or sets the configuration options for the remote-parent-not-sampled sampler, which determines the sampling behavior for telemetry items that have a remote parent context that was not sampled.
        /// </summary>
        [ValidateObjectMembers]
        public SamplerOptions RemoteParentNotSampled { get; set; } = new() { SamplingRatio = 0.0 };

        /// <summary>
        /// Gets or sets the configuration options for the local-parent-sampled sampler, which determines the sampling behavior for telemetry items that have a local parent context that was sampled.
        /// </summary>
        [ValidateObjectMembers]
        public SamplerOptions LocalParentSampled { get; set; } = new() { SamplingRatio = 1.0 };

        /// <summary>
        /// Gets or sets the configuration options for the local-parent-not-sampled sampler, which determines the sampling behavior for telemetry items that have a local parent context that was not sampled.
        /// </summary>
        [ValidateObjectMembers]
        public SamplerOptions LocalParentNotSampled { get; set; } = new() { SamplingRatio = 0.0 };

        internal Sampler ToSampler()
            => new ParentBasedSampler(
                rootSampler: Root.ToSampler(),
                remoteParentSampled: RemoteParentSampled.ToSampler(),
                remoteParentNotSampled: RemoteParentNotSampled.ToSampler(),
                localParentSampled: LocalParentSampled.ToSampler(),
                localParentNotSampled: LocalParentNotSampled.ToSampler());
    }

    /// <summary>
    /// Represents configuration options for controlling a specific sampler's behavior.
    /// </summary>
    public sealed class SamplerOptions
    {
        /// <summary>
        /// Gets or sets the proportion of items to include during sampling.
        /// </summary>
        /// <remarks>The value must be between 0.0 and 1.0, inclusive. A value of 0.0 means no items are sampled;
        /// a value of 1.0 means all items are included.</remarks>
        [Range(0.0, 1.0)]
        public double SamplingRatio { get; set; }

        internal Sampler ToSampler()
            => SamplingRatio switch
            {
                0.0 => new AlwaysOffSampler(),
                1.0 => new AlwaysOnSampler(),
                var v when v > 0.0 && v < 1.0 => new TraceIdRatioBasedSampler(v),
                _ => ThrowHelper.ThrowArgumentOutOfRangeException<Sampler>(nameof(SamplingRatio), $"{nameof(SamplingRatio)} must be between 0 and 1 (inclusive)"),
            };
    }
}
