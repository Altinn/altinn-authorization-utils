using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using Altinn.Authorization.ProblemDetails;
using Altinn.Authorization.ProblemDetails.Validation;

namespace Altinn.Authorization.RepoCtl.Model;

/// <summary>Settings used when creating labels for a vertical kind.</summary>
/// <param name="Prefix">The prefix prepended to the vertical short slug.</param>
/// <param name="Color">The label color in #RRGGBB notation.</param>
public sealed record AltinnRepositoryLabel(string Prefix, string Color)
{
    internal sealed record InputModel
        : IInputModel<AltinnRepositoryLabel>
    {
        [JsonPropertyName("prefix")]
        public string? Prefix { get; init; }

        [JsonPropertyName("color")]
        public string? Color { get; init; }

        public bool TryValidate(ref ValidationContext context, [NotNullWhen(true)] out AltinnRepositoryLabel? validated)
        {
            if (string.IsNullOrEmpty(Prefix))
            {
                context.AddChildProblem(StdValidationErrors.Required, "/prefix");
            }

            if (Color is null || Color.Length != 7 || Color[0] != '#' || !Color.Skip(1).All(Uri.IsHexDigit))
            {
                context.AddChildProblem(StdValidationErrors.InvalidValue, "/color", detail: "Label colors must use #RRGGBB notation.");
            }

            if (context.HasErrors)
            {
                validated = null;
                return false;
            }

            Debug.Assert(Prefix is not null);
            Debug.Assert(Color is not null);
            validated = new(Prefix, Color);
            return true;
        }
    }
}
