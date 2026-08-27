using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Altinn.Authorization.TestUtils.AspNetCore;
using Altinn.Authorization.TestUtils.Shouldly;
using CommunityToolkit.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace Altinn.Authorization.ProblemDetails.Tests.Mvc;

public class AltinnValidationProblemDetailsFactoryTests
    : IAsyncLifetime
{
    private TestClient? _client;

    private CancellationToken CancellationToken
        => TestContext.Current.CancellationToken;

    private TestClient Client
        => _client ?? ThrowHelper.ThrowInvalidOperationException<TestClient>("Test client has not been initialized.");

    public async ValueTask InitializeAsync()
    {
        _client = await TestClient.CreateControllerClient<TestController>(
            configureBuilder: builder =>
            {
                builder.Services.AddAltinnProblemDetails();
            });
    }

    public async ValueTask DisposeAsync()
    {
        if (_client is IAsyncDisposable asyncDisposable)
        {
            await asyncDisposable.DisposeAsync();
        }
        else if (_client is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }

    [Fact]
    public async Task Missing_RequiredByAttribute_Returns_ValidationProblemDetails()
    {
        using var response = await Client.PostAsJsonAsync("/body/required/by-attribute", new { }, CancellationToken);
        await response.ShouldHaveStatusCode(HttpStatusCode.BadRequest);

        var problemDetails = await response.Content.ReadFromJsonAsync<AltinnValidationProblemDetails>(cancellationToken: CancellationToken);
        problemDetails.ShouldNotBeNull();

        problemDetails.Errors.ShouldNotBeNull();
        problemDetails.Errors.Count.ShouldBe(1);

        var error = problemDetails.Errors.First();
        error.ShouldSatisfyAllConditions([
            e => e.Paths.ShouldBe(["/~0value~1"]),
            e => e.ErrorCode.ShouldBe(StdValidationErrors.Required.ErrorCode),
        ]);
    }

    [Fact]
    public async Task Missing_RequiredByKeyword_Returns_ValidationProblemDetails()
    {
        using var response = await Client.PostAsJsonAsync("/body/required/by-keyword", new { }, CancellationToken);
        await response.ShouldHaveStatusCode(HttpStatusCode.BadRequest);

        var problemDetails = await response.Content.ReadFromJsonAsync<AltinnValidationProblemDetails>(cancellationToken: CancellationToken);
        problemDetails.ShouldNotBeNull();

        problemDetails.Errors.ShouldNotBeNull();
        problemDetails.Errors.Count.ShouldBe(2); // json parse errors produces two errors, one for the parsing failing, and one for the body (method parameter) not being set

        var error = problemDetails.Errors.First(e => e.ErrorCode == StdValidationErrors.JsonError.ErrorCode);
        error.ShouldSatisfyAllConditions([
            e => e.Paths.ShouldBe(["/"]),
        ]);
    }

    [Fact]
    public async Task Missing_RequiredByAttribute_List_Returns_ValidationProblemDetails()
    {
        using var response = await Client.PostAsJsonAsync("/body/list/required/by-attribute", new List<RequiredByAttribute> { new(), new() }, CancellationToken);
        await response.ShouldHaveStatusCode(HttpStatusCode.BadRequest);

        var problemDetails = await response.Content.ReadFromJsonAsync<AltinnValidationProblemDetails>(cancellationToken: CancellationToken);
        problemDetails.ShouldNotBeNull();

        problemDetails.Errors.ShouldNotBeNull();
        problemDetails.Errors.Count.ShouldBe(2);

        var error = problemDetails.Errors.First(static e => e.Paths.Any(static p => p.StartsWith("/0")));
        error.ShouldSatisfyAllConditions([
            e => e.Paths.ShouldBe(["/0/~0value~1"]),
            e => e.ErrorCode.ShouldBe(StdValidationErrors.Required.ErrorCode),
        ]);
    }

    [Fact]
    public async Task Missing_NestedRequiredByAttribute_Returns_ValidationProblemDetails()
    {
        using var response = await Client.PostAsJsonAsync("/body/nested/required/by-attribute", new { }, CancellationToken);
        await response.ShouldHaveStatusCode(HttpStatusCode.BadRequest);

        var problemDetails = await response.Content.ReadFromJsonAsync<AltinnValidationProblemDetails>(cancellationToken: CancellationToken);
        problemDetails.ShouldNotBeNull();

        problemDetails.Errors.ShouldNotBeNull();
        problemDetails.Errors.Count.ShouldBe(1);

        var error = problemDetails.Errors.First();
        error.ShouldSatisfyAllConditions([
            e => e.Paths.ShouldBe(["/nested"]),
            e => e.ErrorCode.ShouldBe(StdValidationErrors.Required.ErrorCode),
        ]);
    }

    [Fact]
    public async Task Missing_NestedChildRequiredByAttribute_Returns_ValidationProblemDetails()
    {
        using var response = await Client.PostAsJsonAsync("/body/nested/required/by-attribute", new { nested = new { } }, CancellationToken);
        await response.ShouldHaveStatusCode(HttpStatusCode.BadRequest);

        var problemDetails = await response.Content.ReadFromJsonAsync<AltinnValidationProblemDetails>(cancellationToken: CancellationToken);
        problemDetails.ShouldNotBeNull();

        problemDetails.Errors.ShouldNotBeNull();
        problemDetails.Errors.Count.ShouldBe(1);

        var error = problemDetails.Errors.First();
        error.ShouldSatisfyAllConditions([
            e => e.Paths.ShouldBe(["/nested/~0value~1"]),
            e => e.ErrorCode.ShouldBe(StdValidationErrors.Required.ErrorCode),
        ]);
    }

    [Fact]
    public async Task Missing_RequiredByAttribute_Query_Returns_ValidationProblemDetails()
    {
        using var response = await Client.GetAsync("/query/required/by-attribute", CancellationToken);
        await response.ShouldHaveStatusCode(HttpStatusCode.BadRequest);

        var problemDetails = await response.Content.ReadFromJsonAsync<AltinnValidationProblemDetails>(cancellationToken: CancellationToken);
        problemDetails.ShouldNotBeNull();

        problemDetails.Errors.ShouldNotBeNull();
        problemDetails.Errors.Count.ShouldBe(1);

        var error = problemDetails.Errors.First();
        error.ShouldSatisfyAllConditions([
            e => e.Paths.ShouldBe(["/$QUERY/value"]),
            e => e.ErrorCode.ShouldBe(StdValidationErrors.Required.ErrorCode),
        ]);
    }

    [Fact]
    public async Task Invalid_RequiredByAttribute_Returns_InvalidValue_ValidationProblemDetails()
    {
        using var response = await Client.PostAsJsonAsync("/body/required/with-length", new { value = "abcd" }, CancellationToken);
        await response.ShouldHaveStatusCode(HttpStatusCode.BadRequest);

        var problemDetails = await response.Content.ReadFromJsonAsync<AltinnValidationProblemDetails>(cancellationToken: CancellationToken);
        problemDetails.ShouldNotBeNull();

        problemDetails.Errors.ShouldNotBeNull();
        problemDetails.Errors.Count.ShouldBe(1);

        var error = problemDetails.Errors.First();
        error.ShouldSatisfyAllConditions([
            e => e.Paths.ShouldBe(["/value"]),
            e => e.ErrorCode.ShouldBe(StdValidationErrors.StringLength.ErrorCode),
            e => e.Detail.ShouldBeNull(),
        ]);
    }

    [Fact]
    public async Task Missing_ImplicitRequired_Returns_ValidationProblemDetails()
    {
        using var response = await Client.PostAsJsonAsync("/body/required/implicit", new { }, CancellationToken);
        await response.ShouldHaveStatusCode(HttpStatusCode.BadRequest);

        var problemDetails = await response.Content.ReadFromJsonAsync<AltinnValidationProblemDetails>(cancellationToken: CancellationToken);
        problemDetails.ShouldNotBeNull();

        problemDetails.Errors.ShouldNotBeNull();
        problemDetails.Errors.Count.ShouldBe(1);

        var error = problemDetails.Errors.First();
        error.ShouldSatisfyAllConditions([
            e => e.Paths.ShouldBe(["/value"]),
            e => e.ErrorCode.ShouldBe(StdValidationErrors.Required.ErrorCode),
            e => e.Detail.ShouldBeNull(),
        ]);
    }

    [Fact]
    public async Task Invalid_DisplayNameAttribute_Returns_ValidationProblemDetails()
    {
        using var response = await Client.PostAsJsonAsync("/body/display-name", new { value = "abcd" }, CancellationToken);
        await response.ShouldHaveStatusCode(HttpStatusCode.BadRequest);

        var problemDetails = await response.Content.ReadFromJsonAsync<AltinnValidationProblemDetails>(cancellationToken: CancellationToken);
        problemDetails.ShouldNotBeNull();

        problemDetails.Errors.ShouldNotBeNull();
        problemDetails.Errors.Count.ShouldBe(1);

        var error = problemDetails.Errors.First();
        error.ShouldSatisfyAllConditions([
            e => e.Paths.ShouldBe(["/value"]),
            e => e.ErrorCode.ShouldBe(StdValidationErrors.StringLength.ErrorCode),
            e => e.Detail.ShouldBeNull(),
        ]);
    }

    [Fact]
    public async Task Invalid_CompareWithOtherPropertyDisplayName_Returns_ValidationProblemDetails()
    {
        using var response = await Client.PostAsJsonAsync(
            "/body/compare-display-name",
            new { compare = "left", compareTo = "right" },
            CancellationToken);
        await response.ShouldHaveStatusCode(HttpStatusCode.BadRequest);

        var problemDetails = await response.Content.ReadFromJsonAsync<AltinnValidationProblemDetails>(cancellationToken: CancellationToken);
        problemDetails.ShouldNotBeNull();

        problemDetails.Errors.ShouldNotBeNull();
        problemDetails.Errors.Count.ShouldBe(1);

        var error = problemDetails.Errors.First();
        error.ShouldSatisfyAllConditions([
            e => e.Paths.ShouldBe(["/compare"]),
            e => e.ErrorCode.ShouldBe(StdValidationErrors.Compare.ErrorCode),
            e => e.Detail.ShouldBeNull(),
        ]);
    }

    [Fact]
    public async Task Invalid_CustomError_Returns_CustomError_ValidationProblemDetails()
    {
        using var response = await Client.PostAsJsonAsync("/body/custom-error", new WithCustomErrorProperty { Value = new() }, CancellationToken);
        await response.ShouldHaveStatusCode(HttpStatusCode.BadRequest);

        var problemDetails = await response.Content.ReadFromJsonAsync<AltinnValidationProblemDetails>(cancellationToken: CancellationToken);
        problemDetails.ShouldNotBeNull();

        problemDetails.Errors.ShouldNotBeNull();
        problemDetails.Errors.Count.ShouldBe(2);

        var error = problemDetails.Errors.First(e => e.ErrorCode == StdValidationErrors.DeniedValues.ErrorCode);
        error.ShouldSatisfyAllConditions([
            e => e.Paths.ShouldBe(["/~0c$u%s''t.o\"m[5]\\~1"]),
            e => e.ErrorCode.ShouldBe(StdValidationErrors.DeniedValues.ErrorCode),
            e => e.Detail.ShouldBe("This is a custom JSON error value."),
            e => e.Extensions.ShouldContainKey("foo"),
            e => e.Extensions["foo"].ShouldBeOfType<JsonElement>().ShouldBeStructurallyEquivalentTo("\"bar\""),
        ]);
    }

    public static TheoryData<object, string, ValidationErrorDescriptor> InvalidValidationAttributeData
        => new()
        {
            { ValidationAttributePayload(stringLength: "abcd"), "/stringLength", StdValidationErrors.StringLength },
            { ValidationAttributePayload(minLength: "a"), "/minLength", StdValidationErrors.MinLength },
            { ValidationAttributePayload(maxLength: "abcd"), "/maxLength", StdValidationErrors.MaxLength },
            { ValidationAttributePayload(length: "a"), "/length", StdValidationErrors.Length },
            { ValidationAttributePayload(range: 4), "/range", StdValidationErrors.Range },
            { ValidationAttributePayload(regularExpression: "bbb"), "/regularExpression", StdValidationErrors.RegularExpression },
            { ValidationAttributePayload(compare: "left", compareTo: "right"), "/compare", StdValidationErrors.Compare },
            { ValidationAttributePayload(emailAddress: "not-email"), "/emailAddress", StdValidationErrors.EmailAddress },
            { ValidationAttributePayload(phone: "not-phone"), "/phone", StdValidationErrors.Phone },
            { ValidationAttributePayload(url: "not-url"), "/url", StdValidationErrors.Url },
            { ValidationAttributePayload(creditCard: "123"), "/creditCard", StdValidationErrors.CreditCard },
            { ValidationAttributePayload(allowedValues: "denied"), "/allowedValues", StdValidationErrors.AllowedValues },
            { ValidationAttributePayload(deniedValues: "denied"), "/deniedValues", StdValidationErrors.DeniedValues },
            { ValidationAttributePayload(base64String: "not-base64"), "/base64String", StdValidationErrors.Base64String },
        };

    [Theory]
    [MemberData(nameof(InvalidValidationAttributeData))]
    public async Task Invalid_ValidationAttribute_Returns_InvalidValue_ValidationProblemDetails(
        object payload,
        string expectedPath,
        ValidationErrorDescriptor expectedDescriptor)
    {
        using var response = await Client.PostAsJsonAsync("/body/validation-attributes", payload, CancellationToken);
        await response.ShouldHaveStatusCode(HttpStatusCode.BadRequest);

        var problemDetails = await response.Content.ReadFromJsonAsync<AltinnValidationProblemDetails>(cancellationToken: CancellationToken);
        problemDetails.ShouldNotBeNull();

        problemDetails.Errors.ShouldNotBeNull();
        problemDetails.Errors.Count.ShouldBe(1);

        var error = problemDetails.Errors.First();
        error.ShouldSatisfyAllConditions([
            e => e.Paths.ShouldBe([expectedPath]),
            e => e.ErrorCode.ShouldBe(expectedDescriptor.ErrorCode),
            e => e.Detail.ShouldBeNull(),
        ]);
    }

    private static object ValidationAttributePayload(
        string stringLength = "abc",
        string minLength = "abc",
        string maxLength = "abc",
        string length = "ab",
        int range = 2,
        string regularExpression = "aaa",
        string compare = "same",
        string compareTo = "same",
        string emailAddress = "test@example.com",
        string phone = "12345678",
        string url = "https://example.com",
        string creditCard = "4111111111111111",
        string allowedValues = "allowed",
        string deniedValues = "allowed",
        string base64String = "YWJj")
        => new
        {
            stringLength,
            minLength,
            maxLength,
            length,
            range,
            regularExpression,
            compare,
            compareTo,
            emailAddress,
            phone,
            url,
            creditCard,
            allowedValues,
            deniedValues,
            base64String,
        };

    [JsonConverter(typeof(JsonConverter))]
    public sealed record CustomJsonErrorValue
    {
        private sealed class JsonConverter
            : JsonConverter<CustomJsonErrorValue>
        {
            public override CustomJsonErrorValue? Read(
                ref Utf8JsonReader reader,
                Type typeToConvert,
                JsonSerializerOptions options)
            {
                var error = StdValidationErrors.DeniedValues.Create(
                    detail: "This is a custom JSON error value.",
                    extensions: [new("foo", "bar")]);

                throw new JsonException("I failed on purpose", error.ToException());
            }

            public override void Write(Utf8JsonWriter writer, CustomJsonErrorValue value, JsonSerializerOptions options)
            {
                writer.WriteStringValue("custom value");
            }
        }
    }

    public sealed record WithCustomErrorProperty
    {
        [JsonPropertyName("~c$u%s''t.o\"m[5]\\/")]
        public CustomJsonErrorValue? Value { get; init; }
    }

    public sealed record RequiredByAttribute
    {
        [Required]
        [JsonPropertyName("~value/")]
        public string? Value { get; init; }
    }

    public sealed record RequiredByKeyword
    {
        [JsonPropertyName("/value~")]
        public required string Value { get; init; }
    }

    public sealed record RequiredByBoth
    {
        [Required]
        public required string Value { get; init; }
    }

    public sealed record RequiredWithLength
    {
        [Required]
        [StringLength(3)]
        public string? Value { get; init; }
    }

    public sealed record ImplicitRequired
    {
        public string Value { get; init; } = null!;
    }

    public sealed record WithDisplayName
    {
        [DisplayName("Friendly Value")]
        [StringLength(3)]
        public string? Value { get; init; }
    }

    public sealed record CompareWithOtherPropertyDisplayName
    {
        [DisplayName("Left Value")]
        [Compare(nameof(CompareTo))]
        public string? Compare { get; init; }

        [DisplayName("Right Value")]
        public string? CompareTo { get; init; }
    }

    public sealed record NestedRequiredByAttribute
    {
        [Required]
        public RequiredByAttribute? Nested { get; init; }
    }

    public sealed record NestedRequiredByKeyword
    {
        public required RequiredByKeyword Nested { get; init; }
    }

    public sealed record NestedRequiredByBoth
    {
        [Required]
        public required RequiredByBoth Nested { get; init; }
    }

    public sealed record ValidationAttributes
    {
        [StringLength(3)]
        public string? StringLength { get; init; }

        [MinLength(3)]
        public string? MinLength { get; init; }

        [MaxLength(3)]
        public string? MaxLength { get; init; }

        [Length(2, 3)]
        public string? Length { get; init; }

        [Range(1, 3)]
        public int? Range { get; init; }

        [RegularExpression("^a+$")]
        public string? RegularExpression { get; init; }

        [Compare(nameof(CompareTo))]
        public string? Compare { get; init; }

        public string? CompareTo { get; init; }

        [EmailAddress]
        public string? EmailAddress { get; init; }

        [Phone]
        public string? Phone { get; init; }

        [Url]
        public string? Url { get; init; }

        [CreditCard]
        public string? CreditCard { get; init; }

        [AllowedValues("allowed")]
        public string? AllowedValues { get; init; }

        [DeniedValues("denied")]
        public string? DeniedValues { get; init; }

        [Base64String]
        public string? Base64String { get; init; }
    }

    [ApiController]
    public sealed class TestController
        : ControllerBase
    {
        [HttpPost("body/required/by-attribute")]
        public IActionResult RequiredByAttribute([FromBody] RequiredByAttribute body)
        {
            return Ok();
        }

        [HttpGet("query/required/by-attribute")]
        public IActionResult RequiredByAttributeQuery(
            [FromQuery, Required] string value)
        {
            return Ok();
        }

        [HttpPost("body/required/with-length")]
        public IActionResult RequiredWithLength([FromBody] RequiredWithLength body)
        {
            return Ok();
        }

        [HttpPost("body/required/implicit")]
        public IActionResult ImplicitRequired([FromBody] ImplicitRequired body)
        {
            return Ok();
        }

        [HttpPost("body/display-name")]
        public IActionResult DisplayName([FromBody] WithDisplayName body)
        {
            return Ok();
        }

        [HttpPost("body/compare-display-name")]
        public IActionResult CompareWithOtherPropertyDisplayName([FromBody] CompareWithOtherPropertyDisplayName body)
        {
            return Ok();
        }

        [HttpPost("body/validation-attributes")]
        public IActionResult ValidationAttributes([FromBody] ValidationAttributes body)
        {
            return Ok();
        }

        [HttpPost("body/required/by-keyword")]
        public IActionResult RequiredByKeyword([FromBody] RequiredByKeyword body)
        {
            return Ok();
        }

        [HttpPost("body/required/by-both")]
        public IActionResult RequiredByBoth([FromBody] RequiredByBoth body)
        {
            return Ok();
        }

        [HttpPost("body/nested/required/by-attribute")]
        public IActionResult NestedRequiredByAttribute([FromBody] NestedRequiredByAttribute body)
        {
            return Ok();
        }

        [HttpPost("body/nested/required/by-keyword")]
        public IActionResult NestedRequiredByKeyword([FromBody] NestedRequiredByKeyword body)
        {
            return Ok();
        }

        [HttpPost("body/nested/required/by-both")]
        public IActionResult NestedRequiredByBoth([FromBody] NestedRequiredByBoth body)
        {
            return Ok();
        }

        [HttpPost("body/list/required/by-attribute")]
        public IActionResult ListRequiredByAttribute([FromBody] List<RequiredByAttribute> body)
        {
            return Ok();
        }

        [HttpPost("body/custom-error")]
        public IActionResult CustomError([FromBody] WithCustomErrorProperty body)
        {
            return Ok();
        }

        [HttpPost("body/dict/custom-error")]
        public IActionResult DictCustomError([FromBody] Dictionary<string, WithCustomErrorProperty> body)
        {
            return Ok();
        }
    }
}
