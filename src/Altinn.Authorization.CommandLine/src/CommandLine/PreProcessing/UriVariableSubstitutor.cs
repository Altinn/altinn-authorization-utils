using System.Collections.Immutable;
using System.Text;
using System.Text.RegularExpressions;

namespace Altinn.Authorization.CommandLine.PreProcessing;

internal sealed partial class UriVariableSubstitutor
    : IArgumentSubstitutor
{
    [GeneratedRegex("""\{[a-z]{2,}://[^\}]+?\}""")]
    private static partial Regex UriVariableRegex { get; }

    private readonly ImmutableArray<IArgumentUriResolver> _resolvers;

    public UriVariableSubstitutor(IEnumerable<IArgumentUriResolver> resolvers)
    {
        _resolvers = [.. resolvers];
    }

    public async ValueTask<string?> TrySubstitute(string input, CancellationToken cancellationToken = default)
    {
        var matches = UriVariableRegex.Matches(input);
        if (matches.Count == 0)
        {
            return null;
        }

        var output = new StringBuilder(input.Length * 2);
        var lastIndex = 0;

        foreach (Match match in matches)
        {
            output.Append(input, lastIndex, match.Index - lastIndex);

            if (!Uri.TryCreate(match.Value[1..^1], UriKind.Absolute, out var uri))
            {
                output.Append(match.Value);
                lastIndex = match.Index + match.Length;
                continue;
            }

            var replacement = await Resolve(uri, cancellationToken);
            output.Append(replacement ?? match.Value);

            lastIndex = match.Index + match.Length;
        }

        output.Append(input, lastIndex, input.Length - lastIndex);
        return output.ToString();
    }

    private async ValueTask<string?> Resolve(Uri uri, CancellationToken cancellationToken)
    {
        foreach (var resolver in _resolvers)
        {
            if (await resolver.TryResolve(uri, cancellationToken) is { } resolved)
            {
                return resolved;
            }
        }

        return null;
    }
}
