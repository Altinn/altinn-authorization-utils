using System.Collections.Immutable;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Altinn.Urn.Json;

internal abstract class BaseUrnJsonWrapperConverterFactory
    : JsonConverterFactory
{
    private static ImmutableDictionary<Type, Func<BaseUrnJsonWrapperConverterFactory, JsonConverter>> _factories
        = ImmutableDictionary<Type, Func<BaseUrnJsonWrapperConverterFactory, JsonConverter>>.Empty;

    private readonly static MethodInfo CreateConverterMethod = typeof(BaseUrnJsonWrapperConverterFactory)
        .GetMethod(nameof(CreateConverter), BindingFlags.NonPublic | BindingFlags.Instance)!;

    protected abstract JsonConverter<TWrapper> CreateConverter<TWrapper, TUrn>()
        where TWrapper : IUrnJsonWrapper<TWrapper, TUrn>
        where TUrn : IKeyValueUrn<TUrn>;

    public override bool CanConvert(Type typeToConvert)
    {
        foreach (var iface in typeToConvert.GetInterfaces())
        {
            if (!iface.IsConstructedGenericType)
            {
                continue;
            }

            var genericType = iface.GetGenericTypeDefinition();
            if (genericType == typeof(IUrnJsonWrapper<,>))
            {
                return true;
            }
        }

        return false;
    }

    public sealed override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var factory = ImmutableInterlocked.GetOrAdd(ref _factories, typeToConvert, CreateFactory);
        return factory(this);

        static Func<BaseUrnJsonWrapperConverterFactory, JsonConverter> CreateFactory(Type type)
        {
            Type? wrapperType = null;
            Type? urnType = null;
            foreach (var iface in type.GetInterfaces())
            {
                if (!iface.IsConstructedGenericType)
                {
                    continue;
                }

                var genericType = iface.GetGenericTypeDefinition();
                if (genericType == typeof(IUrnJsonWrapper<,>))
                {
                    var args = iface.GetGenericArguments();
                    wrapperType = args[0];
                    urnType = args[1];
                    break;
                }
            }

            if (wrapperType is null || urnType is null)
            {
                throw new InvalidOperationException($"Type {type} does not implement IUrnJsonWrapper<,>");
            }

            var method = CreateConverterMethod.MakeGenericMethod(wrapperType, urnType);
            var argument = Expression.Parameter(typeof(BaseUrnJsonWrapperConverterFactory), "baseFactory");
            var call = Expression.Call(argument, method);
            var result = Expression.Convert(call, typeof(JsonConverter));
            var lambda = Expression.Lambda<Func<BaseUrnJsonWrapperConverterFactory, JsonConverter>>(result, argument);
            return lambda.Compile();
        }
    }
}
