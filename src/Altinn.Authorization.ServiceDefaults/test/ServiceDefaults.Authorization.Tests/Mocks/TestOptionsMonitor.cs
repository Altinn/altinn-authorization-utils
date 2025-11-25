using Microsoft.Extensions.Options;

namespace Altinn.Authorization.ServiceDefaults.Authorization.Tests.Mocks;

internal sealed class TestOptionsMonitor<T>
    : IOptionsMonitor<T>
{
    public TestOptionsMonitor(T currentValue)
    {
        CurrentValue = currentValue;
    }

    public T CurrentValue { get; }

    public T Get(string? name)
    {
        throw new NotImplementedException();
    }

    public IDisposable? OnChange(Action<T, string?> listener)
    {
        throw new NotImplementedException();
    }
}
