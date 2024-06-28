using Microsoft.IdentityModel.Tokens;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace Altinn.Cli.Jwks.Stores;

[ExcludeFromCodeCoverage]
internal class DirectoryJsonWebKeySetStore
    : JsonWebKeySetStore
{
    private readonly DirectoryInfo _dir;

    public DirectoryJsonWebKeySetStore(DirectoryInfo dir)
    {
        _dir = dir;
    }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
    public override async IAsyncEnumerable<(string Name, JsonWebKeySetEnvironments Variants)> List(JsonWebKeySetEnvironmentFilter filter = JsonWebKeySetEnvironmentFilter.All, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var testKeySetsSuffix = $"-{JsonWebKeySetEnvironment.Test.Name()}.pub.json";
        var prodKeySetsSuffix = $"-{JsonWebKeySetEnvironment.Prod.Name()}.pub.json";

        var keySets = new SortedDictionary<string, JsonWebKeySetEnvironments>();

        if (!_dir.Exists)
        {
            yield break;
        }

        foreach (var file in _dir.EnumerateFiles("*.pub.json", SearchOption.TopDirectoryOnly))
        {
            var fileName = file.Name;
            if (fileName.EndsWith(testKeySetsSuffix))
            {
                var keySetName = fileName[0..^testKeySetsSuffix.Length];
                
                if (keySets.TryGetValue(keySetName, out var envs))
                {
                    keySets[keySetName] = envs | JsonWebKeySetEnvironments.Test;
                }
                else
                {
                    keySets[keySetName] = JsonWebKeySetEnvironments.Test;
                }
            }
            else if (file.Name.EndsWith(prodKeySetsSuffix))
            {
                var keySetName = fileName[0..^prodKeySetsSuffix.Length];

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
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously

    public override Task<Stream> GetKeySetReadStream(string name, JsonWebKeySetEnvironment environment = JsonWebKeySetEnvironment.Test, JsonWebKeySetVariant variant = JsonWebKeySetVariant.Public, CancellationToken cancellationToken = default)
    {
        var path = Path.Combine(_dir.FullName, variant switch
        {
            JsonWebKeySetVariant.Public => PublicKeySetName(name, environment),
            JsonWebKeySetVariant.Private => PrivateKeySetName(name, environment),
            _ => throw new ArgumentOutOfRangeException(nameof(variant))
        });

        return Task.FromResult<Stream>(File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read));
    }

    public override Task<Stream> GetCurrentPrivateKeyReadStream(string name, JsonWebKeySetEnvironment environment = JsonWebKeySetEnvironment.Test, CancellationToken cancellationToken = default)
    {
        var path = Path.Combine(_dir.FullName, PrivateKeyName(name, environment));

        return Task.FromResult<Stream>(File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read));
    }

    public override async Task AddKeyToKeySet(string name, JsonWebKeySetEnvironment environment, JsonWebKey privateKey, JsonWebKey publicKey, CancellationToken cancellationToken = default)
    {
        JsonWebKeySet? priv = null;
        JsonWebKeySet? pub = null;

        if (!_dir.Exists)
        {
            _dir.Create();
        }

        try 
        { 
            await using var fs = await GetKeySetReadStream(name, environment, JsonWebKeySetVariant.Private, cancellationToken);

            priv = await JsonSerializer.DeserializeAsync<JsonWebKeySet>(fs, cancellationToken: cancellationToken);
        }
        catch (FileNotFoundException)
        { }

        try
        {
            await using var fs = await GetKeySetReadStream(name, environment, JsonWebKeySetVariant.Public, cancellationToken);

            pub = await JsonSerializer.DeserializeAsync<JsonWebKeySet>(fs, cancellationToken: cancellationToken);
        }
        catch (FileNotFoundException)
        { }

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

        await using var privTemp = await TempFile.Create(Path.Combine(_dir.FullName, PrivateKeySetName(name, environment)));
        await using var pubTemp = await TempFile.Create(Path.Combine(_dir.FullName, PublicKeySetName(name, environment)));
        await using var keyTemp = await TempFile.Create(Path.Combine(_dir.FullName, PrivateKeyName(name, environment)));

        await JsonSerializer.SerializeAsync(privTemp.Stream, priv, JsonOptions.Options, cancellationToken);
        await JsonSerializer.SerializeAsync(pubTemp.Stream, pub, JsonOptions.Options, cancellationToken);
        await JsonSerializer.SerializeAsync(keyTemp.Stream, privateKey, JsonOptions.Options, cancellationToken);

        await privTemp.Commit();
        await pubTemp.Commit();
        await keyTemp.Commit();
    }

    private class TempFile
    : IAsyncDisposable
    {
        public static async Task<TempFile> Create(string destination)
        {
            var path = Path.GetTempFileName();
            FileStream? fs = null;
            try
            {
                fs = File.Open(path, FileMode.Open, FileAccess.Write, FileShare.None);
                var tempFile = new TempFile(destination, path, fs);
                fs = null;
                path = null;

                return tempFile;
            }
            finally
            {
                if (fs is not null)
                {
                    await fs.DisposeAsync();
                }

                if (path is not null)
                {
                    try
                    {
                        File.Delete(path);
                    }
                    catch (FileNotFoundException)
                    {
                    }
                }
            }
        }

        private readonly string _destination;
        private readonly string _temp;

        private FileStream? _stream;

        private TempFile(string destination, string temp, FileStream stream)
        {
            _destination = destination;
            _temp = temp;
            _stream = stream;
        }

        public FileStream Stream => _stream ?? throw new ObjectDisposedException(nameof(TempFile));

        public async ValueTask DisposeAsync()
        {
            var stream = Interlocked.Exchange(ref _stream, null);
            if (stream is not null)
            {
                await stream.DisposeAsync();
                try
                {
                    File.Delete(_temp);
                }
                catch (FileNotFoundException)
                {
                }
            }
        }

        public async ValueTask Commit()
        {
            var stream = Interlocked.Exchange(ref _stream, null);
            if (stream is null)
            {
                throw new ObjectDisposedException(nameof(TempFile));
            }

            await stream.DisposeAsync();
            File.Move(_temp, _destination, overwrite: true);
        }
    }
}


