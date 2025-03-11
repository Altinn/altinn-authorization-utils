using Altinn.Swashbuckle.Tests.Fixtures;
using Altinn.Swashbuckle.XmlDoc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Altinn.Swashbuckle.Tests.XmlDoc;

public class XmlDocParameterFilterTests
    : XmlDocFilterTestsBase
{
    [Fact]
    public void Apply_SetsDescriptionAndExample_FromActionParamTag()
    {
        var parameter = new OpenApiParameter { Schema = new OpenApiSchema { Type = "string" } };
        var parameterInfo = typeof(FakeControllerWithXmlComments)
            .GetMethod(nameof(FakeControllerWithXmlComments.ActionWithParamTags))!
            .GetParameters()[0];
        var apiParameterDescription = new ApiParameterDescription { };
        var filterContext = new ParameterFilterContext(apiParameterDescription, null, null, parameterInfo: parameterInfo);

        Subject.Apply(parameter, filterContext);

        parameter.Description.ShouldBe("Description for param1");
        parameter.Example.ShouldNotBeNull();
        parameter.Example.ToJson().ShouldBe("\"Example for \\\"param1\\\"\"");
    }

    [Fact]
    public void Apply_SetsDescriptionAndExample_FromUriTypeActionParamTag()
    {
        var parameter = new OpenApiParameter { Schema = new OpenApiSchema { Type = "string" } };
        var parameterInfo = typeof(FakeControllerWithXmlComments)
            .GetMethod(nameof(FakeControllerWithXmlComments.ActionWithParamTags))!
            .GetParameters()[1];
        var apiParameterDescription = new ApiParameterDescription { };
        var filterContext = new ParameterFilterContext(apiParameterDescription, null, null, parameterInfo: parameterInfo);

        Subject.Apply(parameter, filterContext);

        parameter.Description.ShouldBe("Description for param2");
        parameter.Example.ShouldNotBeNull();
        parameter.Example.ToJson().ShouldBe("\"http://test.com/?param1=1&param2=2\"");
    }

    [Fact]
    public void Apply_SetsDescriptionAndExample_FromUnderlyingGenericTypeActionParamTag()
    {
        var parameter = new OpenApiParameter { Schema = new OpenApiSchema { Type = "string" } };
        var parameterInfo = typeof(FakeConstructedControllerWithXmlComments)
            .GetMethod(nameof(FakeConstructedControllerWithXmlComments.ActionWithParamTags))!
            .GetParameters()[0];
        var apiParameterDescription = new ApiParameterDescription { };
        var filterContext = new ParameterFilterContext(apiParameterDescription, null, null, parameterInfo: parameterInfo);

        Subject.Apply(parameter, filterContext);

        parameter.Description.ShouldBe("Description for param1");
        parameter.Example.ShouldNotBeNull();
        parameter.Example.ToJson().ShouldBe("\"Example for \\\"param1\\\"\"");
    }

    [Fact]
    public void Apply_SetsDescriptionAndExample_FromPropertySummaryAndExampleTags()
    {
        var parameter = new OpenApiParameter { Schema = new OpenApiSchema { Type = "string", Description = "schema-level description" } };
        var propertyInfo = typeof(XmlAnnotatedType).GetProperty(nameof(XmlAnnotatedType.StringProperty));
        var apiParameterDescription = new ApiParameterDescription { };
        var filterContext = new ParameterFilterContext(apiParameterDescription, null, null, propertyInfo: propertyInfo);

        Subject.Apply(parameter, filterContext);

        parameter.Description.ShouldBe("Summary for StringProperty");
        parameter.Schema.Description.ShouldBeNull();
        parameter.Example.ShouldNotBeNull();
        parameter.Example.ToJson().ShouldBe("\"Example for StringProperty\"");
    }

    [Fact]
    public void Apply_SetsDescriptionAndExample_FromUriTypePropertySummaryAndExampleTags()
    {
        var parameter = new OpenApiParameter { Schema = new OpenApiSchema { Type = "string", Description = "schema-level description" } };
        var propertyInfo = typeof(XmlAnnotatedType).GetProperty(nameof(XmlAnnotatedType.StringPropertyWithUri));
        var apiParameterDescription = new ApiParameterDescription { };
        var filterContext = new ParameterFilterContext(apiParameterDescription, null, null, propertyInfo: propertyInfo);

        Subject.Apply(parameter, filterContext);

        parameter.Description.ShouldBe("Summary for StringPropertyWithUri");
        parameter.Schema.Description.ShouldBeNull();
        parameter.Example.ShouldNotBeNull();
        parameter.Example.ToJson().ShouldBe("\"https://test.com/a?b=1&c=2\"");
    }

    private IParameterFilter Subject => new XmlDocParameterFilter(Provider);
}
