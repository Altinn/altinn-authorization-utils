using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;

namespace Altinn.Swashbuckle.Filters;

internal abstract class AttributeFilter<T>
    : ISchemaFilter
{
    public void Apply(IOpenApiSchema schema, SchemaFilterContext context)
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

    protected abstract void Apply(T attribute, IOpenApiSchema schema, SchemaFilterContext context);
}
