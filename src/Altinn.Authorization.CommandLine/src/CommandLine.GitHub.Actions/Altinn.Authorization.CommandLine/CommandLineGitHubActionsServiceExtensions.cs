using Altinn.Authorization.CommandLine.GitHub.Actions;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;

namespace Altinn.Authorization.CommandLine;

/// <summary>
/// Extensions for adding GitHub Actions services to the service collection.
/// </summary>
public static class CommandLineGitHubActionsServiceExtensions
{
    extension(IServiceCollection services)
    {
        /// <summary>
        /// Adds GitHub Actions services to the service collection.
        /// </summary>
        /// <returns>The service collection.</returns>
        public IServiceCollection AddGitHubActionsServices()
        {
            services.AddSingleton<IGitHubActionsService, GitHubActionsService>();
            services.AddSingleton<IProfileEnricher, GitHubActionsProfileEnrichment>();

            return services;
        }
    }
}
