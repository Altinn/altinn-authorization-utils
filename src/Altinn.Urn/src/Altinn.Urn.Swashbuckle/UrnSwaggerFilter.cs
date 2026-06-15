using Altinn.Swashbuckle.Examples;
using Altinn.Urn.Json;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Text.Json.Nodes;

namespace Altinn.Urn.Swashbuckle;

internal class UrnSwaggerFilter
    : ISchemaFilter
{
    private static ImmutableDictionary<Type, UrnTypeSchemaFilter?> _types
        = ImmutableDictionary<Type, UrnTypeSchemaFilter?>.Empty;

    private readonly OpenApiExampleProvider _openApiExampleProvider;

    public UrnSwaggerFilter(OpenApiExampleProvider openApiExampleProvider)
    {
        _openApiExampleProvider = openApiExampleProvider;
    }

    public void Apply(IOpenApiSchema schema, SchemaFilterContext context)
    {
        if (schema is not OpenApiSchema openApiSchema)
        {
            return;
        }

        var type = context.Type;
        if (type.IsConstructedGenericType)
        {
            var definition = type.GetGenericTypeDefinition();

            if (definition == typeof(UrnJsonString<>))
            {
                var urnType = type.GetGenericArguments()[0];
                GetFilterFor(urnType)?.ApplyUrnSchemaFilter(openApiSchema, urnType, context, _openApiExampleProvider);
                return;
            }
            else if (definition == typeof(UrnJsonTypeValue<>) || definition == typeof(UrnJsonTypeValueVariant<>))
            {
                var urnType = type.GetGenericArguments()[0];
                GetFilterFor(urnType)?.ApplyUrnTypeValueObjectSchemaFilter(openApiSchema, urnType, context, _openApiExampleProvider);
                return;
            }
            else if (definition == typeof(KeyValueUrnDictionary<,>))
            {
                var urnType = type.GetGenericArguments()[0];
                GetFilterFor(urnType)?.ApplyUrnDictionarySchemaFilter(openApiSchema, urnType, context, _openApiExampleProvider);
                return;
            }
        }

        if (type.IsAssignableTo(typeof(IKeyValueUrn)))
        {
            GetFilterFor(type)?.ApplyUrnSchemaFilter(openApiSchema, type, context, _openApiExampleProvider);
            return;
        }

        if (type == typeof(UrnJsonTypeValue))
        {
            // reset defaults
            openApiSchema.Properties?.Clear();
            openApiSchema.Required?.Clear();
            openApiSchema.AdditionalPropertiesAllowed = false;
            openApiSchema.AdditionalProperties = null;
            openApiSchema.Type = JsonSchemaType.Object;
            openApiSchema.Properties ??= new Dictionary<string, IOpenApiSchema>();
            openApiSchema.Properties.Add("type", new OpenApiSchema
            {
                Type = JsonSchemaType.String,
                Pattern = "^urn:.+[^:]$",
            });
            openApiSchema.Properties.Add("value", new OpenApiSchema
            {
                Type = JsonSchemaType.String,
            });
            openApiSchema.Required ??= new HashSet<string>();
            openApiSchema.Required.Add("type");
            openApiSchema.Required.Add("value");
        }
    }

    private static UrnTypeSchemaFilter? GetFilterFor(Type type)
    {
        return ImmutableInterlocked.GetOrAdd(ref _types, type, CreateFilter);

        static UrnTypeSchemaFilter? CreateFilter(Type type)
        {
            var iface = type.GetInterfaces().FirstOrDefault(i => i.IsConstructedGenericType && i.GetGenericTypeDefinition() == typeof(IKeyValueUrn<,>));
            if (iface is null)
            {
                return null;
            }

            var urnType = iface.GetGenericArguments()[0];
            var variantEnumType = iface.GetGenericArguments()[1];

            if (urnType != type)
            {
                return GetFilterFor(urnType);
            }

            var filterType = typeof(UrnTypeSchemaFilter<,>).MakeGenericType(urnType, variantEnumType);
            return (UrnTypeSchemaFilter?)Activator.CreateInstance(filterType);
        }
    }

    private abstract class UrnTypeSchemaFilter
    {
        public abstract void ApplyUrnSchemaFilter(OpenApiSchema schema, Type type, SchemaFilterContext context, OpenApiExampleProvider exampleProvider);
        public abstract void ApplyUrnTypeValueObjectSchemaFilter(OpenApiSchema schema, Type type, SchemaFilterContext context, OpenApiExampleProvider exampleProvider);
        public abstract void ApplyUrnDictionarySchemaFilter(OpenApiSchema schema, Type type, SchemaFilterContext context, OpenApiExampleProvider exampleProvider);
    }

    private sealed class UrnTypeSchemaFilter<TUrn, TVariants>
        : UrnTypeSchemaFilter
        where TUrn : IKeyValueUrn<TUrn, TVariants>
        where TVariants : struct, Enum
    {
        public override void ApplyUrnSchemaFilter(OpenApiSchema schema, Type type, SchemaFilterContext context, OpenApiExampleProvider exampleProvider)
        {
            if (type == typeof(TUrn))
            {
                ApplyBaseUrnFilter(schema, type, context, exampleProvider);
                return;
            }

            Debug.Assert(typeof(TUrn).IsAssignableFrom(type));
            foreach (var variant in TUrn.Variants)
            {
                if (TUrn.VariantTypeFor(variant) == type)
                {
                    ApplyVariantUrnFilter(schema, type, context, variant, exampleProvider);
                    return;
                }
            }
        }

        public override void ApplyUrnTypeValueObjectSchemaFilter(OpenApiSchema schema, Type type, SchemaFilterContext context, OpenApiExampleProvider exampleProvider)
        {
            if (type == typeof(TUrn))
            {
                ApplyBaseUrnTypeValueObjectFilter(schema, type, context, exampleProvider);
                return;
            }

            Debug.Assert(typeof(TUrn).IsAssignableFrom(type));
            foreach (var variant in TUrn.Variants)
            {
                if (TUrn.VariantTypeFor(variant) == type)
                {
                    ApplyVariantUrnTypeValueObjectFilter(schema, type, context, variant, exampleProvider);
                    return;
                }
            }
        }

        public override void ApplyUrnDictionarySchemaFilter(OpenApiSchema schema, Type type, SchemaFilterContext context, OpenApiExampleProvider exampleProvider)
        {
            TVariants variant = default;

            if (type != typeof(TUrn))
            {
                // we're dealing with a variant type
                Debug.Assert(typeof(TUrn).IsAssignableFrom(type));
                foreach (var v in TUrn.Variants)
                {
                    if (TUrn.VariantTypeFor(v) == type)
                    {
                        variant = v;
                        break;
                    }
                }
            }

            ReadOnlySpan<TVariants> variants = [variant];

            if (type == typeof(TUrn))
            {
                variants = TUrn.Variants;
            }

            // reset defaults
            schema.Properties?.Clear();
            schema.Required?.Clear();
            schema.AdditionalPropertiesAllowed = false;
            schema.AdditionalProperties = null;
            schema.Type = JsonSchemaType.Object;
            schema.Properties ??= new Dictionary<string, IOpenApiSchema>();

            foreach (var v in variants)
            {
                var canonicalPrefix = TUrn.CanonicalPrefixFor(v);
                foreach (var prefix in TUrn.PrefixesFor(v))
                {
                    var valueSchema = context.SchemaGenerator.GenerateSchema(TUrn.ValueTypeFor(v), context.SchemaRepository);
                    var isCanonical = string.Equals(prefix, canonicalPrefix, StringComparison.Ordinal);
                    if (!isCanonical && valueSchema is OpenApiSchema concreteValueSchema)
                    {
                        concreteValueSchema.Deprecated = true;
                    }

                    schema.Properties.Add(prefix, valueSchema);
                }
            }
        }

        private static void ApplyBaseUrnTypeValueObjectFilter(OpenApiSchema schema, Type type, SchemaFilterContext context, OpenApiExampleProvider exampleProvider)
        {
            // reset defaults
            schema.Properties?.Clear();
            schema.Required?.Clear();
            schema.AdditionalPropertiesAllowed = false;
            schema.AdditionalProperties = null;
            schema.Type = JsonSchemaType.Object;
            schema.Properties ??= new Dictionary<string, IOpenApiSchema>();

            var mapping = new Dictionary<string, OpenApiSchemaReference>();
            schema.OneOf ??= new List<IOpenApiSchema>();
            var oneOf = schema.OneOf;
            oneOf.Clear();
            schema.Discriminator = new OpenApiDiscriminator
            {
                PropertyName = "type",
                Mapping = mapping,
            };
            schema.Properties.Add("type", new OpenApiSchema
            {
                Type = JsonSchemaType.String,
                Enum = [.. TUrn.Prefixes],
            });
            schema.Required ??= new HashSet<string>();
            schema.Required.Add("type");

            foreach (var variant in TUrn.Variants)
            {
                var variantType = typeof(UrnJsonTypeValueVariant<>).MakeGenericType(TUrn.VariantTypeFor(variant));
                OpenApiSchemaReference referenceSchema;
                if (!context.SchemaRepository.TryLookupByType(variantType, out var lookedUp))
                {
                    referenceSchema = (OpenApiSchemaReference)context.SchemaGenerator.GenerateSchema(variantType, context.SchemaRepository);
                }
                else
                {
                    referenceSchema = lookedUp;
                }

                oneOf?.Add(referenceSchema);
                foreach (var prefix in TUrn.PrefixesFor(variant))
                {
                    mapping.Add(prefix, referenceSchema);
                }
            }

            schema.Example = exampleProvider.GetExample(typeof(UrnJsonTypeValue<>).MakeGenericType(type))?.FirstOrDefault();
        }

        private static void ApplyVariantUrnTypeValueObjectFilter(OpenApiSchema schema, Type type, SchemaFilterContext context, TVariants variant, OpenApiExampleProvider exampleProvider)
        {
            // reset defaults
            schema.Properties?.Clear();
            schema.Required?.Clear();
            schema.AdditionalPropertiesAllowed = false;
            schema.AdditionalProperties = null;
            schema.Type = JsonSchemaType.Object;
            schema.Properties ??= new Dictionary<string, IOpenApiSchema>();

            schema.OneOf?.Clear();
            schema.Properties.Add("type", new OpenApiSchema
            {
                Type = JsonSchemaType.String,
                Enum = [.. TUrn.PrefixesFor(variant)],
            });
            schema.Required ??= new HashSet<string>();
            schema.Required.Add("type");

            var valueType = TUrn.ValueTypeFor(variant);
            schema.Properties.Add("value", new OpenApiSchema
            {
                Type = JsonSchemaType.String,
            });
            schema.Required.Add("value");

            schema.Example = new JsonObject
            {
                ["type"] = JsonValue.Create(TUrn.CanonicalPrefixFor(variant)),
                ["value"] = exampleProvider.GetExample(valueType, typeof(string), static (object? v) => v switch
                {
                    null => null,
                    IFormattable f => f.ToString(format: null, formatProvider: null),
                    _ => v?.ToString() ?? "string",
                })?.FirstOrDefault() ?? JsonValue.Create("string"),
            };
        }

        private static void ApplyBaseUrnFilter(OpenApiSchema schema, Type type, SchemaFilterContext context, OpenApiExampleProvider exampleProvider)
        {
            // reset defaults
            schema.Properties?.Clear();
            schema.Required?.Clear();
            schema.AdditionalPropertiesAllowed = false;
            schema.AdditionalProperties = null;
            schema.Type = JsonSchemaType.String;
            schema.Format = "urn";
            schema.OneOf ??= new List<IOpenApiSchema>();
            var oneOf = schema.OneOf;
            oneOf.Clear();

            foreach (var variant in TUrn.Variants)
            {
                var variantType = TUrn.VariantTypeFor(variant);
                if (context.SchemaRepository.TryLookupByType(variantType, out var referenceSchema))
                {
                    oneOf?.Add(referenceSchema);
                    continue;
                }

                var variantSchema = context.SchemaGenerator.GenerateSchema(variantType, context.SchemaRepository);
                oneOf?.Add(variantSchema);
            }

            schema.Example = exampleProvider.GetExample(type)?.FirstOrDefault();
        }

        private static void ApplyVariantUrnFilter(OpenApiSchema schema, Type type, SchemaFilterContext context, TVariants variant, OpenApiExampleProvider exampleProvider)
        {
            // reset defaults
            schema.Properties?.Clear();
            schema.Required?.Clear();
            schema.AdditionalPropertiesAllowed = false;
            schema.AdditionalProperties = null;
            schema.OneOf?.Clear();

            schema.Type = JsonSchemaType.String;
            schema.Format = "urn";
            schema.Example = exampleProvider.GetExample(type)?.FirstOrDefault();

            var prefixes = TUrn.PrefixesFor(variant);

            var pattern = GetPattern(prefixes);
            schema.Pattern = pattern;
        }

        private static string GetPattern(ReadOnlySpan<string> prefixes)
        {
            var builder = RegexBuilder.ThreadStaticInstance;
            builder.Clear();

            builder.AppendRaw("^urn:");
            if (prefixes.Length == 1)
            {
                builder.AppendEscaped(prefixes[0].AsSpan(4));
            }
            else
            {
                var first = true;
                foreach (var prefix in prefixes)
                {
                    if (first)
                    {
                        first = false;
                    }
                    else
                    {
                        builder.AppendRaw('|');
                    }

                    builder.AppendRaw("(?:");
                    builder.AppendEscaped(prefix.AsSpan(4));
                    builder.AppendRaw(')');
                }
            }
            builder.AppendRaw(":.+$");

            return builder.ToString();
        }
    }

    private abstract class UrnJsonTypeValueVariant<T>();
}
