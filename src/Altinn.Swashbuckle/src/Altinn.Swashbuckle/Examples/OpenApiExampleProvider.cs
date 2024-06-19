using CommunityToolkit.Diagnostics;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Any;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

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
    /// <returns>An enumerable of <see cref="IOpenApiAny"/> example data.</returns>
    public IEnumerable<IOpenApiAny>? GetExample<T>()
        => GetExample(typeof(T));

    /// <summary>
    /// Gets an example for the specified type.
    /// </summary>
    /// <param name="type">The type to get an example for.</param>
    /// <returns>An enumerable of <see cref="IOpenApiAny"/> example data.</returns>
    public IEnumerable<IOpenApiAny>? GetExample(Type type)
    {
        var exampleDataOptions = _exampleDataOptions.CurrentValue ?? ExampleDataOptions.DefaultOptions;
        var jsonOptions = _jsonOptions.CurrentValue?.SerializerOptions ?? new JsonSerializerOptions(JsonSerializerDefaults.Web);

        return GetExample(type, exampleDataOptions, jsonOptions);
    }

    private IEnumerable<IOpenApiAny>? GetExample(Type type, ExampleDataOptions exampleDataOptions, JsonSerializerOptions jsonSerializerOptions)
    {
        var examples = ExampleData.GetExamples(type, exampleDataOptions);
        if (examples is null)
        {
            return null;
        }

        return ConvertExamples(examples, type, jsonSerializerOptions);
    }

    private static IEnumerable<IOpenApiAny> ConvertExamples(IEnumerable examples, Type type, JsonSerializerOptions jsonSerializerOptions)
    {
        foreach (var example in examples)
        {
            var doc = JsonSerializer.SerializeToDocument(example, type, jsonSerializerOptions);
            if (doc is null)
            {
                continue;
            }

            yield return CreateFromJson(doc.RootElement);
        }
    }

    private static IOpenApiAny CreateFromJson(JsonElement element)
        => element.ValueKind switch
        {
            JsonValueKind.Null => new OpenApiNull(),
            JsonValueKind.True => new OpenApiBoolean(true),
            JsonValueKind.False => new OpenApiBoolean(false),
            JsonValueKind.Number => CreateFromNumber(element),
            JsonValueKind.String => new OpenApiString(element.GetString() ?? string.Empty),
            JsonValueKind.Array => CreateFromArray(element),
            JsonValueKind.Object => CreateFromObject(element),
            _ => ThrowHelper.ThrowArgumentOutOfRangeException<IOpenApiAny>(nameof(element), "Invalid JsonValueKind"),
        };

    private static IOpenApiAny CreateFromNumber(JsonElement element)
    {
        if (element.TryGetInt32(out int intValue))
            return new OpenApiInteger(intValue);

        if (element.TryGetInt64(out long longValue))
            return new OpenApiLong(longValue);

        if (element.TryGetSingle(out float floatValue) && !float.IsInfinity(floatValue))
            return new OpenApiFloat(floatValue);

        if (element.TryGetDouble(out double doubleValue))
            return new OpenApiDouble(doubleValue);

        return ThrowHelper.ThrowArgumentException<IOpenApiAny>(nameof(element), "Could not get value from number element");
    }

    [SuppressMessage("Performance", "CA1859:Use concrete types when possible for improved performance", Justification = "Concrete type is an implementation detail")]
    private static IOpenApiAny CreateFromArray(JsonElement element)
    {
        var array = new OpenApiArray();
        foreach (var item in element.EnumerateArray())
        {
            array.Add(CreateFromJson(item));
        }

        return array;
    }

    [SuppressMessage("Performance", "CA1859:Use concrete types when possible for improved performance", Justification = "Concrete type is an implementation detail")]
    private static IOpenApiAny CreateFromObject(JsonElement element)
    {
        var obj = new OpenApiObject();
        foreach (var property in element.EnumerateObject())
        {
            obj[property.Name] = CreateFromJson(property.Value);
        }

        return obj;
    }
}
