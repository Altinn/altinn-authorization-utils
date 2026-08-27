using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;
using Altinn.Authorization.ProblemDetails;
using Altinn.Authorization.ProblemDetails.Validation;

namespace Altinn.Authorization.RepoCtl.Model;

/// <summary>
/// Represents the configuration of an Altinn repository.
/// </summary>
public sealed partial record AltinnRepositoryConfiguration
{
    /// <summary>
    /// Reads an <see cref="AltinnRepositoryConfiguration"/> from the specified stream.
    /// </summary>
    /// <param name="stream">The stream containing the repository configuration.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The loaded <see cref="AltinnRepositoryConfiguration"/>.</returns>
    public static async Task<Result<AltinnRepositoryConfiguration>> Read(Stream stream, CancellationToken cancellationToken = default)
    {
        InputModel? inputModel;
        ValidationProblemBuilder builder = default;
        ValidationProblemInstance? problem;

        try
        {
            inputModel = await JsonSerializer.DeserializeAsync<InputModel>(stream, RepositoryConfigurationJsonContext.Default.Options, cancellationToken);
        }
        catch (JsonException)
        {
            builder.Add(StdValidationErrors.InvalidValue, "/", detail: "Failed to deserialize the repository configuration.");
            builder.TryBuild(out problem);

            Debug.Assert(problem is not null);
            return problem;
        }

        if (inputModel is null)
        {
            builder.Add(StdValidationErrors.Required, "/", detail: "The repository configuration deserialized to null.");
            builder.TryBuild(out problem);

            Debug.Assert(problem is not null);
            return problem;
        }

        inputModel.TryValidate(ref builder, out AltinnRepositoryConfiguration? validated);
        if (builder.TryBuild(out problem))
        {
            return problem;
        }

        Debug.Assert(validated is not null);
        return validated;
    }

    /// <summary>
    /// Gets the name of the repository.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Gets the kind of verticals the verticals directly under src/ is considered to be.
    /// </summary>
    public required AltinnVerticalKind RootKind { get; init; }

    internal record InputModel
        : IInputModel<AltinnRepositoryConfiguration>
    {
        /// <summary>
        /// Gets the name of the repository.
        /// </summary>
        [JsonPropertyName("name")]
        public string? Name { get; init; }

        /// <summary>
        /// Gets the kind of verticals the verticals directly under src/ is considered to be.
        /// </summary>
        [JsonPropertyName("rootKind")]
        public AltinnVerticalKind RootKind { get; init; }


        public bool TryValidate(ref ValidationContext context, [NotNullWhen(true)] out AltinnRepositoryConfiguration? validated)
        {
            if (string.IsNullOrWhiteSpace(Name))
            {
                context.AddChildProblem(StdValidationErrors.Required, "/name");
            }
            else if (Name.StartsWith(" ") || Name.EndsWith(" "))
            {
                context.AddChildProblem(StdValidationErrors.InvalidValue, "/name", detail: "The name must not start or end with whitespace.");
            }
            else if (Name.StartsWith("."))
            {
                context.AddChildProblem(StdValidationErrors.InvalidValue, "/name", detail: "The name must not start with a dot.");
            }

            if (context.HasErrors)
            {
                validated = null;
                return false;
            }

            var rootKind = RootKind;
            if (!rootKind.HasValue)
            {
                rootKind = AltinnVerticalKind.Package;
            }

            Debug.Assert(Name is not null);
            validated = new AltinnRepositoryConfiguration
            {
                Name = Name,
                RootKind = rootKind,
            };
            return true;
        }
    }

    [JsonSerializable(typeof(InputModel))]
    [JsonSourceGenerationOptions(
        JsonSerializerDefaults.Web,
        AllowTrailingCommas = true,
        PropertyNamingPolicy = JsonKnownNamingPolicy.KebabCaseLower,
        ReadCommentHandling = JsonCommentHandling.Skip,
        UseStringEnumConverter = true)]
    private sealed partial class RepositoryConfigurationJsonContext
        : JsonSerializerContext;
}
