using Altinn.Swashbuckle.Examples;
using Altinn.Urn.Swashbuckle;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods for adding URN support to Swashbuckle.
/// </summary>
public static class UrnSwashbuckleDependencyInjectionExtensions
{
    /// <summary>
    /// Adds URN support to Swashbuckle.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns><paramref name="services"/>.</returns>
    public static IServiceCollection AddUrnSwaggerSupport(this IServiceCollection services)
    {
        services.AddSingleton<UrnExampleDataProviderResolver>();
        services.AddOpenApiExampleProvider()
            .Configure((ExampleDataOptions opts, UrnExampleDataProviderResolver resolver) =>
            {
                opts.ProviderResolverChain.Add(resolver);
            });
        services.AddSingleton<UrnSwaggerFilter>();
        services.AddSingleton<IConfigureOptions<SwaggerGenOptions>, UrnConfigureSwaggerGen>();

        return services;
    }
}
