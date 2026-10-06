using System.Collections.Immutable;
using System.Diagnostics;
using System.Text.Json.Serialization;
using Altinn.Authorization.RepoCtl.Model.Checks;

namespace Altinn.Authorization.RepoCtl.Checks;

internal sealed record CheckRunResult
{
    public static Collector CreateCollector(DirectoryInfo repositoryRoot)
        => new(repositoryRoot);

    [JsonPropertyName("success")]
    public bool IsSuccess
        => CheckResults.All(r => r.IsSuccess);

    [JsonPropertyName("result")]
    public string Result
        => IsSuccess ? "success" : "failure";

    [JsonPropertyName("checks")]
    public ImmutableArray<CheckResult> CheckResults { get; }

    [JsonIgnore]
    public DirectoryInfo RepositoryRoot { get; }

    private CheckRunResult(ImmutableArray<CheckResult> results, DirectoryInfo repositoryRoot)
    {
        CheckResults = results;
        RepositoryRoot = repositoryRoot;
    }

    public sealed class Collector(DirectoryInfo repositoryRoot)
        : ICheckReporter
    {
        private readonly DirectoryInfo _repositoryRoot = repositoryRoot;

        private readonly ImmutableArray<CheckResult>.Builder _results
            = ImmutableArray.CreateBuilder<CheckResult>();

        private IRepositoryCheck? _currentCheck;
        private ImmutableArray<Diagnostic>.Builder _currentDiag
            = ImmutableArray.CreateBuilder<Diagnostic>();

        public CheckRunResult Build()
            => new(_results.DrainToImmutable(), _repositoryRoot);

        ValueTask ICheckReporter.CheckStarted(IRepositoryCheck check, CancellationToken cancellationToken)
        {
            _currentCheck = check;

            Debug.Assert(_currentDiag.Count == 0);
            return ValueTask.CompletedTask;
        }

        ValueTask ICheckReporter.DiagnosticFound(IRepositoryCheck check, Diagnostic diag, CancellationToken cancellationToken)
        {
            Debug.Assert(ReferenceEquals(check, _currentCheck));
            _currentDiag.Add(diag);
            return ValueTask.CompletedTask;
        }

        ValueTask ICheckReporter.CheckCompleted(IRepositoryCheck check, CheckSummary summary, CancellationToken cancellationToken)
        {
            Debug.Assert(ReferenceEquals(check, _currentCheck));
            Debug.Assert(_currentDiag.Count == summary.Warnings + summary.Errors);

            var result = CheckResult.Create(check, _currentDiag.DrainToImmutable());
            _results.Add(result);

            _currentCheck = null;
            _currentDiag.Clear();
            return ValueTask.CompletedTask;
        }
    }
}
