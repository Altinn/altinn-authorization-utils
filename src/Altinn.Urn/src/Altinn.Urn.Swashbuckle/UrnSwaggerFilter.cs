using Altinn.Swashbuckle.Examples;
using Altinn.Urn.Json;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Collections.Immutable;
using System.Diagnostics;

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

    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        var type = context.Type;
        if (type.IsConstructedGenericType)
        {
            var definition = type.GetGenericTypeDefinition();

            if (definition == typeof(UrnJsonString<>))
            {
                var urnType = type.GetGenericArguments()[0];
                GetFilterFor(urnType)?.ApplyUrnSchemaFilter(schema, urnType, context, _openApiExampleProvider);
                return;
            }
            else if (definition == typeof(UrnJsonTypeValue<>))
            {
                var urnType = type.GetGenericArguments()[0];
                GetFilterFor(urnType)?.ApplyUrnTypeValueObjectSchemaFilter(schema, urnType, context, _openApiExampleProvider);
                return;
            }
            else if (definition == typeof(KeyValueUrnDictionary<,>))
            {
                var urnType = type.GetGenericArguments()[0];
                GetFilterFor(urnType)?.ApplyUrnDictionarySchemaFilter(schema, urnType, context, _openApiExampleProvider);
                return;
            }
        }

        if (type.IsAssignableTo(typeof(IKeyValueUrn)))
        {
            GetFilterFor(type)?.ApplyUrnSchemaFilter(schema, type, context, _openApiExampleProvider);
            return;
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

            var keySchema = new OpenApiSchema
            {
                Type = "string",
                Enum = [],
            };

            foreach (var v in variants)
            {
                keySchema.Enum.Add(new OpenApiString(TUrn.CanonicalPrefixFor(v)));
            }

            var valueSchema = new OpenApiSchema
            {
                Type = "string",
            };

            // reset defaults
            schema.Properties.Clear();
            schema.Required.Clear();
            schema.AdditionalPropertiesAllowed = false;
            schema.AdditionalProperties = null;
            schema.Type = "object";
            schema.Properties.Add("type", keySchema);
            schema.Properties.Add("value", valueSchema);
            schema.Required.Add("type");
            schema.Required.Add("value");
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
            schema.Properties.Clear();
            schema.Required.Clear();
            schema.AdditionalPropertiesAllowed = false;
            schema.AdditionalProperties = null;
            schema.Type = "object";

            foreach (var v in variants)
            {
                var canonicalPrefix = TUrn.CanonicalPrefixFor(v);
                foreach (var prefix in TUrn.PrefixesFor(v))
                {
                    var valueSchema = context.SchemaGenerator.GenerateSchema(TUrn.ValueTypeFor(v), context.SchemaRepository);
                    var isCanonical = string.Equals(prefix, canonicalPrefix, StringComparison.Ordinal);
                    if (!isCanonical)
                    {
                        valueSchema.Deprecated = true;
                    }

                    schema.Properties.Add(prefix, valueSchema);
                }
            }
        }

        private void ApplyBaseUrnFilter(OpenApiSchema schema, Type type, SchemaFilterContext context, OpenApiExampleProvider exampleProvider)
        {
            // reset defaults
            schema.Properties.Clear();
            schema.Required.Clear();
            schema.AdditionalPropertiesAllowed = false;
            schema.AdditionalProperties = null;
            schema.Type = "string";
            schema.Format = "urn";
            var oneOf = schema.OneOf;
            oneOf.Clear();

            foreach (var variant in TUrn.Variants)
            {
                var variantType = TUrn.VariantTypeFor(variant);
                if (context.SchemaRepository.TryLookupByType(variantType, out var referenceSchema))
                {
                    oneOf.Add(referenceSchema);
                    continue;
                }

                var variantSchema = context.SchemaGenerator.GenerateSchema(variantType, context.SchemaRepository);
                oneOf.Add(variantSchema);
            }

            schema.Example = exampleProvider.GetExample(type)?.FirstOrDefault();
        }


        private void ApplyVariantUrnFilter(OpenApiSchema schema, Type type, SchemaFilterContext context, TVariants variant, OpenApiExampleProvider exampleProvider)
        {
            // reset defaults
            schema.Properties.Clear();
            schema.Required.Clear();
            schema.AdditionalPropertiesAllowed = false;
            schema.AdditionalProperties = null;
            schema.OneOf.Clear();

            schema.Type = "string";
            schema.Format = "urn";
            schema.Example = exampleProvider.GetExample(type)?.FirstOrDefault();

            var prefixes = TUrn.PrefixesFor(variant);

            var pattern = GetPattern(prefixes);
            schema.Pattern = pattern;
        }

        [ThreadStatic]
        private static RegexBuilder? _regexBuilder;
        private static string GetPattern(ReadOnlySpan<string> prefixes)
        {
            var builder = _regexBuilder ??= new(prefixes[0].Length * 10);
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
}
