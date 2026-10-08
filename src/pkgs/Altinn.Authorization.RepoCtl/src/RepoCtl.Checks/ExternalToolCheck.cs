using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Altinn.Authorization.CommandLine.Shell;
using Altinn.Authorization.RepoCtl.Model;
using Altinn.Authorization.RepoCtl.Model.Checks;
using Altinn.Authorization.RepoCtl.MsBuild;

namespace Altinn.Authorization.RepoCtl.Checks;

/// <summary>
/// Represents a check that executes an external tool and produces diagnostics based on its output.
/// </summary>
public abstract class ExternalToolCheck
    : IRepositoryCheck
{
    /// <inheritdoc/>
    public abstract string CheckId { get; }

    /// <inheritdoc/>
    public abstract string CheckDisplayName { get; }

    /// <summary>
    /// Creates the command to be executed for the external tool.
    /// </summary>
    /// <param name="repository">The repository.</param>
    /// <returns>The command to be executed for the external tool.</returns>
    protected abstract Command CreateCommand(AltinnRepository repository);

    /// <summary>
    /// Tries to create a diagnostic from the given command output line.
    /// </summary>
    /// <param name="repository">The repository.</param>
    /// <param name="line">The command output line.</param>
    /// <param name="diagnostic">The created diagnostic if successful.</param>
    /// <returns>True if a diagnostic was successfully created; otherwise, false.</returns>
    protected virtual bool TryCreateDiagnostic(AltinnRepository repository, CommandOutputLine line, [NotNullWhen(true)] out Diagnostic? diagnostic)
        => MsBuildDiagnosticParser.TryParse(repository.RootDirectory, line.Line, out diagnostic);

    /// <inheritdoc/>
    public async IAsyncEnumerable<Diagnostic> Check(AltinnRepository repository, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var command = CreateCommand(repository);
        bool hasEmittedErrors = false;
        bool processFailed = false;
        int? errorExitCode = null;

        await using var enumerator = command.ExecuteLineCaptured(cancellationToken).GetAsyncEnumerator(cancellationToken);

        while (true)
        {
            CommandOutputLine line;
            try
            {
                if (!await enumerator.MoveNextAsync())
                {
                    break;
                }

                line = enumerator.Current;
            }
            catch (ProcessFailedException e)
            {
                try
                {
                    errorExitCode = e.Process?.ExitCode;
                }
                catch
                {
                    errorExitCode = null;
                }

                processFailed = true;
                break;
            }

            if (TryCreateDiagnostic(repository, line, out var diagnostic))
            {
                hasEmittedErrors |= diagnostic.Severity is DiagnosticSeverity.Error;
                yield return diagnostic;
            }
        }

        // if the process failed, but no errors were emitted, we need to synthesize an error diagnostic
        if (!hasEmittedErrors && processFailed)
        {
            yield return Diagnostics.ExternalToolFailed(command.ToString("display"), errorExitCode);
        }
    }
}
