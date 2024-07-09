﻿namespace Altinn.Authorization.ProblemDetails.Tests;

public class ResultTests
{
    public static TheoryData<Result<ErrorCode>, ErrorCode> ErrorCodeResults => new()
    {
        { StdProblemDescriptors.ErrorCodes.ValidationError, StdProblemDescriptors.ErrorCodes.ValidationError },
        { StdProblemDescriptors.ValidationError, StdProblemDescriptors.ErrorCodes.ValidationError },
    };

    [Theory]
    [MemberData(nameof(ErrorCodeResults))]
    public void CanPatternMatch_OrderIndependent(Result<ErrorCode> input, ErrorCode expected)
    {
        ErrorCode actual;

        // IsSuccess check
        actual = input switch
        {
            { IsSuccess: true } => input.Value,
            _ => input.Problem.ErrorCode,
        };

        actual.Should().Be(expected);

        // IsProblem check
        actual = input switch
        {
            { IsProblem: true } => input.Problem.ErrorCode,
            _ => input.Value,
        };

        actual.Should().Be(expected);

        // Using the 'is' pattern with IsSuccess
        if (input is { IsSuccess: true })
        {
            actual = input.Value;
        }
        else
        {
            actual = input.Problem.ErrorCode;
        }

        actual.Should().Be(expected);

        // Using the 'is' pattern with IsProblem
        if (input is { IsProblem: true })
        {
            actual = input.Problem.ErrorCode;
        }
        else
        {
            actual = input.Value;
        }

        actual.Should().Be(expected);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void ValidationTestCase(bool fail)
    {
        var result = TrySomething(fail);
        result.IsProblem.Should().Be(fail);

        if (result.IsProblem)
        {
            result.Problem.ErrorCode.Should().Be(StdProblemDescriptors.ErrorCodes.ValidationError);
            result.Problem.Should().BeOfType<ValidationProblemInstance>()
                .Which.Errors.Should().ContainSingle()
                .Which.ErrorCode.Should().Be(StdValidationErrors.ErrorCodes.Required);
        }
        else
        {
            result.Value.Should().Be(42);
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
}
