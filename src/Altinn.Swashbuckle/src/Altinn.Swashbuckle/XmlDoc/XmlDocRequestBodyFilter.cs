using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;

namespace Altinn.Swashbuckle.XmlDoc;

/// <summary>
/// Adds OpenAPI documentation to request bodies.
/// </summary>
/// <remarks>
/// Based entirely on <see cref="XmlCommentsRequestBodyFilter"/>, but using <see cref="IXmlDocProvider"/> to get the XML documentation.
/// </remarks>
internal sealed class XmlDocRequestBodyFilter
    : IRequestBodyFilter
{
    private readonly IXmlDocProvider _documentationProvider;

    public XmlDocRequestBodyFilter(IXmlDocProvider documentationProvider)
    {
        _documentationProvider = documentationProvider;
    }

    /// <inheritdoc />
    public void Apply(OpenApiRequestBody requestBody, RequestBodyFilterContext context)
    {
        var bodyParameterDescription = context.BodyParameterDescription;

        if (bodyParameterDescription is not null)
        {
            var propertyInfo = bodyParameterDescription.PropertyInfo();
            if (propertyInfo is not null)
            {
                ApplyPropertyTagsForBody(requestBody, context, propertyInfo);
            }
            else
            {
                var parameterInfo = bodyParameterDescription.ParameterInfo();
                if (parameterInfo is not null)
                {
                    ApplyParamTagsForBody(requestBody, context, parameterInfo);
                }
            }
        }
        else
        {
            var numberOfFromForm = context.FormParameterDescriptions?.Count() ?? 0;
            if (requestBody.Content?.Count is 0 || numberOfFromForm is 0)
            {
                return;
            }

            foreach (var formParameter in context.FormParameterDescriptions!)
            {
                if (formParameter.Name is null || formParameter.PropertyInfo() is not null)
                {
                    continue;
                }

                var parameterFromForm = formParameter.ParameterInfo();
                if (parameterFromForm is null)
                {
                    continue;
                }

                foreach (var item in requestBody.Content!.Values)
                {
                    if (item?.Schema?.Properties is { } properties
                       && (properties.TryGetValue(formParameter.Name, out var value) || properties.TryGetValue(formParameter.Name.ToCamelCase(), out value)))
                    {
                        var (summary, example) = GetParamTags(parameterFromForm);
                        value.Description ??= summary;
                        if (!string.IsNullOrEmpty(example))
                        {
                            value.Example ??= XmlCommentsExampleHelper.Create(context.SchemaRepository, value, example);
                        }
                    }
                }
            }
        }
    }

    private (string? Summary, string? Example) GetPropertyTags(PropertyInfo propertyInfo)
    {
        if (!_documentationProvider.TryGetXmlDoc(propertyInfo, out var propertyNode))
        {
            return (null, null);
        }

        string? summary = null;
        var summaryNode = propertyNode.SelectFirstChild("summary");
        if (summaryNode is not null)
        {
            summary = XmlCommentsTextHelper.Humanize(summaryNode.InnerXml);
        }

        var exampleNode = propertyNode.SelectFirstChild("example");

        return (summary, exampleNode?.ToString());
    }

    private void ApplyPropertyTagsForBody(OpenApiRequestBody requestBody, RequestBodyFilterContext context, PropertyInfo propertyInfo)
    {
        var (summary, example) = GetPropertyTags(propertyInfo);

        if (summary is not null)
        {
            requestBody.Description = summary;
        }

        if (requestBody.Content?.Count is 0 || string.IsNullOrEmpty(example))
        {
            return;
        }

        foreach (var mediaType in requestBody.Content!.Values)
        {
            mediaType.Example = XmlCommentsExampleHelper.Create(context.SchemaRepository, mediaType.Schema, example);
        }
    }

    private (string? Summary, string? Example) GetParamTags(ParameterInfo parameterInfo)
    {
        if (parameterInfo.Member is not MethodInfo methodInfo)
        {
            return (null, null);
        }

        var targetMethod = methodInfo.DeclaringType is not null && methodInfo.DeclaringType.IsConstructedGenericType
            ? methodInfo.GetUnderlyingGenericTypeMethod()
            : methodInfo;

        if (targetMethod is null)
        {
            return (null, null);
        }

        if (!_documentationProvider.TryGetXmlDoc(targetMethod, out var propertyNode))
        {
            return (null, null);
        }

        var paramNode = propertyNode.SelectFirstChildWithAttribute("param", "name", parameterInfo.Name);

        if (paramNode is null)
        {
            return (null, null);
        }

        var summary = XmlCommentsTextHelper.Humanize(paramNode.InnerXml);
        var example = paramNode.GetAttribute("example");

        return (summary, example);
    }

    private void ApplyParamTagsForBody(OpenApiRequestBody requestBody, RequestBodyFilterContext context, ParameterInfo parameterInfo)
    {
        var (summary, example) = GetParamTags(parameterInfo);

        if (summary is not null)
        {
            requestBody.Description = summary;
        }

        if (requestBody.Content?.Count is 0 || string.IsNullOrEmpty(example))
        {
            return;
        }

        foreach (var mediaType in requestBody.Content!.Values)
        {
            mediaType.Example = XmlCommentsExampleHelper.Create(context.SchemaRepository, mediaType.Schema, example);
        }
    }
}
