using Microsoft.IdentityModel.Tokens;
using Nerdbank.Streams;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Altinn.Cli.Jwks.Stores;

[ExcludeFromCodeCoverage]
internal abstract class JsonWebKeySetStore
{
    private readonly string _privateKeySuffix;
    private readonly string _privateKeySetSuffix;
    private readonly string _publicKeySetSuffix;

    protected JsonWebKeySetStore(
        string privateKeySuffix, 
        string privateKeySetSuffix,
        string publicKeySetSuffix)
    {
        _privateKeySuffix = privateKeySuffix;
        _privateKeySetSuffix = privateKeySetSuffix;
        _publicKeySetSuffix = publicKeySetSuffix;
    }

    private string KeySetId(string name, JsonWebKeySetEnvironment environment)
        => $"{name}-{environment.Name()}";

    public string KeyId(string name, JsonWebKeySetEnvironment environment, string suffix)
        => $"{KeySetId(name, environment)}.{suffix}";

    protected string PrivateKeyName(string name, JsonWebKeySetEnvironment environment)
        => $"{KeySetId(name, environment)}{_privateKeySuffix}";

    protected string PrivateKeySetName(string name, JsonWebKeySetEnvironment environment)
        => $"{KeySetId(name, environment)}{_privateKeySetSuffix}";

    protected string PublicKeySetName(string name, JsonWebKeySetEnvironment environment)
        => $"{KeySetId(name, environment)}{_publicKeySetSuffix}";

    protected abstract IAsyncEnumerable<string> ListNames(CancellationToken cancellationToken = default);
    protected abstract Task<bool> GetKeySet(IBufferWriter<byte> writer, string name, JsonWebKeySetEnvironment environment = JsonWebKeySetEnvironment.Test, JsonWebKeySetVariant variant = JsonWebKeySetVariant.Public, CancellationToken cancellationToken = default);
    public abstract Task<bool> GetCurrentPrivateKey(IBufferWriter<byte> writer, string name, JsonWebKeySetEnvironment environment = JsonWebKeySetEnvironment.Test, CancellationToken cancellationToken = default);
    protected abstract Task WriteNewKey(string name, JsonWebKeySetEnvironment environment, ReadOnlySequence<byte> privateKeySet, ReadOnlySequence<byte> publicKeySet, ReadOnlySequence<byte> currentKey, CancellationToken cancellationToken);

    public async Task<JsonWebKeySet> GetKeySet(string name, JsonWebKeySetEnvironment environment = JsonWebKeySetEnvironment.Test, JsonWebKeySetVariant variant = JsonWebKeySetVariant.Public, CancellationToken cancellationToken = default)
    {
        using var data = new Sequence<byte>(ArrayPool<byte>.Shared);
        if (!await GetKeySet(data, name, environment, variant, cancellationToken))
        {
            throw new FileNotFoundException($"Key-set {name} not found.");
        }

        var parsed = JsonUtils.Deserialize<JsonWebKeySet>(data.AsReadOnlySequence);
        if (parsed is null)
        {
            throw new InvalidOperationException("Failed to parse JsonWebKeySet");
        }

        return parsed;
    }

    public async IAsyncEnumerable<(string Name, JsonWebKeySetEnvironments Variants)> List(JsonWebKeySetEnvironmentFilters filter = JsonWebKeySetEnvironmentFilters.All, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var testKeySetsSuffix = $"-{JsonWebKeySetEnvironment.Test.Name()}{_publicKeySetSuffix}";
        var prodKeySetsSuffix = $"-{JsonWebKeySetEnvironment.Prod.Name()}{_publicKeySetSuffix}";

        var keySets = new SortedDictionary<string, JsonWebKeySetEnvironments>();

        await foreach (var name in ListNames(cancellationToken))
        {
            if (name.EndsWith(testKeySetsSuffix))
            {
                var keySetName = name[0..^testKeySetsSuffix.Length];

                if (keySets.TryGetValue(keySetName, out var envs))
                {
                    keySets[keySetName] = envs | JsonWebKeySetEnvironments.Test;
                }
                else
                {
                    keySets[keySetName] = JsonWebKeySetEnvironments.Test;
                }
            }
            else if (name.EndsWith(prodKeySetsSuffix))
            {
                var keySetName = name[0..^prodKeySetsSuffix.Length];

                if (keySets.TryGetValue(keySetName, out var envs))
                {
                    keySets[keySetName] = envs | JsonWebKeySetEnvironments.Prod;
                }
                else
                {
                    keySets[keySetName] = JsonWebKeySetEnvironments.Prod;
                }
            }
        }

        foreach (var (name, variants) in keySets)
        {
            yield return (name, variants);
        }
    }

    public async Task AddKeyToKeySet(string name, JsonWebKeySetEnvironment environment, JsonWebKey privateKey, JsonWebKey publicKey, CancellationToken cancellationToken = default)
    {
        JsonWebKeySet? priv = null;
        JsonWebKeySet? pub = null;

        {
            using var data = new Sequence<byte>(ArrayPool<byte>.Shared);
            if (await GetKeySet(data, name, environment, JsonWebKeySetVariant.Private, cancellationToken))
            {
                priv = JsonUtils.Deserialize<JsonWebKeySet>(data.AsReadOnlySequence);
            }
        }

        {
            using var data = new Sequence<byte>(ArrayPool<byte>.Shared);
            if (await GetKeySet(data, name, environment, JsonWebKeySetVariant.Public, cancellationToken))
            {
                pub = JsonUtils.Deserialize<JsonWebKeySet>(data.AsReadOnlySequence);
            }
        }

        priv ??= new();
        pub ??= new();

        if (priv.Keys.Any(k => k.Kid == privateKey.Kid))
        {
            throw new InvalidOperationException($"Key with id {privateKey.Kid} already exists in key-set.");
        }

        if (pub.Keys.Any(k => k.Kid == publicKey.Kid))
        {
            throw new InvalidOperationException($"Key with id {publicKey.Kid} already exists in key-set.");
        }

        while (priv.Keys.Count > 2)
        {
            priv.Keys.RemoveAt(0);
        }

        while (pub.Keys.Count > 2)
        {
            pub.Keys.RemoveAt(0);
        }

        priv.Keys.Add(privateKey);
        pub.Keys.Add(publicKey);

        using var privData = new Sequence<byte>(ArrayPool<byte>.Shared);
        using var pubData = new Sequence<byte>(ArrayPool<byte>.Shared);
        using var keyData = new Sequence<byte>(ArrayPool<byte>.Shared);

        JsonUtils.Serialize(privData, priv);
        JsonUtils.Serialize(pubData, pub);
        JsonUtils.Serialize(keyData, privateKey);

        await WriteNewKey(name, environment, privData.AsReadOnlySequence, pubData.AsReadOnlySequence, keyData.AsReadOnlySequence, cancellationToken);
    }
}
