namespace Altinn.Authorization.ServiceDefaults.OpenTelemetry;

/// <summary>
/// Specifies the possible actions for handling an event in a logging or telemetry system.
/// </summary>
/// <remarks>Use this enumeration to indicate whether an event should be recorded, dropped, or if the decision is
/// still pending. This is typically used in event processing pipelines to control the flow of data based on sampling or
/// filtering criteria.</remarks>
public enum RecordDecision
    : byte
{
    /// <summary>
    /// The decision is not influenced by this sampler, and should be determined by other samplers or the default behavior.
    /// This is the default value, and indicates that no explicit decision has been made by this sampler.
    /// </summary>
    Undecided = default,

    /// <summary>
    /// The decision is to record the span or event, meaning that it should be included in the telemetry data.
    /// </summary>
    Record,

    /// <summary>
    /// The decision is to drop the span or event, meaning that it should not be included in the telemetry data.
    /// Note that this is a final decision, and no other samplers will be consulted after this decision is made.
    /// </summary>
    Drop,
}
