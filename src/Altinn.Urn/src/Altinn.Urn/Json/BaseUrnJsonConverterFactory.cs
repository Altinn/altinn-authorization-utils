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

    private readonly InnerFactory _inner;

    protected BaseUrnJsonConverterFactory()
    {
        _inner = new InnerFactory(this);
    }

    protected abstract BaseUrnJsonConverter<T> CreateConverter<T>()
        where T : IUrn<T>;

    public override bool CanConvert(Type typeToConvert)
    {
        return true;
    }

    public sealed override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        if (_inner.CanConvert(typeToConvert))
        {
            return _inner.CreateConverter(typeToConvert, options);
        }

        var childOptions = new JsonSerializerOptions(options);
        childOptions.Converters.Insert(0, _inner);
        childOptions.MakeReadOnly();

        var type = typeof(ChildConverter<>).MakeGenericType(typeToConvert);
        return (JsonConverter)Activator.CreateInstance(type, childOptions)!;
    }

    private JsonConverter GetUrnConverter(Type urnType)
    {
        var factory = ImmutableInterlocked.GetOrAdd(ref _factories, urnType, CreateFactory);
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

    private static bool IsUrnType(Type type)
    {
        if (!type.IsAssignableTo(typeof(IUrn)))
        {
            return false;
        }

        try
        {
            var genericType = typeof(IUrn<>).MakeGenericType(type);
            return type.IsAssignableTo(genericType);
        }
        catch
        {
            return false;
        }
    }

    private sealed class InnerFactory 
        : JsonConverterFactory
    {
        private readonly BaseUrnJsonConverterFactory _factory;

        public InnerFactory(BaseUrnJsonConverterFactory factory)
        {
            _factory = factory;
        }

        public override bool CanConvert(Type typeToConvert)
            => IsUrnType(typeToConvert);

        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
            => _factory.GetUrnConverter(typeToConvert);
    }

    private sealed class ChildConverter<T>
        : JsonConverter<T>
    {
        private readonly JsonSerializerOptions _options;

        public ChildConverter(JsonSerializerOptions options)
        {
            _options = options;
        }

        public override bool CanConvert(Type typeToConvert) 
            => IsUrnType(typeToConvert);

        public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            => JsonSerializer.Deserialize<T>(ref reader, _options);

        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
            => JsonSerializer.Serialize(writer, value, _options);
    }
}
