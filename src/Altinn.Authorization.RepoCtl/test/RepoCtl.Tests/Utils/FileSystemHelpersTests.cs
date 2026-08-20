using Altinn.Authorization.RepoCtl.Model.Utils;

namespace Altinn.Authorization.RepoCtl.Tests.Utils;

public class FileSystemHelpersTests
{
    private static readonly string RootPath = Path.Combine(Path.GetTempPath(), "repoctl-filesystem-helpers-tests");

    [Fact]
    public void IsDescendantOf_IdenticalPaths_ReturnsTrue()
    {
        var directory = new DirectoryInfo(Path.Combine(RootPath, "src"));
        var ancestor = new DirectoryInfo(Path.Combine(RootPath, "src"));

        directory.IsDescendantOf(ancestor).ShouldBeTrue();
    }

    [Fact]
    public void IsDescendantOf_NestedPath_ReturnsTrue()
    {
        var directory = new DirectoryInfo(Path.Combine(RootPath, "src", "project"));
        var ancestor = new DirectoryInfo(Path.Combine(RootPath, "src"));

        directory.IsDescendantOf(ancestor).ShouldBeTrue();
    }

    [Fact]
    public void IsDescendantOf_AncestorHasTrailingSeparator_ReturnsTrue()
    {
        var directory = new DirectoryInfo(Path.Combine(RootPath, "src", "project"));
        var ancestor = new DirectoryInfo(WithTrailingSeparator(Path.Combine(RootPath, "src")));

        directory.IsDescendantOf(ancestor).ShouldBeTrue();
    }

    [Fact]
    public void IsDescendantOf_DirectoryHasTrailingSeparator_ReturnsTrue()
    {
        var directory = new DirectoryInfo(WithTrailingSeparator(Path.Combine(RootPath, "src")));
        var ancestor = new DirectoryInfo(Path.Combine(RootPath, "src"));

        directory.IsDescendantOf(ancestor).ShouldBeTrue();
    }

    [Fact]
    public void IsDescendantOf_BothPathsHaveTrailingSeparators_ReturnsTrue()
    {
        var directory = new DirectoryInfo(WithTrailingSeparator(Path.Combine(RootPath, "src", "project")));
        var ancestor = new DirectoryInfo(WithTrailingSeparator(Path.Combine(RootPath, "src")));

        directory.IsDescendantOf(ancestor).ShouldBeTrue();
    }

    [Fact]
    public void IsDescendantOf_SiblingPath_ReturnsFalse()
    {
        var directory = new DirectoryInfo(Path.Combine(RootPath, "src", "project-a"));
        var ancestor = new DirectoryInfo(Path.Combine(RootPath, "src", "project-b"));

        directory.IsDescendantOf(ancestor).ShouldBeFalse();
    }

    [Fact]
    public void IsDescendantOf_UnrelatedPath_ReturnsFalse()
    {
        var directory = new DirectoryInfo(Path.Combine(RootPath, "src"));
        var ancestor = new DirectoryInfo(Path.Combine(Path.GetTempPath(), "repoctl-unrelated"));

        directory.IsDescendantOf(ancestor).ShouldBeFalse();
    }

    [Fact]
    public void IsDescendantOf_AncestorCheckedAgainstChild_ReturnsFalse()
    {
        var directory = new DirectoryInfo(Path.Combine(RootPath, "src"));
        var ancestor = new DirectoryInfo(Path.Combine(RootPath, "src", "project"));

        directory.IsDescendantOf(ancestor).ShouldBeFalse();
    }

    private static string WithTrailingSeparator(string path)
        => path + Path.DirectorySeparatorChar;
}
