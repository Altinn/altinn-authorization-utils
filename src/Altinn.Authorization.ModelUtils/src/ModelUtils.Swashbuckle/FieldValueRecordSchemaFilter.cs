using Altinn.Authorization.ModelUtils.FieldValueRecords;
using Altinn.Authorization.ModelUtils.FieldValueRecords.Converters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Altinn.Authorization.ModelUtils.Swashbuckle;

/// <summary>
/// Schema filter for field-value-record types.
/// </summary>
internal sealed class FieldValueRecordSchemaFilter
    : ISchemaFilter
{
    private readonly Lazy<Func<JsonSerializerOptions>> _jsonSerializerOptions;
    private readonly Lazy<Func<SchemaGeneratorOptions>> _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="FieldValueRecordSchemaFilter"/> class.
    /// </summary>
    public FieldValueRecordSchemaFilter(IServiceProvider serviceProvider)
    {
        _options = new(() =>
        {
            var monitor = serviceProvider.GetRequiredService<IOptionsMonitor<SchemaGeneratorOptions>>();
            return () => monitor.CurrentValue;
        });

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

            return () => JsonSerializerOptions.Web;
        });
    }

    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        var jsonOptions = _jsonSerializerOptions.Value();
        var generatorOptions = _options.Value();

        if (context.Type.GetCustomAttribute<FieldValueRecordAttribute>() is null)
        {
            // Not a field-value-record type
            return;
        }

        JsonConverter converter;
        try
        {
            converter = jsonOptions.GetConverter(context.Type);
        }
        catch (InvalidOperationException)
        {
            return;
        }

        if (converter is IFieldValueRecordJsonConverter fieldValueRecordConverter)
        {
            Apply(schema, context, fieldValueRecordConverter, generatorOptions);
        }
    }

    private void Apply(OpenApiSchema schema, SchemaFilterContext context, IFieldValueRecordJsonConverter converter, SchemaGeneratorOptions options)
    {
        var required = schema.Required;
        var props = schema.Properties switch
        {
            Dictionary<string, OpenApiSchema> dict => dict,
            _ => new Dictionary<string, OpenApiSchema>(schema.Properties),
        };

        schema.Properties = props;

        foreach (var name in props.Keys)
        {
            ref var propSchema = ref CollectionsMarshal.GetValueRefOrNullRef(props, name);
            Debug.Assert(!Unsafe.IsNullRef(ref propSchema));

            if (converter.TryFindPropertyModel(name, out var propModel))
            {
                propSchema = GetSchema(propModel, context, options);
                if (propModel.IsRequired)
                {
                    required.Add(name);
                }
                else
                {
                    required.Remove(name);
                }
            }
        }
    }

    private OpenApiSchema GetSchema(IFieldValueRecordPropertyModel model, SchemaFilterContext context, SchemaGeneratorOptions options)
    {
        var schema = EnsureRef(model.Type, context, options);

        // TODO: nullability does not work properly with $ref schemas: https://stackoverflow.com/questions/40920441/how-to-specify-a-property-can-be-null-or-a-reference-with-swagger

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
