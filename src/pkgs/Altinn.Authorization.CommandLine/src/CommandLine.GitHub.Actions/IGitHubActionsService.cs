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
    /// Sets an output value for the current GitHub Actions workflow.
    /// </summary>
    /// <param name="key">The output key.</param>
    /// <param name="value">The output value.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    public Task SetOutput(string key, string value, CancellationToken cancellationToken = default);
}
