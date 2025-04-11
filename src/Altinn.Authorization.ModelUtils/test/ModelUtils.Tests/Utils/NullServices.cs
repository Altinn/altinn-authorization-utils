namespace Altinn.Authorization.ModelUtils.Tests.Utils;

public sealed class NullServices
    : IServiceProvider
{
    public static NullServices Instance { get; } = new();
    
    private NullServices()
    {
    }

    public object? GetService(Type serviceType)
        => null;
}
