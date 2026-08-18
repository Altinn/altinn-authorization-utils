using System.Text.Json.Serialization;

namespace Altinn.Authorization.RepoCtl.Model;

/// <summary>
/// Represents the type of an Altinn project.
/// </summary>
public enum AltinnProjectType
{
    /// <summary>
    /// Represents an executable project (e.g., a console application).
    /// </summary>
    [JsonStringEnumMemberName("exe")]
    Executable,

    /// <summary>
    /// Represents a packable tool project (e.g., a console application that can be packed as a NuGet package).
    /// </summary>
    [JsonStringEnumMemberName("tool")]
    Tool,

    /// <summary>
    /// Represents an internal library project (e.g., a class library that is not intended to be published as a NuGet package).
    /// </summary>
    [JsonStringEnumMemberName("lib")]
    InternalLibrary,

    /// <summary>
    /// Represents a packable library project (e.g., a class library that can be published as a NuGet package).
    /// </summary>
    [JsonStringEnumMemberName("pkg")]
    PackageLibrary,

    /// <summary>
    /// Represents a test library project (e.g., a class library that contains helpers for testing).
    /// </summary>
    [JsonStringEnumMemberName("testlib")]
    TestLibrary,

    /// <summary>
    /// Represents a test project (e.g., a unit test project).
    /// </summary>
    [JsonStringEnumMemberName("tests")]
    Test,

    /// <summary>
    /// Represents a sample executable project (e.g., a console application that serves as a sample).
    /// </summary>
    [JsonStringEnumMemberName("sample-exe")]
    SampleExecutable,

    /// <summary>
    /// Represents a sample library project (e.g., a class library that serves as a sample).
    /// </summary>
    [JsonStringEnumMemberName("sample-lib")]
    SampleLibrary,
}

/// <summary>
/// Extension methods for <see cref="AltinnProjectType"/>.
/// </summary>
public static class AltinnProjectTypeExtensions
{
    /// <param name="type">The project type.</param>
    extension(AltinnProjectType type)
    {
        /// <summary>
        /// Gets a value indicating whether the project is a sample project.
        /// </summary>
        public bool IsSampleProject
            => type is AltinnProjectType.SampleExecutable or AltinnProjectType.SampleLibrary;

        /// <summary>
        /// Gets a value indicating whether the project is a test project.
        /// </summary>
        public bool IsTestProject
            => type is AltinnProjectType.Test or AltinnProjectType.TestLibrary;

        /// <summary>
        /// Gets a value indicating whether the project is a test library.
        /// </summary>
        public bool IsTestLib
            => type is AltinnProjectType.TestLibrary;

        /// <summary>
        /// Gets a value indicating whether the project can be referenced by other projects.
        /// </summary>
        public bool CanBeReferencedByOtherProjects
            => type is AltinnProjectType.InternalLibrary or AltinnProjectType.PackageLibrary;

        /// <summary>
        /// Gets a value indicating whether the project can be packed as a NuGet package.
        /// </summary>
        public bool IsPackable
            => type is AltinnProjectType.Tool or AltinnProjectType.PackageLibrary;
    }
}
