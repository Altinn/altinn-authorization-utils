using Altinn.Authorization.CommandLine;
using Altinn.Authorization.CommandLine.Factory;
using Altinn.Authorization.ProblemDetails;
using Altinn.Authorization.RepoCtl.Binding;
using Altinn.Authorization.RepoCtl.Model;
using Microsoft.Extensions.DependencyInjection;
using Semver;

namespace Altinn.Authorization.RepoCtl.Tests.Binding;

public class VerticalResolverTests : IDisposable
{
    private readonly DirectoryInfo _root = Directory.CreateTempSubdirectory("repoctl-vertical-tests-");

    [Theory]
    [InlineData("pkg:Altinn.Foo")]
    [InlineData("custom/Foo")]
    [InlineData("--vertical-id", "pkg:Altinn.Foo")]
    [InlineData("--vertical-tag", "pkg/Foo-vblah")]
    public async Task Resolve_BindsSelectedVertical(params string[] selectors)
    {
        var vertical = CreateVertical("pkg:Altinn.Foo", "custom/Foo");
        var result = await Run(Repository(vertical), selectors);

        result.ExitCode.ShouldBe(0);
        result.Vertical.ShouldBeSameAs(vertical);
        result.LoadCalls.ShouldBe(1);
    }

    [Theory]
    [InlineData()]
    [InlineData("pkg:Altinn.Foo", "--vertical-id", "pkg:Altinn.Foo")]
    [InlineData("pkg:Altinn.Foo", "--vertical-tag", "pkg/Foo-v1")]
    [InlineData("--vertical-id", "pkg:Altinn.Foo", "--vertical-tag", "pkg/Foo-v1")]
    [InlineData("pkg:Altinn.Foo", "--vertical-id", "pkg:Altinn.Foo", "--vertical-tag", "pkg/Foo-v1")]
    [InlineData("")]
    [InlineData("--vertical-id", " ")]
    [InlineData("--vertical-tag", "")]
    [InlineData("--vertical-id")]
    [InlineData("--vertical-tag")]
    public async Task Resolve_InvalidSelectorsFailBeforeLoadingRepository(params string[] selectors)
    {
        var result = await Run(Repository(CreateVertical("pkg:Altinn.Foo", "custom/Foo")), selectors);

        result.ExitCode.ShouldNotBe(0);
        result.Vertical.ShouldBeNull();
        result.LoadCalls.ShouldBe(0);
    }

    [Theory]
    [InlineData("--vertical-id", "custom/Foo")]
    [InlineData("--vertical-id", "pkg:Missing")]
    [InlineData("--vertical-tag", "pkg:Altinn.Foo")]
    [InlineData("--vertical-tag", "custom/Foo")]
    [InlineData("--vertical-tag", "pkg/Missing-v1")]
    [InlineData("--vertical-tag", "pkg/Foo-v")]
    [InlineData("--vertical-tag", "pkg/Foo")]
    [InlineData("--vertical-tag", "pkg/Foo-v1", "pkg:Altinn.Foo")]
    public async Task Resolve_ExplicitSelectorsDoNotFallBack(params string[] selectors)
    {
        var result = await Run(Repository(CreateVertical("pkg:Altinn.Foo", "custom/Foo")), selectors);

        result.ExitCode.ShouldNotBe(0);
        result.Vertical.ShouldBeNull();
    }

    [Fact]
    public async Task Resolve_PathIsRelativeToSelectedWorkingDirectory()
    {
        var vertical = CreateVertical("pkg:Altinn.Foo", "custom/Foo");
        var result = await Run(Repository(vertical), ["Foo"], workingDirectory: _root.CreateSubdirectory("custom"));

        result.ExitCode.ShouldBe(0);
        result.Vertical.ShouldBeSameAs(vertical);
    }

    [Theory]
    [InlineData("app:Altinn.Authorization.Foo", "app/Foo-v2026.10.20")]
    [InlineData("pkg:Altinn.Authorization.Foo", "pkg/Foo-v1.2.3-beta")]
    [InlineData("tool:Altinn.Foo", "tool/Foo-vanything")]
    public void TagLookup_UsesCanonicalPrefixWithoutValidatingVersion(string id, string tag)
    {
        var vertical = CreateVertical(id, "custom/Foo");

        var success = VerticalLookup.TryLookup(Repository(vertical).Verticals, VerticalLookup.LookupKind.Tag, tag, _root, out var result, out var error);

        success.ShouldBeTrue();
        result.ShouldBeSameAs(vertical);
        error.ShouldBeNull();
    }

    [Fact]
    public void TagLookup_RejectsPrefixCollisionsAndReportsCandidates()
    {
        var first = CreateVertical("pkg:Foo", "one");
        var second = CreateVertical("pkg:Altinn.Foo", "two");

        var success = VerticalLookup.TryLookupByTag(Repository(first, second).Verticals, "pkg/Foo-v1.2.3", out var result, out var error);

        success.ShouldBeFalse();
        result.ShouldBeNull();
        error.ShouldNotBeNull();
        error.ShouldContain("Multiple verticals");
        error.ShouldContain("pkg:Foo");
        error.ShouldContain("pkg:Altinn.Foo");
    }

    [Theory]
    [InlineData("pkg/FooX-v1")]
    [InlineData("app/Foo-v1")]
    [InlineData("pkg/foo-v1")]
    public void TagLookup_RequiresExactKindAndPrefix(string tag)
    {
        var vertical = CreateVertical("pkg:Altinn.Foo", "custom/Foo");

        var success = VerticalLookup.TryLookupByTag(Repository(vertical).Verticals, tag, out var result, out var error);

        success.ShouldBeFalse();
        result.ShouldBeNull();
        error.ShouldNotBeNull();
    }

