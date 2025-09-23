namespace Altinn.Authorization.ServiceDefaults.Tests;

public class AltinnEnvironmentTests
{
    [Theory]
    [InlineData("LOCAL", true)]
    [InlineData("LOCAL-AT21", true)]
    [InlineData("LOCAL-AT22", true)]
    [InlineData("LOCAL-AT23", true)]
    [InlineData("LOCAL-AT24", true)]
    [InlineData("AT21", false)]
    [InlineData("AT22", false)]
    [InlineData("AT23", false)]
    [InlineData("AT24", false)]
    [InlineData("YT01", false)]
    [InlineData("TT02", false)]
    [InlineData("PROD", false)]
    [InlineData("OTHER", false)]
    public void IsLocalDev(string name, bool expected)
    {
        var env = AltinnEnvironment.Create(name);
        
        env.IsLocalDev.ShouldBe(expected);
    }

    [Theory]
    [InlineData("LOCAL", false)]
    [InlineData("LOCAL-AT21", false)]
    [InlineData("LOCAL-AT22", false)]
    [InlineData("LOCAL-AT23", false)]
    [InlineData("LOCAL-AT24", false)]
    [InlineData("AT21", true)]
    [InlineData("AT22", true)]
    [InlineData("AT23", true)]
    [InlineData("AT24", true)]
    [InlineData("YT01", false)]
    [InlineData("TT02", false)]
    [InlineData("PROD", false)]
    [InlineData("OTHER", false)]
    public void IsAT(string name, bool expected)
    {
        var env = AltinnEnvironment.Create(name);

        env.IsAT.ShouldBe(expected);
    }

    [Theory]
    [InlineData("LOCAL", false)]
    [InlineData("LOCAL-AT21", false)]
    [InlineData("LOCAL-AT22", false)]
    [InlineData("LOCAL-AT23", false)]
    [InlineData("LOCAL-AT24", false)]
    [InlineData("AT21", false)]
    [InlineData("AT22", false)]
    [InlineData("AT23", false)]
    [InlineData("AT24", false)]
    [InlineData("YT01", true)]
    [InlineData("TT02", false)]
    [InlineData("PROD", false)]
    [InlineData("OTHER", false)]
    public void IsYT(string name, bool expected)
    {
        var env = AltinnEnvironment.Create(name);

        env.IsYT.ShouldBe(expected);
    }

    [Theory]
    [InlineData("LOCAL", false)]
    [InlineData("LOCAL-AT21", false)]
    [InlineData("LOCAL-AT22", false)]
    [InlineData("LOCAL-AT23", false)]
    [InlineData("LOCAL-AT24", false)]
    [InlineData("AT21", false)]
    [InlineData("AT22", false)]
    [InlineData("AT23", false)]
    [InlineData("AT24", false)]
    [InlineData("YT01", false)]
    [InlineData("TT02", true)]
    [InlineData("PROD", false)]
    [InlineData("OTHER", false)]
    public void IsTT(string name, bool expected)
    {
        var env = AltinnEnvironment.Create(name);

        env.IsTT.ShouldBe(expected);
    }

    [Theory]
    [InlineData("LOCAL", false)]
    [InlineData("LOCAL-AT21", false)]
    [InlineData("LOCAL-AT22", false)]
    [InlineData("LOCAL-AT23", false)]
    [InlineData("LOCAL-AT24", false)]
    [InlineData("AT21", false)]
    [InlineData("AT22", false)]
    [InlineData("AT23", false)]
    [InlineData("AT24", false)]
    [InlineData("YT01", false)]
    [InlineData("TT02", false)]
    [InlineData("PROD", true)]
    [InlineData("OTHER", false)]
    public void IsProd(string name, bool expected)
    {
        var env = AltinnEnvironment.Create(name);

        env.IsProd.ShouldBe(expected);
    }

    [Theory]
    [InlineData("LOCAL", false)]
    [InlineData("LOCAL-AT21", false)]
    [InlineData("LOCAL-AT22", false)]
    [InlineData("LOCAL-AT23", false)]
    [InlineData("LOCAL-AT24", false)]
    [InlineData("AT21", false)]
    [InlineData("AT22", false)]
    [InlineData("AT23", false)]
    [InlineData("AT24", false)]
    [InlineData("YT01", false)]
    [InlineData("TT02", false)]
    [InlineData("PROD", false)]
    [InlineData("OTHER", true)]
    public void IsUnknown(string name, bool expected)
    {
        var env = AltinnEnvironment.Create(name);

        env.IsUnknown.ShouldBe(expected);
    }

