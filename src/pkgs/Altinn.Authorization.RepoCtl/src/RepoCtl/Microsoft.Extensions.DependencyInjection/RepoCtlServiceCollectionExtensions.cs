using Altinn.Authorization.RepoCtl;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods for registering RepoCtl services.
/// </summary>
public static class RepoCtlServiceCollectionExtensions
{
    /// <param name="services">The service collection.</param>
    extension(IServiceCollection services)
    {
        /// <summary>
        /// Adds the RepoCtl services to the service collection.
        /// </summary>
        /// <returns>The service collection.</returns>
        public IServiceCollection AddRepoCtlServices()
        {
            services.TryAddSingleton<IAltinnRepositoryLoader, AltinnRepositoryLoader>();
            services.AddRepoCtlMsBuildServices();

            return services;
        }
    }
}
