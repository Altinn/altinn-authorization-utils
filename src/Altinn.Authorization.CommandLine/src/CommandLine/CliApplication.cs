using System.CommandLine;
using System.CommandLine.Parsing;
using Altinn.Authorization.CommandLine.Commands;
using Altinn.Authorization.CommandLine.Console;
using Altinn.Authorization.CommandLine.Help;
using Altinn.Authorization.CommandLine.Logging;
using Altinn.Authorization.CommandLine.Utils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Spectre.Console;

namespace Altinn.Authorization.CommandLine;

/// <summary>
/// Represents a command host that can run commands in a console application.
/// </summary>
public sealed class CliApplication
    : IAsyncDisposable
    , ICommandBuilder
{
    /// <summary>
    /// Creates a new <see cref="CliApplicationBuilder"/> instance with the specified description.
    /// </summary>
    /// <param name="description">The description of the application.</param>
    /// <returns>A <see cref="CliApplicationBuilder"/> instance.</returns>
    public static CliApplicationBuilder CreateBuilder(string description)
    {
        return CliApplicationBuilder.Create(description);
    }

    private readonly IHost _host;
    private readonly RootCommand _rootCommand;
    private readonly CommandBuilder _rootBuilder;
    private readonly AtomicBool _built;
    private readonly ConventionsCollection _conventions;
    private readonly CommandPipeline _pipeline;
    private readonly List<CommandBuilder> _commandBuilders = new();
    private readonly IConsole _console;

    internal CliApplication(
        string description,
        IHost host,
        IConsole console)
    {
        _host = host;
        _console = console;

        _rootCommand = new()
        {
            Description = description,
        };

        _built = new AtomicBool(false);
        _conventions = new ConventionsCollection(_built);
        _pipeline = new CommandPipeline(host.Services.GetRequiredService<IServiceScopeFactory>());
        _rootBuilder = new CommandBuilder(this, _rootCommand, _conventions);

        AddDefaultConventions(_conventions, _host.Services);
    }

    private void AddDefaultConventions(ICommandConventionBuilder builder, IServiceProvider serviceProvider)
    {
        // logging
        serviceProvider.GetRequiredService<LogLevelService>().Configure(builder);
        serviceProvider.GetRequiredService<InvocationLogger>().Configure(builder);

        // help
        serviceProvider.GetRequiredService<ExtendedHelpAction>().Configure(builder);
    }

    internal void Add(CommandBuilder builder)
    {
        _commandBuilders.Add(builder);
    }

    /// <summary>
    /// Gets the console used by the command host.
    /// </summary>
    public IConsole Console
        => _console;

    /// <summary>
    /// Runs the command host with the specified arguments and cancellation token.
    /// </summary>
    /// <param name="args">The arguments for the command.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The result of the command execution.</returns>
    public async Task<int> RunAsync(IReadOnlyList<string> args, CancellationToken cancellationToken = default)
    {
        EnsureBuilt();

        try
        {
            var parseResult = _rootCommand.Parse(args, _host.Services.GetService<ParserConfiguration>());

            await _host.StartAsync(cancellationToken);
            var invocationConfiguration = _host.Services.GetRequiredService<IOptions<InvocationConfiguration>>().Value;

            var commandResult = await parseResult.InvokeAsync(invocationConfiguration, cancellationToken);
            await _host.StopAsync(cancellationToken);

            return commandResult;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _console.StdErr.WriteException(ex);
            return -1;
        }
    }

    private void EnsureBuilt()
    {
        if (_built.Exchange(true))
        {
            return;
        }

        foreach (var builder in _commandBuilders)
        {
            builder.Build(_pipeline);
        }
    }

    ValueTask IAsyncDisposable.DisposeAsync()
    {
        if (_host is IAsyncDisposable asyncDisposable)
        {
            return asyncDisposable.DisposeAsync();
        }

        if (_host is IDisposable disposable)
        {
            disposable.Dispose();
            return ValueTask.CompletedTask;
        }

        return ValueTask.CompletedTask;
    }

    #region ICommandBuilder Implementation

    /// <inheritdoc/>
    bool ICommandBuilder.IsRoot
        => _rootBuilder.IsRoot;

    /// <inheritdoc/>
    string ICommandBuilder.Name
        => _rootBuilder.Name;

    /// <inheritdoc/>
    ICollection<string> ICommandBuilder.Aliases
        => _rootBuilder.Aliases;

    /// <inheritdoc/>
    public IList<Option> Options
        => _rootBuilder.Options;

    /// <inheritdoc/>
    public IList<Argument> Arguments
        => _rootBuilder.Arguments;

    /// <inheritdoc/>
    public IList<Action<CommandResult>> Validators
        => _rootBuilder.Validators;

    /// <inheritdoc/>
    public IList<CommandHandlerMiddlewareDelegate> Middleware
        => _rootBuilder.Middleware;

    /// <inheritdoc/>
    CommandHandlerDelegate? ICommandBuilder.CommandHandler
    {
        get => _rootBuilder.CommandHandler;
        set => _rootBuilder.CommandHandler = value;
    }

    /// <inheritdoc/>
    public IServiceProvider ApplicationServices
        => _host.Services;

    /// <inheritdoc/>
    public ICommandConventionBuilder AddCommand(string name, string description, Action<ICommandBuilder> configure)
        => _rootBuilder.AddCommand(name, description, configure);

    #endregion
}
