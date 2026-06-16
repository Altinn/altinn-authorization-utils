using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Altinn.Authorization.ModelUtils.Swashbuckle;

internal sealed class NonExhaustiveFlagsEnumSchemaFilter
    : ISchemaFilter
{
    private readonly Lazy<Func<SchemaGeneratorOptions>> _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="NonExhaustiveSchemaFilter"/> class.
    /// </summary>
    public NonExhaustiveFlagsEnumSchemaFilter(IServiceProvider serviceProvider)
    {
        _options = new(() =>
        {
            var monitor = serviceProvider.GetRequiredService<IOptionsMonitor<SchemaGeneratorOptions>>();
            return () => monitor.CurrentValue;
        });
    }

    public void Apply(IOpenApiSchema schema, SchemaFilterContext context)
    {
        if (NonExhaustiveFlagsEnum.IsNonExhaustiveFlagsEnumType(context.Type, out var innerType))
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

        openApiSchema.Type = JsonSchemaType.Array;
        openApiSchema.Properties = null;
        openApiSchema.AdditionalProperties = null;
        openApiSchema.AdditionalPropertiesAllowed = true;
        openApiSchema.Example = null;
        openApiSchema.Enum = null;
        openApiSchema.UniqueItems = true;
        openApiSchema.Items = GetSchema(typeof(NonExhaustiveEnum<>).MakeGenericType(innerType), context, generatorOptions);
    }

    private IOpenApiSchema GetSchema(Type type, SchemaFilterContext context, SchemaGeneratorOptions options)
        => EnsureRef(type, context, options);

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
            if (schema is not OpenApiSchema openApiSchema)
            {
                throw new InvalidOperationException($"Generated schema for '{type}' is not an {nameof(OpenApiSchema)}.");
            }

            referenceSchema = context.SchemaRepository.AddDefinition(id, openApiSchema);
            context.SchemaRepository.RegisterType(type, id);
            return referenceSchema;
        }

        if (context.SchemaRepository.TryLookupByType(type, out referenceSchema))
        {
            return referenceSchema;
        }

        if (context.SchemaRepository.Schemas.TryGetValue(id, out var existingSchema))
        {
            context.SchemaRepository.RegisterType(type, id);
            return existingSchema;
        }

        return schema;
    }
}
