using Altinn.Authorization.RepoCtl.GitHub;
using CommunityToolkit.Diagnostics;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Octokit;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods for registering RepoCtl services.
/// </summary>
public static class RepoCtlGitHubServiceCollectionExtensions
{
    /// <param name="services">The service collection.</param>
    extension(IServiceCollection services)
    {
        /// <summary>
        /// Adds the RepoCtl GitHub services to the service collection.
        /// </summary>
        /// <returns>The service collection.</returns>
        public IServiceCollection AddRepoCtlGitHubServices()
        {
            services.TryAddSingleton<IChangedFilesService, GitHubChangedFilesService>();
            services.AddSingleton(_ =>
            {
                var githubClient = new GitHubClient(new ProductHeaderValue("repoctl"));
                var tokenString = Environment.GetEnvironmentVariable("GITHUB_TOKEN");
                if (string.IsNullOrEmpty(tokenString))
                {
                    ThrowHelper.ThrowInvalidOperationException("GITHUB_TOKEN environment variable is not set.");
                }

                githubClient.Credentials = new Credentials(tokenString);
                return githubClient;
            });

            return services;
        }
    }
}