    [Fact]
    public void PathLookup_RejectsAmbiguousDirectoryAndReportsCandidates()
    {
        var first = CreateVertical("pkg:Altinn.Foo", "custom/Foo");
        var second = CreateVertical("pkg:Altinn.Bar", "custom/Bar");

        var success = VerticalLookup.TryLookupByPath(Repository(first, second).Verticals, "custom", _root, out var result, out var error);

        success.ShouldBeFalse();
        result.ShouldBeNull();
        error.ShouldNotBeNull();
        error.ShouldContain("pkg:Altinn.Foo");
        error.ShouldContain("pkg:Altinn.Bar");
    }

    [Theory]
    [InlineData("custom/Missing")]
    [InlineData("pkg:Missing")]
    [InlineData("custom")]
    public void IdOrPathLookup_ReportsStaticError(string lookup)
    {
        var vertical = CreateVertical("pkg:Altinn.Foo", "custom/Foo");
        var other = CreateVertical("pkg:Altinn.Bar", "custom/Bar");

        var success = VerticalLookup.TryLookupByIdOrPath(Repository(vertical, other).Verticals, lookup, _root, out var result, out var error);

        success.ShouldBeFalse();
        result.ShouldBeNull();
        error.ShouldBe("Could not resolve the vertical selector as a unique vertical ID or directory path.");
    }

    [Theory]
    [InlineData("custom/Foo", true)]
    [InlineData("custom/Missing", false)]
    public void PathLookup_DispatchesPathKind(string path, bool expectedSuccess)
    {
        var vertical = CreateVertical("pkg:Altinn.Foo", "custom/Foo");

        var success = VerticalLookup.TryLookup(Repository(vertical).Verticals, VerticalLookup.LookupKind.Path, path, _root, out var result, out var error);

        success.ShouldBe(expectedSuccess);
        if (success)
        {
            result.ShouldBeSameAs(vertical);
            error.ShouldBeNull();
        }
        else
        {
            result.ShouldBeNull();
            error.ShouldNotBeNull();
        }
    }

    [Theory]
    [InlineData("pkg:Altinn.Foo")]
    [InlineData("--vertical-id", "pkg:Altinn.Foo")]
    [InlineData("--vertical-tag", "pkg/Foo-v1")]
    public async Task Resolve_PreservesArgumentsAfterSeparator(params string[] selectors)
    {
        var vertical = CreateVertical("pkg:Altinn.Foo", "custom/Foo");
        var result = await Run(Repository(vertical), [.. selectors, "--", "--configuration", "Release"], forward: true);

        result.ExitCode.ShouldBe(0);
        result.Vertical.ShouldBeSameAs(vertical);
        result.Forwarded.ShouldBe(["--configuration", "Release"]);
    }

    public void Dispose() => _root.Delete(recursive: true);

    private AltinnVertical CreateVertical(string id, string path)
        => new(path, _root.CreateSubdirectory(path), AltinnVerticalId.Parse(id, null), new SemVersion(1, 0, 0), [],
            AltinnVerticalConfiguration.Default with { DisplayName = "Different display name" }, []);

    private AltinnRepository Repository(params AltinnVertical[] verticals)
    {
        var builder = AltinnVerticalSet.CreateBuilder();
        foreach (var vertical in verticals)
        {
            builder.Add(vertical);
        }

        return new AltinnRepository(_root, new AltinnRepositoryConfiguration { Name = "Test", RootKind = AltinnVerticalKind.Package }, builder.ToImmutable());
    }

    private async Task<(int ExitCode, AltinnVertical? Vertical, int LoadCalls, string[]? Forwarded)> Run(
        AltinnRepository repository, string[] selectors, DirectoryInfo? workingDirectory = null, bool forward = false)
    {
        var loader = new FixedRepositoryLoader(repository);
        var builder = CliApplication.CreateBuilder("Vertical selector tests");
        builder.Services.AddSingleton<IAltinnRepositoryLoader>(loader);
        builder.Services.AddSingleton<AltinnRepositoryResolver>();
        builder.Services.AddSingleton<ICommandHandlerParameterBinderResolver, AltinnRepositoryBinderResolver>();
        await using var application = builder.Build();
        application.ApplicationServices.GetRequiredService<AltinnRepositoryResolver>().Configure(application);
        AltinnVertical? selected = null;
        string[]? forwarded = null;
        if (forward)
        {
            application.AddCommand("select", "Select", (AltinnVertical vertical, [RestArguments] string[] rest) =>
            {
                selected = vertical;
                forwarded = rest;
            });
        }
        else
        {
            application.AddCommand("select", "Select", (AltinnVertical vertical) => { selected = vertical; });
        }

        var exitCode = await application.RunAsync(["select", "--working-directory", (workingDirectory ?? _root).FullName, .. selectors], TestContext.Current.CancellationToken);
        return (exitCode, selected, loader.LoadCalls, forwarded);
    }

    private sealed class FixedRepositoryLoader(AltinnRepository repository) : IAltinnRepositoryLoader
    {
        public int LoadCalls { get; private set; }

        public Task<Result<AltinnRepository>> Load(DirectoryInfo directory, CancellationToken cancellationToken = default)
        {
            LoadCalls++;
            return Task.FromResult<Result<AltinnRepository>>(repository);
        }
    }
}
