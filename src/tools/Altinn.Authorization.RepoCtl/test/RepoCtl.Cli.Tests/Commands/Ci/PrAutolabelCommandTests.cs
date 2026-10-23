using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using Altinn.Authorization.CommandLine;
using Altinn.Authorization.CommandLine.GitHub.Actions;
using Altinn.Authorization.CommandLine.Results;
using Altinn.Authorization.RepoCtl.Commands.Ci;
using Altinn.Authorization.RepoCtl.GitHub;
using Altinn.Authorization.RepoCtl.Model;
using Octokit;
using Octokit.Internal;
using Octokit.Webhooks;
using Octokit.Webhooks.Events.PullRequest;
using Semver;
using GitHubContext = Altinn.Authorization.CommandLine.GitHub.Actions.GitHubContext;

namespace Altinn.Authorization.RepoCtl.Tests.Commands.Ci;

public class PrAutolabelCommandTests
{
    private static readonly DirectoryInfo Root = new(Path.Combine(Path.GetTempPath(), "repoctl-autolabel-tests"));
    private static readonly GitHubRepositoryContext Context = new() { RepositoryOwner = "owner", RepositoryName = "repo" };

    [Fact]
    public void Labels_UseKindSlugsDisplayNamesAndTransitiveAppDependencies()
    {
        var library = Vertical("lib:Altinn.Common", "custom/common");
        var intermediate = Vertical("lib:Altinn.Intermediate", "custom/intermediate", library);
        var app = Vertical("app:Altinn.Alpha", "custom/alpha", library, intermediate);
        var secondApp = Vertical("app:Altinn.Beta", "custom/beta", library);
        var package = Vertical("pkg:Altinn.Package", "custom/package");
        var tool = Vertical("tool:Altinn.Tool", "custom/tool");
        var unrelated = Vertical("app:Altinn.Unrelated", "custom/unrelated");
        var repository = Repository(library, intermediate, app, secondApp, package, tool, unrelated);

        var labels = PrAutolabelCommand.GetLabels(repository, new HashSet<string>
        {
            "custom/common/deleted.cs", "custom/common/other.cs", "custom/alpha/README.md",
            "custom/package/new.cs", "custom/tool/Program.cs",
        });

        labels.Select(label => label.Name).ShouldBe(["area/alpha", "area/beta", "lib/common", "pkg/package", "tool/tool"]);
        labels.Single(label => label.Name == "area/alpha").ShouldBe(new GitHubLabel("area/alpha", "#0978af", "App: Display Alpha"));
        labels.Single(label => label.Name == "lib/common").ShouldBe(new GitHubLabel("lib/common", "#6c1a0d", "Library: Display Common"));
        labels.Single(label => label.Name == "pkg/package").Color.ShouldBe("#4ad006");
        labels.Single(label => label.Name == "tool/tool").Color.ShouldBe("#3f602d");
    }

    [Theory]
    [InlineData("custom/AlphaExtra/file.cs")]
    [InlineData("custom/alpha/file.cs")]
    [InlineData(".github/workflows/build.yml")]
    [InlineData("README.md")]
    public void Labels_IgnoreSharedFilesAndRespectDirectoryBoundariesAndCase(string path)
        => PrAutolabelCommand.GetLabels(Repository(Vertical("app:Altinn.Alpha", "custom/Alpha")), new HashSet<string> { path }).ShouldBeEmpty();

    [Fact]
    public void Labels_UseConfiguredPrefixesAndColors()
    {
        var app = Vertical("app:Altinn.Alpha", "custom/alpha");
        var repository = new AltinnRepository(Root, new AltinnRepositoryConfiguration
        {
            Name = "Test",
            RootKind = AltinnVerticalKind.Application,
            Labels = new() { App = new("application/", "#123ABC") },
        }, Set(app));

        var label = PrAutolabelCommand.GetLabels(repository, new HashSet<string> { "custom/alpha/file.cs" }).Single();
        label.Color.ShouldBe("#123ABC");
        label.Name.ShouldBe("application/alpha");
    }

