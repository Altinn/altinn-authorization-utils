using Altinn.Swashbuckle.Examples;
using Altinn.Urn.Swashbuckle;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Microsoft.Extensions.DependencyInjection;

public static class UrnSwashbuckleDependencyInjectionExtensions
{
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
