using System.Collections.Immutable;
using System.Text.Json.Serialization;
using Altinn.Authorization.ModelUtils;

namespace Altinn.Authorization.RepoCtl.Checks;

internal sealed record CheckResult
{
    private readonly ImmutableValueArray<Issue> _issues;

    public static Builder CreateBuilder(string name)
        => new(name);

    public string Name { get; }

    public ImmutableArray<Issue> Issues => _issues.ToImmutableArray();

    [JsonIgnore]
    public bool IsSuccess => Issues.IsEmpty;

    private CheckResult(string name, ImmutableValueArray<Issue> issues)
    {
        Name = name;
        _issues = issues;
    }

    internal sealed record Issue(string File, string Message);

    public sealed class Builder
    {
        private readonly string _name;
        private readonly ImmutableArray<Issue>.Builder _issues;

        internal Builder(string name)
        {
            _name = name;
            _issues = ImmutableArray.CreateBuilder<Issue>();
        }

        public Builder AddIssue(string file, string message)
        {
            _issues.Add(new Issue(file, message));
            return this;
        }

        public CheckResult Build()
            => new CheckResult(_name, _issues.DrainToImmutableValueArray());
    }
}
