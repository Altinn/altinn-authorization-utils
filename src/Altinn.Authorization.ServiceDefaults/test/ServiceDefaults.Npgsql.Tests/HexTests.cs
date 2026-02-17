using Altinn.Authorization.ServiceDefaults.Npgsql.Telemetry;

namespace Altinn.Authorization.ServiceDefaults.Npgsql.Tests;

public class HexTests
{
    [Theory]
    [InlineData(0U)]
    [InlineData(42U)]
    [InlineData(uint.MaxValue)]
    [InlineData(uint.MaxValue / 2)]
    [InlineData(uint.MaxValue / 4)]
    [InlineData(uint.MaxValue / 8)]
    [InlineData(uint.MaxValue / 16)]
    public void Format_ProducesExpectedResult32(uint value)
    {
        var expected = value.ToString("x8");
        var actual = string.Create(8, value, static (span, value) => Hex.Format(value, span));

        actual.ShouldBe(expected);
    }

    [Theory]
    [InlineData(0UL)]
    [InlineData(42U)]
    [InlineData(ulong.MaxValue)]
    [InlineData(ulong.MaxValue / 2)]
    [InlineData(ulong.MaxValue / 4)]
    [InlineData(ulong.MaxValue / 8)]
    [InlineData(ulong.MaxValue / 16)]
    public void Format_ProducesExpectedResult64(ulong value)
    {
        var expected = value.ToString("x16");
        var actual = string.Create(16, value, static (span, value) => Hex.Format(value, span));

        actual.ShouldBe(expected);
    }
}
