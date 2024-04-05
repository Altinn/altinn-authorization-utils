using System.Collections.Immutable;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Altinn.Urn.Json;

public abstract class BaseUrnJsonConverterFactory
    : JsonConverterFactory
{
    private static ImmutableDictionary<Type, Func<BaseUrnJsonConverterFactory, JsonConverter>> _factories
        = ImmutableDictionary<Type, Func<BaseUrnJsonConverterFactory, JsonConverter>>.Empty;

    private readonly static MethodInfo CreateConverterMethod = typeof(BaseUrnJsonConverterFactory)
        .GetMethod(nameof(CreateConverter), BindingFlags.NonPublic | BindingFlags.Instance)!;

    protected abstract BaseUrnJsonConverter<T> CreateConverter<T>()
        where T : IUrn<T>;

    public override bool CanConvert(Type typeToConvert)
    {
        if (!typeToConvert.IsAssignableTo(typeof(IUrn)))
        {
            return false;
        }

        try
        {
            var genericType = typeof(IUrn<>).MakeGenericType(typeToConvert);
            return typeToConvert.IsAssignableTo(genericType);
        }
        catch
        {
            return false;
        }
    }

    public sealed override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var factory = ImmutableInterlocked.GetOrAdd(ref _factories, typeToConvert, CreateFactory);

        return factory(this);

        static Func<BaseUrnJsonConverterFactory, JsonConverter> CreateFactory(Type type)
        {
            var method = CreateConverterMethod.MakeGenericMethod(type);
            var argument = Expression.Parameter(typeof(BaseUrnJsonConverterFactory), "baseFactory");
            var call = Expression.Call(argument, method);
            var result = Expression.Convert(call, typeof(JsonConverter));
            var lambda = Expression.Lambda<Func<BaseUrnJsonConverterFactory, JsonConverter>>(result, argument);
            return lambda.Compile();
        }
    }
}
