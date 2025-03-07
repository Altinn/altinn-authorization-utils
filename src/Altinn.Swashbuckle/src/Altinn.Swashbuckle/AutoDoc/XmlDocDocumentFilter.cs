using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Altinn.Swashbuckle.AutoDoc;

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

            var memberName = XmlCommentsNodeNameHelper.GetMemberNameForType(nameAndType.Value);

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
