namespace Altinn.Authorization.ProblemDetails;

/// <summary>
/// Interface for providing a <see cref="ProblemInstance"/>. Typically, this is implemented
/// for an exception, such that it can carry problem information down through the HTTP pipeline.
/// </summary>
public interface IHasProblem
{
    /// <summary>
    /// Gets the <see cref="ProblemInstance"/> for the current instance.
    /// </summary>
    public ProblemInstance? Problem { get; }
}
