using Spectre.Console;
using Spectre.Console.Rendering;

namespace Altinn.Authorization.CommandLine.Help;

/// <summary>
/// Represents a section of help text with a title and content.
/// </summary>
public sealed class Section
    : IRenderable
{
    private static Style TitleStyle { get; } = new(foreground: Color.Yellow, decoration: Decoration.Bold);

    /// <summary>
    /// Creates an optional section with a title and content. If the content is null or whitespace, the section will not be created and null will be returned.
    /// </summary>
    /// <param name="title">The section title.</param>
    /// <param name="content">The section content.</param>
    /// <returns>An optional section.</returns>
    public static Section? Optional(string title, string? content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return null;
        }

        return Optional(title, new Text(content));
    }

    /// <summary>
    /// Creates an optional section with a title and content. If the content is null, the section will not be created and null will be returned.
    /// </summary>
    /// <param name="title">The section title.</param>
    /// <param name="content">The section content.</param>
    /// <returns>An optional section.</returns>
    public static Section? Optional(string title, IRenderable? content)
    {
        if (content is null)
        {
            return null;
        }

        var inner = new Rows([
            new Text($"{title}:", TitleStyle),
            new Padder(content, new Padding(left: 4, top: 0, right: 0, bottom: 0)),
        ]);

        return new Section(title, inner);
    }

    private readonly string _title;
    private readonly IRenderable _inner;

    private Section(string title, IRenderable inner)
    {
        _title = title;
        _inner = inner;
    }

    /// <summary>
    /// Gets the title of the section.
    /// </summary>
    public string Title => _title;

    /// <inheritdoc />
    Measurement IRenderable.Measure(RenderOptions options, int maxWidth)
        => _inner.Measure(options, maxWidth);

    /// <inheritdoc />
    IEnumerable<Segment> IRenderable.Render(RenderOptions options, int maxWidth)
        => _inner.Render(options, maxWidth);
}
