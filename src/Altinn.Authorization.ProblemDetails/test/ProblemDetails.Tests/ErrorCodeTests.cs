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

            errorCode1.ShouldBe(errorCode2);
            errorCode1.GetHashCode().ShouldBe(errorCode2.GetHashCode());
            errorCode1.Equals(errorCode2).ShouldBeTrue();
            errorCode2.Equals(errorCode1).ShouldBeTrue();
            errorCode1.Equals((object)errorCode2).ShouldBeTrue();
            errorCode2.Equals((object)errorCode1).ShouldBeTrue();
            (errorCode1 == errorCode2).ShouldBeTrue();
            (errorCode2 == errorCode1).ShouldBeTrue();
            (errorCode1 != errorCode2).ShouldBeFalse();
            (errorCode2 != errorCode1).ShouldBeFalse();

            var other = _domain.Code(42);
            errorCode1.ShouldNotBe(other);
            errorCode1.Equals(other).ShouldBeFalse();
            other.Equals(errorCode1).ShouldBeFalse();
            errorCode1.Equals((object)other).ShouldBeFalse();
            other.Equals((object)errorCode1).ShouldBeFalse();
            (errorCode1 == other).ShouldBeFalse();
            (other == errorCode1).ShouldBeFalse();
            (errorCode1 != other).ShouldBeTrue();
            (other != errorCode1).ShouldBeTrue();
        }
    }

    [Fact]
    public void Throws_IfTooLarge()
    {
        Action act = () => _domain.Code(100_000);
        act.ShouldThrow<ArgumentOutOfRangeException>();
    }
}
