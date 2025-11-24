using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Metrics;

namespace Altinn.Authorization.TestUtils;

/// <summary>
/// A <see cref="IMeterFactory"/> for use in tests.
/// </summary>
public sealed class TestMeterFactory
    : IMeterFactory
{
    private readonly ConcurrentDictionary<(string Name, string? Version), Meter> _meters = new();

    /// <inheritdoc/>
    public Meter Create(MeterOptions options)
    {
        if (options.Scope is not null || options.Tags is not null)
        {
            throw new NotSupportedException("TestMeterFactory does not support MeterOptions with Scope or Tags.");
        }

        // Simulate DefaultMeterFactory behavior of returning the same meter instance for the same name/version.
        return _meters.GetOrAdd(
            (options.Name, options.Version),
            static (opts, self) => new Meter(opts.Name, opts.Version, tags: [], scope: self),
            this);
    }

    /// <summary>
    /// Returns a collection of all meters currently managed by this instance.
    /// </summary>
    public IEnumerable<Meter> GetMeters()
    {
        var copy = _meters.ToArray();
        return copy.Select(static v => v.Value);
    }

    /// <summary>
    /// Returns a collection of meters that have the specified name.
    /// </summary>
    /// <param name="name">The name of the meters to retrieve. The comparison is case-sensitive.</param>
    /// <returns>An enumerable collection of <see cref="Meter"/> objects whose <see cref="Meter.Name"/> matches the specified
    /// <paramref name="name"/>. The collection is empty if no meters match.</returns>
    public IEnumerable<Meter> GetMeters(string name)
        => GetMeters().Where(m => m.Name == name);

    /// <summary>
    /// Attempts to retrieve a meter with the specified name and version.
    /// </summary>
    /// <param name="name">The name of the meter to retrieve. Cannot be null.</param>
    /// <param name="version">The version of the meter to retrieve, or null to match any version.</param>
    /// <param name="meter">When this method returns, contains the meter associated with the specified name and version, if found;
    /// otherwise, null. This parameter is passed uninitialized.</param>
    /// <returns>true if a meter with the specified name and version was found; otherwise, false.</returns>
    public bool TryGetMeter(string name, string? version, [MaybeNullWhen(false)] out Meter meter)
        => _meters.TryGetValue((name, version), out meter);

    /// <inheritdoc/>
    public void Dispose()
    {
        var copy = _meters.ToArray();
        _meters.Clear();

        foreach (var (_, meter) in copy)
        {
            meter.Dispose();
        }
    }
}
