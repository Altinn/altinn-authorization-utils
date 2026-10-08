namespace Altinn.Authorization.CommandLine.GitHub.Actions;

/// <summary>
/// Defines a service for interacting with GitHub Actions.
/// </summary>
public interface IGitHubActionsService
{
    /// <summary>
    /// Gets a value indicating whether the current process is running in a GitHub Actions environment.
    /// </summary>
    public bool IsGitHubActions { get; }

    /// <summary>
    /// Gets the GitHub Actions context.
    /// </summary>
    public GitHubContext? Context { get; }

    /// <summary>
    /// Sets an output value for the current GitHub Actions workflow.
    /// </summary>
    /// <param name="key">The output key.</param>
    /// <param name="value">The output value.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    public Task SetOutput(string key, string value, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets an environment variable for the current GitHub Actions workflow.
    /// </summary>
    /// <param name="key">The environment variable key.</param>
    /// <param name="value">The environment variable value.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    public Task SetEnvironmentVariable(string key, string value, CancellationToken cancellationToken = default);
}
