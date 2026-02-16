using System.Collections;
using System.Collections.Immutable;
using System.Diagnostics;

namespace Altinn.Authorization.TestUtils;

/// <summary>
/// A collector for <see cref="Activity"/>, used to capture and inspect activities created during tests.
/// This class is thread-safe, and usable together with the in-memory exporters provided by OpenTelemetry for testing purposes.
/// </summary>
public sealed class ActivityCollector
    : ICollection<Activity>
{
    private ImmutableList<Activity> _activities = [];
    
    /// <inheritdoc/>
    public int Count 
        => Volatile.Read(ref _activities).Count;

    /// <inheritdoc/>
    bool ICollection<Activity>.IsReadOnly => false;

    /// <inheritdoc/>
    void ICollection<Activity>.Add(Activity item)
        => ImmutableInterlocked.Update(ref _activities, static (list, item) => list.Add(item), item);

    /// <inheritdoc/>
    void ICollection<Activity>.Clear()
        => Volatile.Write(ref _activities, []);

    /// <inheritdoc/>
    bool ICollection<Activity>.Contains(Activity item)
        => Volatile.Read(ref _activities).Contains(item);

    /// <inheritdoc/>
    void ICollection<Activity>.CopyTo(Activity[] array, int arrayIndex)
        => Volatile.Read(ref _activities).CopyTo(array, arrayIndex);

    /// <inheritdoc/>
    public IEnumerator<Activity> GetEnumerator()
        => Volatile.Read(ref _activities).GetEnumerator();

    /// <inheritdoc/>
    bool ICollection<Activity>.Remove(Activity item)
        => ImmutableInterlocked.Update(ref _activities, static (list, item) => list.Remove(item), item);

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator()
        => GetEnumerator();

    /// <summary>
    /// Gets a thread-safe, immutable snapshot of the current list of activities.
    /// </summary>
    /// <returns>An immutable list containing the activities present at the time the method is called.</returns>
    public ImmutableList<Activity> Snapshot()
        => Volatile.Read(ref _activities);
}
