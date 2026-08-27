using Altinn.Authorization.ModelUtils.AspNet;
using Altinn.Authorization.ModelUtils.Swashbuckle;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Altinn.Authorization.ModelUtils.Tests.Swashbuckle;

public class FlagsEnumSchemaFilterTests
{
    [Theory]
    [InlineData(typeof(FlagsEnum1))]
    [InlineData(typeof(FlagsEnum2))]
    public void Apply_ProducesStringArray(Type enumType)
    {
        // more or less how swashbuckle would generate the initial schema
        var schema = new OpenApiSchema
        {
            Type = JsonSchemaType.Object,
            Properties = new Dictionary<string, IOpenApiSchema>()
            {
                { "value", new OpenApiSchema { Type = JsonSchemaType.String } },
            },
        };

        var filterContext = new SchemaFilterContext(typeof(FlagsEnum<>).MakeGenericType(enumType), null, null, null);
        Subject.Apply(schema, filterContext);

        schema.Enum.ShouldBeNull();
        schema.Format.ShouldBeNull();
        schema.Properties.ShouldBeNull();

        schema.Type.ShouldBe(JsonSchemaType.Array);
        schema.Items.ShouldNotBeNull();
        schema.Items.Type.ShouldBe(JsonSchemaType.String);
    }

    private ISchemaFilter Subject => new FlagsEnumModelSchemaFilter();

    private enum FlagsEnum1
        : byte
    {
        None = default,
        A = 1 << 0,
        B = 1 << 1,
        C = 1 << 2,
    }

    private enum FlagsEnum2
        : ulong
    {
        None = default,
        A = 1 << 0,
        B = 1 << 1,
        C = 1 << 2,
    }
}
