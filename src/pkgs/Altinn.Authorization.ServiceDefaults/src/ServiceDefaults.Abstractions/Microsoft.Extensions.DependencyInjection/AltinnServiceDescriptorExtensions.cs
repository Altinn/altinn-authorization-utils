using System.Diagnostics.CodeAnalysis;
using Altinn.Authorization.ServiceDefaults;
using CommunityToolkit.Diagnostics;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extensions for retrieving the configured Altinn service descriptor.
/// </summary>
public static class AltinnServiceDescriptorExtensions
{
    extension(IServiceCollection services)
    {
        /// <summary>
        /// Gets the configured <see cref="AltinnServiceDescriptor"/>.
        /// </summary>
        /// <returns>The <see cref="AltinnServiceDescriptor"/>.</returns>
        public AltinnServiceDescriptor GetAltinnServiceDescriptor()
        {
            if (services.TryFindAltinnServiceDescription(out var serviceDescription))
            {
                return serviceDescription;
            }

            return ThrowHelper.ThrowInvalidOperationException<AltinnServiceDescriptor>("Service of type AltinnServiceDescription not registered - did you forget to call AddAltinnServiceDefaults?");
        }

        internal bool TryFindAltinnServiceDescription([NotNullWhen(true)] out AltinnServiceDescriptor? serviceDescription)
        {
            var descriptor = services.SingleOrDefault(s => s.ServiceType == typeof(AltinnServiceDescriptor));
            if (descriptor is null)
            {
                serviceDescription = null;
                return false;
            }

            if (descriptor.Lifetime != ServiceLifetime.Singleton)
            {
                ThrowHelper.ThrowInvalidOperationException("Service of type AltinnServiceDescription registered as non-singleton");
            }

            if (descriptor.ImplementationInstance is AltinnServiceDescriptor instance)
            {
                serviceDescription = instance;
                return true;
            }

            serviceDescription = null;
            return ThrowHelper.ThrowInvalidOperationException<bool>("Service of type AltinnServiceDescription registered without an instance");
        }
    }

}
