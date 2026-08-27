using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;
using System.Xml.XPath;

namespace Altinn.Swashbuckle.XmlDoc;

/// <summary>
/// Adds OpenAPI documentation to parameters.
/// </summary>
/// <remarks>
/// Based entirely on <see cref="XmlCommentsParameterFilter"/>, but using <see cref="IXmlDocProvider"/> to get the XML documentation.
/// </remarks>
internal sealed class XmlDocParameterFilter
    : IParameterFilter
{
    private readonly IXmlDocProvider _documentationProvider;

    public XmlDocParameterFilter(IXmlDocProvider documentationProvider)
    {
        _documentationProvider = documentationProvider;
    }

    /// <inheritdoc />
    public void Apply(IOpenApiParameter parameter, ParameterFilterContext context)
    {
        if (context.PropertyInfo != null)
        {
            ApplyPropertyTags(parameter, context);
        }
        else if (context.ParameterInfo != null)
        {
            ApplyParamTags(parameter, context);
        }
    }

    private void ApplyPropertyTags(IOpenApiParameter parameter, ParameterFilterContext context)
    {
        if (!_documentationProvider.TryGetXmlDoc(context.PropertyInfo, out var propertyNode))
        {
            return;
        }
        if (parameter is not OpenApiParameter openApiParameter)
        {
            return;
        }

        var summaryNode = propertyNode.SelectFirstChild("summary");
        if (summaryNode != null)
        {
            openApiParameter.Description = XmlCommentsTextHelper.Humanize(summaryNode.InnerXml);
            if (openApiParameter.Schema is not null)
            {
                openApiParameter.Schema.Description = null; // no need to duplicate
            }
        }

        var exampleNode = propertyNode.SelectFirstChild("example");
        if (exampleNode == null)
        {
            return;
        }

        if (openApiParameter.Schema is not null)
        {
            openApiParameter.Example = XmlCommentsExampleHelper.Create(context.SchemaRepository, openApiParameter.Schema, exampleNode.ToString());
        }
    }

    private void ApplyParamTags(IOpenApiParameter parameter, ParameterFilterContext context)
    {
        if (context.ParameterInfo.Member is not MethodInfo methodInfo)
        {
            return;
        }
        if (parameter is not OpenApiParameter openApiParameter)
        {
            return;
        }

        // If method is from a constructed generic type, look for comments from the generic type method
        var targetMethod = methodInfo.DeclaringType!.IsConstructedGenericType
            ? methodInfo.GetUnderlyingGenericTypeMethod()
            : methodInfo;

        if (targetMethod == null)
        {
            return;
        }

        if (!_documentationProvider.TryGetXmlDoc(targetMethod, out var propertyNode))
        {
            return;
        }

        XPathNavigator? paramNode = propertyNode.SelectFirstChildWithAttribute("param", "name", context.ParameterInfo.Name);

        if (paramNode != null)
        {
            openApiParameter.Description = XmlCommentsTextHelper.Humanize(paramNode.InnerXml);

            var example = paramNode.GetAttribute("example");
            if (string.IsNullOrEmpty(example))
            {
                return;
            }

            if (openApiParameter.Schema is not null)
            {
                openApiParameter.Example = XmlCommentsExampleHelper.Create(context.SchemaRepository, openApiParameter.Schema, example);
            }
        }
    }
}
