using Altinn.Authorization.CommandLine;
using Altinn.Authorization.RepoCtl.Binding;
using Altinn.Authorization.RepoCtl.Model;

namespace Altinn.Authorization.RepoCtl.Utils;

internal sealed class AltinnRepositoryAccessor(
    AltinnRepositoryResolver repositoryResolver,
    ICommandInvocationContextAccessor contextAccessor)
{
    public async ValueTask<AltinnRepository> GetRepository(CancellationToken cancellationToken = default)
    {
        var context = contextAccessor.CommandInvocationContext;
        if (context is null)
        {
            throw new InvalidOperationException("No command invocation context is available.");
        }

        var result = await repositoryResolver.ResolveParameterValue(context, cancellationToken);
        result.EnsureSuccess();

        return result.Value;
    }
}
