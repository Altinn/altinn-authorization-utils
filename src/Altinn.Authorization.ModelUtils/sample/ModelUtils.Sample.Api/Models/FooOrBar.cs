using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Altinn.Authorization.ModelUtils.Sample.Api.Models;

/// <summary>
/// Either "foo" or "bar".
/// </summary>
[JsonConverter(typeof(JsonConverter))]
[SwaggerSchemaFilter(typeof(SchemaFilter))]
public sealed class FooOrBar
{
    /// <summary>
    /// Gets the "foo" value.
    /// </summary>
    public static FooOrBar Foo { get; } = new FooOrBar(1);

    /// <summary>
    /// Gets the "bar" value.
    /// </summary>
    public static FooOrBar Bar { get; } = new FooOrBar(2);

    private readonly byte _value;

    private FooOrBar(byte value)
    {
        _value = value;
    }

    private sealed class JsonConverter
        : JsonConverter<FooOrBar>
    {
        public override FooOrBar? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var value = reader.GetString();

            return value switch
            {
                "foo" => Foo,
                "bar" => Bar,
                _ => throw new JsonException("Expected foo or bar"),
            };
        }

        public override void Write(Utf8JsonWriter writer, FooOrBar value, JsonSerializerOptions options)
        {
            switch (value._value)
            {
                case 1:
                    writer.WriteStringValue("foo");
                    break;
                case 2:
                    writer.WriteStringValue("bar");
                    break;
                default:
                    throw new UnreachableException();
            }
        }
    }

    private sealed class SchemaFilter
        : ISchemaFilter
    {
        public void Apply(OpenApiSchema schema, SchemaFilterContext context)
        {
            schema.Type = null;
            schema.Properties.Clear();
            schema.Required.Clear();

            schema.OneOf = [
                new OpenApiSchema { Type = "string", Enum = [ new OpenApiString("foo") ] },
                new OpenApiSchema { Type = "string", Enum = [ new OpenApiString("bar") ] },
            ];
        }
    }
}
