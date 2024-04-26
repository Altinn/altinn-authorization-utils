using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;

namespace Altinn.Swashbuckle.Filters;

internal class SchemaFilterAttributeFilter
    : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        var filters = context.Type.GetCustomAttributes()
            .Concat(context.ParameterInfo?.GetCustomAttributes() ?? [])
            .Concat(context.MemberInfo?.GetCustomAttributes() ?? [])
            .OfType<ISchemaFilter>();

        foreach (var filter in filters)
        {
            filter.Apply(schema, context);
        }
    }
}