    [Fact]
    public async Task Invoke_PaginatesIncludesRenamesCreatesMissingLabelsAndAddsWithoutRemovingExistingLabels()
    {
        var requests = new List<string>();
        var repositoryLabels = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "area/alpha" };
        var prLabels = new HashSet<string> { "manual" };
        using var http = new HttpClientAdapter(() => new Handler(async request =>
        {
            var path = Uri.UnescapeDataString(request.RequestUri!.AbsolutePath);
            requests.Add($"{request.Method} {path}");
            if (path == "/repos/owner/repo/pulls/42/files")
            {
                if (request.RequestUri.Query.Contains("page=2", StringComparison.Ordinal))
                {
                    return Json(HttpStatusCode.OK, """[{"filename":"custom/beta/moved.cs","previous_filename":"custom/common/moved.cs","status":"renamed"}]""");
                }

                return Json(HttpStatusCode.OK, JsonSerializer.Serialize(Enumerable.Range(0, 100).Select(index => new { filename = $"custom/alpha/file{index}.cs", status = "modified" })));
            }

            if (request.Method == HttpMethod.Get && path == "/repos/owner/repo/labels")
            {
                if (request.RequestUri.Query.Contains("page=2", StringComparison.Ordinal))
                {
                    return Json(HttpStatusCode.OK, """[{"name":"AREA/ALPHA","color":"ffffff"}]""");
                }

                var response = Json(HttpStatusCode.OK, JsonSerializer.Serialize(Enumerable.Range(0, 100).Select(index => new { name = $"unrelated/{index}", color = "ffffff" })));
                response.Headers.Add("Link", "<https://api.github.com/repos/owner/repo/labels?per_page=100&page=2>; rel=\"next\"");
                return response;
            }

            if (request.Method == HttpMethod.Post && path == "/repos/owner/repo/labels")
            {
                using var body = JsonDocument.Parse(await request.Content!.ReadAsStringAsync());
                var name = body.RootElement.GetProperty("name").GetString()!;
                repositoryLabels.Add(name).ShouldBeTrue();
                body.RootElement.GetProperty("color").GetString().ShouldBe(name == "lib/common" ? "6c1a0d" : "0978af");
                body.RootElement.GetProperty("description").GetString().ShouldNotBeNullOrWhiteSpace();
                return Json(HttpStatusCode.Created, body.RootElement.GetRawText());
            }

            request.Method.ShouldBe(HttpMethod.Post);
            path.ShouldBe("/repos/owner/repo/issues/42/labels");
            using var added = JsonDocument.Parse(await request.Content!.ReadAsStringAsync());
            foreach (var label in added.RootElement.EnumerateArray())
            {
                var name = label.GetString()!;
                repositoryLabels.ShouldContain(name);
                prLabels.Add(name);
            }

            return Json(HttpStatusCode.OK, "[]");
        }));
        var client = new GitHubClient(new Connection(new ProductHeaderValue("repoctl-tests"), http));
        var actions = new Actions();
        var command = new PrAutolabelCommand(new GitHubChangedFilesService(actions, client), new GitHubService(null!, client), actions);
        var library = Vertical("lib:Altinn.Common", "custom/common");
        var repository = Repository(library, Vertical("app:Altinn.Alpha", "custom/alpha", library), Vertical("app:Altinn.Beta", "custom/beta"));

        (await Run(await command.Invoke(repository, Context, 42, TestContext.Current.CancellationToken))).ShouldBe(0);

