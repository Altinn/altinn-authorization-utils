using Azure;
using Azure.Security.KeyVault.Secrets;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Altinn.Cli.Jwks.Stores;

[ExcludeFromCodeCoverage]
internal class KeyVaultJsonWebKeySetStore
    : JsonWebKeySetStore
{
    private readonly SecretClient _client;

    public KeyVaultJsonWebKeySetStore(SecretClient client)
        : base("--jwks")
    {
        _client = client;
    }

    public override string ToString()
        => _client.VaultUri.ToString();

    protected override Task<bool> GetKeySet(
        IBufferWriter<byte> writer,
        string name,
        JsonWebKeySetEnvironment environment = JsonWebKeySetEnvironment.Test,
        CancellationToken cancellationToken = default)
    {
        var secretName = KeySetName(name, environment);

        return GetSecretValue(writer, secretName, cancellationToken);
    }

    protected override async IAsyncEnumerable<string> ListNames([EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await foreach (var secret in _client.GetPropertiesOfSecretsAsync(cancellationToken))
        {
            yield return secret.Name;
        }
    }

    protected override async Task UpdateKeySets(
        string name,
        JsonWebKeySetEnvironment environment,
        ReadOnlySequence<byte> keySet,
        CancellationToken cancellationToken)
    {
        var secretName = KeySetName(name, environment);

        var value = Base64Helper.Encode(keySet);

        await _client.SetSecretAsync(secretName, value, cancellationToken);
    }

    private async Task<bool> GetSecretValue(IBufferWriter<byte> writer, string secretName, CancellationToken cancellationToken)
    {
        try
        {
            var secret = await _client.GetSecretAsync(secretName, cancellationToken: cancellationToken);

            Base64Helper.Decode(writer, secret.Value.Value);
            cancellationToken.ThrowIfCancellationRequested();

            return true;
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            return false;
        }
    }
}
