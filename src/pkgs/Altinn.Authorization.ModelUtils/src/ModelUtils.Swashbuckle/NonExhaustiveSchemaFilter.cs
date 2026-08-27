using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Altinn.Authorization.ModelUtils.Swashbuckle;

/// <summary>
/// Schema filter for <see cref="NonExhaustive{T}"/> values.
/// </summary>
internal sealed class NonExhaustiveSchemaFilter
    : ISchemaFilter
{
    private readonly Lazy<Func<SchemaGeneratorOptions>> _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="NonExhaustiveSchemaFilter"/> class.
    /// </summary>
    public NonExhaustiveSchemaFilter(IServiceProvider serviceProvider)
    {
        _options = new(() =>
        {
            var monitor = serviceProvider.GetRequiredService<IOptionsMonitor<SchemaGeneratorOptions>>();
            return () => monitor.CurrentValue;
        });
    }

    public void Apply(IOpenApiSchema schema, SchemaFilterContext context)
    {
        if (NonExhaustive.IsNonExhaustiveType(context.Type, out var innerType))
        {
            Apply(schema, context, innerType);
        }
    }

    private void Apply(IOpenApiSchema schema, SchemaFilterContext context, Type innerType)
    {
        var generatorOptions = _options.Value();

        if (schema is not OpenApiSchema openApiSchema)
        {
            return;
        }

        openApiSchema.Type = null;
        openApiSchema.Properties = null;
        openApiSchema.AdditionalProperties = null;
        openApiSchema.AdditionalPropertiesAllowed = true;
        openApiSchema.OneOf = new List<IOpenApiSchema>
        {
            GetSchema(innerType, context, generatorOptions),
            new OpenApiSchema { Type = JsonSchemaType.String },
        };
        openApiSchema.Example = null;
        openApiSchema.Enum = null;
    }

    private IOpenApiSchema GetSchema(Type type, SchemaFilterContext context, SchemaGeneratorOptions options)
    {
        var schema = EnsureRef(type, context, options);

        return schema;
    }

    private IOpenApiSchema EnsureRef(Type type, SchemaFilterContext context, SchemaGeneratorOptions options)
    {
        if (context.SchemaRepository.TryLookupByType(type, out var referenceSchema))
        {
            return referenceSchema;
        }

        var id = options.SchemaIdSelector(type);
        var schema = context.SchemaGenerator.GenerateSchema(type, context.SchemaRepository);
        if (!context.SchemaRepository.Schemas.ContainsKey(id))
        {
            referenceSchema = context.SchemaRepository.AddDefinition(id, (OpenApiSchema)schema);
            context.SchemaRepository.RegisterType(type, id);
        }
        else
        {
            referenceSchema = new OpenApiSchemaReference(id, null);
        }

        return referenceSchema;
    }
}
