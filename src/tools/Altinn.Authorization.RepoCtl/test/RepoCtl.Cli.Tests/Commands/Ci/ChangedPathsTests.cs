using System.Runtime.CompilerServices;
using System.Text.Json;
using Altinn.Authorization.CommandLine.GitHub.Actions;
using Altinn.Authorization.RepoCtl.Commands.Ci;
using Altinn.Authorization.RepoCtl.GitHub;
using Altinn.Authorization.RepoCtl.Model;
using Microsoft.Extensions.DependencyInjection;
using Semver;

namespace Altinn.Authorization.RepoCtl.Tests.Commands.Ci;

public class ChangedPathsTests
{
    [Theory]
    [InlineData("src/apps/Alpha/src/file.cs", true, false)]
    [InlineData("src/apps/Alpha/.hidden/file.cs", true, false)]
    [InlineData("src/apps/Alpha/infra/main.tf", true, true)]
    [InlineData("src/apps/AlphaExtra/file.cs", false, false)]
    [InlineData("src/apps/alpha/file.cs", false, false)]
    [InlineData("README.md", false, false)]
    public async Task ChangedPaths_MatchDirectoryBoundariesAndCase(string path, bool self, bool infra)
    {
        var app = CreateVertical("app:Alpha", "src/apps/Alpha");
        var repository = CreateRepository(app);
        var files = new ChangedFiles(path);

        using var services = CreateServices(files);
        var result = await new FindVerticalsCommand(new Actions(), services).GetChangedPaths(repository, repository.Verticals, TestContext.Current.CancellationToken);

        result.HasChanges("app:Alpha").ShouldBe(self);
        result.HasChanges("app:Alpha:full").ShouldBe(self);
        result.HasChanges("app:Alpha:infra").ShouldBe(infra);
        files.CallCount.ShouldBe(1);
    }

    [Theory]
    [InlineData(".github/workflows/build.yml")]
    [InlineData("eng/build.sh")]
    [InlineData("Directory.Build.props")]
    [InlineData("src/Directory.Packages.props")]
    [InlineData("global.json")]
    public async Task SharedChanges_AffectAllFiltersExceptSelfOnly(string path)
    {
        var app = CreateVertical("app:Alpha", "src/apps/Alpha");
        var repository = CreateRepository(app);

        using var services = CreateServices(new ChangedFiles(path));
        var result = await new FindVerticalsCommand(new Actions(), services).GetChangedPaths(repository, repository.Verticals, TestContext.Current.CancellationToken);

        result.HasChanges("app:Alpha").ShouldBeTrue();
        result.HasChanges("app:Alpha:full").ShouldBeTrue();
        result.HasChanges("app:Alpha:infra").ShouldBeTrue();
        result.HasChanges("app:Alpha", includeShared: false).ShouldBeFalse();
    }

    [Fact]
    public async Task FullChanges_IncludeDependenciesOutsideSelectedVerticals()
    {
        var dependency = CreateVertical("pkg:Dependency", "src/pkgs/Dependency");
        var transitive = CreateVertical("lib:Transitive", "src/libs/Transitive");
        var app = CreateVertical("app:Alpha", "src/apps/Alpha", dependency, transitive);
        var repository = CreateRepository(app, dependency, transitive);
        var selected = CreateSet(app);

        using var services = CreateServices(new ChangedFiles("src/libs/Transitive/deleted.cs"));
        var result = await new FindVerticalsCommand(new Actions(), services)
            .GetChangedPaths(repository, selected, TestContext.Current.CancellationToken);

        result.HasChanges("app:Alpha:full").ShouldBeTrue();
        result.HasChanges("app:Alpha").ShouldBeFalse();
        result.HasChanges("app:Alpha:infra").ShouldBeFalse();
    }

    [Fact]
    public async Task ExactSharedFiles_DoNotMatchSimilarNames()
    {
        var app = CreateVertical("app:Alpha", "src/apps/Alpha");
        var repository = CreateRepository(app);
        using var services = CreateServices(new ChangedFiles("global.json.backup", "docs/global.json", "Eng/build.sh"));
        var result = await new FindVerticalsCommand(new Actions(), services)
            .GetChangedPaths(repository, repository.Verticals, TestContext.Current.CancellationToken);

        result.HasChanges("app:Alpha").ShouldBeFalse();
    }

