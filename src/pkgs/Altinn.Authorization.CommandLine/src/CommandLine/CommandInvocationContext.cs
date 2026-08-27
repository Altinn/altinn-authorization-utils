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

    private CommandInvocationContext(CommandInvocationContext parent)
        : base(parent.Command)
    {
        Extensions = parent.Extensions;
        ParseResult = parent.ParseResult;
        ApplicationServices = parent.ApplicationServices;
        Console = parent.Console;
        ReturnCode = parent.ReturnCode;
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

    /// <summary>
    /// Creates a child context from the current context.
    /// The child context shares the same command, parse result, service provider, console, and extensions as the parent context,
    /// but has its own return code that can be set independently.
    /// </summary>
    /// <param name="returnCode">The initial return code of the child context.</param>
    /// <returns>The newly created child context.</returns>
    public CommandInvocationContext CreateChildContext(int returnCode = 0)
        => new(this)
        {
            ReturnCode = returnCode,
        };
}
