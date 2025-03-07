using Altinn.Swashbuckle.XmlDoc;
using Microsoft.OpenApi.Models;
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
    public void Apply(OpenApiParameter parameter, ParameterFilterContext context)
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

    private void ApplyPropertyTags(OpenApiParameter parameter, ParameterFilterContext context)
    {
        if (!_documentationProvider.TryGetXmlDoc(context.PropertyInfo, out var propertyNode))
        {
            return;
        }

        var summaryNode = propertyNode.SelectFirstChild("summary");
        if (summaryNode != null)
        {
            parameter.Description = XmlCommentsTextHelper.Humanize(summaryNode.InnerXml);
            parameter.Schema.Description = null; // no need to duplicate
        }

        var exampleNode = propertyNode.SelectFirstChild("example");
        if (exampleNode == null) return;

        parameter.Example = XmlCommentsExampleHelper.Create(context.SchemaRepository, parameter.Schema, exampleNode.ToString());
    }

    private void ApplyParamTags(OpenApiParameter parameter, ParameterFilterContext context)
    {
        if (!(context.ParameterInfo.Member is MethodInfo methodInfo)) return;

        // If method is from a constructed generic type, look for comments from the generic type method
        var targetMethod = methodInfo.DeclaringType.IsConstructedGenericType
            ? methodInfo.GetUnderlyingGenericTypeMethod()
            : methodInfo;

        if (targetMethod == null) return;

        if (!_documentationProvider.TryGetXmlDoc(targetMethod, out var propertyNode))
        {
            return;
        }

        XPathNavigator? paramNode = propertyNode.SelectFirstChildWithAttribute("param", "name", context.ParameterInfo.Name);

        if (paramNode != null)
        {
            parameter.Description = XmlCommentsTextHelper.Humanize(paramNode.InnerXml);

            var example = paramNode.GetAttribute("example");
            if (string.IsNullOrEmpty(example)) return;

            parameter.Example = XmlCommentsExampleHelper.Create(context.SchemaRepository, parameter.Schema, example);
        }
    }
}
