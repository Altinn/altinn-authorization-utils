using Altinn.Authorization.CommandLine.Results;
using Spectre.Console;

namespace Altinn.Authorization.CommandLine.Shell;

internal sealed class CommandResult
    : ICommandResult
{
    private readonly Command _command;
    private readonly bool _echo;

    public CommandResult(Command command, bool echo)
    {
        _command = command;
        _echo = echo;
    }

    public async Task Execute(CommandInvocationContext context, CancellationToken cancellationToken = default)
    {
        if (_echo)
        {
            var paragraph = new Paragraph();
            paragraph.Append("Executing: ");
            paragraph.Append(_command.Filename, Color.Teal);

            foreach (var arg in _command.Arguments)
            {
                paragraph.Append(" ");
                paragraph.Append(arg, Color.Cyan);
            }

            paragraph.Append("\n");
            context.Console.StdErr.Write(paragraph);
        }

        try
        {
            await _command.Execute(cancellationToken);
        }
        catch (ProcessFailedException ex) when (ex.Process is { ExitCode: not 0 })
        {
            context.ReturnCode = ex.Process.ExitCode;
        }
    }
}
