using Altinn.Swashbuckle.Tests.Fixtures;
using Altinn.Swashbuckle.XmlDoc;
using Microsoft.OpenApi.Models;
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
    [InlineData(typeof(XmlAnnotatedType), nameof(XmlAnnotatedType.BoolProperty), "boolean", "true")]
    [InlineData(typeof(XmlAnnotatedType), nameof(XmlAnnotatedType.IntProperty), "integer", "10")]
    [InlineData(typeof(XmlAnnotatedType), nameof(XmlAnnotatedType.LongProperty), "integer", "4294967295")]
    [InlineData(typeof(XmlAnnotatedType), nameof(XmlAnnotatedType.FloatProperty), "number", "1.2")]
    [InlineData(typeof(XmlAnnotatedType), nameof(XmlAnnotatedType.DoubleProperty), "number", "1.25")]
    [InlineData(typeof(XmlAnnotatedType), nameof(XmlAnnotatedType.DateTimeProperty), "string", "\"6/22/2022 12:00:00 AM\"")]
    [InlineData(typeof(XmlAnnotatedType), nameof(XmlAnnotatedType.EnumProperty), "integer", "2")]
    [InlineData(typeof(XmlAnnotatedType), nameof(XmlAnnotatedType.GuidProperty), "string", "\"d3966535-2637-48fa-b911-e3c27405ee09\"")]
    [InlineData(typeof(XmlAnnotatedType), nameof(XmlAnnotatedType.StringProperty), "string", "\"Example for StringProperty\"")]
    [InlineData(typeof(XmlAnnotatedType), nameof(XmlAnnotatedType.ObjectProperty), "object", "{\n  \"prop1\": 1,\n  \"prop2\": \"foobar\"\n}")]
    [InlineData(typeof(XmlAnnotatedType), nameof(XmlAnnotatedType.StringPropertyWithNullExample), "string", "null")]
    [InlineData(typeof(XmlAnnotatedType), nameof(XmlAnnotatedType.StringPropertyWithUri), "string", "\"https://test.com/a?b=1&c=2\"")]
    [InlineData(typeof(XmlAnnotatedRecord), nameof(XmlAnnotatedRecord.BoolProperty), "boolean", "true")]
    [InlineData(typeof(XmlAnnotatedRecord), nameof(XmlAnnotatedRecord.IntProperty), "integer", "10")]
    [InlineData(typeof(XmlAnnotatedRecord), nameof(XmlAnnotatedRecord.LongProperty), "integer", "4294967295")]
    [InlineData(typeof(XmlAnnotatedRecord), nameof(XmlAnnotatedRecord.FloatProperty), "number", "1.2")]
    [InlineData(typeof(XmlAnnotatedRecord), nameof(XmlAnnotatedRecord.DoubleProperty), "number", "1.25")]
    [InlineData(typeof(XmlAnnotatedRecord), nameof(XmlAnnotatedRecord.DateTimeProperty), "string", "\"6/22/2022 12:00:00 AM\"")]
    [InlineData(typeof(XmlAnnotatedRecord), nameof(XmlAnnotatedRecord.EnumProperty), "integer", "2")]
    [InlineData(typeof(XmlAnnotatedRecord), nameof(XmlAnnotatedRecord.GuidProperty), "string", "\"d3966535-2637-48fa-b911-e3c27405ee09\"")]
    [InlineData(typeof(XmlAnnotatedRecord), nameof(XmlAnnotatedRecord.StringProperty), "string", "\"Example for StringProperty\"")]
    [InlineData(typeof(XmlAnnotatedRecord), nameof(XmlAnnotatedRecord.ObjectProperty), "object", "{\n  \"prop1\": 1,\n  \"prop2\": \"foobar\"\n}")]
    [InlineData(typeof(XmlAnnotatedRecord), nameof(XmlAnnotatedRecord.StringPropertyWithNullExample), "string", "null")]
    [InlineData(typeof(XmlAnnotatedRecord), nameof(XmlAnnotatedRecord.StringPropertyWithUri), "string", "\"https://test.com/a?b=1&c=2\"")]
    public void Apply_SetsExample_FromPropertyExampleTag(
        Type declaringType,
        string propertyName,
        string schemaType,
        string expectedExampleAsJson)
    {
        var propertyInfo = declaringType.GetProperty(propertyName)!;
        var schema = new OpenApiSchema { Type = schemaType };
        var filterContext = new SchemaFilterContext(propertyInfo.PropertyType, null, null, memberInfo: propertyInfo);

        Subject.Apply(schema, filterContext);

        schema.Example.ShouldNotBeNull();
        schema.Example.ToJson().ShouldBe(expectedExampleAsJson);
    }

    [Theory]
    [InlineData(typeof(XmlAnnotatedTypeWithoutExample), nameof(XmlAnnotatedTypeWithoutExample.BoolProperty), "boolean")]
    [InlineData(typeof(XmlAnnotatedTypeWithoutExample), nameof(XmlAnnotatedTypeWithoutExample.IntProperty), "integer")]
    [InlineData(typeof(XmlAnnotatedTypeWithoutExample), nameof(XmlAnnotatedTypeWithoutExample.LongProperty), "integer")]
    [InlineData(typeof(XmlAnnotatedTypeWithoutExample), nameof(XmlAnnotatedTypeWithoutExample.FloatProperty), "number")]
    [InlineData(typeof(XmlAnnotatedTypeWithoutExample), nameof(XmlAnnotatedTypeWithoutExample.DoubleProperty), "number")]
    [InlineData(typeof(XmlAnnotatedTypeWithoutExample), nameof(XmlAnnotatedTypeWithoutExample.DateTimeProperty), "string")]
    [InlineData(typeof(XmlAnnotatedTypeWithoutExample), nameof(XmlAnnotatedTypeWithoutExample.EnumProperty), "integer")]
    [InlineData(typeof(XmlAnnotatedTypeWithoutExample), nameof(XmlAnnotatedTypeWithoutExample.GuidProperty), "string")]
    [InlineData(typeof(XmlAnnotatedTypeWithoutExample), nameof(XmlAnnotatedTypeWithoutExample.StringProperty), "string")]
    [InlineData(typeof(XmlAnnotatedTypeWithoutExample), nameof(XmlAnnotatedTypeWithoutExample.ObjectProperty), "object")]
    [InlineData(typeof(XmlAnnotatedTypeWithoutExample), nameof(XmlAnnotatedTypeWithoutExample.StringPropertyWithNullExample), "string")]
    [InlineData(typeof(XmlAnnotatedTypeWithoutExample), nameof(XmlAnnotatedTypeWithoutExample.StringPropertyWithUri), "string")]
    [InlineData(typeof(XmlAnnotatedRecordWithoutExample), nameof(XmlAnnotatedRecordWithoutExample.BoolProperty), "boolean")]
    [InlineData(typeof(XmlAnnotatedRecordWithoutExample), nameof(XmlAnnotatedRecordWithoutExample.IntProperty), "integer")]
    [InlineData(typeof(XmlAnnotatedRecordWithoutExample), nameof(XmlAnnotatedRecordWithoutExample.LongProperty), "integer")]
    [InlineData(typeof(XmlAnnotatedRecordWithoutExample), nameof(XmlAnnotatedRecordWithoutExample.FloatProperty), "number")]
    [InlineData(typeof(XmlAnnotatedRecordWithoutExample), nameof(XmlAnnotatedRecordWithoutExample.DoubleProperty), "number")]
    [InlineData(typeof(XmlAnnotatedRecordWithoutExample), nameof(XmlAnnotatedRecordWithoutExample.DateTimeProperty), "string")]
    [InlineData(typeof(XmlAnnotatedRecordWithoutExample), nameof(XmlAnnotatedRecordWithoutExample.EnumProperty), "integer")]
    [InlineData(typeof(XmlAnnotatedRecordWithoutExample), nameof(XmlAnnotatedRecordWithoutExample.GuidProperty), "string")]
    [InlineData(typeof(XmlAnnotatedRecordWithoutExample), nameof(XmlAnnotatedRecordWithoutExample.StringProperty), "string")]
    [InlineData(typeof(XmlAnnotatedRecordWithoutExample), nameof(XmlAnnotatedRecordWithoutExample.ObjectProperty), "object")]
    [InlineData(typeof(XmlAnnotatedRecordWithoutExample), nameof(XmlAnnotatedRecordWithoutExample.StringPropertyWithNullExample), "string")]
    [InlineData(typeof(XmlAnnotatedRecordWithoutExample), nameof(XmlAnnotatedRecordWithoutExample.StringPropertyWithUri), "string")]
    public void Apply_DoesNotSetExample_WhenPropertyExampleTagIsNotProvided(
        Type declaringType,
        string propertyName,
        string schemaType)
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
        var schema = new OpenApiSchema { Type = "number", Format = "float" };
        var filterContext = new SchemaFilterContext(propertyInfo.PropertyType, null, null, memberInfo: propertyInfo);

        var defaultCulture = CultureInfo.CurrentCulture;
        CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(cultureName);

        Subject.Apply(schema, filterContext);

        CultureInfo.CurrentCulture = defaultCulture;

        schema.Example.GetType().GetProperty("Value")!.GetValue(schema.Example).ShouldBe(expectedValue);
    }

    private ISchemaFilter Subject => new XmlDocSchemaFilter(Provider);
}