    [Fact]
    public async Task FindVerticals_None_DoesNotRequireChangedFilesService()
    {
        var app = CreateVertical("app:Alpha", "src/apps/Alpha");
        var repository = CreateRepository(app);
        var actions = new Actions();
        using var services = new ServiceCollection().BuildServiceProvider();
        var command = ActivatorUtilities.CreateInstance<FindVerticalsCommand>(services, actions);

        await command.Invoke(repository, repository.Verticals, null!, cancellationToken: TestContext.Current.CancellationToken);

        using var matrix = JsonDocument.Parse(actions.Outputs["matrix"]);
        matrix.RootElement.GetProperty("include").GetArrayLength().ShouldBe(1);
    }

    [Fact]
    public async Task FindVerticals_Full_UsesInternallyDetectedDependencyChanges()
    {
        var dependency = CreateVertical("pkg:Dependency", "src/pkgs/Dependency");
        var app = CreateVertical("app:Alpha", "src/apps/Alpha", dependency);
        var repository = CreateRepository(app, dependency);
        var actions = new Actions();
        var files = new ChangedFiles("src/pkgs/Dependency/file.cs");
        using var services = CreateServices(files);
        var command = ActivatorUtilities.CreateInstance<FindVerticalsCommand>(services, actions);

        await command.Invoke(repository, CreateSet(app), null!, FindVerticalsCommand.ChangedFilter.Full, TestContext.Current.CancellationToken);

        files.CallCount.ShouldBe(1);
        using var matrix = JsonDocument.Parse(actions.Outputs["matrix"]);
        var include = matrix.RootElement.GetProperty("include");
        include.GetArrayLength().ShouldBe(1);
        include[0].GetProperty("changedFull").GetBoolean().ShouldBeTrue();
        include[0].GetProperty("changed").GetBoolean().ShouldBeFalse();
    }

    private static ServiceProvider CreateServices(IChangedFilesService changedFiles)
        => new ServiceCollection().AddSingleton(changedFiles).BuildServiceProvider();

    private static readonly DirectoryInfo Root = new(Path.Combine(Path.GetTempPath(), "repoctl-changed-paths-tests"));

    private static AltinnVertical CreateVertical(string id, string path, params AltinnVertical[] dependencies)
        => new(path, new DirectoryInfo(Path.Combine(Root.FullName, path)), AltinnVerticalId.Parse(id, provider: null),
            new SemVersion(1, 0, 0), [], AltinnVerticalConfiguration.Default, [])
        {
            AllDependencies = CreateSet(dependencies),
        };

    private static AltinnRepository CreateRepository(params AltinnVertical[] verticals)
        => new(Root, new AltinnRepositoryConfiguration { Name = "Test", RootKind = AltinnVerticalKind.Application }, CreateSet(verticals));

    private static AltinnVerticalSet CreateSet(params AltinnVertical[] verticals)
    {
        var builder = AltinnVerticalSet.CreateBuilder();
        builder.AddRange(verticals);
        return builder.DrainToImmutable();
    }

    private sealed class ChangedFiles(params string[] paths) : IChangedFilesService
    {
        public int CallCount { get; private set; }

        public async IAsyncEnumerable<string> GetChangedFiles([EnumeratorCancellation] CancellationToken cancellationToken)
        {
            CallCount++;
            await Task.CompletedTask;
            foreach (var path in paths)
            {
                cancellationToken.ThrowIfCancellationRequested();
                yield return path;
            }
        }
    }

    private sealed class Actions : IGitHubActionsService
    {
        public bool IsGitHubActions => true;

        public GitHubContext? Context => null;

        public Dictionary<string, string> Outputs { get; } = new();

        public Task SetOutput(string key, string value, CancellationToken cancellationToken = default)
        {
            Outputs[key] = value;
            return Task.CompletedTask;
        }

        public Task SetEnvironmentVariable(string key, string value, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
    }
}
