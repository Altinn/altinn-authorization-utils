using System.Text.Json.Serialization;

namespace Altinn.Authorization.ProblemDetails;

/// <summary>
/// Serialization context for problem-details types.
/// </summary>
[JsonSerializable(typeof(AltinnProblemDetails))]
[JsonSerializable(typeof(AltinnValidationProblemDetails))]
[JsonSerializable(typeof(AltinnMultipleProblemDetails))]
internal sealed partial class AltinnProblemDetailsJsonContext
    : JsonSerializerContext
{
}
