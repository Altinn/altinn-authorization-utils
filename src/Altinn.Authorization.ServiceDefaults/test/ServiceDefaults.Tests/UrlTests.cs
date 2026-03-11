using Altinn.Authorization.ServiceDefaults.HttpClient;

namespace Altinn.Authorization.ServiceDefaults.Tests;

public class UrlTests
{
    [Fact]
    public void AbsoluteNoParams()
    {
        var url = Url.Create("https://foo/bar/bat", []);
        url.ShouldBe("https://foo/bar/bat");
    }

    [Fact]
    public void RelativeNoParams()
    {
        var url = Url.Create("bar/bat", []);
        url.ShouldBe("bar/bat");
    }

    [Fact]
    public void RelativeStartingWithDotNoParams()
    {
        var url = Url.Create("../bar/bat", []);
        url.ShouldBe("../bar/bat");
    }

    [Fact]
    public void AbsoluteWithParams()
    {
        var url = Url.Create("https://foo/bar/bat", [new("a", "1"), new("b", "2")]);
        url.ShouldBe("https://foo/bar/bat?a=1&b=2");
    }

    [Fact]
    public void RelativeWithParams()
    {
        var url = Url.Create("bar/bat", [new("a", "1"), new("b", "2")]);
        url.ShouldBe("bar/bat?a=1&b=2");
    }

    [Fact]
    public void RelativeStartingWithDotWithParams()
    {
        var url = Url.Create("../bar/bat", [new("a", "1"), new("b", "2")]);
        url.ShouldBe("../bar/bat?a=1&b=2");
    }

    [Fact]
    public void MultipleParamsWithSameName()
    {
        var url = Url.Create("bar/bat", [new("a", "1"), new("a", "2")]);
        url.ShouldBe("bar/bat?a=1&a=2");
    }

    [Fact]
    public void SingleParamNoValue()
    {
        var url = Url.Create("bar/bat", [new("a")]);
        url.ShouldBe("bar/bat?a");
    }

    [Fact]
    public void MultipleParamsNoValue()
    {
        var url = Url.Create("bar/bat", [new("a"), new("b")]);
        url.ShouldBe("bar/bat?a&b");
    }

    [Fact]
    public void EscapedParams()
    {
        var url = Url.Create("bar/bat", [new("a b", "1 2"), new("c&d", "3&4")]);
        url.ShouldBe("bar/bat?a%20b=1%202&c%26d=3%264");
    }
}
