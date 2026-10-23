using System.Collections.Immutable;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Altinn.Authorization.RepoCtl.Containers;

internal sealed partial class BakeOutput
{
    [JsonPropertyName("group")]
    public IReadOnlyDictionary<string, Group> Groups { get; set; }
        = ImmutableDictionary<string, Group>.Empty;

    [JsonPropertyName("target")]
    public IReadOnlyDictionary<string, Target> Targets { get; set; }
        = ImmutableDictionary<string, Target>.Empty;

    public static BakeOutput Deserialize(string json)
        => JsonSerializer.Deserialize(json, BakeJsonContext.Default.BakeOutput)
            ?? throw new JsonException("Expected a bake output object.");

    [JsonSerializable(typeof(BakeOutput))]
    private sealed partial class BakeJsonContext
        : JsonSerializerContext;

    internal sealed class Group
    {
        [JsonPropertyName("targets")]
        public required IReadOnlyList<string> Targets { get; init; }
    }

    internal sealed class Target
    {
        [JsonPropertyName("tags")]
        public required IReadOnlyList<string> Tags { get; init; }
    }
}
