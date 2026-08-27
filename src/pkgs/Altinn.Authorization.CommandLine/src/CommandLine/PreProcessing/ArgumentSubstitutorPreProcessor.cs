using System.Collections.Immutable;

namespace Altinn.Authorization.CommandLine.PreProcessing;

internal sealed class ArgumentSubstitutorPreProcessor
    : IArgumentPreProcessor
{
    private readonly ImmutableArray<IArgumentSubstitutor> _substitutors;

    public ArgumentSubstitutorPreProcessor(IEnumerable<IArgumentSubstitutor> substitutors)
    {
        _substitutors = substitutors.ToImmutableArray();
    }

    /// <inheritdoc/>
    public async ValueTask Preprocess(IList<string> args, CancellationToken cancellationToken = default)
    {
        for (var i = 0; i < args.Count; i++)
        {
            var arg = args[i];
            if (arg.Length > 0)
            {
                if (await TrySubstitute(arg, cancellationToken) is { } substituted
                    && !string.Equals(substituted, arg, StringComparison.Ordinal))
                {
                    args[i] = substituted;
                }
            }
        }
    }

    private async ValueTask<string?> TrySubstitute(string input, CancellationToken cancellationToken = default)
    {
        var value = input;
        var hasSubstituted = false;
        bool hasSubstitutedThisIteration;
        byte iterationCount = 0;

        // continue to substitute until no more substitutions are made in an iteration
        do
        {
            hasSubstitutedThisIteration = false;
            foreach (var substitutor in _substitutors)
            {
                if (await substitutor.TrySubstitute(value, cancellationToken) is { } output
                    && !string.Equals(output, value, StringComparison.Ordinal))
                {
                    value = output;
                    hasSubstituted = true;
                    hasSubstitutedThisIteration = true;
                }
            }

            if (++iterationCount > 20)
            {
                throw new InvalidOperationException("Too many iterations of argument substitution. Possible circular substitution.");
            }
        }
        while (hasSubstitutedThisIteration);

        return hasSubstituted ? value : null;
    }
}
