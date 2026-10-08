using System.Text.Json;
using Octokit.Webhooks;

namespace Altinn.Authorization.CommandLine.GitHub.Actions;

internal sealed class GitHubActionsService
    : IGitHubActionsService
{
    private static readonly bool _isGitHubActions
        = Environment.GetEnvironmentVariable("GITHUB_ACTIONS") is "true";

    private static readonly Lazy<GitHubContext?> _context
        = new(CreateContext);

    public bool IsGitHubActions
        => _isGitHubActions;

    public GitHubContext? Context
        => _context.Value;

    public Task SetOutput(string key, string value, CancellationToken cancellationToken = default)
        => WriteValue("GITHUB_OUTPUT", key, value, cancellationToken);

    public Task SetEnvironmentVariable(string key, string value, CancellationToken cancellationToken = default)
        => WriteValue("GITHUB_ENV", key, value, cancellationToken);

    private static Task WriteValue(string fileEnvName, string key, string value, CancellationToken cancellationToken)
    {
        var file = Environment.GetEnvironmentVariable(fileEnvName);
        if (string.IsNullOrEmpty(file))
        {
            throw new InvalidOperationException($"Environment variable '{fileEnvName}' is not set.");
        }

        return ValueWriter.WriteValue(file, key, value, cancellationToken);
    }

    private static GitHubContext? CreateContext()
    {
        if (!_isGitHubActions)
        {
            return null;
        }

        var eventName = GetRequiredEnvironmentVariable("GITHUB_EVENT_NAME");
        var repository = GetRequiredEnvironmentVariable("GITHUB_REPOSITORY");
        var separator = repository.IndexOf('/');
        if (separator <= 0 || separator == repository.Length - 1 || repository.IndexOf('/', separator + 1) >= 0)
        {
            throw new InvalidOperationException("Environment variable 'GITHUB_REPOSITORY' must contain an owner and repository name separated by '/'.");
        }

        var eventPath = GetRequiredEnvironmentVariable("GITHUB_EVENT_PATH");
        var payload = File.ReadAllText(eventPath);
        var gitRef = Environment.GetEnvironmentVariable("GITHUB_REF");

        WebhookEvent? parsedEvent = null;
        try
        {
            parsedEvent = EventDeserializer.Deserialize(eventName, payload);
        }
        catch (JsonException)
        {
            // we ignore JSON parsing errors and leave parsedEvent as null
        }

        return new GitHubContext
        {
            EventName = eventName,
            Event = parsedEvent,
            Repository = new GitHubRepositoryContext
            {
                RepositoryOwner = repository[..separator],
                RepositoryName = repository[(separator + 1)..],
            },
            Sha = GetRequiredEnvironmentVariable("GITHUB_SHA"),
            Ref = string.IsNullOrEmpty(gitRef) ? null : gitRef,
        };

        static string GetRequiredEnvironmentVariable(string name)
        {
            var value = Environment.GetEnvironmentVariable(name);
            if (string.IsNullOrEmpty(value))
            {
                throw new InvalidOperationException($"Environment variable '{name}' is not set.");
            }

            return value;
        }
    }
}
