using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;

namespace Altinn.Swashbuckle.Filters;

internal abstract class AttributeFilter<T>
    : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        var filters = context.Type.GetCustomAttributes()
            .Concat(context.ParameterInfo?.GetCustomAttributes() ?? [])
            .Concat(context.MemberInfo?.GetCustomAttributes() ?? [])
            .OfType<T>();

        foreach (var filter in filters)
        {
            Apply(filter, schema, context);
        }
    }

    protected abstract void Apply(T attribute, OpenApiSchema schema, SchemaFilterContext context);
}
