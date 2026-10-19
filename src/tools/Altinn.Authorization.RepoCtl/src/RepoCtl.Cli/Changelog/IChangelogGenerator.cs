using Altinn.Authorization.RepoCtl.Model;

namespace Altinn.Authorization.RepoCtl.Changelog;

internal interface IChangelogGenerator
{
    Task<string> Generate(AltinnRepository repository, AltinnVertical vertical, string tag, string sha, CancellationToken cancellationToken = default);
}
