using System.Net;
using System.Text;
using System.Text.Json;
using Altinn.Authorization.CommandLine;
using Altinn.Authorization.CommandLine.GitHub.Actions;
using Altinn.Authorization.CommandLine.Results;
using Altinn.Authorization.RepoCtl.Changelog;
using Altinn.Authorization.RepoCtl.Commands.Ci;
using Altinn.Authorization.RepoCtl.GitHub;
using Altinn.Authorization.RepoCtl.Model;
using Microsoft.Extensions.Time.Testing;
using Octokit;
using Octokit.Internal;
using Semver;

namespace Altinn.Authorization.RepoCtl.Tests.Commands.Ci;

public class DraftReleaseCommandTests
    : IDisposable
{
    private const string Sha = "0123456789012345678901234567890123456789";
    private readonly DirectoryInfo _root = Directory.CreateTempSubdirectory("repoctl-draft-tests-");
    private readonly List<string> _events = [];
    private readonly FakeTimeProvider _time = new(new DateTimeOffset(2026, 1, 2, 0, 30, 0, TimeSpan.Zero));
    private JsonElement? _releaseRequest;

    [Theory]
    [InlineData(null, "2026.01.02")]
    [InlineData("", "2026.01.02")]
    [InlineData("2026.01.02", "2026.01.02")]
    [InlineData("2026.01.02.012", "2026.01.02.012")]
    [InlineData("2026.01.02.001", "2026.01.02.001")]
    [InlineData("2026.10.19.123", "2026.10.19.123")]
    [InlineData("2024.02.29", "2024.02.29")]
    [InlineData("2000.02.29.123", "2000.02.29.123")]
    [InlineData("0001.01.01", "0001.01.01")]
    public async Task DraftRelease_ResolvesCommitAndCreatesNonLatestDraft(string? version, string expectedVersion)
    {
        var (repository, vertical) = CreateRepository();
        using var http = CreateHttpClient(releases: "[{\"draft\":true,\"tag_name\":\"app/Other-v2026.01.02\"},{\"draft\":false,\"tag_name\":\"app/Billing.Api-v2025.1.2\"}]");
        var changelog = new RecordingChangelogGenerator(_events);
        var command = CreateCommand(http, changelog);

        (await Run(command.Invoke(repository, vertical, RepositoryContext(), version, "source"))).ShouldBe(0);

        _events.ShouldBe(["commit", "tag", "releases", "changelog", "create"]);
        var tag = $"app/Billing.Api-v{expectedVersion}";
        changelog.Tag.ShouldBe(tag);
        changelog.Sha.ShouldBe(Sha);
        _releaseRequest.ShouldNotBeNull();
        var request = _releaseRequest.Value;
        request.GetProperty("tag_name").GetString().ShouldBe(tag);
        request.GetProperty("target_commitish").GetString().ShouldBe(Sha);
        request.GetProperty("name").GetString().ShouldBe($"app/Billing.Api: {expectedVersion}");
        request.GetProperty("body").GetString().ShouldBe("## Changes\nRelease notes\n");
        request.GetProperty("draft").GetBoolean().ShouldBeTrue();
        request.GetProperty("make_latest").GetString().ShouldBe("false");
    }

    [Theory]
    [InlineData("2026.1")]
    [InlineData("2026.1.2")]
    [InlineData("2026.01.02.3")]
    [InlineData("2026.01.02.000")]
    [InlineData("2026.01.02.1000")]
    [InlineData("2026.001.2")]
    [InlineData("26.1.2")]
    [InlineData("2026.01.02-beta")]
    [InlineData("2026.01.02\n")]
    [InlineData("2026.01.02\r\n")]
    [InlineData("２０２６.1.2")]
    [InlineData(" ")]
    [InlineData("2026.99.99")]
    [InlineData("2026.00.01")]
    [InlineData("2026.13.01")]
    [InlineData("2026.01.00")]
    [InlineData("2026.04.31")]
    [InlineData("2026.02.29")]
    [InlineData("1900.02.29")]
    [InlineData("2100.02.29.123")]
    [InlineData("0000.01.01")]
    public async Task DraftRelease_InvalidVersionHasNoSideEffects(string version)
    {
        var (repository, vertical) = CreateRepository();
        using var http = CreateHttpClient();
        var command = CreateCommand(http, new RecordingChangelogGenerator(_events));

        (await Run(command.Invoke(repository, vertical, RepositoryContext(), version, "source"))).ShouldBe(1);

        _events.ShouldBeEmpty();
    }

    [Fact]
    public async Task DraftRelease_RejectsNonApplication()
    {
        var (repository, vertical) = CreateRepository(AltinnVerticalKind.Package);
        using var http = CreateHttpClient();
        var command = CreateCommand(http, new RecordingChangelogGenerator(_events));

        (await Run(command.Invoke(repository, vertical, RepositoryContext(), "2026.01.02", "source"))).ShouldBe(1);

        _events.ShouldBeEmpty();
    }

    [Fact]
    public async Task DraftRelease_RejectsEmptyCommit()
    {
        var (repository, vertical) = CreateRepository();
        using var http = CreateHttpClient();
        var command = CreateCommand(http, new RecordingChangelogGenerator(_events));

        (await Run(command.Invoke(repository, vertical, RepositoryContext(), "2026.01.02", ""))).ShouldBe(1);

        _events.ShouldBeEmpty();
    }

    [Fact]
    public async Task DraftRelease_ExistingTagPreventsChangelogAndRelease()
    {
        var (repository, vertical) = CreateRepository();
        using var http = CreateHttpClient(tagStatus: HttpStatusCode.OK);
        var command = CreateCommand(http, new RecordingChangelogGenerator(_events));

        (await Run(command.Invoke(repository, vertical, RepositoryContext(), "2026.01.02", "source"))).ShouldBe(1);

        _events.ShouldBe(["commit", "tag"]);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task DraftRelease_ExistingAppDraftOnAnyPagePreventsChangelogAndRelease(bool onSecondPage)
    {
        var (repository, vertical) = CreateRepository();
        const string drafts = "[{\"draft\":true,\"tag_name\":\"app/Billing.Api-v2025.1.2\"}]";
        using var http = CreateHttpClient(releases: onSecondPage ? "[]" : drafts, nextPage: onSecondPage ? drafts : null);
        var command = CreateCommand(http, new RecordingChangelogGenerator(_events));

        (await Run(command.Invoke(repository, vertical, RepositoryContext(), "2026.01.02", "source"))).ShouldBe(1);

        _events.ShouldBe(onSecondPage ? ["commit", "tag", "releases", "releases"] : ["commit", "tag", "releases"]);
    }

    [Theory]
    [InlineData("commit")]
    [InlineData("tag")]
    [InlineData("releases")]
    [InlineData("create")]
    public async Task DraftRelease_ApiErrorsFailTheCommand(string failAt)
    {
        var (repository, vertical) = CreateRepository();
        using var http = CreateHttpClient(failAt: failAt);
        var command = CreateCommand(http, new RecordingChangelogGenerator(_events));

        (await Run(command.Invoke(repository, vertical, RepositoryContext(), "2026.01.02", "source"))).ShouldBe(1);

        _events[^1].ShouldBe(failAt);
        if (failAt != "create")
        {
            _events.ShouldNotContain("changelog");
            _releaseRequest.ShouldBeNull();
        }
    }

    [Fact]
    public async Task DraftRelease_ChangelogFailureDoesNotCreateRelease()
    {
        var (repository, vertical) = CreateRepository();
        using var http = CreateHttpClient();
        var changelog = new RecordingChangelogGenerator(_events) { Fail = true };
        var command = CreateCommand(http, changelog);

        (await Run(command.Invoke(repository, vertical, RepositoryContext(), "2026.01.02", "source"))).ShouldNotBe(0);

        _events.ShouldBe(["commit", "tag", "releases", "changelog"]);
        _releaseRequest.ShouldBeNull();
    }

    [Fact]
    public void ChangelogCommand_UsesMetadataAndRepeatedPathOptions()
    {
        var (repository, vertical) = CreateRepository();
        var library = CreateVertical("Altinn.Shared.Library", AltinnVerticalKind.Library);
        var package = CreateVertical("Altinn.Shared.Package", AltinnVerticalKind.Package);
        var dependencies = AltinnVerticalSet.CreateBuilder();
        dependencies.Add(library);
        dependencies.Add(package);
        vertical.AllDependencies = dependencies.ToImmutable();

        var command = GitCliffChangelogGenerator.CreateCommand(repository, vertical, "app/Billing.Api-v2026.01.02");

        command.Filename.ShouldBe("git");
        command.Arguments.ShouldBe([
            "cliff", "--config", "cliff.toml", "--tag-pattern", @"^app/Billing\.Api-v.*$",
            "--include-path", "custom/Altinn.Authorization.Billing.Api/**",
            "--include-path", "custom/Altinn.Shared.Library/**",
            "--unreleased", "--tag", "app/Billing.Api-v2026.01.02",
        ]);
    }

    public void Dispose() => _root.Delete(recursive: true);

    private (AltinnRepository Repository, AltinnVertical Vertical) CreateRepository(AltinnVerticalKind? kind = null)
    {
        var vertical = CreateVertical("Altinn.Authorization.Billing.Api", kind ?? AltinnVerticalKind.Application);
        var verticals = AltinnVerticalSet.CreateBuilder();
        verticals.Add(vertical);
        return (new AltinnRepository(_root, new AltinnRepositoryConfiguration { Name = "Test", RootKind = AltinnVerticalKind.Package }, verticals.ToImmutable()), vertical);
    }

    private AltinnVertical CreateVertical(string name, AltinnVerticalKind kind)
        => new($"custom/{name}", _root.CreateSubdirectory($"custom/{name}"), new AltinnVerticalId(kind, name),
            new SemVersion(1, 0, 0), [], AltinnVerticalConfiguration.Default with { DisplayName = "Custom display name" }, [])
        {
            AllDependencies = AltinnVerticalSet.CreateBuilder().ToImmutable(),
        };

    private DraftReleaseCommand CreateCommand(HttpClientAdapter http, IChangelogGenerator changelog)
    {
        var github = new GitHubClient(new Connection(new ProductHeaderValue("repoctl-tests"), http));
        return new DraftReleaseCommand(new GitHubService(null!, github), changelog, _time);
    }

    private HttpClientAdapter CreateHttpClient(HttpStatusCode tagStatus = HttpStatusCode.NotFound, string releases = "[]", string? nextPage = null, string? failAt = null)
        => new(() => new RequestHandler(async (request, cancellationToken) =>
        {
            var path = Uri.UnescapeDataString(request.RequestUri!.AbsolutePath);
            var stage = request.Method == HttpMethod.Post ? "create"
                : path.Contains("/commits/", StringComparison.Ordinal) ? "commit"
                : path.Contains("/git/refs/", StringComparison.Ordinal) ? "tag" : "releases";
            _events.Add(stage);
            if (stage == failAt)
            {
                return Response(HttpStatusCode.Forbidden, "{\"message\":\"denied\"}");
            }

            switch (stage)
            {
                case "commit":
                    path.ShouldBe("/repos/owner/repo/commits/source");
                    return Response(HttpStatusCode.OK, $"{{\"sha\":\"{Sha}\"}}");
                case "tag":
                    path.ShouldContain("/git/refs/tags/app/Billing.Api-v");
                    return Response(tagStatus, "{}");
                case "releases":
                    var secondPage = request.RequestUri.Query.Contains("page=2", StringComparison.Ordinal);
                    var response = Response(HttpStatusCode.OK, secondPage ? nextPage! : releases);
                    if (nextPage is not null && !secondPage)
                    {
                        response.Headers.TryAddWithoutValidation("Link", "<https://api.github.com/repos/owner/repo/releases?page=2>; rel=\"next\"");
                    }

                    return response;
                default:
                    path.ShouldBe("/repos/owner/repo/releases");
                    using (var body = JsonDocument.Parse(await request.Content!.ReadAsStringAsync(cancellationToken)))
                    {
                        _releaseRequest = body.RootElement.Clone();
                    }

                    return Response(HttpStatusCode.Created, "{\"id\":123,\"html_url\":\"https://github.com/owner/repo/releases/123\"}");
            }
        }));

    private static HttpResponseMessage Response(HttpStatusCode status, string json)
        => new(status) { Content = new StringContent(json, Encoding.UTF8, "application/json") };

    private static GitHubRepositoryContext RepositoryContext()
        => new() { RepositoryOwner = "owner", RepositoryName = "repo" };

    private static async Task<int> Run(ICommandResult result)
    {
        await using var application = CliApplication.CreateBuilder("Draft release tests").Build();
        application.AddCommand("draft-release", "Draft release", () => result);
        return await application.RunAsync(["draft-release"], TestContext.Current.CancellationToken);
    }

    private sealed class RequestHandler(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> send)
        : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => send(request, cancellationToken);
    }

    private sealed class RecordingChangelogGenerator(List<string> events)
        : IChangelogGenerator
    {
        public string? Tag { get; private set; }

        public string? Sha { get; private set; }

        public bool Fail { get; init; }

        public Task<string> Generate(AltinnRepository repository, AltinnVertical vertical, string tag, string sha, CancellationToken cancellationToken = default)
        {
            events.Add("changelog");
            Tag = tag;
            Sha = sha;
            return Fail ? Task.FromException<string>(new InvalidOperationException("Changelog failed")) : Task.FromResult("## Changes\nRelease notes\n");
        }
    }
}
