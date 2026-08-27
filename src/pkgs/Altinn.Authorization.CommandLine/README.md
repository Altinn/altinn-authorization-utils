# Altinn.Authorization.CommandLine

`Altinn.Authorization.CommandLine` is a small framework for building hosted command line applications. It combines
`System.CommandLine`, `Microsoft.Extensions.Hosting`, dependency injection, and Spectre.Console-backed output behind a
simple application builder.

## Installation

Install the package with the .NET CLI:

```bash
dotnet add package Altinn.Authorization.CommandLine
```

Or with the NuGet Package Manager Console:

```pwsh
Install-Package Altinn.Authorization.CommandLine
```

## Usage

Create a builder, register any application services, build the `CliApplication`, add commands, and run the application
with the process arguments.

```csharp
using Altinn.Authorization.CommandLine;
using Microsoft.Extensions.Logging;

var builder = CliApplication.CreateBuilder("Example command line application");
var cli = builder.Build();

cli.AddCommand("hello", "Writes a greeting", (
    [Argument] string name,
    ILogger<Program> logger) =>
{
    logger.LogInformation("Hello, {name}!", name);
});

return await cli.RunAsync(args);
```

Commands can also be implemented as handler types. The handler is constructed from the application service provider for
each invocation and disposed after it runs.

```csharp
using Altinn.Authorization.CommandLine;
using Altinn.Authorization.CommandLine.Results;
using Microsoft.Extensions.Logging;
using Spectre.Console;

var builder = CliApplication.CreateBuilder("Example command line application");
var cli = builder.Build();

cli.AddCommand<GreetCommand>("hello", "Writes a greeting");

return await cli.RunAsync(args);

internal sealed class GreetCommand(ILogger<GreetCommand> logger)
{
    public GreetingResult Invoke(
        [Option("--times", "-t")] int times = 5,
        [Argument] string message = "Hello, world!")
    {
        for (var i = 0; i < times; i++)
        {
            logger.LogInformation("Greeting: {message}", message);
        }

        return new GreetingResult(message);
    }
}

internal sealed class GreetingResult(string message)
    : ICommandResult
{
    public Task Execute(CommandInvocationContext context, CancellationToken cancellationToken = default)
    {
        context.Console.WriteLine(message);
        return Task.CompletedTask;
    }
}
```

## Parameter Binding

Handler parameters are bound by the command handler factory:

- `[Argument]` binds a parameter from a command line argument.
- `[Option]` binds a parameter from a command line option. The default option name is `--{parameterName}`.
- `CommandInvocationContext`, `CancellationToken`, `IConsole`, `ParseResult`, and `IServiceProvider` are injected from
  the current invocation.
- Service parameters are resolved from dependency injection when the service provider reports that the parameter type is
  registered.
- Custom parameter attributes can implement `IFromArgumentMetadata`, `IFromOptionMetadata`, or `IFromServiceMetadata` for
  specialized binding.

Parameters with default values are treated as optional. Nullable parameters are also optional according to the parameter
nullability metadata.

## Return Values

Handlers can return:

- `void`, `Task`, or `ValueTask`.
- `T`, `Task<T>`, or `ValueTask<T>` when an `ICommandResultHandlerResolver` can resolve a handler for `T`.
- `ICommandResult`, which is executed directly.

An `int` result handler is registered by default and maps the returned value to the command return code.

## Middleware

Commands expose a middleware pipeline through `ICommandBuilder.Middleware`.

```csharp
cli.Middleware.Add(async (context, next, cancellationToken) =>
{
    var started = DateTimeOffset.UtcNow;
    await next(context, cancellationToken);
    var elapsed = DateTimeOffset.UtcNow - started;
    context.Console.MarkupLineInterpolated($"Completed in {elapsed.TotalMilliseconds:N0} ms");
});
```

## Contributing

Contributions to `Altinn.Authorization.CommandLine` are welcome. Open an issue for bugs or feature requests, or submit a
pull request with the proposed changes.

## License

`Altinn.Authorization.CommandLine` is licensed under the [MIT License](../../LICENSE).
