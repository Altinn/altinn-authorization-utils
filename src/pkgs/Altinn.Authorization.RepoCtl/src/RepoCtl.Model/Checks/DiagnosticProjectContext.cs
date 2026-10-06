using CommunityToolkit.Diagnostics;

namespace Altinn.Authorization.RepoCtl.Model.Checks;

/// <summary>
/// Represents the context of a project related to a diagnostic.
/// </summary>
public sealed record DiagnosticProjectContext
{
    /// <summary>
    /// Gets a <see cref="DiagnosticProjectContext"/> representing a project file.
    /// </summary>
    /// <param name="fullPath">The full path to the project file.</param>
    /// <returns>A <see cref="DiagnosticProjectContext"/> representing the project file.</returns>
    public static DiagnosticProjectContext File(string fullPath)
        => new DiagnosticProjectContext(fullPath, projectProperties: null);

    /// <summary>
    /// Gets the path to the project file.
    /// </summary>
    public string ProjectPath { get; }

    /// <summary>
    /// Gets optional active project properties.
    /// </summary>
    public string? ProjectProperties { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="DiagnosticProjectContext"/> class.
    /// </summary>
    /// <param name="projectPath">The path to the project file.</param>
    /// <param name="projectProperties">Optional active project properties.</param>
    public DiagnosticProjectContext(
        string projectPath,
        string? projectProperties)
    {
        Guard.IsNotNullOrEmpty(projectPath);

        ProjectPath = projectPath;
        ProjectProperties = projectProperties;
    }
}
