using Altinn.Swashbuckle.XmlDoc;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;
using System.Xml.XPath;

namespace Altinn.Swashbuckle.XmlDoc;

/// <summary>
/// Adds OpenAPI documentation to operations.
/// </summary>
/// <remarks>
/// Based entirely on <see cref="XmlCommentsOperationFilter"/>, but using <see cref="IXmlDocProvider"/> to get the XML documentation.
/// </remarks>
internal sealed class XmlDocOperationFilter
    : IOperationFilter
{
    private readonly IXmlDocProvider _documentationProvider;

    public XmlDocOperationFilter(IXmlDocProvider documentationProvider)
    {
        _documentationProvider = documentationProvider;
    }

    /// <inheritdoc />
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (context.MethodInfo == null) return;

        // If method is from a constructed generic type, look for comments from the generic type method
        var targetMethod = context.MethodInfo.DeclaringType is not null && context.MethodInfo.DeclaringType.IsConstructedGenericType
            ? context.MethodInfo.GetUnderlyingGenericTypeMethod()
            : context.MethodInfo;

        if (targetMethod == null) return;

        ApplyControllerTags(operation, targetMethod.DeclaringType);
        ApplyMethodTags(operation, targetMethod);
    }

    private void ApplyControllerTags(OpenApiOperation operation, Type controllerType)
    {
        if (!_documentationProvider.TryGetXmlDoc(controllerType, out var methodNode)) return;

        var responseNodes = methodNode.SelectChildren("response");
        ApplyResponseTags(operation, responseNodes);
    }

    private void ApplyMethodTags(OpenApiOperation operation, MethodInfo methodInfo)
    {
        var methodMemberName = XmlCommentsNodeNameHelper.GetMemberNameForMethod(methodInfo);

        if (!_documentationProvider.TryGetXmlDoc(methodInfo, out var methodNode)) return;

        var summaryNode = methodNode.SelectFirstChild("summary");
        if (summaryNode != null)
            operation.Summary = XmlCommentsTextHelper.Humanize(summaryNode.InnerXml);

        var remarksNode = methodNode.SelectFirstChild("remarks");
        if (remarksNode != null)
            operation.Description = XmlCommentsTextHelper.Humanize(remarksNode.InnerXml);

        var responseNodes = methodNode.SelectChildren("response");
        ApplyResponseTags(operation, responseNodes);
    }

    private void ApplyResponseTags(OpenApiOperation operation, XPathNodeIterator responseNodes)
    {
        while (responseNodes.MoveNext())
        {
            var code = responseNodes.Current?.GetAttribute("code");
            if (code is null)
            {
                continue;
            }

            if (!operation.Responses.TryGetValue(code, out var response))
            {
                response = new OpenApiResponse();
                operation.Responses[code] = response;
            }

            response.Description = XmlCommentsTextHelper.Humanize(responseNodes.Current.InnerXml);
        }
    }
}
