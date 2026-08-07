using System.Text.Json;
using Altinn.Authorization.CommandLine.Formatting.Pretty;

namespace Altinn.Authorization.CommandLine.Formatting.Json;

internal sealed record JsonBoolNode
    : JsonWithCommentsNode
{
    public static readonly JsonBoolNode True = new(true);
    public static readonly JsonBoolNode False = new(false);

    private static readonly Notation TrueNotation = Notation.Text("true", JsonStyles.Boolean);
    private static readonly Notation FalseNotation = Notation.Text("false", JsonStyles.Boolean);

    private readonly bool _value;

    private JsonBoolNode(bool value)
    {
        _value = value;
    }

    private protected override Notation ToNotationInner(in JsonRenderOptions renderOptions)
    {
        return _value ? TrueNotation : FalseNotation;
    }

    internal protected override void Write(Utf8JsonWriter writer)
    {
        writer.WriteBooleanValue(_value);
    }
}
