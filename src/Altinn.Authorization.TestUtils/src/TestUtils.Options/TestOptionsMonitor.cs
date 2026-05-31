using System.Collections;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.Diagnostics;
using Microsoft.Extensions.Options;

namespace Altinn.Authorization.TestUtils.Options.TestOptionsMonitor;

/// <summary>
/// An implementation of <see cref="IOptionsMonitor{T}"/> for testing purposes.
/// </summary>
/// <typeparam name="T">The type of options.</typeparam>
/// <remarks>
/// This class is thread-safe, but somewhat eventually consistent.
/// </remarks>
public class TestOptionsMonitor<T>
    : IOptionsMonitor<T>
    , IDictionary<string, T>
{
    private event Action<T, string>? _onChange;

    private ImmutableDictionary<string, T> _options
        = ImmutableDictionary.Create<string, T>(StringComparer.Ordinal);

    /// <summary>
    /// Gets or sets the default options value.
    /// </summary>
    public T Value
    {
        get => this[Microsoft.Extensions.Options.Options.DefaultName];
        set => this[Microsoft.Extensions.Options.Options.DefaultName] = value;
    }

    /// <inheritdoc/>
    public T this[string key]
    {
        get => ImmutableInterlocked.GetOrAdd(ref _options, key, static (key, state) => state.Create(key), this);

        set
        {
            ImmutableInterlocked.Update(ref _options, static (options, state) => options.SetItem(state.key, state.value), (key, value));
            _onChange?.Invoke(value, key);
        }
    }

    /// <inheritdoc/>
    public ICollection<string> Keys
        => _options.Keys.ToList();

    /// <inheritdoc/>
    public ICollection<T> Values
        => _options.Values.ToList();

    /// <inheritdoc/>
    public int Count
        => _options.Count;

    /// <inheritdoc/>
    public bool IsReadOnly
        => false;

    /// <inheritdoc/>
    T IOptionsMonitor<T>.CurrentValue
        => ((IOptionsMonitor<T>)this).Get(Microsoft.Extensions.Options.Options.DefaultName);

    /// <inheritdoc/>
    public void Add(string key, T value)
    {
        ImmutableInterlocked.Update(ref _options, static (options, state) => options.SetItem(state.key, state.value), (key, value));
        _onChange?.Invoke(value, key);
    }

    /// <inheritdoc/>
    public void Add(KeyValuePair<string, T> item)
        => Add(item.Key, item.Value);

    /// <inheritdoc/>
    public void Clear()
        => ImmutableInterlocked.Update(ref _options, static options => options.Clear());

    /// <inheritdoc/>
    public bool Contains(KeyValuePair<string, T> item)
        => _options.Contains(item);

    /// <inheritdoc/>
    public bool ContainsKey(string key)
        => _options.ContainsKey(key);

    /// <inheritdoc/>
    public void CopyTo(KeyValuePair<string, T>[] array, int arrayIndex)
        => ((IDictionary<string, T>)_options).CopyTo(array, arrayIndex);

    /// <inheritdoc/>
    public IEnumerator<KeyValuePair<string, T>> GetEnumerator()
        => _options.GetEnumerator();

    /// <inheritdoc/>
    public bool Remove(string key)
        => ImmutableInterlocked.Update(ref _options, static (options, key) => options.Remove(key), key);

    /// <inheritdoc/>
    public bool Remove(KeyValuePair<string, T> item)
    {
        return ImmutableInterlocked.Update(
            ref _options,
            static (options, item) =>
            {
                if (options.TryGetValue(item.Key, out var value) && EqualityComparer<T>.Default.Equals(value, item.Value))
                {
                    return options.Remove(item.Key);
                }

                return options;
            },
            item);
    }

    /// <inheritdoc/>
    public bool TryGetValue(string key, [MaybeNullWhen(false)] out T value)
        => _options.TryGetValue(key, out value);

    /// <summary>
    /// Creates an options instance for the given name. By default, this method throws a NotSupportedException, and should be overridden in derived classes to provide options creation logic.
    /// </summary>
    /// <param name="name">The name of the options instance to create.</param>
    /// <returns>The created options instance.</returns>
    protected virtual T Create(string name)
    {
        return ThrowHelper.ThrowNotSupportedException<T>("Creating options is not supported by default. Override Create to provide options creation logic.");
    }

    /// <inheritdoc/>
    T IOptionsMonitor<T>.Get(string? name)
        => this[name ?? Microsoft.Extensions.Options.Options.DefaultName];

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    /// <inheritdoc/>
    IDisposable? IOptionsMonitor<T>.OnChange(Action<T, string?> listener)
    {
        var disposable = new ChangeTrackerDisposable(this, listener);
        _onChange += disposable.OnChange;
        return disposable;
    }

    private sealed class ChangeTrackerDisposable
        : IDisposable
    {
        private readonly Action<T, string> _listener;
        private readonly TestOptionsMonitor<T> _monitor;

        public ChangeTrackerDisposable(TestOptionsMonitor<T> monitor, Action<T, string> listener)
        {
            _listener = listener;
            _monitor = monitor;
        }

        public void OnChange(T options, string name) => _listener.Invoke(options, name);

        public void Dispose() => _monitor._onChange -= OnChange;
    }
}
