using Spectre.Console;
using Spectre.Console.Rendering;

namespace Altinn.Authorization.CommandLine.Utils;

/// <summary>
/// "Renders" the text to the console verbatime, without any formatting or layout.
/// </summary>
/// <param name="text">The text to render.</param>
public sealed class VerbatimText(string text)
    : IRenderable
{
    /// <inheritdoc/>
    public Measurement Measure(RenderOptions options, int maxWidth)
        => new(0, 0);

    /// <inheritdoc/>
    public IEnumerable<Segment> Render(RenderOptions options, int maxWidth)
        => [new(text, Style.Plain)];
}
