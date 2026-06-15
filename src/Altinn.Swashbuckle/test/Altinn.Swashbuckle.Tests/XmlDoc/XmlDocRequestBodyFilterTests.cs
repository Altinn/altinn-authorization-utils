using Altinn.Swashbuckle.Tests.Fixtures;
using Altinn.Swashbuckle.XmlDoc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Altinn.Swashbuckle.Tests.XmlDoc;

public class XmlDocRequestBodyFilterTests
    : XmlDocFilterTestsBase
{
    [Fact]
    public void Apply_SetsDescriptionAndExample_FromActionParamTag()
    {
        var requestBody = new OpenApiRequestBody
        {
            Content = new Dictionary<string, OpenApiMediaType>
            {
                ["application/json"] = new OpenApiMediaType { Schema = new OpenApiSchema { Type = JsonSchemaType.String } }
            }
        };
        var parameterInfo = typeof(FakeControllerWithXmlComments)
            .GetMethod(nameof(FakeControllerWithXmlComments.ActionWithParamTags))!
            .GetParameters()[0];
        var bodyParameterDescription = new ApiParameterDescription
        {
            ParameterDescriptor = new ControllerParameterDescriptor { ParameterInfo = parameterInfo }
        };
        var filterContext = new RequestBodyFilterContext(bodyParameterDescription, null, null, null, new OpenApiDocument());

        Subject.Apply(requestBody, filterContext);

        requestBody.Description.ShouldBe("Description for param1");
        requestBody.Content["application/json"].Example.ShouldNotBeNull();
        ToJsonString(requestBody.Content["application/json"].Example).ShouldBe("\"Example for \\\"param1\\\"\"");
    }

    [Fact]
    public void Apply_SetsDescriptionAndExample_FromUnderlyingGenericTypeActionParamTag()
    {
        var requestBody = new OpenApiRequestBody
        {
            Content = new Dictionary<string, OpenApiMediaType>
            {
                ["application/json"] = new OpenApiMediaType { Schema = new OpenApiSchema { Type = JsonSchemaType.String } }
            }
        };
        var parameterInfo = typeof(FakeConstructedControllerWithXmlComments)
            .GetMethod(nameof(FakeConstructedControllerWithXmlComments.ActionWithParamTags))!
            .GetParameters()[0];
        var bodyParameterDescription = new ApiParameterDescription
        {
            ParameterDescriptor = new ControllerParameterDescriptor { ParameterInfo = parameterInfo }
        };
        var filterContext = new RequestBodyFilterContext(bodyParameterDescription, null, null, null, new OpenApiDocument());

        Subject.Apply(requestBody, filterContext);

        requestBody.Description.ShouldBe("Description for param1");
        requestBody.Content["application/json"].Example.ShouldNotBeNull();
        ToJsonString(requestBody.Content["application/json"].Example).ShouldBe("\"Example for \\\"param1\\\"\"");
    }

    [Fact]
    public void Apply_SetsDescriptionAndExample_FromPropertySummaryAndExampleTags()
    {
        var requestBody = new OpenApiRequestBody
        {
            Content = new Dictionary<string, OpenApiMediaType>
            {
                ["application/json"] = new OpenApiMediaType { Schema = new OpenApiSchema { Type = JsonSchemaType.String } }
            }
        };
        var bodyParameterDescription = new ApiParameterDescription
        {
            ModelMetadata = ModelMetadataFactory.CreateForProperty(typeof(XmlAnnotatedType), nameof(XmlAnnotatedType.StringProperty))
        };
        var filterContext = new RequestBodyFilterContext(bodyParameterDescription, null, null, null, new OpenApiDocument());

        Subject.Apply(requestBody, filterContext);

        requestBody.Description.ShouldBe("Summary for StringProperty");
        requestBody.Content["application/json"].Example.ShouldNotBeNull();
        ToJsonString(requestBody.Content["application/json"].Example).ShouldBe("\"Example for StringProperty\"");
    }


    [Fact]
    public void Apply_SetsDescriptionAndExample_FromUriTypePropertySummaryAndExampleTags()
    {
        var requestBody = new OpenApiRequestBody
        {
            Content = new Dictionary<string, OpenApiMediaType>
            {
                ["application/json"] = new OpenApiMediaType { Schema = new OpenApiSchema { Type = JsonSchemaType.String } }
            }
        };
        var bodyParameterDescription = new ApiParameterDescription
        {
            ModelMetadata = ModelMetadataFactory.CreateForProperty(typeof(XmlAnnotatedType), nameof(XmlAnnotatedType.StringPropertyWithUri))
        };
        var filterContext = new RequestBodyFilterContext(bodyParameterDescription, null, null, null, new OpenApiDocument());

        Subject.Apply(requestBody, filterContext);

        requestBody.Description.ShouldBe("Summary for StringPropertyWithUri");
        requestBody.Content["application/json"].Example.ShouldNotBeNull();
        ToJsonString(requestBody.Content["application/json"].Example).ShouldBe("\"https://test.com/a?b=1&c=2\"");
    }

    [Fact]
    public void Apply_SetsDescription_ForParameterFromBody()
    {
        var requestBody = new OpenApiRequestBody
        {
            Content = new Dictionary<string, OpenApiMediaType>
            {
                ["application/json"] = new OpenApiMediaType { Schema = new OpenApiSchema { Type = JsonSchemaType.String } }
            }
        };
        var parameterInfo = typeof(FakeControllerWithXmlComments)
            .GetMethod(nameof(FakeControllerWithXmlComments.PostBody))!
            .GetParameters()[0];
        var bodyParameterDescription = new ApiParameterDescription
        {
            ParameterDescriptor = new ControllerParameterDescriptor { ParameterInfo = parameterInfo }
        };
        var filterContext = new RequestBodyFilterContext(bodyParameterDescription, null, null, null, new OpenApiDocument());

        Subject.Apply(requestBody, filterContext);

        requestBody.Description.ShouldBe("Parameter from JSON body");
    }

    [Fact]
    public void Apply_SetsDescription_ForParameterFromForm()
    {
        var parameterInfo = typeof(FakeControllerWithXmlComments)
            .GetMethod(nameof(FakeControllerWithXmlComments.PostForm))!
            .GetParameters()[0];

        var requestBody = new OpenApiRequestBody
        {
            Content = new Dictionary<string, OpenApiMediaType>
            {
                ["multipart/form-data"] = new OpenApiMediaType
                {
                    Schema = new OpenApiSchema
                    {
                        Type = JsonSchemaType.String,
                        Properties = new Dictionary<string, IOpenApiSchema>()
                        {
                            [parameterInfo.Name!] = new OpenApiSchema { Type = JsonSchemaType.String }
                        }
                    },
                }
            }
        };

        var bodyParameterDescription = new ApiParameterDescription
        {
            ParameterDescriptor = new ControllerParameterDescriptor { ParameterInfo = parameterInfo },
            Name = parameterInfo.Name!,
            Source = BindingSource.Form
        };
        var filterContext = new RequestBodyFilterContext(null, [bodyParameterDescription], null, null, new OpenApiDocument());

        Subject.Apply(requestBody, filterContext);

        requestBody.Content["multipart/form-data"].Schema.Properties[parameterInfo.Name].Description.ShouldBe("Parameter from form body");
    }

    private IRequestBodyFilter Subject => new XmlDocRequestBodyFilter(Provider);
}
