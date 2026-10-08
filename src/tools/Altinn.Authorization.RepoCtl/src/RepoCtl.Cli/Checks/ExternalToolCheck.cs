using System.Runtime.CompilerServices;
using Altinn.Authorization.CommandLine.Shell;
using Altinn.Authorization.RepoCtl.Model;

namespace Altinn.Authorization.RepoCtl.Checks;

internal abstract class ExternalToolCheck
    : IRepositoryCheck
{
    public abstract string CheckId { get; }
    public abstract string CheckDisplayName { get; }
    protected abstract Command CreateCommand(AltinnRepository repository);

    public async IAsyncEnumerable<CheckIssue> Check(AltinnRepository repository, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        // TODO: we want to capure the tool output and defer the output of it, but this required .NET 11 so will be implemented later
        var command = CreateCommand(repository);
        CheckIssue? issue = null;

        try
        {
            await command.Execute(cancellationToken);
        }
        catch (ProcessFailedException)
        {
            issue = CheckIssue.CreateCommand(command.ToString("display"));
        }

        if (issue is not null)
        {
            yield return issue;
        }
    }
}
