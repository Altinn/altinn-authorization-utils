using System.Text.Json;
using Altinn.Authorization.CommandLine.Formatting.Pretty;
using CommunityToolkit.Diagnostics;

namespace Altinn.Authorization.CommandLine.Formatting.Json;

internal sealed record JsonPropertyNode
    : JsonWithCommentsNode
{
    private static readonly Notation ColonNotation = Notation.Text(":", JsonStyles.Colon);
    private static readonly Notation Space = Notation.Text(" ");

    private readonly string _name;
    private readonly JsonWithCommentsNode _value;

    internal JsonPropertyNode(string name, JsonWithCommentsNode value)
    {
        Guard.IsNotNull(name, nameof(name));
        Guard.IsNotNull(value, nameof(value));

        _name = name;
        _value = value;
    }

    private protected override Notation ToNotationInner(in JsonRenderOptions renderOptions)
    {
        var nameEncoded = renderOptions.Encoder.Encode(_name);
        var nameQuoted = $"\"{nameEncoded}\"";
        var nameNotation = Notation.Text(nameQuoted, JsonStyles.Member);

        var valueNotation = _value.ToNotation(in renderOptions);

        var inlineNotation = nameNotation + ColonNotation + Space + valueNotation;
        var multilineNotation = nameNotation + ColonNotation + Notation.Indent(renderOptions.IndentSize, Notation.Newline + valueNotation);

        return inlineNotation | multilineNotation;
    }

    internal protected override void Write(Utf8JsonWriter writer)
    {
        writer.WritePropertyName(_name);
        _value.Write(writer);
    }
}
