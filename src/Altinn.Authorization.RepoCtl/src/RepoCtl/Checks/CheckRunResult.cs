using System.Collections.Immutable;
using System.Diagnostics;
using System.Text.Json.Serialization;

namespace Altinn.Authorization.RepoCtl.Checks;

internal sealed record CheckRunResult
{
    public static Collector CreateCollector()
        => new();

    [JsonPropertyName("success")]
    public bool IsSuccess
        => CheckResults.All(r => r.IsSuccess);

    [JsonPropertyName("result")]
    public string Result
        => IsSuccess ? "success" : "failure";

    [JsonPropertyName("checks")]
    public ImmutableArray<CheckResult> CheckResults { get; }

    private CheckRunResult(ImmutableArray<CheckResult> results)
    {
        CheckResults = results;
    }

    public sealed class Collector
        : ICheckReporter
    {
        private readonly ImmutableArray<CheckResult>.Builder _results
            = ImmutableArray.CreateBuilder<CheckResult>();

        private IRepositoryCheck? _currentCheck;
        private ImmutableArray<CheckIssue>.Builder _currentIssues
            = ImmutableArray.CreateBuilder<CheckIssue>();

        public CheckRunResult Build()
            => new(_results.DrainToImmutable());

        ValueTask ICheckReporter.CheckStarted(IRepositoryCheck check, CancellationToken cancellationToken)
        {
            _currentCheck = check;

            Debug.Assert(_currentIssues.Count == 0);
            return ValueTask.CompletedTask;
        }

        ValueTask ICheckReporter.IssueFound(IRepositoryCheck check, CheckIssue issue, CancellationToken cancellationToken)
        {
            Debug.Assert(ReferenceEquals(check, _currentCheck));
            _currentIssues?.Add(issue);
            return ValueTask.CompletedTask;
        }

        ValueTask ICheckReporter.CheckCompleted(IRepositoryCheck check, uint issues, CancellationToken cancellationToken)
        {
            Debug.Assert(ReferenceEquals(check, _currentCheck));
            Debug.Assert(_currentIssues.Count == issues);

            var result = CheckResult.Create(check, _currentIssues.DrainToImmutable());
            _results.Add(result);

            _currentCheck = null;
            return ValueTask.CompletedTask;
        }
    }
}
