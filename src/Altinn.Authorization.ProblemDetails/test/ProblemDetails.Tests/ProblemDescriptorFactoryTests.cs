using System.Net;

namespace Altinn.Authorization.ProblemDetails.Tests;

public class ProblemDescriptorFactoryTests
{
    [Fact]
    public void Create_ReturnsCorrectAltinnProblemDetails()
    {
        // Arrange
        var factory = ProblemDescriptorFactory.New("TEST");

        // Act
        var result = factory.Create(1, HttpStatusCode.BadRequest, "Test error");

        // Assert
        result.ShouldNotBeNull();
        result.ErrorCode.ToString().ShouldBe("TEST-00001");
        result.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        result.Detail.ShouldBe("Test error");
    }

    [Fact]
    public void New_WithInvalidDomainName_ThrowsArgumentException()
    {
        // Arrange & Act & Assert
        Action act = () => ProblemDescriptorFactory.New("TESTDOMAIN");
        act.ShouldThrow<ArgumentException>();
    }
}
