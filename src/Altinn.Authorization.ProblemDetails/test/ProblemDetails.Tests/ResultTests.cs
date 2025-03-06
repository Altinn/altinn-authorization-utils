namespace Altinn.Authorization.ProblemDetails.Tests;

public class ResultTests
{
    [Fact]
    public void CanPatternMatch_OrderIndependent()
    {
        Test(StdProblemDescriptors.ErrorCodes.ValidationError, StdProblemDescriptors.ErrorCodes.ValidationError);
        Test(StdProblemDescriptors.ValidationError, StdProblemDescriptors.ErrorCodes.ValidationError);

        static void Test(Result<ErrorCode> input, ErrorCode expected)
        {
            ErrorCode actual;

            // IsSuccess check
            actual = input switch
            {
                { IsSuccess: true } => input.Value,
                _ => input.Problem.ErrorCode,
            };

            actual.ShouldBe(expected);

            // IsProblem check
            actual = input switch
            {
                { IsProblem: true } => input.Problem.ErrorCode,
                _ => input.Value,
            };

            actual.ShouldBe(expected);

            // Using the 'is' pattern with IsSuccess
            if (input is { IsSuccess: true })
            {
                actual = input.Value;
            }
            else
            {
                actual = input.Problem.ErrorCode;
            }

            actual.ShouldBe(expected);

            // Using the 'is' pattern with IsProblem
            if (input is { IsProblem: true })
            {
                actual = input.Problem.ErrorCode;
            }
            else
            {
                actual = input.Value;
            }

            actual.ShouldBe(expected);
        }
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void ValidationTestCase(bool fail)
    {
        var result = TrySomething(fail);
        result.IsProblem.ShouldBe(fail);

        if (result.IsProblem)
        {
            result.Problem.ErrorCode.ShouldBe(StdProblemDescriptors.ErrorCodes.ValidationError);
            var problem = result.Problem.ShouldBeOfType<ValidationProblemInstance>();
            problem.Errors.Length.ShouldBe(1);
            problem.Errors[0].ErrorCode.ShouldBe(StdValidationErrors.ErrorCodes.Required);
        }
        else
        {
            result.Value.ShouldBe(42);
        }

        static Result<int> TrySomething(bool fail)
        {
            ValidationErrorBuilder errors = new();

            if (fail)
            {
                errors.Add(StdValidationErrors.Required, "/field");
            }

            if (errors.TryBuild(out var errorInstance))
            {
                return errorInstance;
            }

            return 42;
        }
    }

    [Fact]
    public void EnsureSuccess_ThrowsIfProblem()
    {
        Result<int> result = StdProblemDescriptors.ValidationError;
        Action action = () => result.EnsureSuccess();
        action.ShouldThrow<ProblemInstanceException>()
            .Problem.ErrorCode.ShouldBe(StdProblemDescriptors.ErrorCodes.ValidationError);
    }

    [Fact]
    public void EnsureSuccess_DoesNotThrowIfSuccess()
    {
        Result<int> result = 42;
        result.EnsureSuccess();
    }
}
