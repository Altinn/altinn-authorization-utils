using System.Text.Json;
using Altinn.Authorization.RepoCtl.Model.ReleasePlease;

namespace Altinn.Authorization.RepoCtl.Model.Tests;

public class ReleasePleaseConfigTests
{
    [Fact]
    public void Deserialize_ReadsPackageOptionsAndExtraFileVariants()
    {
        const string json = """
            {
              "packages": {
                "src/package": {
                  "release-type": "simple",
                  "include-v-in-tag": false,
                  "extra-files": [
                    "version.txt",
                    {
                      "type": "toml",
                      "path": "Cargo.toml",
                      "jsonpath": "$.package.version",
                      "glob": false
                    },
                    {
                      "type": "pom",
                      "path": "pom.xml"
                    }
                  ]
                }
              }
            }
            """;

        var config = ReleasePleaseConfig.Deserialize(json);

        config.ShouldNotBeNull();
        var package = config.Packages["src/package"];
        package.ReleaseType.ShouldBe(ReleasePleaseReleaseType.Simple);
        package.IncludeVInTag.ShouldBe(false);
        package.ExtraFiles.ShouldNotBeNull();
        package.ExtraFiles.Count.ShouldBe(3);
        package.ExtraFiles[0].ShouldBeOfType<ReleasePleaseGenericExtraFile>();
        package.ExtraFiles[1].ShouldBeOfType<ReleasePleaseTomlExtraFile>().JsonPath.ShouldBe("$.package.version");
        package.ExtraFiles[2].ShouldBeOfType<ReleasePleasePomExtraFile>();
    }

    [Fact]
    public void Serialize_PreservesExplicitFalseValues()
    {
        var config = new ReleasePleaseConfig
        {
            Packages = new SortedDictionary<string, ReleasePleasePackage>
            {
                ["src/package"] = new()
                {
                    IncludeComponentInTag = false,
                    ExtraFiles = [new ReleasePleaseGenericExtraFile { Path = "version.txt", Glob = false }],
                },
            },
        };

        using var document = JsonDocument.Parse(JsonSerializer.Serialize(config));
        var package = document.RootElement.GetProperty("packages").GetProperty("src/package");

        package.GetProperty("include-component-in-tag").GetBoolean().ShouldBeFalse();
        package.GetProperty("extra-files")[0].GetProperty("glob").GetBoolean().ShouldBeFalse();
    }
}
