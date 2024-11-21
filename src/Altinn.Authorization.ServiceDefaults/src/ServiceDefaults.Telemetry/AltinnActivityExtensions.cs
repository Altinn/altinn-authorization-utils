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
    public static Activity? StartActivity(
        this ActivitySource source,
        ActivityKind kind,
        string name,
        ReadOnlySpan<KeyValuePair<string, object?>> tags)
        => StartActivity(source, name, kind, parentContext: default, tags, links: default, startTime: default);

    /// <summary>
    /// Creates and starts a new <see cref="Activity"/> object if there is any listener to the Activity, returns null otherwise.
    /// </summary>
    /// <param name="name">The operation name of the Activity.</param>
    /// <param name="source">The <see cref="ActivitySource"/>.</param>
    /// <param name="kind">The <see cref="ActivityKind"/>.</param>
    /// <param name="parentContext">The parent <see cref="ActivityContext"/> object to initialize the created Activity object with.</param>
    /// <param name="tags">The optional tags list to initialize the created Activity object with.</param>
    /// <param name="links">The optional <see cref="ActivityLink"/> list to initialize the created Activity object with.</param>
    /// <param name="startTime">The optional start timestamp to set on the created Activity object.</param>
    /// <returns>The created <see cref="Activity"/> object or null if there is no any event listener.</returns>
    public static Activity? StartActivity(
        this ActivitySource source,
        string name,
        ActivityKind kind = ActivityKind.Internal,
        ActivityContext parentContext = default,
        ReadOnlySpan<KeyValuePair<string, object?>> tags = default,
        ReadOnlySpan<ActivityLink> links = default,
        DateTimeOffset startTime = default)
    {
        using var state = ActivityHelper.ThreadLocalState;
        state.AddTags(tags);
        state.AddLinks(links);

        return source.StartActivity(kind, parentContext: parentContext, tags: state.Tags, links: state.Links, name: name);
    }
}
