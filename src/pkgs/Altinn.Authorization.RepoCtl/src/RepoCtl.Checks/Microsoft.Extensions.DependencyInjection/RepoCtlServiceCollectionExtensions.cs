using Altinn.Authorization.RepoCtl.Checks;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods for registering RepoCtl services.
/// </summary>
public static class RepoCtlChecksServiceCollectionExtensions
{
    /// <param name="services">The service collection.</param>
    extension(IServiceCollection services)
    {
        /// <summary>
        /// Adds the RepoCtl services to the service collection.
        /// </summary>
        /// <returns>The service collection.</returns>
        public IServiceCollection AddRepoCtlChecks()
        {
            services.TryAddEnumerable(ServiceDescriptor.Singleton<IRepositoryCheck, LoaderCheck>());
            services.TryAddEnumerable(ServiceDescriptor.Singleton<IRepositoryCheck, PackableProjectCheck>());
            services.TryAddEnumerable(ServiceDescriptor.Singleton<IRepositoryCheck, DotnetFormatCheck>());

            return services;
        }
    }
}
