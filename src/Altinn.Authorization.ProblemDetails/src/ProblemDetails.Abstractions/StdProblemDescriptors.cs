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

    /// <summary>
    /// Gets a problem descriptor for a multiple-problems error.
    /// </summary>
    /// <remarks>
    /// This property should remain internal to avoid direct use. To create a multiple-problems error
    /// use the AltinnMultipleProblemDetails class from the Altinn.Authorization.ProblemDetails project.
    /// </remarks>
    internal static ProblemDescriptor MultipleProblems { get; }
        = _factory.Create(1, HttpStatusCode.InternalServerError, "Multiple problems occurred.");

    /// <summary>
    /// Standard problem descriptors' error codes.
    /// </summary>
    public static class ErrorCodes
    {
        /// <summary>
        /// Gets the error code for a validation error.
        /// </summary>
        public static ErrorCode ValidationError 
            => StdProblemDescriptors.ValidationError.ErrorCode;

        /// <summary>
        /// Gets the error code for a multiple-problems error.
        /// </summary>
        public static ErrorCode MultipleProblems 
            => StdProblemDescriptors.MultipleProblems.ErrorCode;
    }
}
