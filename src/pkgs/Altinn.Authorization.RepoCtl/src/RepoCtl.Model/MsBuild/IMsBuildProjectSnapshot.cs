namespace Altinn.Authorization.RepoCtl.Model.MsBuild;

/// <summary>
/// Provides read-only access to an evaluated MSBuild project state.
/// </summary>
internal interface IMsBuildProjectSnapshot
{
    /// <summary>
    /// Gets the full path to the project file.
    /// </summary>
    public string FullPath { get; }

    /// <summary>
    /// Gets the evaluated value of an MSBuild property.
    /// </summary>
    /// <param name="propertyName">The name of the property.</param>
    /// <returns>The evaluated property value, or an empty string when the property is not defined.</returns>
    public string GetPropertyValue(string propertyName);

    /// <summary>
    /// Determines whether the project defines a target with the specified name.
    /// </summary>
    /// <param name="targetName">The name of the target.</param>
    /// <returns><see langword="true"/> when the target is defined; otherwise, <see langword="false"/>.</returns>
    public bool ContainsTarget(string targetName);
}
