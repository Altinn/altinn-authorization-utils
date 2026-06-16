using Altinn.Swashbuckle.Tests.Fixtures;
using Altinn.Swashbuckle.XmlDoc;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Globalization;

namespace Altinn.Swashbuckle.Tests.XmlDoc;

public class XmlDocSchemaFilterTests
    : XmlDocFilterTestsBase
{
    [Theory]
    [InlineData(typeof(XmlAnnotatedType), "Summary for XmlAnnotatedType")]
    [InlineData(typeof(XmlAnnotatedType.NestedType), "Summary for NestedType")]
    [InlineData(typeof(XmlAnnotatedGenericType<int, string>), "Summary for XmlAnnotatedGenericType")]
    public void Apply_SetsDescription_FromTypeSummaryTag(
            Type type,
            string expectedDescription)
    {
        var schema = new OpenApiSchema { };
        var filterContext = new SchemaFilterContext(type, null, null);

        Subject.Apply(schema, filterContext);

        schema.Description.ShouldBe(expectedDescription);
    }

    [Fact]
    public void Apply_SetsDescription_FromFieldSummaryTag()
    {
        var fieldInfo = typeof(XmlAnnotatedType).GetField(nameof(XmlAnnotatedType.BoolField))!;
        var schema = new OpenApiSchema { };
        var filterContext = new SchemaFilterContext(fieldInfo.FieldType, null, null, memberInfo: fieldInfo);

        Subject.Apply(schema, filterContext);

        schema.Description.ShouldBe("Summary for BoolField");
    }

    [Theory]
    [InlineData(typeof(XmlAnnotatedType), nameof(XmlAnnotatedType.StringProperty), "Summary for StringProperty")]
    [InlineData(typeof(XmlAnnotatedSubType), nameof(XmlAnnotatedType.StringProperty), "Summary for StringProperty")]
    [InlineData(typeof(XmlAnnotatedGenericType<string, bool>), "GenericProperty", "Summary for GenericProperty")]
    public void Apply_SetsDescription_FromPropertySummaryTag(
        Type declaringType,
        string propertyName,
        string expectedDescription)
    {
        var propertyInfo = declaringType.GetProperty(propertyName)!;
        var schema = new OpenApiSchema();
        var filterContext = new SchemaFilterContext(propertyInfo.PropertyType, null, null, memberInfo: propertyInfo);

        Subject.Apply(schema, filterContext);

        schema.Description.ShouldBe(expectedDescription);
    }

    [Theory]
    [InlineData(typeof(XmlAnnotatedType), nameof(XmlAnnotatedType.BoolProperty), JsonSchemaType.Boolean, "true")]
    [InlineData(typeof(XmlAnnotatedType), nameof(XmlAnnotatedType.IntProperty), JsonSchemaType.Integer, "10")]
    [InlineData(typeof(XmlAnnotatedType), nameof(XmlAnnotatedType.LongProperty), JsonSchemaType.Integer, "4294967295")]
    [InlineData(typeof(XmlAnnotatedType), nameof(XmlAnnotatedType.FloatProperty), JsonSchemaType.Number, "1.2")]
    [InlineData(typeof(XmlAnnotatedType), nameof(XmlAnnotatedType.DoubleProperty), JsonSchemaType.Number, "1.25")]
    [InlineData(typeof(XmlAnnotatedType), nameof(XmlAnnotatedType.DateTimeProperty), JsonSchemaType.String, "\"6/22/2022 12:00:00 AM\"")]
    [InlineData(typeof(XmlAnnotatedType), nameof(XmlAnnotatedType.EnumProperty), JsonSchemaType.Integer, "2")]
    [InlineData(typeof(XmlAnnotatedType), nameof(XmlAnnotatedType.GuidProperty), JsonSchemaType.String, "\"d3966535-2637-48fa-b911-e3c27405ee09\"")]
    [InlineData(typeof(XmlAnnotatedType), nameof(XmlAnnotatedType.StringProperty), JsonSchemaType.String, "\"Example for StringProperty\"")]
    [InlineData(typeof(XmlAnnotatedType), nameof(XmlAnnotatedType.ObjectProperty), JsonSchemaType.Object, "{\"prop1\":1,\"prop2\":\"foobar\"}")]
    [InlineData(typeof(XmlAnnotatedType), nameof(XmlAnnotatedType.StringPropertyWithUri), JsonSchemaType.String, "\"https://test.com/a?b=1&c=2\"")]
    [InlineData(typeof(XmlAnnotatedRecord), nameof(XmlAnnotatedRecord.BoolProperty), JsonSchemaType.Boolean, "true")]
    [InlineData(typeof(XmlAnnotatedRecord), nameof(XmlAnnotatedRecord.IntProperty), JsonSchemaType.Integer, "10")]
    [InlineData(typeof(XmlAnnotatedRecord), nameof(XmlAnnotatedRecord.LongProperty), JsonSchemaType.Integer, "4294967295")]
    [InlineData(typeof(XmlAnnotatedRecord), nameof(XmlAnnotatedRecord.FloatProperty), JsonSchemaType.Number, "1.2")]
    [InlineData(typeof(XmlAnnotatedRecord), nameof(XmlAnnotatedRecord.DoubleProperty), JsonSchemaType.Number, "1.25")]
    [InlineData(typeof(XmlAnnotatedRecord), nameof(XmlAnnotatedRecord.DateTimeProperty), JsonSchemaType.String, "\"6/22/2022 12:00:00 AM\"")]
    [InlineData(typeof(XmlAnnotatedRecord), nameof(XmlAnnotatedRecord.EnumProperty), JsonSchemaType.Integer, "2")]
    [InlineData(typeof(XmlAnnotatedRecord), nameof(XmlAnnotatedRecord.GuidProperty), JsonSchemaType.String, "\"d3966535-2637-48fa-b911-e3c27405ee09\"")]
    [InlineData(typeof(XmlAnnotatedRecord), nameof(XmlAnnotatedRecord.StringProperty), JsonSchemaType.String, "\"Example for StringProperty\"")]
    [InlineData(typeof(XmlAnnotatedRecord), nameof(XmlAnnotatedRecord.ObjectProperty), JsonSchemaType.Object, "{\"prop1\":1,\"prop2\":\"foobar\"}")]
    [InlineData(typeof(XmlAnnotatedRecord), nameof(XmlAnnotatedRecord.StringPropertyWithUri), JsonSchemaType.String, "\"https://test.com/a?b=1&c=2\"")]
    public void Apply_SetsExample_FromPropertyExampleTag(
        Type declaringType,
        string propertyName,
        JsonSchemaType schemaType,
        string expectedExampleAsJson)
    {
        var propertyInfo = declaringType.GetProperty(propertyName)!;
        var schema = new OpenApiSchema { Type = schemaType };
        var filterContext = new SchemaFilterContext(propertyInfo.PropertyType, null, null, memberInfo: propertyInfo);

        Subject.Apply(schema, filterContext);

        schema.Example.ShouldNotBeNull();
        ToJsonString(schema.Example).ShouldBe(expectedExampleAsJson);
    }

    [Theory]
    [InlineData(typeof(XmlAnnotatedType), nameof(XmlAnnotatedType.StringPropertyWithNullExample), JsonSchemaType.String)]
    [InlineData(typeof(XmlAnnotatedRecord), nameof(XmlAnnotatedRecord.StringPropertyWithNullExample), JsonSchemaType.String)]
    public void Apply_SetsNullExample_WhenPropertyExampleTagIsNull(
        Type declaringType,
        string propertyName,
        JsonSchemaType schemaType)
    {
        // JsonNode represents JSON null as C# null, so schema.Example will be null
        // even though the <example>null</example> tag was present in the XML docs.
        var propertyInfo = declaringType.GetProperty(propertyName)!;
        var schema = new OpenApiSchema { Type = schemaType };
        var filterContext = new SchemaFilterContext(propertyInfo.PropertyType, null, null, memberInfo: propertyInfo);

        Subject.Apply(schema, filterContext);

        schema.Example.ShouldBeNull();
    }

    [Theory]
    [InlineData(typeof(XmlAnnotatedTypeWithoutExample), nameof(XmlAnnotatedTypeWithoutExample.BoolProperty), JsonSchemaType.Boolean)]
    [InlineData(typeof(XmlAnnotatedTypeWithoutExample), nameof(XmlAnnotatedTypeWithoutExample.IntProperty), JsonSchemaType.Integer)]
    [InlineData(typeof(XmlAnnotatedTypeWithoutExample), nameof(XmlAnnotatedTypeWithoutExample.LongProperty), JsonSchemaType.Integer)]
    [InlineData(typeof(XmlAnnotatedTypeWithoutExample), nameof(XmlAnnotatedTypeWithoutExample.FloatProperty), JsonSchemaType.Number)]
    [InlineData(typeof(XmlAnnotatedTypeWithoutExample), nameof(XmlAnnotatedTypeWithoutExample.DoubleProperty), JsonSchemaType.Number)]
    [InlineData(typeof(XmlAnnotatedTypeWithoutExample), nameof(XmlAnnotatedTypeWithoutExample.DateTimeProperty), JsonSchemaType.String)]
    [InlineData(typeof(XmlAnnotatedTypeWithoutExample), nameof(XmlAnnotatedTypeWithoutExample.EnumProperty), JsonSchemaType.Integer)]
    [InlineData(typeof(XmlAnnotatedTypeWithoutExample), nameof(XmlAnnotatedTypeWithoutExample.GuidProperty), JsonSchemaType.String)]
    [InlineData(typeof(XmlAnnotatedTypeWithoutExample), nameof(XmlAnnotatedTypeWithoutExample.StringProperty), JsonSchemaType.String)]
    [InlineData(typeof(XmlAnnotatedTypeWithoutExample), nameof(XmlAnnotatedTypeWithoutExample.ObjectProperty), JsonSchemaType.Object)]
    [InlineData(typeof(XmlAnnotatedTypeWithoutExample), nameof(XmlAnnotatedTypeWithoutExample.StringPropertyWithNullExample), JsonSchemaType.String)]
    [InlineData(typeof(XmlAnnotatedTypeWithoutExample), nameof(XmlAnnotatedTypeWithoutExample.StringPropertyWithUri), JsonSchemaType.String)]
    [InlineData(typeof(XmlAnnotatedRecordWithoutExample), nameof(XmlAnnotatedRecordWithoutExample.BoolProperty), JsonSchemaType.Boolean)]
    [InlineData(typeof(XmlAnnotatedRecordWithoutExample), nameof(XmlAnnotatedRecordWithoutExample.IntProperty), JsonSchemaType.Integer)]
    [InlineData(typeof(XmlAnnotatedRecordWithoutExample), nameof(XmlAnnotatedRecordWithoutExample.LongProperty), JsonSchemaType.Integer)]
    [InlineData(typeof(XmlAnnotatedRecordWithoutExample), nameof(XmlAnnotatedRecordWithoutExample.FloatProperty), JsonSchemaType.Number)]
    [InlineData(typeof(XmlAnnotatedRecordWithoutExample), nameof(XmlAnnotatedRecordWithoutExample.DoubleProperty), JsonSchemaType.Number)]
    [InlineData(typeof(XmlAnnotatedRecordWithoutExample), nameof(XmlAnnotatedRecordWithoutExample.DateTimeProperty), JsonSchemaType.String)]
    [InlineData(typeof(XmlAnnotatedRecordWithoutExample), nameof(XmlAnnotatedRecordWithoutExample.EnumProperty), JsonSchemaType.Integer)]
    [InlineData(typeof(XmlAnnotatedRecordWithoutExample), nameof(XmlAnnotatedRecordWithoutExample.GuidProperty), JsonSchemaType.String)]
    [InlineData(typeof(XmlAnnotatedRecordWithoutExample), nameof(XmlAnnotatedRecordWithoutExample.StringProperty), JsonSchemaType.String)]
    [InlineData(typeof(XmlAnnotatedRecordWithoutExample), nameof(XmlAnnotatedRecordWithoutExample.ObjectProperty), JsonSchemaType.Object)]
    [InlineData(typeof(XmlAnnotatedRecordWithoutExample), nameof(XmlAnnotatedRecordWithoutExample.StringPropertyWithNullExample), JsonSchemaType.String)]
    [InlineData(typeof(XmlAnnotatedRecordWithoutExample), nameof(XmlAnnotatedRecordWithoutExample.StringPropertyWithUri), JsonSchemaType.String)]
    public void Apply_DoesNotSetExample_WhenPropertyExampleTagIsNotProvided(
        Type declaringType,
        string propertyName,
        JsonSchemaType schemaType)
    {
        var propertyInfo = declaringType.GetProperty(propertyName)!;
        var schema = new OpenApiSchema { Type = schemaType };
        var filterContext = new SchemaFilterContext(propertyInfo.PropertyType, null, null, memberInfo: propertyInfo);

        Subject.Apply(schema, filterContext);

        schema.Example.ShouldBeNull();
    }

    [Theory]
    [InlineData("en-US", 1.2F)]
    [InlineData("sv-SE", 1.2F)]
    public void Apply_UsesInvariantCulture_WhenSettingExample(
        string cultureName,
        float expectedValue)
    {
        var propertyInfo = typeof(XmlAnnotatedType).GetProperty(nameof(XmlAnnotatedType.FloatProperty))!;
        var schema = new OpenApiSchema { Type = JsonSchemaType.Number, Format = "float" };
        var filterContext = new SchemaFilterContext(propertyInfo.PropertyType, null, null, memberInfo: propertyInfo);

        var defaultCulture = CultureInfo.CurrentCulture;
        CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(cultureName);

        Subject.Apply(schema, filterContext);

        CultureInfo.CurrentCulture = defaultCulture;

        schema.Example!.GetValue<float>().ShouldBe(expectedValue);
    }

    private ISchemaFilter Subject => new XmlDocSchemaFilter(Provider);
}
