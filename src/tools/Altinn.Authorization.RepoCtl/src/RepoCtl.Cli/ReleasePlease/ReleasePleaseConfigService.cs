using System.Buffers;
using System.Runtime.CompilerServices;
using Altinn.Authorization.RepoCtl.Checks;
using Altinn.Authorization.RepoCtl.Model;
using Altinn.Authorization.RepoCtl.Model.ReleasePlease;
using Altinn.Authorization.RepoCtl.Model.Utils;
using Altinn.Authorization.RepoCtl.Utils;
using Nerdbank.Streams;

namespace Altinn.Authorization.RepoCtl.ReleasePlease;

internal sealed class ReleasePleaseConfigService
    : IRepositoryCheck
{
    private static readonly ReadOnlyMemory<byte> NewLine
        = new byte[1] { (byte)'\n' };

    string IRepositoryCheck.CheckId => "release-please-config";
    string IRepositoryCheck.CheckDisplayName => "Release Please config";

    public async Task UpdateReleasePleaseConfigFile(AltinnRepository repository, CancellationToken cancellationToken)
    {
        await using var fs = GetReleasePleaseConfigFile(repository, FileAccess.ReadWrite);
        if (fs is null)
        {
            return;
        }

        var config = await ReleasePleaseConfig.DeserializeAsync(fs, cancellationToken);
        if (config is null)
        {
            return;
        }

        UpdateReleasePleaseConfig(repository, config);
        fs.Seek(0, SeekOrigin.Begin);
        fs.SetLength(0);

        await ReleasePleaseConfig.SerializeAsync(fs, config, cancellationToken);
        await fs.WriteAsync(NewLine, cancellationToken);
    }

    async IAsyncEnumerable<CheckIssue> IRepositoryCheck.Check(
        AltinnRepository repository,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await using var fs = GetReleasePleaseConfigFile(repository);
        if (fs is null)
        {
            yield break;
        }

        using var actual = new Sequence<byte>(ArrayPool<byte>.Shared);
        await fs.NormalizeLineEndingsAndCopyToAsync(actual, cancellationToken);

        var config = ReleasePleaseConfig.Deserialize(actual.AsReadOnlySequence);
        if (config is null)
        {
            yield break;
        }

        UpdateReleasePleaseConfig(repository, config);

        using var wanted = new Sequence<byte>(ArrayPool<byte>.Shared);
        ReleasePleaseConfig.Serialize(wanted, config);
        wanted.Write(NewLine.Span);

        if (!wanted.AsReadOnlySequence.SequenceEqual(actual.AsReadOnlySequence))
        {
            yield return CheckIssue.CreateFile(GetRelativePath(repository.RootDirectory.FullName, fs.FileInfo.FullName), "Release Please configuration is out of date.");
        }
    }

    private OpenedFileStream? GetReleasePleaseConfigFile(AltinnRepository repository, FileAccess fileAccess = FileAccess.Read)
        => repository.RootDirectory.Find(
            ["release-please-config"],
            ["json"],
            fileAccess);

    private void UpdateReleasePleaseConfig(AltinnRepository repository, ReleasePleaseConfig config)
    {
        config.ReleaseType = ReleasePleaseReleaseType.Simple;
        config.PullRequestTitlePattern = "release${scope}:${component} ${version}";
        config.SeparatePullRequests = true;

        foreach (var vertical in repository.Verticals)
        {
            if (!vertical.Kind.IsPackable)
            {
                continue;
            }

            if (!config.Packages.TryGetValue(vertical.RelPath, out var package))
            {
                package = new ReleasePleasePackage
                {
                    Component = vertical.Id.ToString("tag-prefix", null),
                };

                config.Packages.Add(vertical.RelPath, package);
            }

            package.ExtraFiles ??= new();
            package.ExtraFiles.RemoveAll(f => f.Path == "Version.props");
            package.ExtraFiles.Add(new ReleasePleaseXmlExtraFile
            {
                Path = "Version.props",
                XPath = "//Project/PropertyGroup/Version",
            });

            package.ExtraFiles.Sort(static (a, b) =>
            {
                if (string.Compare(a.Type, b.Type, StringComparison.Ordinal) is var cmp && cmp != 0)
                {
                    return cmp;
                }

                return string.Compare(a.Path, b.Path, StringComparison.Ordinal);
            });
        }
    }

    private static string GetRelativePath(string rootPath, string fullPath)
    {
        var relativePath = Path.GetRelativePath(rootPath, fullPath);
        return relativePath.Replace(Path.DirectorySeparatorChar, '/');
    }
}