        prLabels.Order().ShouldBe(["area/alpha", "area/beta", "lib/common", "manual"]);
        requests.Count(request => request.Contains("/pulls/42/files", StringComparison.Ordinal)).ShouldBe(2);
        requests.ShouldNotContain(request => request.StartsWith("PUT", StringComparison.Ordinal) || request.StartsWith("DELETE", StringComparison.Ordinal));
        requests.Count(request => request == "POST /repos/owner/repo/labels").ShouldBe(2);
        requests.Count(request => request == "GET /repos/owner/repo/labels").ShouldBe(2);
        requests.ShouldNotContain(request => request.StartsWith("GET /repos/owner/repo/labels/", StringComparison.Ordinal));
    }

    [Theory]
    [InlineData(null)]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task Invoke_RejectsMissingOrInvalidNumberBeforeReadingFiles(int? number)
    {
        var files = new Files();
        var command = new PrAutolabelCommand(files, null!, new Actions());
        (await Run(await command.Invoke(Repository(), Context, number, TestContext.Current.CancellationToken))).ShouldBe(1);
        files.Calls.ShouldBe(0);
    }

    [Theory]
    [InlineData("pull_request")]
    [InlineData("pull_request_target")]
    public void Number_IsInferredFromPullRequestEvent(string eventName)
    {
        var context = new GitHubContext
        {
            EventName = eventName,
            Event = new PullRequestOpenedEvent { Number = 42, PullRequest = null!, Repository = null!, Sender = null! },
            Repository = Context,
            Sha = "abc",
            Ref = null,
        };
        PrAutolabelCommand.GetPullRequestNumber(context).ShouldBe(42);
    }

    [Fact]
    public async Task Invoke_RejectsOtherEventsButAllowsExplicitNumberForDebugging()
    {
        var actions = new Actions(new GitHubContext
        {
            EventName = "other",
            Event = new OtherEvent { Repository = null!, Sender = null! },
            Repository = Context,
            Sha = "abc",
            Ref = null,
        });
        var files = new Files();
        var command = new PrAutolabelCommand(files, new GitHubService(null!, null!), actions);

        (await Run(await command.Invoke(Repository(), Context, cancellationToken: TestContext.Current.CancellationToken))).ShouldBe(1);
        files.Calls.ShouldBe(0);
        (await Run(await command.Invoke(Repository(), Context, 123, TestContext.Current.CancellationToken))).ShouldBe(0);
        files.Number.ShouldBe(123);
    }

    [Fact]
    public async Task Invoke_LabelCreationFailurePreventsAddingLabelsToPullRequest()
    {
        var issueLabelCalls = 0;
        using var http = new HttpClientAdapter(() => new Handler(request =>
        {
            if (request.RequestUri!.AbsolutePath.EndsWith("/issues/42/labels", StringComparison.Ordinal))
            {
                issueLabelCalls++;
            }

            return Task.FromResult(request.Method == HttpMethod.Get
                ? Json(HttpStatusCode.OK, "[]")
                : Json(HttpStatusCode.Forbidden, """{"message":"Forbidden"}"""));
        }));
        var client = new GitHubClient(new Connection(new ProductHeaderValue("repoctl-tests"), http));
        var files = new Files("custom/alpha/file.cs");
        var command = new PrAutolabelCommand(files, new GitHubService(null!, client), new Actions());

        (await Run(await command.Invoke(Repository(Vertical("app:Altinn.Alpha", "custom/alpha")), Context, 42, TestContext.Current.CancellationToken))).ShouldBe(1);
        issueLabelCalls.ShouldBe(0);
    }

    [Fact]
    public async Task Labels_HandleConcurrentCreationWithoutUpdatingExistingMetadata()
    {
        var requests = new List<string>();
        using var http = new HttpClientAdapter(() => new Handler(request =>
        {
            requests.Add($"{request.Method} {Uri.UnescapeDataString(request.RequestUri!.AbsolutePath)}");
            return Task.FromResult(requests.Count switch
            {
                1 => Json(HttpStatusCode.OK, "[]"),
                2 => Json(HttpStatusCode.UnprocessableEntity, """{"message":"Validation Failed","errors":[]}"""),
                3 => Json(HttpStatusCode.OK, """{"name":"area/alpha","color":"ffffff"}"""),
                _ => Json(HttpStatusCode.OK, "[]"),
            });
        }));
        var client = new GitHubClient(new Connection(new ProductHeaderValue("repoctl-tests"), http));

        await new GitHubService(null!, client).AddPullRequestLabels(Context, 42,
            [new GitHubLabel("area/alpha", "#0978AF", "App: Alpha")], TestContext.Current.CancellationToken);

        requests.ShouldBe([
            "GET /repos/owner/repo/labels",
            "POST /repos/owner/repo/labels",
            "GET /repos/owner/repo/labels/area/alpha",
            "POST /repos/owner/repo/issues/42/labels",
        ]);
    }

    private sealed record OtherEvent : WebhookEvent;

    [Fact]
    public async Task Labels_EmptyListMakesNoApiCalls()
    {
        using var http = new HttpClientAdapter(() => new Handler(_ => throw new InvalidOperationException("Unexpected API call")));
        var client = new GitHubClient(new Connection(new ProductHeaderValue("repoctl-tests"), http));

        await new GitHubService(null!, client).AddPullRequestLabels(Context, 42, [], TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task Invoke_UsesPullRequestEventWhenNumberIsOmitted()
    {
        var actions = new Actions(new GitHubContext
        {
            EventName = "pull_request",
            Event = new PullRequestOpenedEvent { Number = 42, PullRequest = null!, Repository = null!, Sender = null! },
            Repository = Context,
            Sha = "abc",
            Ref = null,
        });
        var files = new Files();
        var command = new PrAutolabelCommand(files, new GitHubService(null!, null!), actions);

        (await Run(await command.Invoke(Repository(), Context, cancellationToken: TestContext.Current.CancellationToken))).ShouldBe(0);
        files.Number.ShouldBe(42);
    }

    private static AltinnVertical Vertical(string id, string path, params AltinnVertical[] dependencies)
        => new(path, new DirectoryInfo(Path.Combine(Root.FullName, path)), AltinnVerticalId.Parse(id, null), new SemVersion(1, 0, 0), [],
            AltinnVerticalConfiguration.Default with { DisplayName = $"Display {id[(id.LastIndexOf('.') + 1)..]}" }, [])
        {
            AllDependencies = Set(dependencies),
            Dependents = Set(),
        };

    private static AltinnRepository Repository(params AltinnVertical[] verticals)
    {
        foreach (var vertical in verticals)
        {
            vertical.Dependents = Set(verticals.Where(dependent => dependent.AllDependencies.Contains(vertical)).ToArray());
        }

        return new(Root, new AltinnRepositoryConfiguration { Name = "Test", RootKind = AltinnVerticalKind.Application }, Set(verticals));
    }

    private static AltinnVerticalSet Set(params AltinnVertical[] verticals)
    {
        var builder = AltinnVerticalSet.CreateBuilder();
        builder.AddRange(verticals);
        return builder.DrainToImmutable();
    }

    private static async Task<int> Run(ICommandResult result)
    {
        await using var app = CliApplication.CreateBuilder("Autolabel tests").Build();
        app.AddCommand("run", "Run", () => result);
        return await app.RunAsync(["run"], TestContext.Current.CancellationToken);
    }

    private static HttpResponseMessage Json(HttpStatusCode status, string json)
        => new(status) { Content = new StringContent(json, Encoding.UTF8, "application/json") };

    private sealed class Handler(Func<HttpRequestMessage, Task<HttpResponseMessage>> send) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) => send(request);
    }

    private sealed class Files(params string[] paths) : IChangedFilesService
    {
        public int Calls { get; private set; }
        public int Number { get; private set; }
        public IAsyncEnumerable<string> GetChangedFiles(CancellationToken cancellationToken) => throw new NotSupportedException();
        public async IAsyncEnumerable<string> GetPullRequestChangedFiles(GitHubRepositoryContext repository, int number, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            Calls++;
            Number = number;
            await Task.CompletedTask;
            foreach (var path in paths)
            {
                cancellationToken.ThrowIfCancellationRequested();
                yield return path;
            }
        }
    }

    private sealed class Actions(GitHubContext? context = null) : IGitHubActionsService
    {
        public bool IsGitHubActions => false;
        public GitHubContext? Context => context;
        public Task SetOutput(string key, string value, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task SetEnvironmentVariable(string key, string value, bool print, CancellationToken cancellationToken = default) => throw new NotSupportedException();
    }
}
