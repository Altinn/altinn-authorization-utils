using System.CommandLine;
using Altinn.Authorization.CommandLine.Console;
using Altinn.Authorization.CommandLine.Extensions;

namespace Altinn.Authorization.CommandLine;

/// <summary>
/// Represents the context of a command invocation, including the parse result, service provider, console, and cancellation token.
/// </summary>
public sealed class CommandInvocationContext
    : CommandContext
{
    internal CommandInvocationContext(
        ParseResult parseResult,
        IServiceProvider applicationServices,
        IConsole console)
        : base(parseResult.CommandResult.Command)
    {
        Extensions = new ContextExtensions();
        ParseResult = parseResult;
        ApplicationServices = applicationServices;
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
    public IServiceProvider ApplicationServices { get; }

    /// <summary>
    /// Gets the console for the command invocation.
    /// </summary>
    public IConsole Console { get; }

    /// <summary>
    /// Gets the extensions for the command invocation.
    /// </summary>
    public IContextExtensions Extensions { get; }

    /// <summary>
    /// Gets or sets the return code for the command invocation.
    /// </summary>
    public int ReturnCode { get; set; }
}
