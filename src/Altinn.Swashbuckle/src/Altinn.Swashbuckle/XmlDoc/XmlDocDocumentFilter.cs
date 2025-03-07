using Altinn.Swashbuckle.XmlDoc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Altinn.Swashbuckle.XmlDoc;

/// <summary>
/// Adds OpenAPI descriptions to the document tags based on XML documentation comments on the controller types.
/// </summary>
/// <remarks>
/// Based entirely on <see cref="XmlCommentsDocumentFilter"/>, but using <see cref="IXmlDocProvider"/> to get the XML documentation.
/// </remarks>
internal sealed class XmlDocDocumentFilter
    : IDocumentFilter
{
    private readonly SwaggerGeneratorOptions _options;
    private readonly IXmlDocProvider _documentationProvider;
    
    public XmlDocDocumentFilter(IXmlDocProvider documentationProvider, SwaggerGeneratorOptions options)
    {
        _options = options;
        _documentationProvider = documentationProvider;
    }

    /// <inheritdoc />
    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        // Collect (unique) controller names and types in a dictionary
        var controllerNamesAndTypes = context.ApiDescriptions
            .Select(apiDesc => new { ApiDesc = apiDesc, ActionDesc = apiDesc.ActionDescriptor as ControllerActionDescriptor })
            .Where(x => x.ActionDesc != null)
            .GroupBy(x => _options?.TagsSelector(x.ApiDesc).FirstOrDefault() ?? x.ActionDesc?.ControllerName)
            .Select(group => new KeyValuePair<string?, Type?>(group.Key, group.First().ActionDesc?.ControllerTypeInfo.AsType()));

        foreach (var nameAndType in controllerNamesAndTypes)
        {
            if (nameAndType.Value is null)
            {
                continue;
            }

            if (!_documentationProvider.TryGetXmlDoc(nameAndType.Value, out var typeNode))
            {
                continue;
            }

            var summaryNode = typeNode.SelectFirstChild("summary");
            if (summaryNode != null)
            {
                swaggerDoc.Tags ??= [];

                swaggerDoc.Tags.Add(new OpenApiTag
                {
                    Name = nameAndType.Key,
                    Description = XmlCommentsTextHelper.Humanize(summaryNode.InnerXml),
                });
            }
        }
    }
}
