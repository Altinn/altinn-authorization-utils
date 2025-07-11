﻿using Nerdbank.Streams;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Altinn.Cli.Jwks.Stores;

[ExcludeFromCodeCoverage]
internal class DirectoryJsonWebKeySetStore
    : JsonWebKeySetStore
{
    private readonly DirectoryInfo _dir;

    public DirectoryJsonWebKeySetStore(DirectoryInfo dir)
        : base(".key.json", ".json", ".pub.json")
    {
        _dir = dir;
    }

    public override string ToString()
        => Path.GetRelativePath(Directory.GetCurrentDirectory(), _dir.FullName).Replace('\\', '/');

    protected override async IAsyncEnumerable<string> ListNames([EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (!_dir.Exists)
        {
            yield break;
        }

        foreach (var file in _dir.EnumerateFiles("*.pub.json", SearchOption.TopDirectoryOnly))
        {
            cancellationToken.ThrowIfCancellationRequested();

            await Task.Yield();
            yield return file.Name;
        }
    }

    protected override async Task<bool> GetKeySet(
        IBufferWriter<byte> writer,
        string name,
        JsonWebKeySetEnvironment environment = JsonWebKeySetEnvironment.Test,
        JsonWebKeySetVariant variant = JsonWebKeySetVariant.Public,
        CancellationToken cancellationToken = default)
    {
        var path = Path.Combine(_dir.FullName, variant switch
        {
            JsonWebKeySetVariant.Public => PublicKeySetName(name, environment),
            JsonWebKeySetVariant.Private => PrivateKeySetName(name, environment),
            _ => throw new ArgumentOutOfRangeException(nameof(variant))
        });

        try
        {
            await using var fs = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read);
            await fs.CopyToAsync(writer.AsStream(), cancellationToken);
            return true;
        }
        catch (FileNotFoundException)
        {
            return false;
        }
        catch (DirectoryNotFoundException)
        {
            return false;
        }
    }

    public override async Task<bool> GetCurrentPrivateKey(
        IBufferWriter<byte> writer,
        string name,
        JsonWebKeySetEnvironment environment = JsonWebKeySetEnvironment.Test,
        CancellationToken cancellationToken = default)
    {
        var path = Path.Combine(_dir.FullName, PrivateKeyName(name, environment));

        try
        {
            await using var fs = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read);
            await fs.CopyToAsync(writer.AsStream(), cancellationToken);
            return true;
        }
        catch (FileNotFoundException)
        {
            return false;
        }
        catch (DirectoryNotFoundException)
        {
            return false;
        }
    }

    protected override async Task WriteNewKey(
        string name,
        JsonWebKeySetEnvironment environment,
        ReadOnlySequence<byte> privateKeySet,
        ReadOnlySequence<byte> publicKeySet,
        ReadOnlySequence<byte> currentKey,
        CancellationToken cancellationToken)
    {
        if (!_dir.Exists)
        {
            _dir.Create();
        }

        await using var privTemp = await TempFile.Create(Path.Combine(_dir.FullName, PrivateKeySetName(name, environment)));
        await using var pubTemp = await TempFile.Create(Path.Combine(_dir.FullName, PublicKeySetName(name, environment)));
        await using var keyTemp = await TempFile.Create(Path.Combine(_dir.FullName, PrivateKeyName(name, environment)));

        await privTemp.Stream.WriteAsync(privateKeySet, cancellationToken);
        await pubTemp.Stream.WriteAsync(publicKeySet, cancellationToken);
        await keyTemp.Stream.WriteAsync(currentKey, cancellationToken);

        await privTemp.Commit();
        await pubTemp.Commit();
        await keyTemp.Commit();
    }

    private sealed class TempFile
        : IAsyncDisposable
    {
        public static async Task<TempFile> Create(string destination)
        {
            string? path = null;
            FileStream? fs = null;
            try
            {
                (fs, path) = CreateTempFile();
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

            static (FileStream Stream, string Path) CreateTempFile()
            {
                var retries = 0;
                while (true)
                {
                    var path = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
                    try
                    {
                        var fs = File.Open(path, FileMode.CreateNew, FileAccess.Write, FileShare.None);
                        return (fs, path);
                    }
                    catch (IOException) when (retries++ < 5)
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
            ObjectDisposedException.ThrowIf(stream is null, this);

            await stream.DisposeAsync();
            File.Move(_temp, _destination, overwrite: true);
        }
    }
}
