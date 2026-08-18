using CommunityToolkit.Diagnostics;
using Spectre.Console.Rendering;

namespace Altinn.Authorization.CommandLine.Results;

/// <summary>
/// Represents a command result that renders an <see cref="IRenderable"/> to the console.
/// </summary>
public class RenderResult
    : ICommandResult
{
    private readonly IRenderable _renderable;

    /// <summary>
    /// Gets the return code to be set in the <see cref="CommandInvocationContext"/> after rendering.
    /// </summary>
    public int ReturnCode { get; init; } = 0;

    /// <summary>
    /// Initializes a new instance of the <see cref="RenderResult"/> class with the specified <see cref="IRenderable"/>.
    /// </summary>
    /// <param name="renderable">The <see cref="IRenderable"/> to render to the console.</param>
    public RenderResult(IRenderable renderable)
    {
        Guard.IsNotNull(renderable);

        _renderable = renderable;
    }

    /// <inheritdoc/>
    public virtual Task Execute(CommandInvocationContext context, CancellationToken cancellationToken = default)
    {
        context.Console.Write(_renderable);
        context.ReturnCode = ReturnCode;
        return Task.CompletedTask;
    }
}
