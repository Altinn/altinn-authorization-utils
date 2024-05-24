using System.Net;

namespace Altinn.Authorization.ProblemDetails;

/// <summary>
/// Standard problem descriptors.
/// </summary>
public static class StdProblemDescriptors
{
    internal const string DOMAIN_NAME = "STD";

    private static readonly ProblemDescriptorFactory _factory
        = ProblemDescriptorFactory.New(DOMAIN_NAME);

    /// <summary>
    /// Gets a problem descriptor for a validation error.
    /// </summary>
    /// <remarks>
    /// This property should remain internal to avoid direct use. To create a validation error
    /// use the AltinnValidationProblemDetails class from the Altinn.Authorization.ProblemDetails project.
    /// </remarks>
    internal static ProblemDescriptor ValidationError { get; }
        = _factory.Create(0, HttpStatusCode.BadRequest, "One or more validation errors occurred.");
}
