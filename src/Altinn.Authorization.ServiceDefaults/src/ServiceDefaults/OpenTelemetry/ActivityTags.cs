using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Altinn.Authorization.ServiceDefaults.OpenTelemetry;

/// <summary>
/// Represents a read-only collection of tags associated with an activity, providing access to tag values by key.
/// </summary>
public readonly struct ActivityTags
{
    private readonly Activity _activity;

    internal ActivityTags(Activity activity)
    {
        _activity = activity;
    }

    /// <inheritdoc cref="IReadOnlyDictionary{TKey, TValue}.this[TKey]"/>
    public object? this[string key]
        => _activity.GetTagItem(key);

    /// <inheritdoc cref="IEnumerable{T}.GetEnumerator()"/>
    public IEnumerator<KeyValuePair<string, object?>> GetEnumerator()
        => _activity.TagObjects.GetEnumerator();

    /// <inheritdoc cref="IReadOnlyDictionary{TKey, TValue}.TryGetValue(TKey, out TValue)"/>
    public bool TryGetValue(string key, [MaybeNullWhen(false)] out object? value)
    {
        value = _activity.GetTagItem(key);
        return value is not null;
    }
}
