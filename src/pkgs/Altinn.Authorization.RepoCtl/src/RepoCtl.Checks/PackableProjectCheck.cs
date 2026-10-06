using System.Runtime.CompilerServices;
using Altinn.Authorization.RepoCtl.Model;
using Altinn.Authorization.RepoCtl.Model.Checks;
using Altinn.Authorization.RepoCtl.Model.MsBuild;

namespace Altinn.Authorization.RepoCtl.Checks;

/// <summary>
/// Checks for packable projects.
/// </summary>
internal sealed class PackableProjectCheck
    : IRepositoryCheck
{
    private readonly IMsBuildContextFactory _msbuildContextFactory;

    public PackableProjectCheck(IMsBuildContextFactory msbuildContextFactory)
    {
        _msbuildContextFactory = msbuildContextFactory;
    }

    /// <inheritdoc/>
    public string CheckId
        => "packable-project";

    /// <inheritdoc/>
    public string CheckDisplayName
        => "Packable Projects";

    /// <inheritdoc/>
    public async IAsyncEnumerable<Diagnostic> Check(
        AltinnRepository repository,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        const string InitializeSourceControlTargetName = "InitializeSourceControlInformation";

        using var context = _msbuildContextFactory.CreateBuildContext(new Dictionary<string, string>
        {
            ["CI"] = "true",
            ["GITHUB_ACTIONS"] = "true",
        });

        foreach (var altinnProject in repository.Verticals
            .AsEnumerable()
            .Where(static vertical => AltinnVerticalKindSet.Packable.Contains(vertical.Kind))
            .SelectMany(static vertical => vertical.Projects)
            .Where(static project => project.Type.IsPackable))
        {
            var project = await context.LoadProject(altinnProject.ProjectFile.FullName, cancellationToken);

            if (!project.GetPropertyValueAsBool("Deterministic"))
            {
                yield return Diagnostics.PackableProjectShouldBeDeterministic(altinnProject.Name, altinnProject.ProjectFile.FullName, "Deterministic");
            }

            if (!project.GetPropertyValueAsBool("ContinuousIntegrationBuild"))
            {
                yield return Diagnostics.PackableProjectShouldBeDeterministic(altinnProject.Name, altinnProject.ProjectFile.FullName, "ContinuousIntegrationBuild");
            }

            if (!project.GetPropertyValueAsBool("EnableSourceLink"))
            {
                yield return Diagnostics.PackableProjectInvalidSourceLink(altinnProject.Name, altinnProject.ProjectFile.FullName, "'EnableSourceLink' should be true");
            }

            if (!project.GetPropertyValueAsBool("SourceControlInformationFeatureSupported"))
            {
                yield return Diagnostics.PackableProjectInvalidSourceLink(altinnProject.Name, altinnProject.ProjectFile.FullName, "'SourceControlInformationFeatureSupported' should be true");
            }

            var debugType = project.GetPropertyValue("DebugType");
            if (!string.Equals(debugType, "embedded", StringComparison.OrdinalIgnoreCase))
            {
                yield return Diagnostics.PackableProjectInvalidDebugType(altinnProject.Name, altinnProject.ProjectFile.FullName, debugType);
            }

            if (!project.ContainsTarget(InitializeSourceControlTargetName))
            {
                yield return Diagnostics.PackableProjectInvalidSourceLink(altinnProject.Name, altinnProject.ProjectFile.FullName, $"'Target {InitializeSourceControlTargetName}' should exist");
                continue;
            }

            IMsBuildProjectSnapshot? buildResult = null;
            try
            {
                buildResult = await project.Build(InitializeSourceControlTargetName, cancellationToken);
            }
            catch
            {
                // ignore errors, we can't yield in catch
            }

            if (buildResult is null)
            {
                yield return Diagnostics.ProjectTargetExecutionFailed(altinnProject.Name, altinnProject.ProjectFile.FullName, InitializeSourceControlTargetName);
                continue;
            }

            var sourceLink = buildResult.GetPropertyValue("PrivateRepositoryUrl");
            if (string.IsNullOrWhiteSpace(sourceLink))
            {
                yield return Diagnostics.PackableProjectInvalidSourceLink(altinnProject.Name, altinnProject.ProjectFile.FullName, $"'PrivateRepositoryUrl' should be initialized by '{InitializeSourceControlTargetName}'");
            }
        }
    }
}
