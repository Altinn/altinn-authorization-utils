using Altinn.Swashbuckle.Examples;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.DependencyInjection;

public static class AltinnSwashbuckleServiceCollectionExtensions
{
    public static OptionsBuilder<ExampleDataOptions> AddOpenApiExampleProvider(this IServiceCollection services)
    {
        var builder = services.AddExampleDataOptions();

        services.TryAddSingleton<OpenApiExampleProvider>();

        return builder;
    }
}
