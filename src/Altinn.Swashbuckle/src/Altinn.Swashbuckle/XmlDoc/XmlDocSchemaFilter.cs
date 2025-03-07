using Altinn.Swashbuckle.XmlDoc;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Xml.XPath;

namespace Altinn.Swashbuckle.XmlDoc;

/// <summary>
/// Adds OpenAPI documentation to request bodies.
/// </summary>
/// <remarks>
/// Based entirely on <see cref="XmlCommentsSchemaFilter"/>, but using <see cref="IXmlDocProvider"/> to get the XML documentation.
/// </remarks>
internal sealed class XmlDocSchemaFilter
    : ISchemaFilter
{
    private readonly IXmlDocProvider _documentationProvider;

    public XmlDocSchemaFilter(IXmlDocProvider documentationProvider)
    {
        _documentationProvider = documentationProvider;
    }

    /// <inheritdoc />
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        ApplyTypeTags(schema, context.Type);

        if (context.MemberInfo != null)
        {
            ApplyMemberTags(schema, context);
        }
    }

    private void ApplyTypeTags(OpenApiSchema schema, Type type)
    {
        if (!_documentationProvider.TryGetXmlDoc(type, out var typeNode))
        {
            return;
        }

        var typeSummaryNode = typeNode.SelectFirstChild("summary");

        if (typeSummaryNode != null)
        {
            schema.Description = XmlCommentsTextHelper.Humanize(typeSummaryNode.InnerXml);
        }
    }

    private void ApplyMemberTags(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (context.MemberInfo.DeclaringType is not null && _documentationProvider.TryGetXmlDoc(context.MemberInfo.DeclaringType, out var recordTypeNode))
        {
            XPathNavigator? recordDefaultConstructorProperty = recordTypeNode.SelectFirstChildWithAttribute("param", "name", context.MemberInfo.Name);

            if (recordDefaultConstructorProperty != null)
            {
                var summaryNode = recordDefaultConstructorProperty.Value;
                if (summaryNode != null)
                {
                    schema.Description = XmlCommentsTextHelper.Humanize(summaryNode);
                }

                var example = recordDefaultConstructorProperty.GetAttribute("example");
                if (!string.IsNullOrEmpty(example))
                {
                    TrySetExample(schema, context, example);
                }
            }
        }

        if (context.MemberInfo is not null && _documentationProvider.TryGetXmlDoc(context.MemberInfo, out var fieldOrPropertyNode))
        {
            var summaryNode = fieldOrPropertyNode.SelectFirstChild("summary");
            if (summaryNode != null)
            {
                schema.Description = XmlCommentsTextHelper.Humanize(summaryNode.InnerXml);
            }

            var exampleNode = fieldOrPropertyNode.SelectFirstChild("example");
            TrySetExample(schema, context, exampleNode?.Value);
        }
    }

    private static void TrySetExample(OpenApiSchema schema, SchemaFilterContext context, string? example)
    {
        if (example == null)
            return;

        schema.Example = XmlCommentsExampleHelper.Create(context.SchemaRepository, schema, example);
    }
}
