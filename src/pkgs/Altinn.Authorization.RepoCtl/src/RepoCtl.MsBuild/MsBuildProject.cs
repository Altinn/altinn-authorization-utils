using Microsoft.Build.Evaluation;

namespace Altinn.Authorization.RepoCtl.Model.MsBuild;

internal sealed class MsBuildProject
    : IMsBuildProject
{
    public MsBuildProject(Project project)
    {
        _project = project;
    }

    private readonly Project _project;

    public string FullPath => _project.FullPath;

    public string GetPropertyValue(string propertyName)
        => _project.GetPropertyValue(propertyName);

    public bool ContainsTarget(string targetName)
        => _project.Targets.ContainsKey(targetName);
}
