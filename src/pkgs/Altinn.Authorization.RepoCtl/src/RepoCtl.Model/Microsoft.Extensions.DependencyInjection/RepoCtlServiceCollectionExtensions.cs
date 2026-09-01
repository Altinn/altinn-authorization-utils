using Altinn.Authorization.RepoCtl.Model;
using Altinn.Authorization.RepoCtl.Model.MsBuild;

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
            services.AddSingleton<IAltinnRepositoryLoader, AltinnRepositoryLoader>();
            services.AddSingleton<MsBuildContextFactory>();

            return services;
        }
    }
}
