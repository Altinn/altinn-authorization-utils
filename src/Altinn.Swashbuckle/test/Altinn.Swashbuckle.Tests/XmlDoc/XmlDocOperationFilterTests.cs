using Altinn.Swashbuckle.Tests.Fixtures;
using Altinn.Swashbuckle.XmlDoc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Altinn.Swashbuckle.Tests.XmlDoc;

public class XmlDocOperationFilterTests
    : XmlDocFilterTestsBase
{
    [Fact]
    public void Apply_SetsSummaryAndDescription_FromActionSummaryAndRemarksTags()
    {
        var operation = new OpenApiOperation();
        var methodInfo = typeof(FakeControllerWithXmlComments)
            .GetMethod(nameof(FakeControllerWithXmlComments.ActionWithSummaryAndRemarksTags))!;

        var apiDescription = ApiDescriptionFactory.Create(methodInfo: methodInfo, groupName: "v1", httpMethod: "POST", relativePath: "resource");
        var filterContext = new OperationFilterContext(apiDescription, null, null, methodInfo);

        Subject.Apply(operation, filterContext);

        operation.Summary.ShouldBe("Summary for ActionWithSummaryAndRemarksTags");
        operation.Description.ShouldBe("Remarks for ActionWithSummaryAndRemarksTags");
    }

    [Fact]
    public void Apply_SetsSummaryAndDescription_FromUnderlyingGenericTypeActionSummaryAndRemarksTags()
    {
        var operation = new OpenApiOperation();
        var methodInfo = typeof(FakeConstructedControllerWithXmlComments)
            .GetMethod(nameof(FakeConstructedControllerWithXmlComments.ActionWithSummaryAndResponseTags))!;

        var apiDescription = ApiDescriptionFactory.Create(methodInfo: methodInfo, groupName: "v1", httpMethod: "POST", relativePath: "resource");
        var filterContext = new OperationFilterContext(apiDescription, null, null, methodInfo);

        Subject.Apply(operation, filterContext);

        operation.Summary.ShouldBe("Summary for ActionWithSummaryAndRemarksTags");
        operation.Description.ShouldBe("Remarks for ActionWithSummaryAndRemarksTags");
    }

    [Fact]
    public void Apply_SetsResponseDescription_FromActionOrControllerResponseTags()
    {
        var operation = new OpenApiOperation
        {
            Responses = new OpenApiResponses
            {
                { "200", new OpenApiResponse { Description = "Success" } },
                { "400", new OpenApiResponse { Description = "Client Error" } },
            }
        };
        var methodInfo = typeof(FakeControllerWithXmlComments)
            .GetMethod(nameof(FakeControllerWithXmlComments.ActionWithResponseTags))!;

        var apiDescription = ApiDescriptionFactory.Create(
            methodInfo: methodInfo,
            groupName: "v1",
            httpMethod: "POST",
            relativePath: "resource",
            supportedResponseTypes:
            [
                new ApiResponseType { StatusCode = 200 },
                new ApiResponseType { StatusCode = 400 },
            ]);
        var filterContext = new OperationFilterContext(apiDescription, null, null, methodInfo: methodInfo);

        Subject.Apply(operation, filterContext);

        operation.Responses.ShouldContainKey("200");
        operation.Responses.ShouldContainKey("400");
        operation.Responses.ShouldContainKey("default");
        operation.Responses["200"].Description.ShouldBe("Description for 200 response");
        operation.Responses["400"].Description.ShouldBe("Description for 400 response");
        operation.Responses["default"].Description.ShouldBe("Description for default response");
    }

    private IOperationFilter Subject => new XmlDocOperationFilter(Provider);
}
