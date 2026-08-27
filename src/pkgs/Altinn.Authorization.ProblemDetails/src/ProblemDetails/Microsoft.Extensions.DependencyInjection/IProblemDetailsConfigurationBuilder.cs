namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Defines a builder for configuring ProblemDetails services.
/// </summary>
public interface IProblemDetailsConfigurationBuilder
{
    /// <summary>
    /// Gets the <see cref="IServiceCollection"/> that is being configured.
    /// </summary>
    public IServiceCollection Services { get; }
}
