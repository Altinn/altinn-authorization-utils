using System.CommandLine;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Xml.XPath;
using Altinn.Authorization.CommandLine.Console;
using Altinn.Authorization.CommandLine.Factory;
using Altinn.Authorization.CommandLine.Results;
using Altinn.Authorization.CommandLine.XmlDoc;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;
using CommandConsole = Altinn.Authorization.CommandLine.Console.Console;

namespace Altinn.Authorization.CommandLine.Tests.Factory;

public class CommandHandlerDelegateFactoryTests
{
    [Fact]
    public void Create_WhenHandlerAlreadyIsCommandHandlerDelegate_ReturnsOriginalDelegate()
    {
        using var services = CreateServices();
        CommandHandlerDelegate handler = static (_, _) => Task.CompletedTask;

        var result = CommandHandlerDelegateFactory.Create(handler, services);

        result.Delegate.ShouldBeSameAs(handler);
        result.Options.ShouldBeEmpty();
        result.Arguments.ShouldBeEmpty();
    }

    [Fact]
    public async Task Create_BindsArgumentsAndOptionsFromParseResult()
    {
        using var services = CreateServices();

        string? recordedName = null;
        int? recordedCount = null;
        Delegate handler = (
            [Argument("name")] string name,
            [Option("--count", "-c")] int count) =>
        {
            recordedName = name;
            recordedCount = count;
        };

        var result = CommandHandlerDelegateFactory.Create(handler, services);
        var context = CreateContext(result, services, "alice", "--count", "3");

        await result.Delegate(context, TestContext.Current.CancellationToken);

        result.Arguments.ShouldHaveSingleItem().Name.ShouldBe("name");
        result.Options.ShouldHaveSingleItem().Name.ShouldBe("--count");
        result.Options[0].Aliases.ShouldContain("-c");
        recordedName.ShouldBe("alice");
        recordedCount.ShouldBe(3);
    }

    [Fact]
    public async Task Create_UsesParameterDefaultValuesWhenOptionalSymbolsAreOmitted()
    {
        using var services = CreateServices();

        string? recordedName = null;
        int? recordedCount = null;
        Delegate handler = (
            [Argument] string name = "default-name",
            [Option] int count = 42) =>
        {
            recordedName = name;
            recordedCount = count;
        };

        var result = CommandHandlerDelegateFactory.Create(handler, services);
        var context = CreateContext(result, services);

        await result.Delegate(context, TestContext.Current.CancellationToken);

        recordedName.ShouldBe("default-name");
        recordedCount.ShouldBe(42);
    }

    [Fact]
    public async Task Create_BindsKnownContextParametersFromInvocation()
    {
        using var services = CreateServices();

        CommandInvocationContext? recordedContext = null;
        CancellationToken recordedCancellationToken = default;
        IConsole? recordedConsole = null;
        ParseResult? recordedParseResult = null;
        IServiceProvider? recordedServiceProvider = null;
        Delegate handler = (
            CommandInvocationContext context,
            CancellationToken cancellationToken,
            IConsole console,
            ParseResult parseResult,
            IServiceProvider serviceProvider) =>
        {
            recordedContext = context;
            recordedCancellationToken = cancellationToken;
            recordedConsole = console;
            recordedParseResult = parseResult;
            recordedServiceProvider = serviceProvider;
        };

        var result = CommandHandlerDelegateFactory.Create(handler, services);
        var context = CreateContext(result, services);
        using var cts = new CancellationTokenSource();

        await result.Delegate(context, cts.Token);

        recordedContext.ShouldBeSameAs(context);
        recordedCancellationToken.ShouldBe(cts.Token);
        recordedConsole.ShouldBeSameAs(context.Console);
        recordedParseResult.ShouldBeSameAs(context.ParseResult);
        recordedServiceProvider.ShouldBeSameAs(services);
    }

