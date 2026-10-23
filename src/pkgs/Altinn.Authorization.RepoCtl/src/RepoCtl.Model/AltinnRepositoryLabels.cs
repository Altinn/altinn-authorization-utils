using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using Altinn.Authorization.ProblemDetails.Validation;

namespace Altinn.Authorization.RepoCtl.Model;

/// <summary>Settings used when creating repository labels for verticals.</summary>
public sealed record AltinnRepositoryLabels
{
    /// <summary>Gets application label settings.</summary>
    public AltinnRepositoryLabel App { get; init; } = new("area/", "#0978af");

    /// <summary>Gets library label settings.</summary>
    public AltinnRepositoryLabel Library { get; init; } = new("lib/", "#6c1a0d");

    /// <summary>Gets package label settings.</summary>
    public AltinnRepositoryLabel Package { get; init; } = new("pkg/", "#4ad006");

    /// <summary>Gets tool label settings.</summary>
    public AltinnRepositoryLabel Tool { get; init; } = new("tool/", "#3f602d");

    internal sealed record InputModel
        : IInputModel<AltinnRepositoryLabels>
    {
        [JsonPropertyName("app")]
        public AltinnRepositoryLabel.InputModel? App { get; init; }

        [JsonPropertyName("lib")]
        public AltinnRepositoryLabel.InputModel? Library { get; init; }

        [JsonPropertyName("pkg")]
        public AltinnRepositoryLabel.InputModel? Package { get; init; }

        [JsonPropertyName("tool")]
        public AltinnRepositoryLabel.InputModel? Tool { get; init; }

        public bool TryValidate(ref ValidationContext context, [NotNullWhen(true)] out AltinnRepositoryLabels? validated)
        {
            var defaults = new AltinnRepositoryLabels();
            context.TryValidateChild("/app", WithDefaults(App, defaults.App), out AltinnRepositoryLabel? app);
            context.TryValidateChild("/lib", WithDefaults(Library, defaults.Library), out AltinnRepositoryLabel? library);
            context.TryValidateChild("/pkg", WithDefaults(Package, defaults.Package), out AltinnRepositoryLabel? package);
            context.TryValidateChild("/tool", WithDefaults(Tool, defaults.Tool), out AltinnRepositoryLabel? tool);

            if (context.HasErrors)
            {
                validated = null;
                return false;
            }

            Debug.Assert(app is not null);
            Debug.Assert(library is not null);
            Debug.Assert(package is not null);
            Debug.Assert(tool is not null);
            validated = new() { App = app, Library = library, Package = package, Tool = tool };
            return true;
        }

        private static AltinnRepositoryLabel.InputModel WithDefaults(AltinnRepositoryLabel.InputModel? input, AltinnRepositoryLabel defaults)
            => new() { Prefix = input?.Prefix ?? defaults.Prefix, Color = input?.Color ?? defaults.Color };
    }
}
