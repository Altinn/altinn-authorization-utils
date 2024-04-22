using Altinn.Swashbuckle.Examples;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.DependencyInjection;

public static class ExampleDataProviderDependencyInjectionExtensions
{
    public static OptionsBuilder<ExampleDataOptions> AddExampleDataOptions(this IServiceCollection services)
    {
        var builder = services.AddOptions<ExampleDataOptions>();
        services.TryAddTransient<IOptionsFactory<ExampleDataOptions>, ExampleDataOptionsFactory>();

        return builder;
    }

    public static OptionsBuilder<ExampleDataOptions> AddExampleDataOptions(this IServiceCollection services, string name)
    {
        var builder = services.AddOptions<ExampleDataOptions>(name);
        services.TryAddTransient<IOptionsFactory<ExampleDataOptions>, ExampleDataOptionsFactory>();

        return builder;
    }

    private class ExampleDataOptionsFactory : OptionsFactory<ExampleDataOptions>
    {
        public ExampleDataOptionsFactory(
            IEnumerable<IConfigureOptions<ExampleDataOptions>> setups, 
            IEnumerable<IPostConfigureOptions<ExampleDataOptions>> postConfigures, 
            IEnumerable<IValidateOptions<ExampleDataOptions>> validations) 
            : base(setups, postConfigures, validations)
        {
        }

        protected override ExampleDataOptions CreateInstance(string name)
            => new(ExampleDataOptions.DefaultOptions);
    }
}
