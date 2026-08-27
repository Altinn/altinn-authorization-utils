using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.Options;
using System.Collections;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Altinn.Swashbuckle.Examples;

/// <summary>
/// A provider for OpenAPI examples.
/// </summary>
public sealed class OpenApiExampleProvider
{
    private readonly IOptionsMonitor<ExampleDataOptions> _exampleDataOptions;
    private readonly IOptionsMonitor<JsonOptions> _jsonOptions;

    /// <summary>
    /// Initializes a new instance of the <see cref="OpenApiExampleProvider"/> class.
    /// </summary>
    public OpenApiExampleProvider(
        IOptionsMonitor<ExampleDataOptions> exampleDataOptions,
        IOptionsMonitor<JsonOptions> jsonOptions)
    {
        _exampleDataOptions = exampleDataOptions;
        _jsonOptions = jsonOptions;
    }

    /// <summary>
    /// Gets an example for the specified type.
    /// </summary>
    /// <typeparam name="T">The type to get an example for.</typeparam>
    /// <returns>An enumerable of example data values.</returns>
    public IEnumerable<JsonNode?>? GetExample<T>()
        => GetExample(typeof(T));

    /// <summary>
    /// Gets an example for the specified type mapped by an optional mapper.
    /// </summary>
    /// <typeparam name="T">The type to get an example for.</typeparam>
    /// <typeparam name="U">The mapped type.</typeparam>
    /// <param name="mapper">A mapper to apply to all example items.</param>
    /// <returns>An enumerable of example data values.</returns>
    public IEnumerable<JsonNode?>? GetExample<T, U>(Func<T, U> mapper)
        => GetExample(typeof(T), typeof(U), x => x is null ? null : mapper((T)x));

    /// <summary>
    /// Gets an example for the specified type.
    /// </summary>
    /// <param name="type">The type to get an example for.</param>
    /// <returns>An enumerable of example data values.</returns>
    public IEnumerable<JsonNode?>? GetExample(Type type)
    {
        var exampleDataOptions = _exampleDataOptions.CurrentValue ?? ExampleDataOptions.DefaultOptions;
        var jsonOptions = _jsonOptions.CurrentValue?.SerializerOptions ?? new JsonSerializerOptions(JsonSerializerDefaults.Web);

        return GetExample(type, type, mapper: null, exampleDataOptions, jsonOptions);
    }

    /// <summary>
    /// Gets an example for the specified type mapped by an optional mapper.
    /// </summary>
    /// <param name="type">The type to get an example for.</param>
    /// <param name="mappedType">The type produced by the mapper.</param>
    /// <param name="mapper">A mapper to apply to all example items.</param>
    /// <returns>An enumerable of example data values.</returns>
    public IEnumerable<JsonNode?>? GetExample(Type type, Type mappedType, Func<object?, object?> mapper)
    {
        var exampleDataOptions = _exampleDataOptions.CurrentValue ?? ExampleDataOptions.DefaultOptions;
        var jsonOptions = _jsonOptions.CurrentValue?.SerializerOptions ?? new JsonSerializerOptions(JsonSerializerDefaults.Web);

        return GetExample(type, mappedType, mapper, exampleDataOptions, jsonOptions);
    }

    private static IEnumerable<JsonNode?>? GetExample(Type type, Type mappedType, Func<object?, object?>? mapper, ExampleDataOptions exampleDataOptions, JsonSerializerOptions jsonSerializerOptions)
    {
        var examples = ExampleData.GetExamples(type, exampleDataOptions);
        if (examples is null)
        {
            return null;
        }

        if (mapper is not null)
        {
            examples = examples.Cast<object>().Select(mapper);
        }

        return ConvertExamples(examples, mappedType, jsonSerializerOptions);
    }

    private static IEnumerable<JsonNode?> ConvertExamples(IEnumerable examples, Type type, JsonSerializerOptions jsonSerializerOptions)
    {
        foreach (var example in examples)
        {
            yield return JsonSerializer.SerializeToNode(example, type, jsonSerializerOptions);
        }
    }
}
