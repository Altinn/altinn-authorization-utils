using System.CommandLine;
using Altinn.Authorization.CommandLine.AsyncState;
using Altinn.Authorization.CommandLine.Console;
using Microsoft.Extensions.AsyncState;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;

namespace Altinn.Authorization.CommandLine.Tests.AsyncState;

public class AsyncContextCommandInvocationContextTests
{
    private readonly ICommandInvocationContextAccessor _accessorMock;
    private readonly IAsyncState _asyncState;
    private readonly IAsyncContext<Thing> _context;

    public AsyncContextCommandInvocationContextTests()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton<IExclusivityMode, SharedExclusivityMode>();
        serviceCollection.AddSingleton<IConsole, Console.Console>();
        serviceCollection.AddSingleton<ICommandInvocationContextAccessor, CommandInvocationContextAccessor>();
        serviceCollection.AddAsyncState();
        serviceCollection.AddSingleton(typeof(IAsyncContext<>), typeof(AsyncContextCommandInvocationContext<>));

        var serviceProvider = serviceCollection.BuildServiceProvider();
        _accessorMock = serviceProvider.GetRequiredService<ICommandInvocationContextAccessor>();
        _accessorMock.CommandInvocationContext = new CommandInvocationContext(
            parseResult: new RootCommand().Parse(Array.Empty<string>()),
            applicationServices: serviceProvider,
            console: serviceProvider.GetRequiredService<IConsole>());

        _context = serviceProvider.GetRequiredService<IAsyncContext<Thing>>();
        _asyncState = serviceProvider.GetRequiredService<IAsyncState>();
        _asyncState.Reset();
    }

    [Fact]
    public void TryGetReturnsTrueWhenHttpContextPresent()
    {
        var value = new Thing();
        _context.Set(value);

        _context.TryGet(out Thing? stored).ShouldBeTrue();
        stored.ShouldBeSameAs(value);
    }

    [Fact]
    public void TryGetReturnsTrueWhenHttpContextPresentAndValueNotSet()
    {
        _context.TryGet(out Thing? stored).ShouldBeTrue();
        stored.ShouldBeNull();
    }

    [Fact]
    public void GetReturnsNullWhenHttpContextPresentAndValueNotSet()
    {
        _context.Get().ShouldBeNull();
    }

    [Fact]
    public void TryGetReturnsFalseWhenHttpContextNotPresent()
    {
        _accessorMock.CommandInvocationContext = null;

        _context.TryGet(out Thing? stored).ShouldBeFalse();
        stored.ShouldBeNull();
    }

    [Fact]
    public void SetThrowsWhenHttpContextNotPresent()
    {
        _accessorMock.CommandInvocationContext = null;
        var value = new Thing();

        Should.Throw<InvalidOperationException>(() => _context.Set(value));
    }

    [Fact]
    public void GetThrowsWhenHttpContextNotPresent()
    {
        _accessorMock.CommandInvocationContext = null;
        Should.Throw<InvalidOperationException>(() => _context.Get());
    }

    [Fact]
    public void TryGet_WhenAsyncStateIsUsed_ReturnsTrue()
    {
        _accessorMock.CommandInvocationContext = null;
        _asyncState.Initialize();

        var value = new Thing();
        _context.Set(value);

        _context.TryGet(out Thing? stored).ShouldBeTrue();
        stored.ShouldBeSameAs(value);
    }

    [Fact]
    public void TryGet_WhenAsyncStateIsUsedAndValueNotSet_ReturnsNull()
    {
        _accessorMock.CommandInvocationContext = null;
        _asyncState.Initialize();

        _context.Get().ShouldBeNull();
    }
}
