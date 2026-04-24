namespace Altinn.Authorization.ProblemDetails.Tests;

public class ProblemInstanceTests
{
    [Fact]
    public void Equality_Ignores_Exception()
    {
        var withoutException = ProblemInstance.Create(TestErrors.BadRequest);
        var withException = ProblemInstance.Create(TestErrors.BadRequest, new InvalidOperationException("boom"));

        withoutException.ShouldBe(withException);
        withException.ShouldBe(withoutException);
        withoutException.GetHashCode().ShouldBe(withException.GetHashCode());
        withoutException.Equals(withException).ShouldBeTrue();
        withException.Equals(withoutException).ShouldBeTrue();
        withoutException.Equals((object)withException).ShouldBeTrue();
        withException.Equals((object)withoutException).ShouldBeTrue();
        (withoutException == withException).ShouldBeTrue();
        (withException == withoutException).ShouldBeTrue();
        (withoutException != withException).ShouldBeFalse();
        (withException != withoutException).ShouldBeFalse();
    }

    [Fact]
    public void ToException_Returns_ProblemInstanceException()
    {
        var innerException = new InvalidOperationException("boom");
        var instance = ProblemInstance.Create(TestErrors.BadRequest, innerException);

        var exception = instance.ToException();

        exception.ShouldBeOfType<ProblemInstanceException>();
        exception.Problem.ShouldBe(instance);
        exception.InnerException.ShouldBe(innerException);
    }

    [Fact]
    public void Throw_Throws_ProblemInstanceException()
    {
        var innerException = new InvalidOperationException("boom");
        var instance = ProblemInstance.Create(TestErrors.BadRequest, innerException);

        var exception = Should.Throw<ProblemInstanceException>(() => instance.Throw());
        exception.Problem.ShouldBe(instance);
        exception.InnerException.ShouldBe(innerException);
    }

    [Fact]
    public void Throw_Generic_Throws_ProblemInstanceException()
    {
        var innerException = new InvalidOperationException("boom");
        var instance = ProblemInstance.Create(TestErrors.BadRequest, innerException);

        var exception = Should.Throw<ProblemInstanceException>(() => instance.Throw<int>());
        exception.Problem.ShouldBe(instance);
        exception.InnerException.ShouldBe(innerException);
    }
}
