using Spectre.Console.Rendering;

namespace Altinn.Authorization.CommandLine.Utils;

/// <summary>
/// Represents a renderable that computes its size just-in-time based on the maximum width provided during measurement.
/// </summary>
public abstract class JustInTimeSizeDependentRenderable
    : IRenderable
{
    private IRenderable? _rendered;
    private int _cachedMaxWidth;

    Measurement IRenderable.Measure(RenderOptions options, int maxWidth)
    {
        if (_rendered is null || maxWidth != _cachedMaxWidth)
        {
            _cachedMaxWidth = maxWidth;
            _rendered = Build(maxWidth);
        }

        return _rendered.Measure(options, maxWidth);
    }

    IEnumerable<Segment> IRenderable.Render(RenderOptions options, int maxWidth)
    {
        // TODO: Also check if the maxWidth is valid compared to the Measurement we gave out last time
        if (_rendered is null)
        {
            _cachedMaxWidth = maxWidth;
            _rendered = Build(maxWidth);
        }

        return _rendered.Render(options, maxWidth);
    }

    /// <summary>
    /// Builds the inner renderable based on the provided maximum width.
    /// </summary>
    /// <param name="maxWidth">The maximum width available for rendering.</param>
    /// <returns>The inner renderable.</returns>
    protected abstract IRenderable Build(int maxWidth);
}
