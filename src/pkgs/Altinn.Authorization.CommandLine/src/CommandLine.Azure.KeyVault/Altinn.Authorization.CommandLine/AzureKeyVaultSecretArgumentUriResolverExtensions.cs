using Altinn.Authorization.CommandLine.Azure.KeyVault;
using Altinn.Authorization.CommandLine.PreProcessing;
using Microsoft.Extensions.DependencyInjection;

namespace Altinn.Authorization.CommandLine;

/// <summary>
/// Extension methods for registering the <see cref="AzureKeyVaultSecretArgumentUriResolver"/> in the dependency injection container.
/// </summary>
public static class AzureKeyVaultSecretArgumentUriResolverExtensions
{
    extension(IServiceCollection services)
    {
        /// <summary>
        /// Adds a <see cref="IArgumentUriResolver"/> that resolves <c>kv://</c> URIs to secrets stored in Azure Key Vault.
        /// </summary>
        /// <returns><paramref name="services"/>.</returns>
        public IServiceCollection AddAzureKeyVaultSecretArgumentUriResolver()
            => services.AddArgumentUriResolver<AzureKeyVaultSecretArgumentUriResolver>();
    }
}
