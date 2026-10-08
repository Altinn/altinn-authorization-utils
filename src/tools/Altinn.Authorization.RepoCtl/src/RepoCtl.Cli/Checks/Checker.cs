using Altinn.Authorization.RepoCtl.Model;
using Altinn.Authorization.RepoCtl.Model.Checks;

namespace Altinn.Authorization.RepoCtl.Checks;

internal sealed class Checker
{
    public async Task Check(AltinnRepository repository, IReadOnlyList<IRepositoryCheck> checks, ICheckReporter? reporter, CancellationToken cancellationToken = default)
    {
        reporter ??= new NullReporter();

        await reporter.RunStarted(checks, cancellationToken);
        uint totalWarnings = 0;
        uint totalErrors = 0;

        foreach (var check in checks)
        {
            await reporter.CheckStarted(check, cancellationToken);

            uint warnings = 0;
            uint errors = 0;
            await foreach (var diag in check.Check(repository, cancellationToken))
            {
                await reporter.DiagnosticFound(check, diag, cancellationToken);

                if (diag.Severity is DiagnosticSeverity.Warning)
                {
                    warnings++;
                }
                else if (diag.Severity is DiagnosticSeverity.Error)
                {
                    errors++;
                }
            }

            await reporter.CheckCompleted(check, new(warnings, errors), cancellationToken);
            totalWarnings += warnings;
            totalErrors += errors;
        }

        await reporter.RunCompleted(checks, new(totalWarnings, totalErrors), cancellationToken);
    }

    private sealed class NullReporter : ICheckReporter;
}

internal readonly record struct CheckSummary(uint Warnings, uint Errors);
