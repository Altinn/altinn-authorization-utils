using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Altinn.Authorization.ProblemDetails.Tests;

public class AltinnValidationProblemDetailsTests 
{
    private static readonly JsonSerializerOptions _options = new(JsonSerializerDefaults.Web);

    [Fact]
    public void HasCorrectErrorCode()
    {
        var problemDetails = new AltinnValidationProblemDetails();

        problemDetails.ErrorCode.ShouldBe(StdProblemDescriptors.ValidationError.ErrorCode);
    }

    [Fact]
    public void CanDeserializeAsValidationProblemDetails()
    {
        var problemDetails = new AltinnValidationProblemDetails([
            ValidationDescriptors.FieldRequired.ToValidationError("/field1"),
            ValidationDescriptors.FieldRequired.ToValidationError("/field2"),
            ValidationDescriptors.PasswordsMustMatch.ToValidationError(["/password", "/confirmPassword"]),
        ]);

        var serialized = JsonSerializer.Serialize(problemDetails, _options);
        var deserialized = JsonSerializer.Deserialize<ValidationProblemDetails>(serialized, _options);

        Assert.NotNull(deserialized);
        deserialized.Errors.ShouldBeEmpty();
        deserialized.Status.ShouldBe(problemDetails.Status);
        deserialized.Detail.ShouldBe(problemDetails.Detail);
    }

    [Fact]
    public void CanRoundTripThroughJson()
    {
        var problemDetails = new AltinnValidationProblemDetails([
            ValidationDescriptors.FieldRequired.ToValidationError("/field1"),
            ValidationDescriptors.FieldRequired.ToValidationError("/field2"),
            ValidationDescriptors.PasswordsMustMatch.ToValidationError(["/password", "/confirmPassword"]),
        ]);

        var serialized = JsonSerializer.Serialize(problemDetails, _options);
        var deserialized = JsonSerializer.Deserialize<AltinnValidationProblemDetails>(serialized, _options);

        Assert.NotNull(deserialized);
        deserialized.Errors.ShouldBeEquivalentTo(problemDetails.Errors);
        deserialized.Status.ShouldBe(problemDetails.Status);
        deserialized.Detail.ShouldBe(problemDetails.Detail);
    }

    [Fact]
    public void CanDeserializeEmptyObject()
    {
        var json = """{}""";

        var deserialized = JsonSerializer.Deserialize<AltinnValidationProblemDetails>(json, _options);
        Assert.NotNull(deserialized);

        deserialized.ErrorCode.ShouldNotBe(StdProblemDescriptors.ValidationError.ErrorCode);
        deserialized.Errors.ShouldBeEmpty();
    }

    private static class ValidationDescriptors
    {
        private static readonly ValidationErrorDescriptorFactory _factory
            = ValidationErrorDescriptorFactory.New("TEST");

        public static ValidationErrorDescriptor FieldRequired { get; }
            = _factory.Create(1, "Field is required.");

        public static ValidationErrorDescriptor FieldOutOfRange { get; }
            = _factory.Create(2, "Field is out of range.");

        public static ValidationErrorDescriptor PasswordsMustMatch { get; }
            = _factory.Create(3, "Passwords must match.");
    }
}
