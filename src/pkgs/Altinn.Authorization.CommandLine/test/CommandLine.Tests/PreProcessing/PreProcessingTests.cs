using Altinn.Authorization.CommandLine.PreProcessing;

namespace Altinn.Authorization.CommandLine.Tests.PreProcessing;

public class PreProcessingTests
{
    private CancellationToken CancellationToken => TestContext.Current.CancellationToken;

    [Fact]
    public async Task ArgumentPreProcessor_AppliesProcessorsInOrderWithoutMutatingInput()
    {
        var first = new RecordingProcessor(static args => args[0] += "-first");
        var second = new RecordingProcessor(static args => args[0] += "-second");
        var processor = new ArgumentPreProcessor([first, second]);
        var input = new[] { "value", "other" };

        var result = await processor.Preprocess(input, CancellationToken);

        result.ShouldBe(["value-first-second", "other"]);
        input.ShouldBe(["value", "other"]);
        first.Seen.ShouldBe(["value", "other"]);
        second.Seen.ShouldBe(["value-first", "other"]);
    }

    [Fact]
    public async Task ArgumentSubstitutorPreProcessor_SubstitutesUntilStableAndSkipsEmptyArguments()
    {
        var substitutor = new DelegateSubstitutor(input => input switch
        {
            "{ONE}" => "{TWO}",
            "{TWO}" => "done",
            _ => null,
        });
        var processor = new ArgumentSubstitutorPreProcessor([substitutor]);
        var args = new List<string> { "{ONE}", string.Empty, "unchanged" };

        await processor.Preprocess(args, CancellationToken);

        args.ShouldBe(["done", string.Empty, "unchanged"]);
        substitutor.Inputs.ShouldBe(["{ONE}", "{TWO}", "done", "unchanged"]);
    }

    [Fact]
    public async Task ArgumentSubstitutorPreProcessor_ThrowsForCircularSubstitution()
    {
        var substitutor = new DelegateSubstitutor(input => input switch
        {
            "A" => "B",
            "B" => "A",
            _ => null,
        });
        var processor = new ArgumentSubstitutorPreProcessor([substitutor]);

        var exception = await Should.ThrowAsync<InvalidOperationException>(
            () => processor.Preprocess(["A"], CancellationToken).AsTask());

        exception.Message.ShouldContain("Possible circular substitution");
    }

    [Fact]
    public async Task EnvironmentVariableSubstitutor_SubstitutesKnownAndUnknownVariables()
    {
        const string variableName = "ALTINN_PREPROCESSING_TEST_VALUE";
        var originalValue = Environment.GetEnvironmentVariable(variableName);
        Environment.SetEnvironmentVariable(variableName, "configured");

        try
        {
            var substitutor = new EnvironmentVariableSubstitutor();

            (await substitutor.TrySubstitute("prefix-${ALTINN_PREPROCESSING_TEST_VALUE}-${MISSING}-suffix", CancellationToken))
                .ShouldBe("prefix-configured--suffix");
            (await substitutor.TrySubstitute("${lowercase}", CancellationToken))
                .ShouldBeNull();
        }
        finally
        {
            Environment.SetEnvironmentVariable(variableName, originalValue);
        }
    }

    [Fact]
    public async Task EnvironmentVariableSubstitutor_ResolvesEnvUrisAndIgnoresOtherSchemes()
    {
        const string variableName = "ALTINN_PREPROCESSING_TEST_URI";
        var originalValue = Environment.GetEnvironmentVariable(variableName);
        Environment.SetEnvironmentVariable(variableName, "uri-value");

        try
        {
            var substitutor = new EnvironmentVariableSubstitutor();

            (await substitutor.TryResolve(new Uri("env://altinn_preprocessing_test_uri"), CancellationToken))
                .ShouldBe("uri-value");
            (await substitutor.TryResolve(new Uri("other://value"), CancellationToken)).ShouldBeNull();
            (await substitutor.TryResolve(new Uri("env://ALTINN_PREPROCESSING_MISSING"), CancellationToken))
                .ShouldBe(string.Empty);
        }
        finally
        {
            Environment.SetEnvironmentVariable(variableName, originalValue);
        }
    }

    [Fact]
    public async Task UriVariableSubstitutor_ReplacesResolvedUrisAndPreservesUnresolvedUris()
    {
        var resolver = new RecordingResolver(new Dictionary<string, string?>
        {
            ["test://known/"] = "resolved",
        });
        var substitutor = new UriVariableSubstitutor([resolver]);

        var result = await substitutor.TrySubstitute(
            "before {test://known} middle {test://unknown} after {test://known}",
            CancellationToken);

        result.ShouldBe("before resolved middle {test://unknown} after resolved");
        resolver.Uris.Select(uri => uri.ToString()).ShouldBe(["test://known/", "test://unknown/", "test://known/"]);
    }

    [Fact]
    public async Task UriVariableSubstitutor_ReturnsNullWhenInputHasNoUriVariables()
    {
        var substitutor = new UriVariableSubstitutor([]);

        (await substitutor.TrySubstitute("plain text", CancellationToken)).ShouldBeNull();
    }

    [Fact]
    public async Task UriVariableSubstitutor_ForwardsCancellationTokenToResolver()
    {
        using var cancellation = new CancellationTokenSource();
        var resolver = new RecordingResolver(new Dictionary<string, string?>(), cancellation.Token);
        var substitutor = new UriVariableSubstitutor([resolver]);

        await substitutor.TrySubstitute("{test://value}", cancellation.Token);

        resolver.Tokens.ShouldBe([cancellation.Token]);
    }

    private sealed class RecordingProcessor(Action<IList<string>> action) : IArgumentPreProcessor
    {
        public List<string> Seen { get; } = [];

        public ValueTask Preprocess(IList<string> args, CancellationToken cancellationToken = default)
        {
            Seen.AddRange(args);
            action(args);
            return ValueTask.CompletedTask;
        }
    }

    private sealed class DelegateSubstitutor(Func<string, string?> substitute)
    : IArgumentSubstitutor
    {
        public List<string> Inputs { get; } = [];

        public ValueTask<string?> TrySubstitute(string input, CancellationToken cancellationToken = default)
        {
            Inputs.Add(input);
            return ValueTask.FromResult(substitute(input));
        }
    }

    private sealed class RecordingResolver(
        IReadOnlyDictionary<string, string?> values,
        CancellationToken? expectedToken = null)
        : IArgumentUriResolver
    {
        public List<Uri> Uris { get; } = [];
        public List<CancellationToken> Tokens { get; } = [];

        public ValueTask<string?> TryResolve(Uri uri, CancellationToken cancellationToken = default)
        {
            Uris.Add(uri);
            Tokens.Add(cancellationToken);
            expectedToken?.ShouldBe(cancellationToken);
            return ValueTask.FromResult(values.GetValueOrDefault(uri.ToString()));
        }
    }
}
