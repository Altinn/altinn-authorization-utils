using System.Collections.Immutable;
using System.Text.Json;
using Altinn.Authorization.CommandLine.Formatting.Pretty;
using CommunityToolkit.Diagnostics;

namespace Altinn.Authorization.CommandLine.Formatting.Json;

internal sealed record JsonArrayNode
    : JsonContainerNode
{
    private static readonly Notation OpenNotation = Notation.Text("[", JsonStyles.Brackets);
    private static readonly Notation CloseNotation = Notation.Text("]", JsonStyles.Brackets);

    private readonly ImmutableArray<JsonWithCommentsNode> _children;

    private protected override Notation Open => OpenNotation;
    private protected override Notation Close => CloseNotation;

    internal JsonArrayNode(ImmutableArray<JsonWithCommentsNode> children)
    {
        Guard.IsNotNull(children, nameof(children));

        _children = children;
    }

    private protected override IReadOnlyList<Notation> GetChildren(in JsonRenderOptions renderOptions)
    {
        var children = new Notation[_children.Length];
        for (var i = 0; i < _children.Length; i++)
        {
            children[i] = _children[i].ToNotation(in renderOptions);
        }

        return children;
    }

    internal protected override void Write(Utf8JsonWriter writer)
    {
        writer.WriteStartArray();
        foreach (var child in _children)
        {
            child.Write(writer);
        }
        writer.WriteEndArray();
    }
}
