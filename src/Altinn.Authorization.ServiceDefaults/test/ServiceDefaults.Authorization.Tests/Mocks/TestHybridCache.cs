#if NET9_0_OR_GREATER
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Caching.Memory;
using System.Buffers;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;

namespace Altinn.Authorization.ServiceDefaults.Authorization.Tests.Mocks;

public sealed class TestHybridCache
    : HybridCache
{
    private readonly IMemoryCache _cache = new MemoryCache(new MemoryCacheOptions());
    private readonly Dictionary<string, List<string>> _tagMap = new();
    private readonly Lock _lock = new();

    public override ValueTask<T> GetOrCreateAsync<TState, T>(
        string key,
        TState state,
        Func<TState, CancellationToken, ValueTask<T>> factory,
        HybridCacheEntryOptions? options = null,
        IEnumerable<string>? tags = null,
        CancellationToken cancellationToken = default)
    {
        var serializer = GetSerializer<T>();
        if (_cache.TryGetValue(key, out byte[]? cacheData)
            && cacheData is not null)
        {
            return ValueTask.FromResult(Deserialize(serializer, cacheData));
        }

        return CreateAndGetAsync(serializer, key, state, factory, options, tags, cancellationToken);
    }

    public override ValueTask RemoveAsync(
        string key,
        CancellationToken cancellationToken = default)
    {
        _cache.Remove(key);
        return ValueTask.CompletedTask;
    }

    public override ValueTask RemoveByTagAsync(
        string tag,
        CancellationToken cancellationToken = default)
    {
        List<string>? keys;
        lock (_lock)
        {
            if (!_tagMap.Remove(tag, out keys))
            {
                return ValueTask.CompletedTask;
            }
        }

        foreach (var key in keys)
        {
            _cache.Remove(key);
        }

        return ValueTask.CompletedTask;
    }

    public override async ValueTask SetAsync<T>(
        string key,
        T value,
        HybridCacheEntryOptions? options = null,
        IEnumerable<string>? tags = null,
        CancellationToken cancellationToken = default)
    {
        var serializer = GetSerializer<T>();

        await SetCoreAsync(serializer, key, value, options, tags, cancellationToken);
    }

    private async ValueTask<T> CreateAndGetAsync<TState, T>(
        IHybridCacheSerializer<T> serializer,
        string key,
        TState state,
        Func<TState, CancellationToken, ValueTask<T>> factory,
        HybridCacheEntryOptions? options,
        IEnumerable<string>? tags,
        CancellationToken cancellationToken)
    {
        var value = await factory(state, cancellationToken);
        var cacheData = await SetCoreAsync(serializer, key, value, options, tags, cancellationToken);

        return Deserialize(serializer, cacheData);
    }

    private async ValueTask<byte[]> SetCoreAsync<T>(
        IHybridCacheSerializer<T> serializer,
        string key,
        T value,
        HybridCacheEntryOptions? options,
        IEnumerable<string>? tags,
        CancellationToken cancellationToken)
    {
        var serialized = Serialize(serializer, value);
        using var entry = _cache.CreateEntry(key);

        if (options is not null)
        {
            TimeSpan? exp = options switch
            {
                { Expiration: { } expiration, LocalCacheExpiration: null } => expiration,
                { Expiration: null, LocalCacheExpiration: { } localCacheExpiration } => localCacheExpiration,
                { Expiration: { } expiration, LocalCacheExpiration: { } localCacheExpiration } =>
                    expiration < localCacheExpiration ? expiration : localCacheExpiration,
                _ => null,
            };

            if (exp.HasValue)
            {
                entry.AbsoluteExpirationRelativeToNow = exp.Value;
            }
        }

        entry.Value = serialized;

        if (tags is not null)
        {

            foreach (var tag in tags)
            {
                lock (_lock)
                {
                    ref var keys = ref CollectionsMarshal.GetValueRefOrAddDefault(_tagMap, tag, out var exists);
                    if (!exists || keys is null)
                    {
                        keys = [];
                    }

                    keys.Add(key);
                }
            }
        }

        return serialized;
    }

    private static byte[] Serialize<T>(IHybridCacheSerializer<T> serializer, T value)
    {
        var writer = new ArrayBufferWriter<byte>();
        serializer.Serialize(value, writer);
        return writer.WrittenSpan.ToArray();
    }

    private static T Deserialize<T>(IHybridCacheSerializer<T> serializer, byte[] data)
    {
        var sequence = new ReadOnlySequence<byte>(data);
        return serializer.Deserialize(sequence);
    }

    private static IHybridCacheSerializer<T> GetSerializer<T>()
    {
        if (typeof(T) == typeof(byte[]))
        {
            return (IHybridCacheSerializer<T>)(object)BinarySerializer.Instance;
        }
        else if (typeof(T) == typeof(string))
        {
            return (IHybridCacheSerializer<T>)(object)BinarySerializer.Instance;
        }
        else
        {
            return JsonBasedSerializer<T>.Instance;
        }
    }

    private sealed class BinarySerializer
        : IHybridCacheSerializer<string>
        , IHybridCacheSerializer<byte[]>
    {
        public static BinarySerializer Instance { get; } = new();

        private BinarySerializer()
        {
        }

        string IHybridCacheSerializer<string>.Deserialize(ReadOnlySequence<byte> source)
            => Encoding.UTF8.GetString(source);

        void IHybridCacheSerializer<string>.Serialize(string value, IBufferWriter<byte> target)
            => Encoding.UTF8.GetBytes(value, target);

        byte[] IHybridCacheSerializer<byte[]>.Deserialize(ReadOnlySequence<byte> source)
            => source.ToArray();

        public void Serialize(byte[] value, IBufferWriter<byte> target)
            => target.Write(value);
    }

    private abstract class JsonBasedSerializer
    {
        protected static JsonSerializerOptions Options { get; } = new(JsonSerializerDefaults.Web);
    }

    private sealed class JsonBasedSerializer<T>
        : JsonBasedSerializer
        , IHybridCacheSerializer<T>
    {
        public static JsonBasedSerializer<T> Instance { get; } = new();

        private JsonBasedSerializer()
        {
        }

        public T Deserialize(ReadOnlySequence<byte> source)
        {
            var reader = new Utf8JsonReader(source);
            return JsonSerializer.Deserialize<T>(ref reader, Options)!;
        }

        public void Serialize(T value, IBufferWriter<byte> target)
        {
            using var writer = new Utf8JsonWriter(target);

            JsonSerializer.Serialize(writer, value, Options);
        }
    }
}
#endif
