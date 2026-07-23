using System.CommandLine;
using Altinn.Authorization.CommandLine.Console;

namespace Altinn.Authorization.CommandLine;

/// <summary>
/// Represents the context of a command invocation, including the parse result, service provider, console, and cancellation token.
/// </summary>
public sealed class CommandInvocationContext
{
    internal CommandInvocationContext(
        ParseResult parseResult,
        IServiceProvider serviceProvider,
        IConsole console)
    {
        ParseResult = parseResult;
        ServiceProvider = serviceProvider;
        Console = console;
        ReturnCode = 0;
    }

    /// <summary>
    /// Gets the parse result for the command invocation.
    /// </summary>
    public ParseResult ParseResult { get; }

    /// <summary>
    /// Gets the service provider for the command invocation.
    /// </summary>
    public IServiceProvider ServiceProvider { get; }

    /// <summary>
    /// Gets the console for the command invocation.
    /// </summary>
    public IConsole Console { get; }

    /// <summary>
    /// Gets or sets the return code for the command invocation.
    /// </summary>
    public int ReturnCode { get; set; }
}
