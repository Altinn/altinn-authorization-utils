using System.Reflection;
using Altinn.Authorization.TestUtils.AspNetCore;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extensions for <see cref="IMvcBuilder"/>.
/// </summary>
public static class TestClientMvcBuilderExtensions
{
    extension(IMvcBuilder builder)
    {
        /// <summary>
        /// Adds a test controller to the MVC builder.
        /// </summary>
        /// <typeparam name="TController">The controller type.</typeparam>
        /// <returns><paramref name="builder"/>.</returns>
        public IMvcBuilder AddTestController<TController>()
            where TController : class
        {
            TypeInfo type = typeof(TController).GetTypeInfo();

            builder.PartManager.ApplicationParts.Add(new TestControllerApplicationPart(type));
            return builder.AddTestControllerFeatureProvider();
        }

        private IMvcBuilder AddTestControllerFeatureProvider()
        {
            if (!builder.PartManager.FeatureProviders.Contains(TestControllerApplicationFeatureProvider.Instance))
            {
                builder.PartManager.FeatureProviders.Add(TestControllerApplicationFeatureProvider.Instance);
            }

            return builder;
        }
    }
}
