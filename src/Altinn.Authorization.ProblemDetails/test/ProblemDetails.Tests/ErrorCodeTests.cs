using System.Text.Json;

namespace Altinn.Authorization.ProblemDetails.Tests;

public class ErrorCodeTests
{
    private static readonly JsonSerializerOptions _options = new(JsonSerializerDefaults.Web);
    private static readonly ErrorCodeDomain _domain = ErrorCodeDomain.Get("TEST");
    private static readonly ErrorCodeDomain _subDomain = _domain.SubDomain("SUBD");

    [Theory]
    [InlineData(0u)]
    [InlineData(1u)]
    [InlineData(10_000u)]
    [InlineData(99_999u)]
    public void Equality(uint value)
    {
        CheckEquality(_domain, value);
        CheckEquality(_subDomain, value);

        static void CheckEquality(ErrorCodeDomain domain, uint value)
        {
            var errorCode1 = domain.Code(value);
            var errorCode2 = domain.Code(value);

            errorCode1.Should().Be(errorCode2);
            errorCode1.GetHashCode().Should().Be(errorCode2.GetHashCode());
            errorCode1.Equals(errorCode2).Should().BeTrue();
            errorCode2.Equals(errorCode1).Should().BeTrue();
            errorCode1.Equals((object)errorCode2).Should().BeTrue();
            errorCode2.Equals((object)errorCode1).Should().BeTrue();
            (errorCode1 == errorCode2).Should().BeTrue();
            (errorCode2 == errorCode1).Should().BeTrue();
            (errorCode1 != errorCode2).Should().BeFalse();
            (errorCode2 != errorCode1).Should().BeFalse();

            var other = _domain.Code(42);
            errorCode1.Should().NotBe(other);
            errorCode1.Equals(other).Should().BeFalse();
            other.Equals(errorCode1).Should().BeFalse();
            errorCode1.Equals((object)other).Should().BeFalse();
            other.Equals((object)errorCode1).Should().BeFalse();
            (errorCode1 == other).Should().BeFalse();
            (other == errorCode1).Should().BeFalse();
            (errorCode1 != other).Should().BeTrue();
            (other != errorCode1).Should().BeTrue();
        }
    }

    [Fact]
    public void Throws_IfTooLarge()
    {
        Action act = () => _domain.Code(100_000);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }
}
