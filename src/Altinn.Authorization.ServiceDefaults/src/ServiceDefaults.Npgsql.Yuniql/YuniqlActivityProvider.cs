using System.Diagnostics;

namespace Altinn.Authorization.ServiceDefaults.Npgsql.Yuniql;

internal static class YuniqlActivityProvider
{
    /// <summary>
    /// Name of the activity source.
    /// </summary>
    internal static readonly string SourceName = "Altinn.Authorization.ServiceDefaults.Npgsql.Yuniql";
    private static readonly ActivitySource _activitySource = new(SourceName);

    public static Activity? StartActivity(ActivityKind kind, string activityName, ReadOnlySpan<KeyValuePair<string, object?>> tags)
    {
        using var state = ActivityHelper.ThreadLocalState;
        state.AddTags(tags);

        return _activitySource.StartActivity(kind, tags: state.Tags, name: activityName);
    }
}
