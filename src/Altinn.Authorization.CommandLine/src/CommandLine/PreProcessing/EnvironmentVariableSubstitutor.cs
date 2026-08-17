using System.Text.RegularExpressions;

namespace Altinn.Authorization.CommandLine.PreProcessing;

internal sealed partial class EnvironmentVariableSubstitutor
    : IArgumentSubstitutor
    , IArgumentUriResolver
{
    [GeneratedRegex("""\$\{([A-Z0-9_]+)\}""")]
    private static partial Regex EnvironmentVariableRegex { get; }

    public ValueTask<string?> TryResolve(Uri uri, CancellationToken cancellationToken = default)
    {
        if (!string.Equals(uri.Scheme, "env", StringComparison.OrdinalIgnoreCase))
        {
            return ValueTask.FromResult<string?>(null);
        }

        // We intentionally only support upper-case environment variable names in url format
        var name = uri.Host.ToUpperInvariant();
        if (string.IsNullOrEmpty(name))
        {
            return ValueTask.FromResult<string?>(null);
        }

        var value = Environment.GetEnvironmentVariable(name);
        return ValueTask.FromResult<string?>(value ?? string.Empty);
    }

    public ValueTask<string?> TrySubstitute(string input, CancellationToken cancellationToken = default)
    {
        var output = EnvironmentVariableRegex.Replace(input, static match =>
        {
            var name = match.Groups[1].Value;
            return Environment.GetEnvironmentVariable(name) ?? string.Empty;
        });

        return new ValueTask<string?>(output != input ? output : null);
    }
}
