using Spectre.Console.Rendering;

namespace Altinn.Authorization.CommandLine.Help;

/// <summary>
/// Builder for CLI help text.
/// </summary>
public interface IHelpBuilder
{
    /// <summary>
    /// Builds helptext.
    /// </summary>
    /// <param name="context">The help context.</param>
    /// <returns>An <see cref="IRenderable"/>.</returns>
    public IRenderable Build(HelpContext context);
}
