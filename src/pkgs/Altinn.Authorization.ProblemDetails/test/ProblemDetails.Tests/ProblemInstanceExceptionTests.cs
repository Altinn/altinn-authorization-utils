namespace Altinn.Authorization.ProblemDetails.Tests;

public class ProblemInstanceExceptionTests
{
    [Fact]
    public void Uses_Provided_InnerException_When_NonNull()
    {
        var problemInnerException = new InvalidOperationException("problem");
        var providedInnerException = new ArgumentException("provided");
        var instance = ProblemInstance.Create(TestErrors.BadRequest, problemInnerException);

        var exception = new ProblemInstanceException(message: null, providedInnerException, instance);

        exception.Problem.ShouldBe(instance);
        exception.InnerException.ShouldBe(providedInnerException);
    }

    [Fact]
    public void Uses_ProblemInstance_Exception_When_InnerException_IsNull()
    {
        var problemInnerException = new InvalidOperationException("problem");
        var instance = ProblemInstance.Create(TestErrors.BadRequest, problemInnerException);

        var exception = new ProblemInstanceException(message: null, innerException: null, instance);

        exception.Problem.ShouldBe(instance);
        exception.InnerException.ShouldBe(problemInnerException);
    }
}