    [Fact]
    public async Task Create_BindsImplicitExplicitAndOptionalServices()
    {
        using var services = CreateServices(s =>
        {
            s.AddSingleton(new TestService("implicit"));
            s.AddKeyedSingleton("keyed", new TestService("keyed"));
        });

        TestService? recordedImplicitService = null;
        TestService? recordedKeyedService = null;
        OptionalService? recordedOptionalService = new();
        Delegate handler = (
            TestService implicitService,
            [FromTestService("keyed")] TestService keyedService,
            [FromTestService("missing")] OptionalService? optionalService) =>
        {
            recordedImplicitService = implicitService;
            recordedKeyedService = keyedService;
            recordedOptionalService = optionalService;
        };

        var result = CommandHandlerDelegateFactory.Create(handler, services);
        var context = CreateContext(result, services);

        await result.Delegate(context, TestContext.Current.CancellationToken);

        recordedImplicitService.ShouldNotBeNull();
        recordedImplicitService.Name.ShouldBe("implicit");
        recordedKeyedService.ShouldNotBeNull();
        recordedKeyedService.Name.ShouldBe("keyed");
        recordedOptionalService.ShouldBeNull();
    }

    [Fact]
    public async Task Create_HandlesSupportedReturnTypes()
    {
        var sink = new ResultSink();
        using var services = CreateServices(s =>
        {
            s.AddSingleton(sink);
            s.AddSingleton<ICommandResultHandlerResolver, IntResultHandler>();
            s.AddSingleton<ICommandResultHandlerResolver, CaptureResultHandler<string>>();
        });

        await Invoke((Action)sink.Increment, services);
        await Invoke((Func<Task>)sink.IncrementAsync, services);
        await Invoke((Func<ValueTask>)sink.IncrementValueTaskAsync, services);
        await Invoke((Func<int>)ReturnCodeHandler.Return, services);
        await Invoke((Func<Task<string>>)ReturnCodeHandler.ReturnStringAsync, services);
        await Invoke((Func<ValueTask<string>>)ReturnCodeHandler.ReturnStringValueTaskAsync, services);
        await Invoke((Func<TestCommandResult>)ReturnCodeHandler.ReturnCommandResult, services);

        sink.Count.ShouldBe(3);
        sink.StringResults.ShouldBe(["task", "value-task"]);

        static async Task Invoke(Delegate handler, IServiceProvider services)
        {
            var result = CommandHandlerDelegateFactory.Create(handler, services);
            var context = CreateContext(result, services);

            await result.Delegate(context, TestContext.Current.CancellationToken);
        }
    }

    [Fact]
    public void Create_WhenReturnTypeHasNoHandler_Throws()
    {
        using var services = CreateServices();
        Delegate handler = () => new UnsupportedResult();

        var exception = Should.Throw<InvalidOperationException>(
            () => CommandHandlerDelegateFactory.Create(handler, services));

        exception.Message.ShouldContain("The return type 'UnsupportedResult' is not supported.");
    }

    [Fact]
    public async Task CommandResultHandler_ForMatchingType_ResolvesSelfAndHandlesTypedValue()
    {
        var sink = new ResultSink();
        var resolver = new CaptureResultHandler<string>(sink);

        var resolved = ((ICommandResultHandlerResolver)resolver).TryResolve(typeof(string), out var handler);

        resolved.ShouldBeTrue();
        handler.ShouldBeSameAs(resolver);

        using var services = CreateServices();
        await handler!.HandleResult("value", CreateContext(new(static (_, _) => Task.CompletedTask, [], []), services), TestContext.Current.CancellationToken);

        sink.StringResults.ShouldBe(["value"]);
        ((ICommandResultHandlerResolver)resolver).TryResolve(typeof(int), out _).ShouldBeFalse();
    }

    [Fact]
    public async Task Create_ForHandlerType_ConstructsInvokesAndDisposesHandler()
    {
        var sink = new HandlerTypeSink();
        using var services = CreateServices(s => s.AddSingleton(sink));
        var method = typeof(DisposableHandler).GetMethod(nameof(DisposableHandler.Invoke))!;
        var result = CommandHandlerDelegateFactory.Create(typeof(DisposableHandler), method, services);
        var context = CreateContext(result, services, "value");

        await result.Delegate(context, TestContext.Current.CancellationToken);

        sink.Value.ShouldBe("value");
        sink.DisposeCount.ShouldBe(1);
    }

