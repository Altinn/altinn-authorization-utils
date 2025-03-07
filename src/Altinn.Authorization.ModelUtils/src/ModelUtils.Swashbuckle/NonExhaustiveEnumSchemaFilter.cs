using Altinn.Authorization.ModelUtils.Swashbuckle.OpenApi;
using Altinn.Swashbuckle.XmlDoc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Extensions;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Reflection;
using System.Text.Json;
using System.Xml.Linq;
using System.Xml.XPath;

namespace Altinn.Authorization.ModelUtils.Swashbuckle;

/// <summary>
/// Schema filter for <see cref="NonExhaustiveEnum{T}"/> values.
/// </summary>
internal class NonExhaustiveEnumSchemaFilter
    : ISchemaFilter
{
    private readonly static ConcurrentDictionary<Type, ConstructorInvoker> _wrappers = new();

    private readonly Lazy<Func<JsonSerializerOptions>> _jsonSerializerOptions;
    private readonly IXmlDocProvider _xmlDocProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="NonExhaustiveEnumSchemaFilter"/> class.
    /// </summary>
    public NonExhaustiveEnumSchemaFilter(IServiceProvider serviceProvider, IXmlDocProvider xmlDocProvider)
    {
        _xmlDocProvider = xmlDocProvider;

        _jsonSerializerOptions = new(() =>
        {
            if (serviceProvider.GetService<IOptionsMonitor<Microsoft.AspNetCore.Mvc.JsonOptions>>() is { } mvcJsonOptions)
            {
                return () => mvcJsonOptions.CurrentValue.JsonSerializerOptions;
            }

            if (serviceProvider.GetService<IOptionsMonitor<Microsoft.AspNetCore.Http.Json.JsonOptions>>() is { } httpJsonOptions)
            {
                return () => httpJsonOptions.CurrentValue.SerializerOptions;
            }

#if NET9_0_OR_GREATER
            return () => JsonSerializerOptions.Web;
#else
            var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
            return () => options;
#endif
        });
    }

    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (NonExhaustiveEnum.IsNonExhaustiveEnumType(context.Type, out var enumType))
        {
            Apply(schema, context, enumType);
        }
    }

    private void Apply(OpenApiSchema schema, SchemaFilterContext context, Type enumType)
    {
        var type = context.Type;
        var options = _jsonSerializerOptions.Value();

        var extensibleEnum = new ExtensibleEnumOpenApiExtension();
        var oneOf = new List<OpenApiSchema>();

        var wrapper = _wrappers.GetOrAdd(type, CreateWrapper);
        foreach (var enumVal in enumType.GetEnumValues().Cast<object>())
        {
            using var serialized = JsonSerializer.SerializeToDocument(wrapper.Invoke(enumVal), type, options);
            Debug.Assert(serialized.RootElement.ValueKind == JsonValueKind.String);
            var stringValue = serialized.RootElement.GetString()!;

            var title = enumVal.ToString();
            string? description = null;

            var field = enumType.GetField(title!, BindingFlags.Public | BindingFlags.Static);
            if (field is not null && _xmlDocProvider.TryGetXmlDoc(field, out var fieldNode))
            {
                var summaryNode = fieldNode.SelectChildren("summary", string.Empty)
                    ?.OfType<XPathNavigator>()
                    .FirstOrDefault();
                
                if (summaryNode != null)
                {
                    description = XmlCommentsTextHelper.Humanize(summaryNode.InnerXml);
                }
            }

            // TODO: Get description
            var valSchema = new OpenApiSchema
            {
                Type = "string",
                Enum = [new OpenApiString(stringValue)],
                Title = title,
                Description = description,
            };

            var enumValue = new ExtensibleEnumValue
            {
                Value = stringValue,
                Title = title,
                Description = description,
            };

            extensibleEnum.Add(enumValue);
            oneOf.Add(valSchema);
        }

        oneOf.Add(new OpenApiSchema
        {
            Type = "string",
            Example = new OpenApiString("other string value"),
        });

        schema.Type = null;
        schema.Properties = null;
        schema.AdditionalProperties = null;
        schema.AdditionalPropertiesAllowed = true;
        schema.OneOf = oneOf;
        schema.Example = null;
        schema.AddExtension("x-extensible-enum", extensibleEnum);
    }

    private static ConstructorInvoker CreateWrapper(Type type)
    {
        Debug.Assert(NonExhaustiveEnum.IsNonExhaustiveEnumType(type, out var enumType));
        var ctor = type.GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, [enumType]);
        Debug.Assert(ctor is not null);
        return ConstructorInvoker.Create(ctor);
    }
}

