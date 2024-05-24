using Altinn.Swashbuckle.Examples;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods for adding <see cref="ExampleDataOptions"/> to the <see cref="IServiceCollection"/>.
/// </summary>
public static class ExampleDataProviderDependencyInjectionExtensions
{
    /// <summary>
    /// Add <see cref="ExampleDataOptions"/> to the <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/>.</param>
    /// <returns><paramref name="services"/>.</returns>
    public static OptionsBuilder<ExampleDataOptions> AddExampleDataOptions(this IServiceCollection services)
    {
        var builder = services.AddOptions<ExampleDataOptions>();
        services.TryAddTransient<IOptionsFactory<ExampleDataOptions>, ExampleDataOptionsFactory>();

        return builder;
    }

    /// <summary>
    /// Add a named <see cref="ExampleDataOptions"/> to the <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/>.</param>
    /// <param name="name">The options name.</param>
    /// <returns><paramref name="services"/>.</returns>
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