    [Theory]
    [InlineData("LOCAL", AltinnEnvironmentId.AT22)]
    [InlineData("LOCAL-AT21", AltinnEnvironmentId.AT21)]
    [InlineData("LOCAL-AT22", AltinnEnvironmentId.AT22)]
    [InlineData("LOCAL-AT23", AltinnEnvironmentId.AT23)]
    [InlineData("LOCAL-AT24", AltinnEnvironmentId.AT24)]
    [InlineData("AT21", AltinnEnvironmentId.AT21)]
    [InlineData("AT22", AltinnEnvironmentId.AT22)]
    [InlineData("AT23", AltinnEnvironmentId.AT23)]
    [InlineData("AT24", AltinnEnvironmentId.AT24)]
    [InlineData("YT01", AltinnEnvironmentId.YT01)]
    [InlineData("TT02", AltinnEnvironmentId.TT02)]
    [InlineData("PROD", AltinnEnvironmentId.PROD)]
    [InlineData("OTHER", AltinnEnvironmentId.Unknown)]
    public void EnvironmentId(string name, AltinnEnvironmentId expectedId)
    {
        var env = AltinnEnvironment.Create(name);
        
        env.Id.ShouldBe(expectedId);
    }

    [Theory]
    [InlineData("LOCAL", "AT22")]
    [InlineData("LOCAL-AT21", "AT21")]
    [InlineData("LOCAL-AT22", "AT22")]
    [InlineData("LOCAL-AT23", "AT23")]
    [InlineData("LOCAL-AT24", "AT24")]
    [InlineData("AT21", "AT21")]
    [InlineData("AT22", "AT22")]
    [InlineData("AT23", "AT23")]
    [InlineData("AT24", "AT24")]
    [InlineData("YT01", "YT01")]
    [InlineData("TT02", "TT02")]
    [InlineData("PROD", "PROD")]
    [InlineData("OTHER", "OTHER")]
    public void Format_PlatformEnv(string name, string expected)
    {
        var env = AltinnEnvironment.Create(name);

        env.ToString("P").ShouldBe(expected);
    }

    [Theory]
    [InlineData("LOCAL", "at22")]
    [InlineData("LOCAL-AT21", "at21")]
    [InlineData("LOCAL-AT22", "at22")]
    [InlineData("LOCAL-AT23", "at23")]
    [InlineData("LOCAL-AT24", "at24")]
    [InlineData("AT21", "at21")]
    [InlineData("AT22", "at22")]
    [InlineData("AT23", "at23")]
    [InlineData("AT24", "at24")]
    [InlineData("YT01", "yt01")]
    [InlineData("TT02", "tt02")]
    [InlineData("PROD", "prod")]
    [InlineData("OTHER", "other")]
    public void Format_PlatformEnvLower(string name, string expected)
    {
        var env = AltinnEnvironment.Create(name);

        env.ToString("p").ShouldBe(expected);
    }

    [Theory]
    [InlineData("LOCAL", "LOCAL-AT22")]
    [InlineData("LOCAL-AT21", "LOCAL-AT21")]
    [InlineData("LOCAL-AT22", "LOCAL-AT22")]
    [InlineData("LOCAL-AT23", "LOCAL-AT23")]
    [InlineData("LOCAL-AT24", "LOCAL-AT24")]
    [InlineData("AT21", "AT21")]
    [InlineData("AT22", "AT22")]
    [InlineData("AT23", "AT23")]
    [InlineData("AT24", "AT24")]
    [InlineData("YT01", "YT01")]
    [InlineData("TT02", "TT02")]
    [InlineData("PROD", "PROD")]
    [InlineData("OTHER", "OTHER")]
    public void Format(string name, string expected)
    {
        var env = AltinnEnvironment.Create(name);

        env.ToString().ShouldBe(expected);
    }

    [Theory]
    [InlineData("LOCAL", "local-at22")]
    [InlineData("LOCAL-AT21", "local-at21")]
    [InlineData("LOCAL-AT22", "local-at22")]
    [InlineData("LOCAL-AT23", "local-at23")]
    [InlineData("LOCAL-AT24", "local-at24")]
    [InlineData("AT21", "at21")]
    [InlineData("AT22", "at22")]
    [InlineData("AT23", "at23")]
    [InlineData("AT24", "at24")]
    [InlineData("YT01", "yt01")]
    [InlineData("TT02", "tt02")]
    [InlineData("PROD", "prod")]
    [InlineData("OTHER", "other")]
    public void Format_Lower(string name, string expected)
    {
        var env = AltinnEnvironment.Create(name);

        env.ToString("l").ShouldBe(expected);
    }

    [Theory]
    [InlineData("LOCAL-XX")]
    [InlineData("LOCAL-YT01")]
    [InlineData("LOCAL-TT02")]
    [InlineData("LOCAL-PROD")]
    public void UnkwnownLocalPrefix_Throws(string name)
    {
        Should.Throw<ArgumentException>(() => AltinnEnvironment.Create(name));
    }
}