    [Fact]
    public async Task Create_ForAsyncDisposableHandlerType_DisposesHandler()
    {
        var sink = new HandlerTypeSink();
        using var services = CreateServices(s => s.AddSingleton(sink));
        var method = typeof(AsyncDisposableHandler).GetMethod(nameof(AsyncDisposableHandler.InvokeAsync))!;
        var result = CommandHandlerDelegateFactory.Create(typeof(AsyncDisposableHandler), method, services);
        var context = CreateContext(result, services, "value");

        await result.Delegate(context, TestContext.Current.CancellationToken);

        sink.Value.ShouldBe("value");
        sink.DisposeCount.ShouldBe(1);
    }

    [Fact]
    public void Create_WhenParameterIsUnsupported_Throws()
    {
        using var services = CreateServices();
        Delegate handler = (UnsupportedParameter value) => { };

        var exception = Should.Throw<NotSupportedException>(
            () => CommandHandlerDelegateFactory.Create(handler, services));

        exception.Message.ShouldContain("is not supported");
    }

    [Theory]
    [InlineData(nameof(ByRefHandler.HandleRef), "ref")]
    [InlineData(nameof(ByRefHandler.HandleOut), "out")]
    [InlineData(nameof(ByRefHandler.HandleIn), "in")]
    public void Create_WhenParameterIsByReference_Throws(string methodName, string modifier)
    {
        using var services = CreateServices();
        var method = typeof(ByRefHandler).GetMethod(methodName)!;

        var exception = Should.Throw<NotSupportedException>(
            () => CommandHandlerDelegateFactory.Create(typeof(ByRefHandler), method, services));

        exception.Message.ShouldContain($"The by reference parameter '{modifier}");
    }

    [Fact]
    public void Create_WhenArgumentNamesAreDuplicated_Throws()
    {
        using var services = CreateServices();
        Delegate handler = (
            [Argument("value")] string first,
            [Argument("value")] string second) =>
        { };

        var exception = Should.Throw<InvalidOperationException>(
            () => CommandHandlerDelegateFactory.Create(handler, services));

        exception.Message.ShouldContain("An argument with the name 'value' has already been added to the command.");
    }

    [Fact]
    public void Create_WhenOptionNamesAreDuplicated_Throws()
    {
        using var services = CreateServices();
        Delegate handler = (
            [Option("--value")] string first,
            [Option("--value")] string second) =>
        { };

        var exception = Should.Throw<InvalidOperationException>(
            () => CommandHandlerDelegateFactory.Create(handler, services));

        exception.Message.ShouldContain("An option with the name '--value' has already been added to the command.");
    }

    [Fact]
    public async Task Invoke_WhenRequiredReferenceParameterBindsNull_Throws()
    {
        using var services = CreateServices();
        Delegate handler = ([NullArgument] string value) => { };
        var result = CommandHandlerDelegateFactory.Create(handler, services);
        var context = CreateContext(result, services);

        var exception = await Should.ThrowAsync<InvalidOperationException>(
            () => result.Delegate(context, TestContext.Current.CancellationToken));

        exception.Message.ShouldContain("Required parameter 'string value' was not provided from argument.");
    }

    private static ServiceProvider CreateServices(Action<IServiceCollection>? configure = null)
    {
        var services = new ServiceCollection();
        services.AddSingleton<OptionFactory, DefaultOptionFactory>();
        services.AddSingleton<ArgumentFactory, DefaultArgumentFactory>();
        services.AddSingleton<IXmlDocProvider, NullXmlDocProvider>();
        services.AddSingleton<IExclusivityMode, SharedExclusivityMode>();
        services.AddSingleton<IConsole, CommandConsole>();
        services.AddSingleton<CommandResultHandler>();
        configure?.Invoke(services);

        return services.BuildServiceProvider(validateScopes: true);
    }

    private static CommandInvocationContext CreateContext(
        CommandHandlerDelegateResult result,
        IServiceProvider services,
        params string[] args)
    {
        var command = new RootCommand("test");

        foreach (var argument in result.Arguments)
        {
            command.Arguments.Add(argument);
        }

        foreach (var option in result.Options)
        {
            command.Options.Add(option);
        }

        return new CommandInvocationContext(
            command.Parse(args),
            services,
            services.GetRequiredService<IConsole>());
    }

