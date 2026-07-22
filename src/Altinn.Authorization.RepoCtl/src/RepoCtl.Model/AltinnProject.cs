using System.Collections.Immutable;
using System.Text.Json.Serialization;
using Microsoft.CodeAnalysis;

namespace Altinn.Authorization.RepoCtl.Model;

public sealed class AltinnProject
{
    private readonly Project _project;
}

public sealed class AltinnVertical
{
    private readonly Solution _solution;
    private readonly AltinnVerticalId _id;

    public AltinnVerticalId Id => _id;

    public AltinnVerticalKind Kind => _id.Kind;
}

public sealed class AltinnRepository<TConfig>
    where TConfig : AltinnRepositoryConfiguration
{
    private readonly Solution _solution;
    private readonly TConfig _configuration;
}

public record AltinnRepositoryConfiguration
{
    [JsonPropertyName("deps")]
    internal ImmutableArray<AltinnVerticalId> Dependencies { get; init; } = [];
}
