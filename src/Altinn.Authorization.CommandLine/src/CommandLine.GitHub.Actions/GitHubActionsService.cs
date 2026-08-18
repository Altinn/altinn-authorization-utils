namespace Altinn.Authorization.CommandLine.GitHub.Actions;

internal sealed class GitHubActionsService
    : IGitHubActionsService
{
    private static readonly bool _isGitHubActions
        = Environment.GetEnvironmentVariable("GITHUB_ACTIONS") is "true";

    public bool IsGitHubActions
        => _isGitHubActions;

    public Task SetOutput(string key, string value, CancellationToken cancellationToken = default)
        => WriteValue("GITHUB_OUTPUT", key, value, cancellationToken);

    private static Task WriteValue(string fileEnvName, string key, string value, CancellationToken cancellationToken)
    {
        var file = Environment.GetEnvironmentVariable(fileEnvName);
        if (string.IsNullOrEmpty(file))
        {
            throw new InvalidOperationException($"Environment variable '{fileEnvName}' is not set.");
        }

        return ValueWriter.WriteValue(file, key, value, cancellationToken);
    }
}
