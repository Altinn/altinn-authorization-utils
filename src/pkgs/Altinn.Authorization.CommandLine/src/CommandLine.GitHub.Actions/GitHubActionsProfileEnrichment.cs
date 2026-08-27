using Spectre.Console;

namespace Altinn.Authorization.CommandLine.GitHub.Actions;

internal sealed class GitHubActionsProfileEnrichment(IGitHubActionsService githubActions)
    : IProfileEnricher
{
    private const int ACTIONS_CONSOLE_SIMULATED_WIDTH = 160;

    public string Name => "GitHub Actions";

    public bool Enabled(IDictionary<string, string> environmentVariables)
        => githubActions.IsGitHubActions;

    public void Enrich(Profile profile)
    {
        if (profile.Width < ACTIONS_CONSOLE_SIMULATED_WIDTH)
        {
            profile.Width = ACTIONS_CONSOLE_SIMULATED_WIDTH;
        }
    }
}
