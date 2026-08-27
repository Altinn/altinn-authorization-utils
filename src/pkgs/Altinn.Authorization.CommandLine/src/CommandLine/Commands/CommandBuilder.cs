using System.CommandLine;
using System.CommandLine.Parsing;
using CommunityToolkit.Diagnostics;

namespace Altinn.Authorization.CommandLine.Commands;

internal sealed class CommandBuilder
    : ICommandBuilder
{
    private readonly CliApplication _application;
    private readonly Command _command;
    private readonly CommandExtensions _extensions;
    private readonly ConventionsCollection _conventions;
    private readonly List<CommandHandlerMiddlewareDelegate> _middleware = new();

    internal CommandBuilder(
        CliApplication application,
        Command command,
        ConventionsCollection conventions)
    {
        Guard.IsNotNull(application);
        Guard.IsNotNull(command);
        Guard.IsNotNull(conventions);

        if (command.Action is not null)
        {
            ThrowHelper.ThrowInvalidOperationException("Command cannot have an action assigned.");
        }

        if (conventions.IsRoot != command is RootCommand)
        {
            ThrowHelper.ThrowInvalidOperationException("Conventions collection must match the command type (root or subcommand).");
        }

        _application = application;
        _command = command;
        _extensions = CommandExtensions.For(command);
        _conventions = conventions;

        application.Add(this);
    }

    public bool IsRoot
        => _command is RootCommand;

    public string Name
        => _command.Name;

    public ICollection<string> Aliases
        => _command.Aliases;

    public IList<Option> Options
        => _command.Options;

    public IList<Argument> Arguments
        => _command.Arguments;

    public IList<Action<CommandResult>> Validators
        => _command.Validators;

    public IList<CommandHandlerMiddlewareDelegate> Middleware
        => _middleware;

    public IList<object> Metadata
        => _extensions.Metadata;

    public CommandHandlerDelegate? CommandHandler { get; set; }

    public IServiceProvider ApplicationServices
        => _application.ApplicationServices;

    public ICommandConventionBuilder AddCommand(string name, string description, Action<ICommandBuilder> configure)
    {
        var command = new Command(name, description);
        var builder = new CommandBuilder(_application, command, _conventions.CreateChild());

        configure(builder);
        _command.Add(command);

        return builder._conventions;
    }

    internal void Build(CommandPipeline pipeline)
    {
        if (_command.Action is not null)
        {
            ThrowHelper.ThrowInvalidOperationException("Command cannot have an action assigned.");
        }

        foreach (var convention in _conventions)
        {
            convention(this);
        }

        if (CommandHandler is not null)
        {
            var handler = _middleware.AsEnumerable().Reverse().Aggregate(CommandHandler, ApplyMiddleware);

            _command.Action = pipeline.CreateAction(handler);
        }

        static CommandHandlerDelegate ApplyMiddleware(CommandHandlerDelegate handler, CommandHandlerMiddlewareDelegate middleware)
            => (context, cancellationToken) => middleware(context, handler, cancellationToken);
    }
}
