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
        : base("--key", "--priv", "--pub")
    {
        _client = client;
    }

    public override string ToString()
        => _client.VaultUri.ToString();

    public override Task<bool> GetCurrentPrivateKey(
        IBufferWriter<byte> writer,
        string name,
        JsonWebKeySetEnvironment environment = JsonWebKeySetEnvironment.Test,
        CancellationToken cancellationToken = default)
    {
        var secretName = PrivateKeyName(name, environment);

        return GetSecretValue(writer, secretName, cancellationToken);
    }

    protected override Task<bool> GetKeySet(
        IBufferWriter<byte> writer,
        string name,
        JsonWebKeySetEnvironment environment = JsonWebKeySetEnvironment.Test,
        JsonWebKeySetVariant variant = JsonWebKeySetVariant.Public,
        CancellationToken cancellationToken = default)
    {
        var secretName = variant switch
        {
            JsonWebKeySetVariant.Public => PublicKeySetName(name, environment),
            JsonWebKeySetVariant.Private => PrivateKeySetName(name, environment),
            _ => throw new ArgumentOutOfRangeException(nameof(variant))
        };

        return GetSecretValue(writer, secretName, cancellationToken);
    }

    protected override async IAsyncEnumerable<string> ListNames([EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await foreach (var secret in _client.GetPropertiesOfSecretsAsync(cancellationToken))
        {
            yield return secret.Name;
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
        var privSecretName = PrivateKeySetName(name, environment);
        var pubSecretName = PublicKeySetName(name, environment);
        var keySecretName = PrivateKeyName(name, environment);

        var privValue = Base64Helper.Encode(privateKeySet);
        var pubValue = Base64Helper.Encode(publicKeySet);
        var keyValue = Base64Helper.Encode(currentKey);

        await _client.SetSecretAsync(privSecretName, privValue, CancellationToken.None);
        await _client.SetSecretAsync(pubSecretName, pubValue, CancellationToken.None);
        await _client.SetSecretAsync(keySecretName, keyValue, CancellationToken.None);
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
