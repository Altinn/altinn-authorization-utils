using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
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

    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (NonExhaustiveFlagsEnum.IsNonExhaustiveFlagsEnumType(context.Type, out var innerType))
        {
            Apply(schema, context, innerType);
        }
    }

    private void Apply(OpenApiSchema schema, SchemaFilterContext context, Type innerType)
    {
        var generatorOptions = _options.Value();

        schema.Type = "array";
        schema.Properties = null;
        schema.AdditionalProperties = null;
        schema.AdditionalPropertiesAllowed = true;
        schema.Example = null;
        schema.Enum = null;
        schema.UniqueItems = true;
        schema.Items = GetSchema(typeof(NonExhaustiveEnum<>).MakeGenericType(innerType), context, generatorOptions);
    }

    private OpenApiSchema GetSchema(Type type, SchemaFilterContext context, SchemaGeneratorOptions options)
    {
        var schema = EnsureRef(type, context, options);

        return schema;
    }

    private OpenApiSchema EnsureRef(Type type, SchemaFilterContext context, SchemaGeneratorOptions options)
    {
        if (context.SchemaRepository.TryLookupByType(type, out var referenceSchema))
        {
            return referenceSchema;
        }

        var id = options.SchemaIdSelector(type);
        var schema = context.SchemaGenerator.GenerateSchema(type, context.SchemaRepository);
        if (!context.SchemaRepository.Schemas.ContainsKey(id))
        {
            referenceSchema = context.SchemaRepository.AddDefinition(id, schema);
            context.SchemaRepository.RegisterType(type, id);
        }
        else
        {
            referenceSchema = new()
            {
                Reference = new()
                {
                    Type = ReferenceType.Schema,
                    Id = id,
                },
            };
        }

        return referenceSchema;
    }
}
