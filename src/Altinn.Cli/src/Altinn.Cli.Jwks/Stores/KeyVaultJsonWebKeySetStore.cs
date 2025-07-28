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
        : base("--key", "--pub")
    {
        _client = client;
    }

    public override string ToString()
        => _client.VaultUri.ToString();

    public override async Task<bool> KeyExists(
        string name,
        JsonWebKeySetEnvironment environment,
        JsonWebKeyVariant variant,
        CancellationToken cancellationToken = default)
    {
        var secretName = GetSecretName(name, environment, variant);
        return await GetSecretValue(secretName, cancellationToken) is not null;
    }
    
    public override async Task<bool> GetKey(
        IBufferWriter<byte> writer,
        string name,
        JsonWebKeySetEnvironment environment,
        JsonWebKeyVariant variant,
        CancellationToken cancellationToken = default)
    {
        var secretName = GetSecretName(name, environment, variant);
        return await GetSecretValue(writer, secretName, cancellationToken);
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
        ReadOnlySequence<byte> privateKey,
        ReadOnlySequence<byte> publicKey,
        CancellationToken cancellationToken)
    {
        var privSecretName = PrivateKeyName(name, environment);
        var pubSecretName = PublicKeyName(name, environment);

        var privValue = Base64Helper.Encode(privateKey);
        var pubValue = Base64Helper.Encode(publicKey);

        await _client.SetSecretAsync(privSecretName, privValue, CancellationToken.None);
        await _client.SetSecretAsync(pubSecretName, pubValue, CancellationToken.None);
    }

    private string GetSecretName(string name, JsonWebKeySetEnvironment environment, JsonWebKeyVariant variant) =>
        variant switch
        {
            JsonWebKeyVariant.Public => PublicKeyName(name, environment),
            JsonWebKeyVariant.Private => PrivateKeyName(name, environment),
            _ => throw new ArgumentOutOfRangeException(nameof(variant))
        };
    
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
    
    private async Task<KeyVaultSecret?> GetSecretValue(string secretName, CancellationToken cancellationToken)
    {
        try
        {
            var secret = await _client.GetSecretAsync(secretName, cancellationToken: cancellationToken);
            return secret.Value;
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            return null;
        }
    }
}
