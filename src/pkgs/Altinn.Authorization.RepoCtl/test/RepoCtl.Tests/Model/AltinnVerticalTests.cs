using Semver;

namespace Altinn.Authorization.RepoCtl.Model.Tests;

public class AltinnVerticalTests
{
    [Theory]
    [InlineData("Altinn.Authorization.RepoCtl.Model", null, "RepoCtl.Model")]
    [InlineData("ALTINN.AUTHORIZATION.RepoCtl.Model", null, "RepoCtl.Model")]
    [InlineData("Altinn.RepoCtl.Model", null, "RepoCtl.Model")]
    [InlineData("altinn.RepoCtl.Model", null, "RepoCtl.Model")]
    [InlineData("Other.RepoCtl.Model", null, "Other.RepoCtl.Model")]
    [InlineData("RepoCtl.Model", null, "RepoCtl.Model")]
    [InlineData("Altinn.Authorization.RepoCtl.Model", "Custom display name", "Custom display name")]
    public void DisplayName_UsesShortNameUnlessConfigured(string name, string? configuredName, string expected)
    {
        var vertical = new AltinnVertical(
            "src/pkgs/Test",
            new DirectoryInfo(Path.Combine(Path.GetTempPath(), "repoctl-display-name-tests")),
            new AltinnVerticalId(AltinnVerticalKind.Package, name),
            new SemVersion(1, 0, 0),
            [],
            AltinnVerticalConfiguration.Default with { DisplayName = configuredName },
            []);

        vertical.DisplayName.ShouldBe(expected);
    }
}
