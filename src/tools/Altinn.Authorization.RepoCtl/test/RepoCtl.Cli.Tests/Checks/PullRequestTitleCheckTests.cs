using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Altinn.Authorization.CommandLine.GitHub.Actions;
using Altinn.Authorization.RepoCtl.Checks;
using Altinn.Authorization.RepoCtl.Model.Checks;
using Octokit.Webhooks.Events.PullRequest;
using PullRequest = Octokit.Webhooks.Models.PullRequestEvent.PullRequest;

namespace Altinn.Authorization.RepoCtl.Tests.Checks;

public class PullRequestTitleCheckTests
{
    [Theory]
    [InlineData("feat: add login")]
    [InlineData("fix(auth): handle expired tokens")]
    [InlineData("feat!: remove legacy API")]
    [InlineData("refactor(auth)!: replace token validation")]
    [InlineData("chore(deps): update dependencies")]
    [InlineData("fix(access-management): handle expired tokens")]
    [InlineData("feat(a-b-c)!: replace API")]
    [InlineData("custom: support additional commit types")]
    public async Task Check_AcceptsConventionalTitles(string title)
    {
        var diagnostics = await Check(title);
        diagnostics.ShouldBeEmpty();
    }

    [Theory]
    [InlineData("")]
    [InlineData("Fix login")]
    [InlineData("Feat: add login")]
    [InlineData("fix:missing space")]
    [InlineData("fix: ")]
    [InlineData("fix:   ")]
    [InlineData("fix(): add login")]
    [InlineData("fix(Auth): add login")]
    [InlineData("fix(auth2): add login")]
    [InlineData("fix(auth_api): add login")]
    [InlineData("fix(auth api): add login")]
    [InlineData("fix(-auth): add login")]
    [InlineData("fix(auth-): add login")]
    [InlineData("fix(auth--api): add login")]
    [InlineData("fix(auth: add login")]
    [InlineData("fix(auth)!!: add login")]
    [InlineData("fix: add login\n")]
    [InlineData("fix: add\nlogin")]
    public async Task Check_ReportsInvalidTitles(string title)
    {
        var diagnostics = await Check(title);
        var diagnostic = diagnostics.ShouldHaveSingleItem();
        diagnostic.Code.ShouldBe("REPOCTL016");
        diagnostic.Severity.ShouldBe(DiagnosticSeverity.Error);
        diagnostic.Text.ShouldContain("type(scope)!: description");
    }

    [Theory]
    [InlineData("pull_request")]
    [InlineData("pull_request_target")]
    public async Task Check_RunsForPullRequestEvents(string eventName)
        => (await Check("invalid title", eventName)).ShouldHaveSingleItem();

    [Theory]
    [InlineData(null)]
    [InlineData("push")]
    public async Task Check_SkipsOutsidePullRequestContext(string? eventName)
    {
        var context = eventName is null ? null : Context(eventName);
        var check = new PullRequestTitleCheck(new Actions(context));
        var diagnostics = await check.Check(null!, TestContext.Current.CancellationToken).ToListAsync(TestContext.Current.CancellationToken);
        diagnostics.ShouldBeEmpty();
    }

    private static async Task<List<Diagnostic>> Check(string title, string eventName = "pull_request")
    {
        // Only the title is relevant to this check; omit the remaining webhook fields.
        var resolver = new DefaultJsonTypeInfoResolver();
        resolver.Modifiers.Add(info =>
        {
            foreach (var property in info.Properties)
            {
                property.IsRequired = false;
            }
        });
        var pullRequest = JsonSerializer.Deserialize<PullRequest>(
            JsonSerializer.Serialize(new { title }),
            new JsonSerializerOptions { TypeInfoResolver = resolver })!;
        var context = Context(eventName) with
        {
            Event = new PullRequestOpenedEvent { Number = 42, PullRequest = pullRequest, Repository = null!, Sender = null! },
        };
        var check = new PullRequestTitleCheck(new Actions(context));
        return await check.Check(null!, TestContext.Current.CancellationToken).ToListAsync(TestContext.Current.CancellationToken);
    }

    private static GitHubContext Context(string eventName)
        => new()
        {
            EventName = eventName,
            Event = null!,
            Repository = new() { RepositoryOwner = "owner", RepositoryName = "repo" },
            Sha = "abc",
            Ref = null,
        };

    private sealed class Actions(GitHubContext? context) : IGitHubActionsService
    {
        public bool IsGitHubActions => context is not null;

        public GitHubContext? Context => context;

        public Task SetOutput(string key, string value, CancellationToken cancellationToken = default) => throw new NotSupportedException();

        public Task SetEnvironmentVariable(string key, string value, bool print, CancellationToken cancellationToken = default) => throw new NotSupportedException();
    }
}
