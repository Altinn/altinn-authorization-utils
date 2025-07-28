using Microsoft.IdentityModel.Tokens;
using Nerdbank.Streams;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace Altinn.Cli.Jwks.Stores;

[ExcludeFromCodeCoverage]
internal abstract class JsonWebKeySetStore
{
    private readonly string _privateKeySuffix;
    private readonly string _publicKeySetSuffix;

    protected JsonWebKeySetStore(
        string privateKeySuffix, 
        string publicKeySetSuffix)
    {
        _privateKeySuffix = privateKeySuffix;
        _publicKeySetSuffix = publicKeySetSuffix;
    }
    
    public string KeyNamePrefix(string name, JsonWebKeySetEnvironment environment)
        => $"{name}-{environment.Name()}";
    
    protected string PrivateKeyName(string name, JsonWebKeySetEnvironment environment)
        => $"{KeyNamePrefix(name, environment)}{_privateKeySuffix}";

    protected string PublicKeyName(string name, JsonWebKeySetEnvironment environment)
        => $"{KeyNamePrefix(name, environment)}{_publicKeySetSuffix}";

    protected abstract IAsyncEnumerable<string> ListNames(CancellationToken cancellationToken = default);
    public abstract Task<bool> KeyExists(string name, JsonWebKeySetEnvironment environment, JsonWebKeyVariant variant, CancellationToken cancellationToken = default);
    public abstract Task<bool> GetKey(IBufferWriter<byte> writer, string name, JsonWebKeySetEnvironment environment, JsonWebKeyVariant variant, CancellationToken cancellationToken = default);
    protected abstract Task WriteNewKey(string name, JsonWebKeySetEnvironment environment, ReadOnlySequence<byte> privateKey, ReadOnlySequence<byte> publicKey, CancellationToken cancellationToken);

    public async Task<JsonWebKey> GetDeserializedKey(string name, JsonWebKeySetEnvironment environment, JsonWebKeyVariant variant, CancellationToken cancellationToken = default)
    {
        using var data = new Sequence<byte>(ArrayPool<byte>.Shared);
        if (!await GetKey(data, name, environment, variant, cancellationToken))
        {
            throw new FileNotFoundException($"Key '{name}' not found.");
        }

        var parsed = JsonUtils.Deserialize<JsonWebKey>(data.AsReadOnlySequence);
        if (parsed is null)
        {
            throw new InvalidOperationException("Failed to parse JsonWebKey");
        }

        return parsed;
    }

    public async IAsyncEnumerable<(string Name, JsonWebKeySetEnvironments Variants)> List(
        JsonWebKeySetEnvironmentFilters filter = JsonWebKeySetEnvironmentFilters.All,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var testKeySuffix = $"-{JsonWebKeySetEnvironment.Test.Name()}{_publicKeySetSuffix}";
        var prodKeySuffix = $"-{JsonWebKeySetEnvironment.Prod.Name()}{_publicKeySetSuffix}";

        var keys = new SortedDictionary<string, JsonWebKeySetEnvironments>();

        await foreach (var name in ListNames(cancellationToken))
        {
            if (name.EndsWith(testKeySuffix))
            {
                var keyName = name[0..^testKeySuffix.Length];

                if (keys.TryGetValue(keyName, out var envs))
                {
                    keys[keyName] = envs | JsonWebKeySetEnvironments.Test;
                }
                else
                {
                    keys[keyName] = JsonWebKeySetEnvironments.Test;
                }
            }
            else if (name.EndsWith(prodKeySuffix))
            {
                var keyName = name[0..^prodKeySuffix.Length];

                if (keys.TryGetValue(keyName, out var envs))
                {
                    keys[keyName] = envs | JsonWebKeySetEnvironments.Prod;
                }
                else
                {
                    keys[keyName] = JsonWebKeySetEnvironments.Prod;
                }
            }
        }

        foreach (var (name, variants) in keys)
        {
            yield return (name, variants);
        }
    }

    public async Task SerializeAndStoreKeys(string name, JsonWebKeySetEnvironment environment, JsonWebKey privateKey, JsonWebKey publicKey, CancellationToken cancellationToken = default)
    {
        if (
            await KeyExists(name, environment, JsonWebKeyVariant.Private, cancellationToken)
            || await KeyExists(name, environment, JsonWebKeyVariant.Public, cancellationToken))
        {
            throw new InvalidOperationException($"Key '{name}' already exists.");
        }

        using var privData = new Sequence<byte>(ArrayPool<byte>.Shared);
        using var pubData = new Sequence<byte>(ArrayPool<byte>.Shared);

        JsonUtils.Serialize(privData, privateKey);
        JsonUtils.Serialize(pubData, publicKey);

        await WriteNewKey(name, environment, privData.AsReadOnlySequence, pubData.AsReadOnlySequence, cancellationToken);
    }
}