    private sealed class FromTestServiceAttribute(string? serviceKey = null)
        : Attribute
        , IFromServiceMetadata
    {
        public object? ServiceKey { get; } = serviceKey;
    }

    private sealed record TestService(string Name);

    private sealed class OptionalService;

    private sealed class ResultSink
    {
        public int Count { get; private set; }

        public List<string> StringResults { get; } = [];

        public void Increment()
            => Count++;

        public Task IncrementAsync()
        {
            Count++;
            return Task.CompletedTask;
        }

        public ValueTask IncrementValueTaskAsync()
        {
            Count++;
            return ValueTask.CompletedTask;
        }
    }

    private sealed class CaptureResultHandler<T>(ResultSink sink)
        : CommandResultHandler<T>
        where T : notnull
    {
        protected override Task HandleResult(T result, CommandInvocationContext context, CancellationToken cancellationToken = default)
        {
            sink.StringResults.Add(result?.ToString() ?? string.Empty);
            return Task.CompletedTask;
        }
    }

    private sealed class TestCommandResult
        : ICommandResult
    {
        public Task Execute(CommandInvocationContext context, CancellationToken cancellationToken = default)
        {
            context.ReturnCode = 123;
            return Task.CompletedTask;
        }
    }

    private static class ReturnCodeHandler
    {
        public static int Return()
            => 7;

        public static Task<string> ReturnStringAsync()
            => Task.FromResult("task");

        public static ValueTask<string> ReturnStringValueTaskAsync()
            => ValueTask.FromResult("value-task");

        public static TestCommandResult ReturnCommandResult()
            => new();

    }

    private sealed class UnsupportedResult;

    private sealed class HandlerTypeSink
    {
        public string? Value { get; set; }

        public int DisposeCount { get; set; }
    }

    private sealed class DisposableHandler(HandlerTypeSink sink)
        : IDisposable
    {
        public void Invoke([Argument] string value)
            => sink.Value = value;

        public void Dispose()
            => sink.DisposeCount++;
    }

    private sealed class AsyncDisposableHandler(HandlerTypeSink sink)
        : IAsyncDisposable
    {
        public Task InvokeAsync([Argument] string value)
        {
            sink.Value = value;
            return Task.CompletedTask;
        }

        public ValueTask DisposeAsync()
        {
            sink.DisposeCount++;
            return ValueTask.CompletedTask;
        }
    }

    private sealed class UnsupportedParameter;

    private sealed class ByRefHandler
    {
        public void HandleRef(ref int value)
        {
        }

        public void HandleOut(out int value)
            => value = 0;

        public void HandleIn(in int value)
        {
        }
    }

    private sealed class NullArgumentAttribute
        : Attribute
        , IFromArgumentMetadata
    {
        public Argument CreateArgument(
            IServiceProvider serviceProvider,
            Type parameterType,
            string parameterName,
            StrongBox<object?>? defaultValueBox)
        {
            return new Argument<string>(parameterName)
            {
                DefaultValueFactory = _ => null!,
            };
        }
    }

    private sealed class NullXmlDocProvider
        : IXmlDocProvider
    {
        public bool TryGetXmlDoc(Type type, [NotNullWhen(true)] out XPathNavigator? node)
            => NotFound(out node);

        public bool TryGetXmlDoc(MemberInfo member, [NotNullWhen(true)] out XPathNavigator? node)
            => NotFound(out node);

        public bool TryGetXmlDoc(MethodInfo method, [NotNullWhen(true)] out XPathNavigator? node)
            => NotFound(out node);

        public bool TryGetXmlDoc(FieldInfo field, [NotNullWhen(true)] out XPathNavigator? node)
            => NotFound(out node);

        public bool TryGetXmlDoc(PropertyInfo property, [NotNullWhen(true)] out XPathNavigator? node)
            => NotFound(out node);

        public bool TryGetXmlDoc(ParameterInfo parameter, [NotNullWhen(true)] out XPathNavigator? node)
            => NotFound(out node);

        private static bool NotFound([NotNullWhen(true)] out XPathNavigator? node)
        {
            node = null;
            return false;
        }
    }
}
