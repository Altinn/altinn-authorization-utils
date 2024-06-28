using Microsoft.IdentityModel.Tokens;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace Altinn.Cli.Jwks.Stores;

[ExcludeFromCodeCoverage]
internal abstract class JsonWebKeySetStore
{
    public abstract IAsyncEnumerable<(string Name, JsonWebKeySetEnvironments Variants)> List(JsonWebKeySetEnvironmentFilter filter = JsonWebKeySetEnvironmentFilter.All, CancellationToken cancellationToken = default);
    public abstract Task<Stream> GetKeySetReadStream(string name, JsonWebKeySetEnvironment environment = JsonWebKeySetEnvironment.Test, JsonWebKeySetVariant variant = JsonWebKeySetVariant.Public, CancellationToken cancellationToken = default);
    public abstract Task<Stream> GetCurrentPrivateKeyReadStream(string name, JsonWebKeySetEnvironment environment = JsonWebKeySetEnvironment.Test, CancellationToken cancellationToken = default);
    public abstract Task AddKeyToKeySet(string name, JsonWebKeySetEnvironment environment, JsonWebKey privateKey, JsonWebKey publicKey, CancellationToken cancellationToken = default);

    public async Task<JsonWebKeySet> GetKeySet(string name, JsonWebKeySetEnvironment environment = JsonWebKeySetEnvironment.Test, JsonWebKeySetVariant variant = JsonWebKeySetVariant.Public, CancellationToken cancellationToken = default)
    {
        await using var stream = await GetKeySetReadStream(name, environment, variant, cancellationToken);

        var parsed = await JsonSerializer.DeserializeAsync<JsonWebKeySet>(stream, cancellationToken: cancellationToken);
        if (parsed is null)
        {
            throw new InvalidOperationException("Failed to parse JsonWebKeySet");
        }

        return parsed;
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
