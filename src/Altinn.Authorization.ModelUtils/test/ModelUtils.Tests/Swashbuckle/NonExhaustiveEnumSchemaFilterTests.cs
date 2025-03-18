using Altinn.Authorization.ModelUtils.Swashbuckle;
using Altinn.Authorization.ModelUtils.Tests.Utils;
using Altinn.Swashbuckle.XmlDoc;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Text.Json;

namespace Altinn.Authorization.ModelUtils.Tests.Swashbuckle;

public class NonExhaustiveEnumSchemaFilterTests
{
    [Theory]
    [Enums.EnumTypes]
    public void Apply_ChangesToOneOf(Enums.EnumModel enumModel)
    {
        var schema = new OpenApiSchema
        {
            Properties = new Dictionary<string, OpenApiSchema>(),
        };

        var filterContext = new SchemaFilterContext(typeof(NonExhaustiveEnum<>).MakeGenericType(enumModel.Type), null, null, null);
        Subject.Apply(schema, filterContext);

        schema.Example.ShouldBeNull();
        schema.Type.ShouldBeNull();
        schema.Enum.ShouldBeNull();
        schema.Properties.ShouldBeNull();
        schema.OneOf.ShouldNotBeNull();
        schema.OneOf.Count.ShouldBe(enumModel.Cases.Length + 1);
    }

    private IXmlDocProvider Provider { get; } = new DefaultXmlDocProvider();

    private ISchemaFilter Subject => new NonExhaustiveEnumSchemaFilter(NullServices.Instance, Provider);
}
