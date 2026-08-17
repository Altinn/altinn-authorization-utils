using Altinn.Authorization.CommandLine.PreProcessing;
using Azure.Security.KeyVault.Secrets;

namespace Altinn.Authorization.CommandLine.Azure.KeyVault;

internal sealed class AzureKeyVaultSecretArgumentUriResolver
    : IArgumentUriResolver
{
    public async ValueTask<string?> TryResolve(Uri uri, CancellationToken cancellationToken = default)
    {
        if (!string.Equals(uri.Scheme, "kv", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        var vault = uri.Host;
        var (secret, version) = GetSecretAndVersionFromSegments(uri.Segments);
        var credentials = AzureCredentialHelper.GetCredential(uri);
        SecretClient client = new(new Uri($"https://{vault}.vault.azure.net/"), credentials);

        var response = version is null
            ? await client.GetSecretAsync(secret, cancellationToken: cancellationToken)
            : await client.GetSecretAsync(secret, version, cancellationToken: cancellationToken);

        return response.Value.Value;
    }

    private (string secret, string? version) GetSecretAndVersionFromSegments(string[] segments)
    {
        if (segments.Length is < 2 or > 3)
        {
            throw new ArgumentException($"Invalid number of segments in Key Vault URI. Expected 2 or 3 segments, but got {segments.Length}.", nameof(segments));
        }

        var secret = segments[1].Trim('/');
        string? version = null;
        if (segments.Length > 2)
        {
            version = segments[2].Trim('/');
        }

        return (secret, version);
    }
}
