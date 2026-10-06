using System.Collections.Immutable;
using System.Text.Json.Serialization;
using Altinn.Authorization.RepoCtl.Model.Checks;

namespace Altinn.Authorization.RepoCtl.Checks;

internal sealed class CheckResult
{
    public static CheckResult Create(IRepositoryCheck check, ImmutableArray<Diagnostic> issues)
        => new(id: check.CheckId, name: check.CheckDisplayName, issues);

    [JsonPropertyName("id")]
    public string Id { get; }

    [JsonPropertyName("name")]
    public string Name { get; }

    [JsonPropertyName("diagnostics")]
    public ImmutableArray<Diagnostic> Diagnostics { get; }

    [JsonPropertyName("success")]
    public bool IsSuccess => !Diagnostics.Any(static d => d.Severity == DiagnosticSeverity.Error);

    private CheckResult(string id, string name, ImmutableArray<Diagnostic> issues)
    {
        Name = name;
        Id = id;

        Diagnostics = issues;
    }
}
