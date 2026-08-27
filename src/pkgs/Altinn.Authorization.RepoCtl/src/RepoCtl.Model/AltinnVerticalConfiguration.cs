using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;
using Altinn.Authorization.ModelUtils;
using Altinn.Authorization.ProblemDetails;
using Altinn.Authorization.ProblemDetails.Validation;

namespace Altinn.Authorization.RepoCtl.Model;

/// <summary>
/// Represents the configuration of an Altinn vertical.
/// </summary>
public sealed partial record AltinnVerticalConfiguration
{
    /// <summary>
    /// Gets the default configuration for a vertical.
    /// </summary>
    public static AltinnVerticalConfiguration Default { get; } = new AltinnVerticalConfiguration
    {
        Dependencies = [],
        DisplayName = null,
        Infrastructure = null,
    };

    /// <summary>
    /// Reads an <see cref="AltinnVerticalConfiguration"/> from the specified stream.
    /// </summary>
    /// <param name="stream">The stream containing the repository configuration.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The loaded <see cref="AltinnVerticalConfiguration"/>.</returns>
    public static async Task<Result<AltinnVerticalConfiguration>> Read(Stream stream, CancellationToken cancellationToken = default)
    {
        InputModel? inputModel;
        ValidationProblemBuilder builder = default;
        ValidationProblemInstance? problem;

        try
        {
            inputModel = await JsonSerializer.DeserializeAsync<InputModel>(stream, VerticalConfigurationJsonContext.Default.Options, cancellationToken);
        }
        catch (JsonException)
        {
            builder.Add(StdValidationErrors.InvalidValue, "/", detail: "Failed to deserialize the vertical configuration.");
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

        inputModel.TryValidate(ref builder, out AltinnVerticalConfiguration? validated);
        if (builder.TryBuild(out problem))
        {
            return problem;
        }

        Debug.Assert(validated is not null);
        return validated;
    }

    /// <summary>
    /// Gets the set of verticals that this vertical depends on.
    /// </summary>
    public required ImmutableValueSet<AltinnVerticalId> Dependencies { get; init; }

    /// <summary>
    /// Gets the (overridden) display name of the vertical.
    /// </summary>
    public required string? DisplayName { get; init; }

    /// <summary>
    /// Gets the infrastructure configuration of the vertical.
    /// </summary>
    public required AltinnVerticalInfrastructureConfiguration? Infrastructure { get; init; }

    internal record InputModel
        : IInputModel<AltinnVerticalConfiguration>
    {
        [JsonPropertyName("deps")]
        public List<AltinnVerticalId>? Dependencies { get; init; }

        [JsonPropertyName("displayName")]
        public string? DisplayName { get; init; }

        [Obsolete]
        [JsonPropertyName("shortName")]
        public string? ShortName
        {
            init => DisplayName ??= value;
        }

        [JsonPropertyName("infra")]
        public InfrastructureInputModel? Infrastructure { get; init; }

        public bool TryValidate(ref ValidationContext context, [NotNullWhen(true)] out AltinnVerticalConfiguration? validated)
        {
            ImmutableValueSet<AltinnVerticalId> dependencies = Dependencies is not null
                ? [.. Dependencies]
                : [];

            AltinnVerticalInfrastructureConfiguration? infrastructure = null;
            if (Infrastructure is not null)
            {
                context.TryValidateChild("/infra", Infrastructure, out infrastructure);
            }

            if (context.HasErrors)
            {
                validated = null;
                return false;
            }

            validated = new AltinnVerticalConfiguration
            {
                Dependencies = dependencies,
                DisplayName = DisplayName,
                Infrastructure = infrastructure,
            };
            return true;
        }
    }

    internal record InfrastructureInputModel
        : IInputModel<AltinnVerticalInfrastructureConfiguration>
    {
        [JsonPropertyName("terraform")]
        public TerraformInputModel? Terraform { get; init; }

        public bool TryValidate(ref ValidationContext context, [NotNullWhen(true)] out AltinnVerticalInfrastructureConfiguration? validated)
        {
            context.TryValidateChild("/terraform", Terraform, out AltinnVerticalTerraformConfiguration? terraform);

            if (context.HasErrors)
            {
                validated = null;
                return false;
            }

            Debug.Assert(terraform is not null);
            validated = new AltinnVerticalInfrastructureConfiguration
            {
                Terraform = terraform,
            };
            return true;
        }
    }

    internal record TerraformInputModel
        : IInputModel<AltinnVerticalTerraformConfiguration>
    {
        [JsonPropertyName("stateFile")]
        public string? StateFile { get; init; }

        public bool TryValidate(ref ValidationContext context, [NotNullWhen(true)] out AltinnVerticalTerraformConfiguration? validated)
        {
            if (string.IsNullOrEmpty(StateFile))
            {
                context.AddChildProblem(StdValidationErrors.Required, "/stateFile");
            }
            else if (StateFile.Length < 10)
            {
                context.AddChildProblem(StdValidationErrors.MinLength, "/stateFile", detail: "The state file path must be at least 10 characters long.");
            }

            if (context.HasErrors)
            {
                validated = null;
                return false;
            }

            Debug.Assert(StateFile is not null);
            validated = new AltinnVerticalTerraformConfiguration
            {
                StateFile = StateFile!,
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
    private sealed partial class VerticalConfigurationJsonContext
        : JsonSerializerContext;
}
