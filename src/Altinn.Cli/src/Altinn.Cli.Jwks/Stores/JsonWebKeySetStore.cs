using Microsoft.IdentityModel.Tokens;
using System.Diagnostics.CodeAnalysis;
using System.IO.Pipelines;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace Altinn.Cli.Jwks.Stores;

[ExcludeFromCodeCoverage]
internal abstract class JsonWebKeySetStore
{
    protected abstract IAsyncEnumerable<string> ListNames(CancellationToken cancellationToken = default);
    protected abstract Task<PipeReader?> GetKeySetReader(string name, JsonWebKeySetEnvironment environment = JsonWebKeySetEnvironment.Test, JsonWebKeySetVariant variant = JsonWebKeySetVariant.Public, CancellationToken cancellationToken = default);
    protected abstract Task<PipeReader?> GetCurrentPrivateKeyReader(string name, JsonWebKeySetEnvironment environment = JsonWebKeySetEnvironment.Test, CancellationToken cancellationToken = default);
    protected abstract Task WriteNewKey(string name, JsonWebKeySetEnvironment environment, PipeReader privateKeySetReader, PipeReader publicKeySetReader, PipeReader currentKeyReader, CancellationToken cancellationToken);

    public async Task<Stream?> GetCurrentPrivateKeyStream(string name, JsonWebKeySetEnvironment environment = JsonWebKeySetEnvironment.Test, CancellationToken cancellationToken = default)
        => (await GetCurrentPrivateKeyReader(name, environment, cancellationToken))?.AsStream();

    public async Task<JsonWebKeySet> GetKeySet(string name, JsonWebKeySetEnvironment environment = JsonWebKeySetEnvironment.Test, JsonWebKeySetVariant variant = JsonWebKeySetVariant.Public, CancellationToken cancellationToken = default)
    {
        var reader = await GetKeySetReader(name, environment, variant, cancellationToken);
        if (reader is null)
        {
            throw new FileNotFoundException($"Key-set {name} not found.");
        }
            
        await using var stream = reader.AsStream();

        var parsed = await JsonSerializer.DeserializeAsync<JsonWebKeySet>(stream, cancellationToken: cancellationToken);
        if (parsed is null)
        {
            throw new InvalidOperationException("Failed to parse JsonWebKeySet");
        }

        return parsed;
    }

    public async IAsyncEnumerable<(string Name, JsonWebKeySetEnvironments Variants)> List(JsonWebKeySetEnvironmentFilters filter = JsonWebKeySetEnvironmentFilters.All, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var testKeySetsSuffix = $"-{JsonWebKeySetEnvironment.Test.Name()}.pub.json";
        var prodKeySetsSuffix = $"-{JsonWebKeySetEnvironment.Prod.Name()}.pub.json";

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
            var reader = await GetKeySetReader(name, environment, JsonWebKeySetVariant.Private, cancellationToken);
            if (reader is not null)
            {
                await using var stream = reader.AsStream();

                priv = await JsonSerializer.DeserializeAsync<JsonWebKeySet>(stream, cancellationToken: cancellationToken);
            }
        }

        {
            var reader = await GetKeySetReader(name, environment, JsonWebKeySetVariant.Public, cancellationToken);
            if (reader is not null)
            {
                await using var stream = reader.AsStream();

                pub = await JsonSerializer.DeserializeAsync<JsonWebKeySet>(stream, cancellationToken: cancellationToken);
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

        var privPipe = new Pipe();
        var pubPipe = new Pipe();
        var keyPipe = new Pipe();

        var writeTask = Task.Run(() => WriteNewKey(name, environment, privPipe.Reader, pubPipe.Reader, keyPipe.Reader, cancellationToken));

        { 
            await using var privStream = privPipe.Writer.AsStream();
            await using var pubStream = pubPipe.Writer.AsStream();
            await using var keyStream = keyPipe.Writer.AsStream();

            await JsonSerializer.SerializeAsync(privStream, priv, JsonOptions.Options, cancellationToken);
            await JsonSerializer.SerializeAsync(pubStream, pub, JsonOptions.Options, cancellationToken);
            await JsonSerializer.SerializeAsync(keyStream, privateKey, JsonOptions.Options, cancellationToken);
        }

        await writeTask;
    }

    private string KeySetId(string name, JsonWebKeySetEnvironment environment)
        => $"{name}-{environment.Name()}";

    public string KeyId(string name, JsonWebKeySetEnvironment environment, string suffix)
        => $"{KeySetId(name, environment)}.{suffix}";

    protected string PrivateKeyName(string name, JsonWebKeySetEnvironment environment)
        => $"{KeySetId(name, environment)}.key.json";

    protected string PrivateKeySetName(string name, JsonWebKeySetEnvironment environment)
        => $"{KeySetId(name, environment)}.json";

    protected string PublicKeySetName(string name, JsonWebKeySetEnvironment environment)
        => $"{KeySetId(name, environment)}.pub.json";
}
