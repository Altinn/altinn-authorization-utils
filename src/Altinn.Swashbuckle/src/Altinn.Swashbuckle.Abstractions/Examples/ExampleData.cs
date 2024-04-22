using System.Collections;

namespace Altinn.Swashbuckle.Examples;

public static class ExampleData 
{
    public static ExampleDataOptions DefaultOptions => ExampleDataOptions.DefaultOptions;

    public static IEnumerable<T>? GetExamples<T>(ExampleDataOptions? options = null)
        => GetExamples(typeof(T), options)?.Cast<T>();

    public static IEnumerable? GetExamples(Type type, ExampleDataOptions? options = null)
    {
        options ??= DefaultOptions;

        var provider = options.GetProvider(type);

        return provider?.GetExamples(type, options);
    }
}
