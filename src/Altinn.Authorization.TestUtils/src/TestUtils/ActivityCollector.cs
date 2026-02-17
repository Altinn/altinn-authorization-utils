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
    private string? _rootSourceName;
    private ImmutableList<Activity> _activities = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="ActivityCollector"/> class with an optional root source name filter.
    /// </summary>
    /// <param name="rootSourceName">The name of the root source from which to collect activities. This parameter can be null to indicate that no
    /// specific root source is targeted.</param>
    public ActivityCollector(string? rootSourceName)
    {
        _rootSourceName = rootSourceName;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ActivityCollector"/> class.
    /// </summary>
    public ActivityCollector()
        : this(rootSourceName: null)
    {
    }

    /// <inheritdoc/>
    public int Count 
        => Volatile.Read(ref _activities).Count;

    /// <inheritdoc/>
    bool ICollection<Activity>.IsReadOnly => false;

    /// <inheritdoc/>
    void ICollection<Activity>.Add(Activity item)
    {
        if (_rootSourceName is { } name && !IsActivityFromRootSource(item, name))
        {
            return;
        }

        ImmutableInterlocked.Update(ref _activities, static (list, item) => list.Add(item), item);

        static bool IsActivityFromRootSource(Activity activity, string rootSourceName)
        {
            if (activity.Source.Name == rootSourceName)
            {
                // we do not keep the actual root activity
                return false;
            }

            var current = activity;

            while (current is not null)
            {
                if (current.Source.Name == rootSourceName)
                {
                    return true;
                }

                current = current.Parent;
            }

            return false;
        }
    }

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
