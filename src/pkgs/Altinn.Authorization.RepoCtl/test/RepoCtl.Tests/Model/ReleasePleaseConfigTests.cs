using System.Buffers;
using System.Text;
using System.Text.Json;
using Altinn.Authorization.RepoCtl.Model.ReleasePlease;

namespace Altinn.Authorization.RepoCtl.Model.Tests;

public class ReleasePleaseConfigTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Deserialize_ReadsUtf8BomWithoutChangingInput(bool segmented)
    {
        var bytes = Encoding.UTF8.GetBytes("\uFEFF{\"packages\":{\"src/package\":{\"component\":\"example\"}}}");
        var first = new ByteSegment(bytes.AsMemory(0, 1));
        var second = first.Append(bytes.AsMemory(1, 1));
        var last = second.Append(bytes.AsMemory(2));
        var json = segmented
            ? new ReadOnlySequence<byte>(first, 0, last, last.Memory.Length)
            : new ReadOnlySequence<byte>(bytes);
        if (segmented)
        {
            json.IsSingleSegment.ShouldBeFalse();
        }

        var config = ReleasePleaseConfig.Deserialize(json);

        config.ShouldNotBeNull();
        config.Packages["src/package"].Component.ShouldBe("example");
        json.ToArray().ShouldBe(bytes);
        Encoding.UTF8.GetBytes(ReleasePleaseConfig.Serialize(config)).ShouldNotBe(bytes);
    }

    private sealed class ByteSegment : ReadOnlySequenceSegment<byte>
    {
        public ByteSegment(ReadOnlyMemory<byte> memory)
        {
            Memory = memory;
        }

        public ByteSegment Append(ReadOnlyMemory<byte> memory)
        {
            var next = new ByteSegment(memory) { RunningIndex = RunningIndex + Memory.Length };
            Next = next;
            return next;
        }
    }

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
            Packages =
            {
                ["src/package"] = new()
                {
                    IncludeComponentInTag = false,
                    ExtraFiles = [new ReleasePleaseGenericExtraFile { Path = "version.txt", Glob = false }],
                },
            },
        };

        using var document = JsonDocument.Parse(ReleasePleaseConfig.Serialize(config));
        var package = document.RootElement.GetProperty("packages").GetProperty("src/package");

        package.GetProperty("include-component-in-tag").GetBoolean().ShouldBeFalse();
        package.GetProperty("extra-files")[0].GetProperty("glob").GetBoolean().ShouldBeFalse();
    }
}
