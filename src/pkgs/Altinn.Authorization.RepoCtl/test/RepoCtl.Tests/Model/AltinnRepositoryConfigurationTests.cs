using System.Text;

namespace Altinn.Authorization.RepoCtl.Model.Tests;

public class AltinnRepositoryConfigurationTests
{
    [Theory]
    [InlineData("{}", "area/", "#0978af")]
    [InlineData("{\"app\":{\"color\":\"#123ABC\"}}", "area/", "#123ABC")]
    [InlineData("{\"app\":{\"prefix\":\"application/\"}}", "application/", "#0978af")]
    public async Task Labels_UseDefaultsForMissingSettings(string labels, string prefix, string color)
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes("{\"name\":\"Test\",\"labels\":" + labels + "}"));
        var result = await AltinnRepositoryConfiguration.Read(stream, TestContext.Current.CancellationToken);
        result.IsProblem.ShouldBeFalse(result.IsProblem && result.Problem is Altinn.Authorization.ProblemDetails.ValidationProblemInstance validation
            ? string.Join("\n", validation.Errors) : null);
        result.Value.Labels.ShouldBe(new AltinnRepositoryLabels { App = new(prefix, color) });
    }

    [Theory]
    [InlineData("\"bad\"")]
    [InlineData("\"#12345g\"")]
    [InlineData("\"123456\"")]
    public async Task Labels_RejectInvalidColors(string color)
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes("{\"name\":\"Test\",\"labels\":{\"lib\":{\"color\":" + color + "}}}"));
        var result = await AltinnRepositoryConfiguration.Read(stream, TestContext.Current.CancellationToken);
        result.IsProblem.ShouldBeTrue();
    }

    [Fact]
    public async Task Labels_RejectEmptyPrefixes()
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes("""{"name":"Test","labels":{"app":{"prefix":""}}}"""));
        var result = await AltinnRepositoryConfiguration.Read(stream, TestContext.Current.CancellationToken);
        result.IsProblem.ShouldBeTrue();
    }
}
