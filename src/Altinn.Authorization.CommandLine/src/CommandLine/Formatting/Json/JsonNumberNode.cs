using System.Text.Json;
using Altinn.Authorization.CommandLine.Formatting.Pretty;
using CommunityToolkit.Diagnostics;

namespace Altinn.Authorization.CommandLine.Formatting.Json;

internal sealed record JsonNumberNode
    : JsonWithCommentsNode
{
    private readonly string _raw;

    internal JsonNumberNode(string raw)
    {
        Guard.IsNotNull(raw, nameof(raw));

        _raw = raw;
    }

    private protected override Notation ToNotationInner(in JsonRenderOptions renderOptions)
    {
        var encoded = renderOptions.Encoder.Encode(_raw);

        return Notation.Text(encoded, JsonStyles.Number);
    }

    internal protected override void Write(Utf8JsonWriter writer)
    {
        writer.WriteRawValue(_raw);
    }
}
