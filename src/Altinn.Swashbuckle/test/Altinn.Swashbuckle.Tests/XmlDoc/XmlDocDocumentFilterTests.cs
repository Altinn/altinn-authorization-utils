using Altinn.Swashbuckle.Tests.Fixtures;
using Altinn.Swashbuckle.XmlDoc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;

namespace Altinn.Swashbuckle.Tests.XmlDoc;

public class XmlDocDocumentFilterTests
    : XmlDocFilterTestsBase
{
    [Fact]
    public void Apply_SetsTagDescription_FromControllerSummaryTags()
    {
        var document = new OpenApiDocument();
        var filterContext = new DocumentFilterContext(
            [
                new ApiDescription
                {
                    ActionDescriptor = new ControllerActionDescriptor
                    {
                        ControllerTypeInfo = typeof(FakeControllerWithXmlComments).GetTypeInfo(),
                        ControllerName = nameof(FakeControllerWithXmlComments),
                    }
                },
                new ApiDescription
                {
                    ActionDescriptor = new ControllerActionDescriptor
                    {
                        ControllerTypeInfo = typeof(FakeControllerWithXmlComments).GetTypeInfo(),
                        ControllerName = nameof(FakeControllerWithXmlComments),
                    }
                }
            ],
            null,
            null);

        Subject().Apply(document, filterContext);

        document.Tags.ShouldHaveSingleItem()
            .Description.ShouldBe("Summary for FakeControllerWithXmlComments");
    }

    [Fact]
    public void Apply_SetsTagDescription_FromControllerSummaryTags_OneControllerWithoutDescription()
    {
        var document = new OpenApiDocument();
        var filterContext = new DocumentFilterContext(
            [
                new ApiDescription
                {
                    ActionDescriptor = new ControllerActionDescriptor
                    {
                        ControllerTypeInfo = typeof(FakeController).GetTypeInfo(),
                        ControllerName = nameof(FakeController),
                    }
                },
                new ApiDescription
                {
                    ActionDescriptor = new ControllerActionDescriptor
                    {
                        ControllerTypeInfo = typeof(FakeControllerWithXmlComments).GetTypeInfo(),
                        ControllerName = nameof(FakeControllerWithXmlComments),
                    }
                }
            ],
            null,
            null);

        Subject().Apply(document, filterContext);

        document.Tags.ShouldHaveSingleItem()
            .Description.ShouldBe("Summary for FakeControllerWithXmlComments");
    }

    [Fact]
    public void Uses_Proper_Tag_Name()
    {
        var expectedTagName = "AliasControllerWithXmlComments";
        var document = new OpenApiDocument();
        var filterContext = new DocumentFilterContext(
            [
                new ApiDescription
                {
                    ActionDescriptor = new ControllerActionDescriptor
                    {
                        ControllerTypeInfo = typeof(FakeControllerWithXmlComments).GetTypeInfo(),
                        ControllerName = nameof(FakeControllerWithXmlComments),
                        RouteValues = new Dictionary<string, string?> { { "controller", expectedTagName } },
                    }
                },
                new ApiDescription
                {
                    ActionDescriptor = new ControllerActionDescriptor
                    {
                        ControllerTypeInfo = typeof(FakeControllerWithXmlComments).GetTypeInfo(),
                        ControllerName = nameof(FakeControllerWithXmlComments),
                        RouteValues = new Dictionary<string, string?> { { "controller", expectedTagName } },
                    }
                }
            ],
            null,
            null);

        Subject(true).Apply(document, filterContext);

        document.Tags.ShouldHaveSingleItem()
            .Name.ShouldBe(expectedTagName);
    }

    [Fact]
    public void Uses_Proper_Tag_Name_With_Custom_TagSelector()
    {
        var expectedTagName = "AliasControllerWithXmlComments";
        var document = new OpenApiDocument();
        var filterContext = new DocumentFilterContext(
            [
                new ApiDescription
                {
                    ActionDescriptor = new ControllerActionDescriptor
                    {
                        ControllerTypeInfo = typeof(FakeControllerWithXmlComments).GetTypeInfo(),
                        ControllerName = nameof(FakeControllerWithXmlComments),
                    }
                },
                new ApiDescription
                {
                    ActionDescriptor = new ControllerActionDescriptor
                    {
                        ControllerTypeInfo = typeof(FakeControllerWithXmlComments).GetTypeInfo(),
                        ControllerName = nameof(FakeControllerWithXmlComments),
                    }
                }
            ],
            null,
            null);

        Subject(
            opts =>
            {
                opts.TagsSelector = apiDesc => [expectedTagName];
            })
            .Apply(document, filterContext);

        document.Tags.ShouldHaveSingleItem()
            .Name.ShouldBe(expectedTagName);
    }

    private IDocumentFilter Subject(Action<SwaggerGeneratorOptions>? configureOptions = null)
    {
        SwaggerGeneratorOptions? options = null;
        if (configureOptions != null)
        {
            options = new();
            configureOptions(options);
        }

        return new XmlDocDocumentFilter(Provider, options);
    }

    private IDocumentFilter Subject(bool withOptions)
        => withOptions
        ? Subject(_ => { })
        : Subject();
}
