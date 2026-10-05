using Altinn.Authorization.RepoCtl.Model;

namespace Altinn.Authorization.RepoCtl.Checks;

internal sealed class Checker
{
    public Task Check(AltinnRepository repository, IReadOnlyList<IRepositoryCheck> checks, CancellationToken cancellationToken = default)
        => Check(repository, checks, null, cancellationToken);

    public async Task Check(AltinnRepository repository, IReadOnlyList<IRepositoryCheck> checks, ICheckReporter? reporter, CancellationToken cancellationToken = default)
    {
        reporter ??= new NullReporter();

        await reporter.RunStarted(checks, cancellationToken);
        uint totalIssues = 0;

        foreach (var check in checks)
        {
            await reporter.CheckStarted(check, cancellationToken);

            uint issues = 0;
            await foreach (var issue in check.Check(repository, cancellationToken))
            {
                await reporter.IssueFound(check, issue, cancellationToken);
                issues++;
            }

            await reporter.CheckCompleted(check, issues, cancellationToken);
            totalIssues += issues;
        }

        await reporter.RunCompleted(checks, totalIssues, cancellationToken);
    }

    private sealed class NullReporter : ICheckReporter;
}
