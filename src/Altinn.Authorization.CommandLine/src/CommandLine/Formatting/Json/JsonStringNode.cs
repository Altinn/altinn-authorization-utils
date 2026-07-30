using System.Text.Json;
using Altinn.Authorization.CommandLine.Formatting.Pretty;
using CommunityToolkit.Diagnostics;

namespace Altinn.Authorization.CommandLine.Formatting.Json;

internal sealed record JsonStringNode
    : JsonWithCommentsNode
{
    private readonly string _value;

    internal JsonStringNode(string value)
    {
        Guard.IsNotNull(value, nameof(value));

        _value = value;
    }

    private protected override Notation ToNotationInner(in JsonRenderOptions renderOptions)
    {
        var encoded = renderOptions.Encoder.Encode(_value);
        var quoted = $"\"{encoded}\"";

        return Notation.Text(quoted, JsonStyles.String);
    }

    internal protected override void Write(Utf8JsonWriter writer)
    {
        writer.WriteStringValue(_value);
    }
}
