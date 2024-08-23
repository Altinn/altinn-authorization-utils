using System.Diagnostics;

namespace Altinn.Authorization.ServiceDefaults.Telemetry;

/// <summary>
/// Extension methods for <see cref="Activity"/> and <see cref="ActivitySource"/>.
/// </summary>
public static class AltinnActivityExtensions
{
    /// <summary>
    /// Creates and starts a new <see cref="Activity"/> object if there is any listener to the Activity, returns null otherwise.
    /// </summary>
    /// <param name="source">The <see cref="ActivitySource"/>.</param>
    /// <param name="kind">The <see cref="ActivityKind"/>.</param>
    /// <param name="name">The operation name of the Activity.</param>
    /// <param name="tags">The operation tags.</param>
    /// <returns>The created <see cref="Activity"/> object or null if there is no any event listener.</returns>
    public static Activity? StartActivity(this ActivitySource source, ActivityKind kind, string name, ReadOnlySpan<KeyValuePair<string, object?>> tags)
    {
        using var state = ActivityHelper.ThreadLocalState;
        state.AddTags(tags);

        return source.StartActivity(kind, tags: state.Tags, name: name);
    }
}
