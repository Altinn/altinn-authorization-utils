using CommunityToolkit.Diagnostics;
using Microsoft.IdentityModel.Tokens;
using Nerdbank.Streams;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Altinn.Cli.Jwks.Stores;

[ExcludeFromCodeCoverage]
internal abstract class JsonWebKeySetStore
{
    private readonly string _suffix;

    protected JsonWebKeySetStore(string suffix)
    {
        _suffix = suffix;
    }

    private string KeySetId(string name, JsonWebKeySetEnvironment environment)
        => $"{name}-{environment.Name()}";

    public string KeyId(string name, JsonWebKeySetEnvironment environment, string suffix)
        => $"{KeySetId(name, environment)}.{suffix}";

    protected string KeySetName(string name, JsonWebKeySetEnvironment environment)
        => $"{KeySetId(name, environment)}{_suffix}";

    protected abstract IAsyncEnumerable<string> ListNames(CancellationToken cancellationToken = default);
    protected abstract Task<bool> GetKeySet(IBufferWriter<byte> writer, string name, JsonWebKeySetEnvironment environment = JsonWebKeySetEnvironment.Test, CancellationToken cancellationToken = default);
    protected abstract Task UpdateKeySets(string name, JsonWebKeySetEnvironment environment, ReadOnlySequence<byte> keySet, CancellationToken cancellationToken);

    public async Task<JsonWebKeySet> GetKeySet(string name, JsonWebKeySetEnvironment environment = JsonWebKeySetEnvironment.Test, JsonWebKeySetVariant variant = JsonWebKeySetVariant.Public, CancellationToken cancellationToken = default)
    {
        using var data = new Sequence<byte>(ArrayPool<byte>.Shared);
        if (!await GetKeySet(data, name, environment, cancellationToken))
        {
            throw new FileNotFoundException($"Key-set {name} not found.");
        }

        var parsed = JsonUtils.Deserialize<JsonWebKeySet>(data.AsReadOnlySequence);
        if (parsed is null)
        {
            throw new InvalidOperationException("Failed to parse JsonWebKeySet");
        }

        if (variant == JsonWebKeySetVariant.Public)
        {
            foreach (var key in parsed.Keys)
            {
                ToPublicKey(key, modify: true);
            }
        }

        return parsed;
    }

    public async Task<JsonWebKey> GetCurrentKey(string name, JsonWebKeySetEnvironment environment = JsonWebKeySetEnvironment.Test, JsonWebKeySetVariant variant = JsonWebKeySetVariant.Public, CancellationToken cancellationToken = default)
    {
        var keySet = await GetKeySet(name, environment, variant, cancellationToken);

        return keySet.Keys.LastOrDefault()
            ?? ThrowHelper.ThrowInvalidOperationException<JsonWebKey>($"No keys found in key-set {name}.");
    }

    public async IAsyncEnumerable<(string Name, JsonWebKeySetEnvironments Variants)> List(JsonWebKeySetEnvironmentFilters filter = JsonWebKeySetEnvironmentFilters.All, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var testKeySetsSuffix = $"-{JsonWebKeySetEnvironment.Test.Name()}{_suffix}";
        var prodKeySetsSuffix = $"-{JsonWebKeySetEnvironment.Prod.Name()}{_suffix}";

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

    public async Task AddKeyToKeySet(string name, JsonWebKeySetEnvironment environment, JsonWebKey newKey, CancellationToken cancellationToken = default)
    {
        JsonWebKeySet? keySet = null;

        {
            using var data = new Sequence<byte>(ArrayPool<byte>.Shared);
            if (await GetKeySet(data, name, environment, cancellationToken))
            {
                keySet = JsonUtils.Deserialize<JsonWebKeySet>(data.AsReadOnlySequence);
            }
        }

        keySet ??= new();

        if (keySet.Keys.Any(k => k.Kid == newKey.Kid))
        {
            throw new InvalidOperationException($"Key with id {newKey.Kid} already exists in key-set.");
        }

        while (keySet.Keys.Count > 5)
        {
            keySet.Keys.RemoveAt(0);
        }

        keySet.Keys.Add(newKey);

        using var keySetData = new Sequence<byte>(ArrayPool<byte>.Shared);

        JsonUtils.Serialize(keySetData, keySet);

        await UpdateKeySets(name, environment, keySetData.AsReadOnlySequence, cancellationToken);
    }

    private static JsonWebKey ToPublicKey(JsonWebKey privateKey, bool modify = false)
    {
        var key = modify ? privateKey : JsonUtils.DeepClone(privateKey);

        switch (key.Kty)
        {
            case var kty when JsonWebAlgorithmsKeyTypes.RSA.Equals(kty):
                // Remove private key parameters for RSA keys
                key.D = null;
                key.DP = null;
                key.DQ = null;
                key.P = null;
                key.Q = null;
                key.QI = null;
                break;

            default:
                ThrowHelper.ThrowNotSupportedException($"Key type {key.Kty} is not supported for public keys.");
                break;
        }

        return key;
    }
}
