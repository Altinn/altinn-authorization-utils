using System.Collections.Immutable;

namespace Altinn.Authorization.CommandLine.PreProcessing;

internal sealed class ArgumentPreProcessor
{
    private readonly ImmutableArray<IArgumentPreProcessor> _preProcessors;

    /// <summary>
    /// Initializes a new instance of the <see cref="ArgumentPreProcessor"/> class.
    /// </summary>
    /// <param name="preProcessors">The collection of argument preprocessors.</param>
    public ArgumentPreProcessor(IEnumerable<IArgumentPreProcessor> preProcessors)
    {
        _preProcessors = [.. preProcessors];
    }

    /// <inheritdoc/>
    public async ValueTask<ImmutableArray<string>> Preprocess(IReadOnlyList<string> args, CancellationToken cancellationToken = default)
    {
        var builder = ImmutableArray.CreateBuilder<string>(args.Count);
        builder.AddRange(args);

        foreach (var preProcessor in _preProcessors)
        {
            await preProcessor.Preprocess(builder, cancellationToken);
        }

        return builder.ToImmutable();
    }
}
