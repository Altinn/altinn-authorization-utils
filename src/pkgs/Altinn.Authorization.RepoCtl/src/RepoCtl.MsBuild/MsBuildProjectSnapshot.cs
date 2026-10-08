using Microsoft.Build.Execution;

namespace Altinn.Authorization.RepoCtl.Model.MsBuild;

internal sealed class MsBuildProjectSnapshot
    : IMsBuildProjectSnapshot
{
    private readonly ProjectInstance _project;

    public MsBuildProjectSnapshot(ProjectInstance project)
    {
        _project = project;
    }

    public string FullPath
        => _project.FullPath;

    public string GetPropertyValue(string propertyName)
        => _project.GetPropertyValue(propertyName);

    public bool ContainsTarget(string targetName)
        => _project.Targets.ContainsKey(targetName);
}
