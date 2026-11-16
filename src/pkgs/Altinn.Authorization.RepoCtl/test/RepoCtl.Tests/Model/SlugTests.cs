using Altinn.Authorization.RepoCtl.Model.Utils;

namespace Altinn.Authorization.RepoCtl.Model.Tests;

public class SlugTests
{
    [Theory]
    [InlineData("AuthorizationService", "authorization-service")]
    [InlineData("pkg:Altinn.Authorization", "pkg-altinn-authorization")]
    [InlineData("blåbær", "blabaer")]
    [InlineData("привет", "privet")]
    public void Slugify_PreservesTransliterationAndSeparators(string value, string expected)
    {
        Slug.Slugify(value).ShouldBe(expected);
    }
}
