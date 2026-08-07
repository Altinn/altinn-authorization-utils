using System.Text.Json;
using Altinn.Authorization.CommandLine.Formatting.Pretty;

namespace Altinn.Authorization.CommandLine.Formatting.Json;

internal sealed record JsonNullNode
    : JsonWithCommentsNode
{
    private static readonly Notation NullNotation = Notation.Text("null", JsonStyles.Null);

    public static readonly JsonNullNode Instance = new();

    private JsonNullNode()
    {
    }

    private protected override Notation ToNotationInner(in JsonRenderOptions renderOptions)
    {
        return NullNotation;
    }

    internal protected override void Write(Utf8JsonWriter writer)
    {
        writer.WriteNullValue();
    }
}
