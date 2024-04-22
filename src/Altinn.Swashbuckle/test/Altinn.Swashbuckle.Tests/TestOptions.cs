using Microsoft.Extensions.Options;
using System.Collections;

namespace Altinn.Swashbuckle.Tests;

internal static class TestOptions
{
    public static TestOptions<T> Create<T>(T value)
        => new(value);
}

internal class TestOptions<T>
    : IOptionsMonitor<T>
    , IEnumerable<KeyValuePair<string, T>>
{
    private readonly IDictionary<string, T> _options;

    public TestOptions(T value)
    {
        _options = new Dictionary<string, T> { { Options.DefaultName, value } };
    }

    public void Add(string name, T value)
    {
        _options.Add(name, value);
    }

    public T CurrentValue => Get(Options.DefaultName);

    public T Get(string? name)
        => _options[name ?? Options.DefaultName];

    public IDisposable? OnChange(Action<T, string?> listener)
        => null;

    IEnumerator<KeyValuePair<string, T>> IEnumerable<KeyValuePair<string, T>>.GetEnumerator()
        => _options.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => _options.GetEnumerator();
}
