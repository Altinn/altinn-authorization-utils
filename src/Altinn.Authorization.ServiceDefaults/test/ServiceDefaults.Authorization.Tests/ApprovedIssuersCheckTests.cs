using Altinn.Authorization.ServiceDefaults.Authorization.Scopes.PlatformAccessToken;

namespace Altinn.Authorization.ServiceDefaults.Authorization.Tests;

public class ApprovedIssuersCheckTests
{
    [Theory]
    [InlineData(WellKnownPlatformAccessTokenIssuers.Platform, "platform")]
    public void WellKnownCheck_Match(WellKnownPlatformAccessTokenIssuers wellknown, string issuer)
    {
        ApprovedIssuersCheck check = ApprovedIssuersCheck.Create(wellknown);
        bool result = check.Check(issuer);

        result.ShouldBeTrue();
    }

    [Theory]
    [InlineData(WellKnownPlatformAccessTokenIssuers.Platform, "app_foo_bar")]
    public void WellKnownCheck_Mismatch(WellKnownPlatformAccessTokenIssuers wellknown, string issuer)
    {
        ApprovedIssuersCheck check = ApprovedIssuersCheck.Create(wellknown);
        bool result = check.Check(issuer);

        result.ShouldBeFalse();
    }

    [Theory]
    [InlineData(new string[] {}, "any")]
    [InlineData(new string[] { "foo", "bar", "foobar" }, "foo")]
    [InlineData(new string[] { "foo", "bar", "foobar" }, "bar")]
    [InlineData(new string[] { "foo", "bar", "foobar" }, "foobar")]
    public void Check_True(string[] valid, string issuer)
    {
        ApprovedIssuersCheck check = ApprovedIssuersCheck.Create(valid);
        bool result = check.Check(issuer);

        result.ShouldBeTrue();
    }

    [Theory]
    [InlineData(new string[] { "foo", "bar", "foobar" }, "")]
    [InlineData(new string[] { "foo", "bar", "foobar" }, "f")]
    [InlineData(new string[] { "foo", "bar", "foobar" }, "o")]
    [InlineData(new string[] { "foo", "bar", "foobar" }, "ba")]
    [InlineData(new string[] { "foo", "bar", "foobar" }, "ar")]
    [InlineData(new string[] { "foo", "bar", "foobar" }, "oob")]
    [InlineData(new string[] { "foo", "bar", "foobar" }, "oba")]
    public void Check_False(string[] valid, string issuer)
    {
        ApprovedIssuersCheck check = ApprovedIssuersCheck.Create(valid);
        bool result = check.Check(issuer);

        result.ShouldBeFalse();
    }
}
