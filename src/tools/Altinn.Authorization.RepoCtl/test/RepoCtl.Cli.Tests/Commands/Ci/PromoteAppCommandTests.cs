using System.Net;
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
using Semver;

namespace Altinn.Authorization.RepoCtl.Tests.Commands.Ci;

public class PromoteAppCommandTests
    : IDisposable
{
    private readonly DirectoryInfo _root = Directory.CreateTempSubdirectory("repoctl-deploy-tests-");
    private readonly List<string> _events = [];
    private readonly List<JsonElement> _requests = [];

    [Fact]
    public async Task Deploy_CreatesAllPendingDeploymentsBeforeAnyPromotion()
    {
        var repository = CreateRepository(("Altinn.Authorization", true), ("Altinn.Other", true));
        using var http = CreateHttpClient();
        var containers = new RecordingContainerService(_events);
        var command = CreateCommand(http, containers);

        var result = command.Invoke(repository, RepositoryContext(), new(["app:Altinn.Authorization", "app:Altinn.Other"]), new(["test", "prod"]), "abc123");

        (await Run(result)).ShouldBe(0);
        _events.ShouldBe([
            "deployment:authorization/test", "deployment:authorization/prod",
            "deployment:other/test", "deployment:other/prod",
            "promote:ghcr.io/altinn/altinn-auth/authorization-manifests",
            "promote:ghcr.io/altinn/altinn-auth/other-manifests",
        ]);

        foreach (var request in _requests)
        {
            request.GetProperty("ref").GetString().ShouldBe("abc123");
            request.GetProperty("auto_merge").GetBoolean().ShouldBeFalse();
            request.GetProperty("required_contexts").GetArrayLength().ShouldBe(0);
            request.GetProperty("description").GetString().ShouldBe("OCI artifact promotion requested; awaiting promotion and Flux reconciliation");
        }

        _requests[0].GetProperty("production_environment").GetBoolean().ShouldBeFalse();
        _requests[1].GetProperty("production_environment").GetBoolean().ShouldBeTrue();
    }

    [Fact]
    public async Task Deploy_SkipsAppsWithoutManifests()
    {
        var repository = CreateRepository(("Altinn.Authorization", false));
        using var http = CreateHttpClient();
        var command = CreateCommand(http, new RecordingContainerService(_events));

        (await Run(command.Invoke(repository, RepositoryContext(), new("app:Altinn.Authorization"), new("test"), "abc123"))).ShouldBe(0);

        _events.ShouldBeEmpty();
    }

    [Theory]
    [InlineData("app:Missing", "test", "abc123")]
    [InlineData("pkg:Altinn.Authorization", "test", "abc123")]
    [InlineData("", "test", "abc123")]
    [InlineData("app:Altinn.Authorization", "", "abc123")]
    [InlineData("app:Altinn.Authorization", "PROD", "abc123")]
    [InlineData("app:Altinn.Authorization", "test;echo", "abc123")]
    [InlineData("app:Altinn.Authorization", "test-prod", "abc123")]
    [InlineData("app:Altinn.Authorization", "-", "abc123")]
    [InlineData("app:Altinn.Authorization", "test", "")]
    [InlineData("app:Altinn.Authorization", "test", "--invalid")]
    [InlineData("app:Altinn.Authorization", "test", "abc/123")]
    [InlineData("app:Altinn.Authorization", "test", "abcé123")]
    public async Task Deploy_ValidatesEntireRequestBeforeSideEffects(string id, string environments, string sha)
    {
        var repository = CreateRepository(("Altinn.Authorization", true));
        using var http = CreateHttpClient();
        var command = CreateCommand(http, new RecordingContainerService(_events));

        string[] ids = string.IsNullOrWhiteSpace(id) ? [id] : ["app:Altinn.Authorization", id];
        (await Run(command.Invoke(repository, RepositoryContext(), ids, new(environments), sha))).ShouldBe(1);

        _events.ShouldBeEmpty();
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    public async Task Deploy_FailedDeploymentContinuesRequestsButPreventsAllPromotions(int failAtRequest)
    {
        var repository = CreateRepository(("Altinn.Authorization", true), ("Altinn.Other", true));
        using var http = CreateHttpClient(failAtRequest);
        var command = CreateCommand(http, new RecordingContainerService(_events));

        (await Run(command.Invoke(repository, RepositoryContext(), new(["app:Altinn.Authorization", "app:Altinn.Other"]), new(["test", "prod"]), "abc123"))).ShouldBe(1);

        _events.ShouldBe([
            "deployment:authorization/test", "deployment:authorization/prod",
            "deployment:other/test", "deployment:other/prod",
        ]);
    }

    [Theory]
    [InlineData(17, 0, 17)]
    [InlineData(0, 23, 23)]
    [InlineData(17, 23, 23)]
    public async Task Deploy_FailedPromotionContinuesPromotionsAndPreservesFailure(int firstExitCode, int secondExitCode, int expectedExitCode)
    {
        var repository = CreateRepository(("Altinn.Authorization", true), ("Altinn.Other", true));
        using var http = CreateHttpClient();
        var command = CreateCommand(http, new RecordingContainerService(_events, firstExitCode, secondExitCode));

        (await Run(command.Invoke(repository, RepositoryContext(), new(["app:Altinn.Authorization", "app:Altinn.Other"]), new("test"), "abc123"))).ShouldBe(expectedExitCode);

        _events.ShouldBe([
            "deployment:authorization/test", "deployment:other/test",
            "promote:ghcr.io/altinn/altinn-auth/authorization-manifests",
            "promote:ghcr.io/altinn/altinn-auth/other-manifests",
        ]);
    }

    [Fact]
    public async Task Deploy_SkipsAppWithoutManifestsBetweenDeployableApps()
    {
        var repository = CreateRepository(("Altinn.Authorization", true), ("Altinn.Skipped", false), ("Altinn.Other", true));
        using var http = CreateHttpClient();
        var command = CreateCommand(http, new RecordingContainerService(_events));

        var result = command.Invoke(repository, RepositoryContext(), new(["app:Altinn.Authorization", "app:Altinn.Skipped", "app:Altinn.Other"]), new("test"), "abc123");

        (await Run(result)).ShouldBe(0);
        _events.ShouldBe([
            "deployment:authorization/test", "deployment:other/test",
            "promote:ghcr.io/altinn/altinn-auth/authorization-manifests",
            "promote:ghcr.io/altinn/altinn-auth/other-manifests",
        ]);
    }

    [Fact]
    public async Task Deploy_UsesSelectedRegistryAndDeduplicatesInputs()
    {
        var repository = CreateRepository(("Altinn.Authorization", true));
        using var http = CreateHttpClient();
        var command = CreateCommand(http, new RecordingContainerService(_events));

        (await Run(command.Invoke(repository, RepositoryContext(), new(["app:Altinn.Authorization", "app:Altinn.Authorization"]), new(["test", "test"]), "abc123", "registry.example/team/"))).ShouldBe(0);

        _events.ShouldBe(["deployment:authorization/test", "promote:registry.example/team/authorization-manifests"]);
    }

    [Fact]
    public void Promotion_UsesArgumentArrayWithAllTargetTagsAndOriginalSource()
    {
        var command = ContainerService.CreateManifestPromotionCommand(_root.FullName, "registry.example/team/app-manifests", "abc123", ["test", "prod"]);

        command.Filename.ShouldBe("docker");
        command.Arguments.ShouldBe([
            "buildx", "imagetools", "create",
            "-t", "registry.example/team/app-manifests:test",
            "-t", "registry.example/team/app-manifests:prod",
            "registry.example/team/app-manifests:abc123",
        ]);
    }

    public void Dispose() => _root.Delete(recursive: true);

    private AltinnRepository CreateRepository(params (string Name, bool Manifests)[] apps)
    {
        var verticals = AltinnVerticalSet.CreateBuilder();
        foreach (var (name, manifests) in apps)
        {
            // Deliberately use a nonstandard directory to ensure the model supplies the location.
            var directory = _root.CreateSubdirectory($"custom/{name}");
            if (manifests)
            {
                directory.CreateSubdirectory("manifests");
            }

            verticals.Add(new AltinnVertical(
                $"custom/{name}", directory, new AltinnVerticalId(AltinnVerticalKind.Application, name),
                new SemVersion(1, 0, 0), [], AltinnVerticalConfiguration.Default, []));
        }

        return new AltinnRepository(_root, new AltinnRepositoryConfiguration { Name = "Test", RootKind = AltinnVerticalKind.Package }, verticals.ToImmutable());
    }

    private HttpClientAdapter CreateHttpClient(int? failAtRequest = null)
        => new(() => new DeploymentHandler(async (request, cancellationToken) =>
        {
            request.Method.ShouldBe(HttpMethod.Post);
            request.RequestUri!.AbsolutePath.ShouldBe("/repos/owner/repo/deployments");
            using var body = JsonDocument.Parse(await request.Content!.ReadAsStringAsync(cancellationToken));
            var json = body.RootElement.Clone();
            _requests.Add(json);
            _events.Add($"deployment:{json.GetProperty("environment").GetString()}");
            return new HttpResponseMessage(_requests.Count == failAtRequest ? HttpStatusCode.Forbidden : HttpStatusCode.Created)
            {
                Content = new StringContent(_requests.Count == failAtRequest ? "{\"message\":\"denied\"}" : "{\"id\":123}", Encoding.UTF8, "application/json"),
            };
        }));

    private static PromoteAppCommand CreateCommand(HttpClientAdapter http, ContainerService containers)
    {
        var github = new GitHubClient(new Connection(new ProductHeaderValue("repoctl-tests"), http));
        return new PromoteAppCommand(new GitHubService(null!, github), containers);
    }

    private static GitHubRepositoryContext RepositoryContext()
        => new() { RepositoryOwner = "owner", RepositoryName = "repo" };

    private static async Task<int> Run(ICommandResult result)
    {
        await using var application = CliApplication.CreateBuilder("Deploy tests").Build();
        application.AddCommand("deploy", "Deploy", () => result);
        return await application.RunAsync(["deploy"], TestContext.Current.CancellationToken);
    }

    private sealed class DeploymentHandler(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> send)
        : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => send(request, cancellationToken);
    }

    private sealed class RecordingContainerService(List<string> events, params int[] exitCodes)
        : ContainerService
    {
        private int _promotionCount;

        public override ICommandResult PromoteManifests(AltinnRepository repository, string image, string sourceTag, IReadOnlyList<string> environments)
            => new CallbackResult(context =>
            {
                sourceTag.ShouldBe("abc123");
                events.Add($"promote:{image}");
                var exitCode = exitCodes.Length == 0 ? 0 : exitCodes[_promotionCount++];
                // Match CommandResult: a successful process leaves the existing return code intact.
                if (exitCode != 0)
                {
                    context.ReturnCode = exitCode;
                }
            });
    }

    private sealed class CallbackResult(Action<CommandInvocationContext> callback)
        : ICommandResult
    {
        public Task Execute(CommandInvocationContext context, CancellationToken cancellationToken = default)
        {
            callback(context);
            return Task.CompletedTask;
        }
    }
}
