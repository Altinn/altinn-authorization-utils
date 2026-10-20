using Altinn.Authorization.RepoCtl.Utils;
using Altinn.Authorization.RepoCtl.Containers;

namespace Altinn.Authorization.RepoCtl.Tests.Commands.Ci;

public class TagAppImagesCommandTests
{
    [Fact]
    public void BakeOutput_DeserializesGroupsAndTargetsIgnoringUnneededBuildSettings()
    {
        var bake = BakeOutput.Deserialize("""
            {"group":{"default":{"targets":["app"]}},
             "target":{"app":{"context":".","dockerfile":"Dockerfile","tags":["app:abc"]}}}
            """);

        bake.Groups["default"].Targets.ShouldBe(["app"]);
        bake.Targets["app"].Tags.ShouldBe(["app:abc"]);
    }

    [Theory]
    [InlineData("null")]
    [InlineData("{\"group\":{\"default\":{}}}")]
    [InlineData("{\"target\":{\"app\":{\"tags\":42}}}")]
    [InlineData("{\"target\":{\"default\":{}}}")]
    public void BakeOutput_RejectsInvalidJsonShape(string json)
        => Should.Throw<System.Text.Json.JsonException>(() => BakeOutput.Deserialize(json));

    [Fact]
    public void BakeImages_TraversesGroupsAndDeduplicatesSelectedTargets()
    {
        var bake = """
            {"group":{"default":{"targets":["matrix","app"]},"matrix":{"targets":["app","worker"]}},
             "target":{"app":{"tags":["registry:5000/app:abc","registry:5000/app:latest"]},
                       "worker":{"tags":["registry/worker:abc"]},"dependency":{"tags":["registry/dependency:abc"]}}}
            """;
        ContainerService.GetReleaseImages(BakeOutput.Deserialize(bake), "abc").ShouldBe(["registry/worker:abc", "registry:5000/app:abc"]);
    }

    [Theory]
    [InlineData("{}")]
    [InlineData("{\"group\":{\"default\":{\"targets\":[]}}}")]
    [InlineData("{\"group\":{\"default\":{\"targets\":[\"default\"]}}}")]
    [InlineData("{\"target\":{\"default\":{\"tags\":[\"app:other\"]}}}")]
    [InlineData("{\"group\":{\"default\":{\"targets\":[\"app\",\"worker\"]}},\"target\":{\"app\":{\"tags\":[\"app:abc\"]},\"worker\":{\"tags\":[\"worker:latest\"]}}}")]
    public void BakeImages_RejectsMissingImagesAndCycles(string bake)
        => Should.Throw<InvalidOperationException>(() => ContainerService.GetReleaseImages(BakeOutput.Deserialize(bake), "abc"));

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void BakeImages_HandlesNestedSharedGroupsAndDetectsIndirectCycles(bool cycle)
    {
        const int depth = 20;
        var groups = new Dictionary<string, BakeOutput.Group>
        {
            ["default"] = new() { Targets = ["group0", "group0"] },
        };
        for (var i = 0; i < depth; i++)
        {
            groups[$"group{i}"] = new() { Targets = [i + 1 < depth ? $"group{i + 1}" : cycle ? "group0" : "app"] };
        }

        var bake = new BakeOutput
        {
            Groups = groups,
            Targets = new Dictionary<string, BakeOutput.Target> { ["app"] = new() { Tags = ["app:abc"] } },
        };
        if (cycle)
        {
            Should.Throw<InvalidOperationException>(() => ContainerService.GetReleaseImages(bake, "abc"));
        }
        else
        {
            ContainerService.GetReleaseImages(bake, "abc").ShouldBe(["app:abc"]);
        }
    }

    [Theory]
    [InlineData("2026.01.02", true)]
    [InlineData("2026.1.2", false)]
    [InlineData("2026.02.30", false)]
    [InlineData("2026.01.02.1000", false)]
    [InlineData("2026.01.02.100", true)]
    [InlineData("2026.01.02.012", true)]
    [InlineData("2026.01.02.001", true)]
    [InlineData("2026.01.02.000", false)]
    [InlineData("2026.01.02\n", false)]
    [InlineData("2026.01.02.", false)]
    [InlineData("-bad", false)]
    [InlineData("2026.001.02", false)]
    public void VersionValidation_UsesCalendarVersionRules(string version, bool valid)
        => CalendarVersion.TryValidate(version, out _).ShouldBe(valid);
}
