namespace Altinn.Authorization.CommandLine.PreProcessing;

/// <summary>
/// Defines a substitutor for command line arguments.
/// </summary>
public interface IArgumentSubstitutor
{
    /// <summary>
    /// Tries to substitute the specified input argument with a corresponding value.
    /// </summary>
    /// <param name="input">The input argument to substitute.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The substituted value if a substitution was made; otherwise, <c>null</c>.</returns>
    public ValueTask<string?> TrySubstitute(string input, CancellationToken cancellationToken = default);
}
