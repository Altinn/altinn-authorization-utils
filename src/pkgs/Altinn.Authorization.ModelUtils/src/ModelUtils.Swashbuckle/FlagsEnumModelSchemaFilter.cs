using Altinn.Authorization.ModelUtils.AspNet;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Collections.Concurrent;

namespace Altinn.Authorization.ModelUtils.Swashbuckle;

internal sealed class FlagsEnumModelSchemaFilter
    : ISchemaFilter
{
    private static readonly ConcurrentDictionary<Type, ISchemaFilter> _inner = new();

    public void Apply(IOpenApiSchema schema, SchemaFilterContext context)
    {
        if (FlagsEnum.IsFlagsEnumModelType(context.Type, out var enumType))
        {
            var inner = _inner.GetOrAdd(enumType, static t => CreateSchemaFilter(t));
            inner.Apply(schema, context);
        }

        static ISchemaFilter CreateSchemaFilter(Type enumType)
        {
            var schemaFilterType = typeof(InnerSchemaFilter<>).MakeGenericType(enumType);
            return (ISchemaFilter)Activator.CreateInstance(schemaFilterType)!;
        }
    }

    private sealed class InnerSchemaFilter<TEnum>
        : ISchemaFilter
        where TEnum : struct, Enum
    {
        public void Apply(IOpenApiSchema schema, SchemaFilterContext context)
        {
            if (schema is not OpenApiSchema openApiSchema)
            {
                return;
            }
            openApiSchema.Enum = null;
            openApiSchema.Format = null;
            openApiSchema.Properties = null;
            openApiSchema.Required = null;
            openApiSchema.AdditionalProperties = null;
            openApiSchema.Type = JsonSchemaType.Array;
            openApiSchema.Items = new OpenApiSchema
            {
                Type = JsonSchemaType.String,
                // TODO: Figure out how to make this an extensible enum
                // Enum = [.. FlagsEnum<TEnum>.Model.Items.Select(v => (IOpenApiAny)new OpenApiString(v.Name))],
            };
        }
    }
}
