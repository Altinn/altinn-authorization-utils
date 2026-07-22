using System.CommandLine;
using Altinn.Authorization.CommandLine;
using Altinn.Authorization.CommandLine.Results;
using Microsoft.Extensions.Logging;
using Spectre.Console;

var builder = CliApplication.CreateBuilder("Altinn Authorization Repository Manager (repoctl)");
var cli = builder.Build();

cli.AddCommand("test", "Test command", (
    [Argument] string arg,
    ILogger<Program> logger) =>
{
    logger.LogInformation("Test command executed with argument: {arg}", arg);
});

cli.AddCommand<OtherCommand>("other", "The other command");

return await cli.RunAsync(args);

internal sealed class OtherCommand(ILogger<OtherCommand> logger)
{
    /// <summary>
    /// Executes the other command with the specified parameters.
    /// </summary>
    /// <param name="times">The number of times to print the message.</param>
    /// <param name="message">The message to log.</param>
    public TestCommandResult Invoke([Option("--times", "-t")] int times = 5, [Argument] string message = "Hello, world!")
    {
        for (int i = 0; i < times; i++)
        {
            logger.LogInformation("Other command executed: {message}", message);
        }

        return new TestCommandResult(message);
    }
}

internal sealed class TestCommandResult(string message)
    : ICommandResult
{
    public Task Execute(CommandInvocationContext context, CancellationToken cancellationToken = default)
    {
        context.Console.WriteLine(message);
        return Task.CompletedTask;
    }
}