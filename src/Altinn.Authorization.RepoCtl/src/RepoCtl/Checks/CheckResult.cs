using System.Collections.Immutable;
using System.Text.Json.Serialization;

namespace Altinn.Authorization.RepoCtl.Checks;

internal sealed class CheckResult
{
    public static CheckResult Success(IRepositoryCheck check)
        => Create(check, []);

    public static CheckResult Create(IRepositoryCheck check, ImmutableArray<CheckIssue> issues)
        => new(id: check.CheckId, name: check.CheckDisplayName, issues);

    [JsonPropertyName("id")]
    public string Id { get; }

    [JsonPropertyName("name")]
    public string Name { get; }

    [JsonPropertyName("issues")]
    public ImmutableArray<CheckIssue> Issues { get; }

    [JsonPropertyName("success")]
    public bool IsSuccess => Issues.IsEmpty;

    private CheckResult(string id, string name, ImmutableArray<CheckIssue> issues)
    {
        Name = name;
        Id = id;

        Issues = issues;
    }
}
