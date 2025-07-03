using Altinn.Authorization.ModelUtils.FieldValueRecords;
using Altinn.Authorization.ModelUtils.FieldValueRecords.Converters;
using Altinn.Authorization.ModelUtils.FieldValueRecords.Polymorphic;
using CommunityToolkit.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Altinn.Authorization.ModelUtils.Swashbuckle;

internal sealed class PolymorphicFieldValueRecordSchemaFilter
    : ISchemaFilter
{
    private const string PROPS_SCHEMA_SUFFIX = "-BaseType";

    private readonly Lazy<Func<JsonSerializerOptions>> _jsonSerializerOptions;
    private readonly Lazy<Func<SchemaGeneratorOptions>> _options;
    private readonly ConcurrentDictionary<object, string> _discriminatorStrings = new();
    private readonly Func<object, string> _computeDiscriminatorStringValue;

    /// <summary>
    /// Initializes a new instance of the <see cref="PolymorphicFieldValueRecordSchemaFilter"/> class.
    /// </summary>
    public PolymorphicFieldValueRecordSchemaFilter(IServiceProvider serviceProvider)
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

        _computeDiscriminatorStringValue = ComputeDiscriminatorStringValue;
    }

    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        var jsonOptions = _jsonSerializerOptions.Value();
        var generatorOptions = _options.Value();

        if (context.Type.GetCustomAttribute<PolymorphicFieldValueRecordAttribute>() is null)
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

        if (converter is IPolymorphicFieldValueRecordJsonConverter polymorphicFieldValueRecordConverter)
        {
            Apply(schema, context, polymorphicFieldValueRecordConverter, generatorOptions);
        }
    }

    private void Apply(
        OpenApiSchema schema,
        SchemaFilterContext context,
        IPolymorphicFieldValueRecordJsonConverter converter,
        SchemaGeneratorOptions options)
    {
        if (converter.Model.Descendants.Length == 0)
        {
            // No descendants, treat this as a props-type
            ApplyProps(schema, context, converter, options);
            return;
        }

        OpenApiSchema? propsSchema = null;
        if (converter.Model.Constructor is not null)
        {
            // We can construct this type directly, thus we need a props-type
            var propsSchemaId = $"{options.SchemaIdSelector(converter.Model.Type)}{PROPS_SCHEMA_SUFFIX}";

            ref var propsSchemaSlot = ref CollectionsMarshal.GetValueRefOrAddDefault(context.SchemaRepository.Schemas, propsSchemaId, out var exists);
            if (!exists)
            {
                // clone generated schema
                propsSchemaSlot = new(schema);
                ApplyProps(propsSchemaSlot, context, converter, options);
            }

            propsSchema = new OpenApiSchema
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.Schema,
                    Id = propsSchemaId,
                },
            };
        }

        schema.Properties.Clear();
        schema.Required.Clear();

        var discriminator = schema.Discriminator ??= new();
        var mapping = discriminator.Mapping switch
        {
            Dictionary<string, string> dict => dict,
            _ => new Dictionary<string, string>(),
        };

        var oneOf = schema.OneOf switch
        {
            List<OpenApiSchema> list => list,
            _ => new(), 
        };

        schema.OneOf = oneOf;
        discriminator.Mapping = mapping;

        oneOf.Clear();
        mapping.Clear();

        discriminator.PropertyName = converter.DiscriminatorPropertyName;
        schema.Required.Add(discriminator.PropertyName);

        var seen = new Dictionary<string, OpenApiReference>();
        foreach (var descendant in converter.Model.Descendants)
        {
            var descendantSchemaRef = GetPropsSchemaRef(descendant, context, options);
            foreach (var discriminatorValue in descendant.Discriminators(includeDescendants: false))
            {
                var stringValue = GetDiscriminatorStringValue(discriminatorValue);
                ref var slot = ref CollectionsMarshal.GetValueRefOrAddDefault(mapping, stringValue, out var exists);
                if (exists)
                {
                    ThrowHelper.ThrowInvalidOperationException($"Discriminator value '{stringValue}' is already registered for type '{slot}'. Discriminator values must be unique.");
                }

                slot = descendantSchemaRef.ReferenceV3;
                seen[slot] = descendantSchemaRef;
            }
        }

        foreach (var reference in seen.Values)
        {
            oneOf.Add(new OpenApiSchema
            {
                Reference = reference,
            });
        }

        if (propsSchema is not null)
        {
            oneOf.Add(propsSchema);
        }
    }

    private void ApplyProps(
        OpenApiSchema schema,
        SchemaFilterContext context,
        IPolymorphicFieldValueRecordJsonConverter converter,
        SchemaGeneratorOptions options)
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
                if (converter.IsDiscriminatorProperty(propModel))
                {
                    var discriminatorValues = converter.Model.Discriminators(includeDescendants: true);
                    propSchema = CreateDiscriminatorSubset(converter.Model, discriminatorValues);
                }
                else
                {
                    propSchema = GetSchema(propModel, context, options);
                }

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

    private OpenApiSchema CreateDiscriminatorSubset(
        IPolymorphicFieldValueRecordModel model,
        IEnumerable<object> discriminatorValues)
    {
        var oneOf = new List<OpenApiSchema>();

        foreach (var value in discriminatorValues)
        {
            var stringValue = GetDiscriminatorStringValue(value);
            oneOf.Add(new OpenApiSchema
            {
                Type = "string",
                Enum = new List<IOpenApiAny>
                {
                    new OpenApiString(stringValue),
                },
            });
        }

        if (model.IsNonExhaustive)
        {
            oneOf.Add(new OpenApiSchema
            {
                Type = "string",
                Example = new OpenApiString("other string value"),
            });
        }

        var schema = new OpenApiSchema
        {
            Type = "string",
            OneOf = oneOf,
        };

        if (model.IsRoot && model.IsNonExhaustive)
        {
            schema.Example = new OpenApiString("other value");
        }

        return schema;
    }

    private string GetDiscriminatorStringValue(object discriminatorValue)
        => _discriminatorStrings.GetOrAdd(discriminatorValue, _computeDiscriminatorStringValue);

    private string ComputeDiscriminatorStringValue(object discriminatorValue)
    {
        Debug.Assert(discriminatorValue is not null);
        
        var type = discriminatorValue.GetType();
        Debug.Assert(type.IsEnum);

        using var stringDoc = JsonSerializer.SerializeToDocument(discriminatorValue, type, _jsonSerializerOptions.Value());
        Debug.Assert(stringDoc.RootElement.ValueKind == JsonValueKind.String);

        return stringDoc.RootElement.GetString()!;
    }

    private OpenApiReference GetPropsSchemaRef(IPolymorphicFieldValueRecordModel model, SchemaFilterContext context, SchemaGeneratorOptions options)
    {
        if (model.Descendants.Length == 0)
        {
            // leaf types are directly prop-types
            return EnsureRef(model.Type, context, options).Reference!;
        }

        var baseId = options.SchemaIdSelector(model.Type);
        var propsId = $"{baseId}{PROPS_SCHEMA_SUFFIX}";
        if (!context.SchemaRepository.Schemas.ContainsKey(propsId))
        {
            EnsureRef(model.Type, context, options);
        }

        return new()
        {
            Type = ReferenceType.Schema,
            Id = propsId,
        };
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
