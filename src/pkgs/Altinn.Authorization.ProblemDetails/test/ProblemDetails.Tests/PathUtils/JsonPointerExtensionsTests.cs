using Altinn.Authorization.ProblemDetails.PathUtils;

namespace Altinn.Authorization.ProblemDetails.Tests.PathUtils;

public class JsonPointerExtensionsTests
{
    [Theory]
    [InlineData("$", "/")]
    [InlineData("$.foo", "/foo")]
    [InlineData("$.items[0]", "/items/0")]
    [InlineData("$['foo']['bar']", "/foo/bar")]
    public void CreateFromSystemTextJsonPath_ReturnsJsonPointer(string jsonPath, string expected)
    {
        var result = JsonPointerExtensions.CreateFromSystemTextJsonPath(jsonPath);

        result.ShouldBe(expected);
    }
}
