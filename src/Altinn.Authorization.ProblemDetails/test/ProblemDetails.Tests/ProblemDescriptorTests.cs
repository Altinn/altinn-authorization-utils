using System.Net;

namespace Altinn.Authorization.ProblemDetails.Tests;

public class ProblemDescriptorTests
{
    [Fact]
    public void Create_WithExtensionsDictionary_ReturnsAltinnProblemDetailsWithExtensions()
    {
        // Arrange
        var factory = ProblemDescriptorFactory.New("TEST");
        var descriptor = factory.Create(1, HttpStatusCode.BadRequest, "Test error");
        var extensions = new Dictionary<string, object?>
        {
            { "Key1", "Value1" },
            { "Key2", 123 },
        };

        // Act
        var result = descriptor.ToProblemDetails(extensions);

        // Assert
        result.ShouldNotBeNull();
        result.ErrorCode.ToString().ShouldBe("TEST-00001");
        result.Status.ShouldBe((int)HttpStatusCode.BadRequest);
        result.Title.ShouldBe("Test error");
        result.Extensions.ShouldContainKeyAndValue("Key1", "Value1");
        result.Extensions.ShouldContainKeyAndValue("Key2", 123);
    }

    [Fact]
    public void Create_WithExtensionsCollectionExpression_ReturnsAltinnProblemDetailsWithExtensions()
    {
        // Arrange
        var factory = ProblemDescriptorFactory.New("TEST");
        var descriptor = factory.Create(1, HttpStatusCode.BadRequest, "Test error");

        // Act
        var result = descriptor.ToProblemDetails([
            new("Key1", "Value1"),
            new("Key2", 123),
        ]);

        // Assert
        result.ShouldNotBeNull();
        result.ErrorCode.ToString().ShouldBe("TEST-00001");
        result.Status.ShouldBe((int)HttpStatusCode.BadRequest);
        result.Title.ShouldBe("Test error");
        result.Extensions.ShouldContainKeyAndValue("Key1", "Value1");
        result.Extensions.ShouldContainKeyAndValue("Key2", 123);
    }
}
